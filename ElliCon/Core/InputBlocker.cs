using System.Runtime.InteropServices;
using Dalamud;
using Dalamud.Bindings.ImGui;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using ElliCon.Configuration;

namespace ElliCon.Core;

/// <summary>
/// Blocks game input from the controller when ImGui has focus.
/// </summary>
public unsafe class InputBlocker : IDisposable
{
    private readonly ControllerConfig _config;
    private readonly IGameInteropProvider _gameInterop;
    private readonly IPluginLog? _log;
    private bool _isEnabled;
    private readonly HashSet<string> _blockingWindows = new();
    private readonly HashSet<string> _nonBlockingWindows = new();
    private string? _lastFocusedWindow = null;

    [StructLayout(LayoutKind.Explicit, Size = 0x18)]
    private struct PlayerMoveControllerFlyInput
    {
        [FieldOffset(0x0)] public float Forward;
        [FieldOffset(0x4)] public float Left;
        [FieldOffset(0x8)] public float Up;
        [FieldOffset(0xC)] public float Turn;
        [FieldOffset(0x10)] public float u10;
        [FieldOffset(0x14)] public byte DirMode;
        [FieldOffset(0x15)] public byte HaveBackwardOrStrafe;
    }

    private delegate void RMIWalkDelegate(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
    [Signature("E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D")]
    private Hook<RMIWalkDelegate> _rmiWalkHook = null!;

    private delegate void RMIFlyDelegate(void* self, PlayerMoveControllerFlyInput* result);
    [Signature("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? B8")]
    private Hook<RMIFlyDelegate> _rmiFlyHook = null!;

    private enum CameraControlType : uint
    {
        None = 0,
        Keyboard = 1,
        Gamepad = 2,
        Mouse = 3
    }

    private delegate CameraControlType GetCameraControlTypeDelegate();
    [Signature("E8 ?? ?? ?? ?? 83 F8 01 74 5F")]
    private Hook<GetCameraControlTypeDelegate>? _getCameraControlTypeHook = null!;

    private Hook<PadDevicePollDelegate>? _padDevicePollHook;
    private delegate nint PadDevicePollDelegate(PadDevice* self);

    public bool IsEnabled => _isEnabled;

    public bool ShouldBlock => ShouldBlockGamepadInput();

    public InputBlocker(IGameInteropProvider gameInteropProvider, ControllerConfig config, IPluginLog? log = null)
    {
        _config = config;
        _gameInterop = gameInteropProvider;
        _log = log;

        gameInteropProvider.InitializeFromAttributes(this);

        if (_config.EnableDebugLogging && _log != null)
        {
            var cameraStatus = _getCameraControlTypeHook?.Address != IntPtr.Zero ? $"0x{_getCameraControlTypeHook.Address:X}" : "FAILED";
            _log.Information($"[ElliCon] InputBlocker initialized - Movement: 0x{_rmiWalkHook.Address:X}/0x{_rmiFlyHook.Address:X}, Camera: {cameraStatus}");
        }
    }

    public void Enable()
    {
        if (_isEnabled)
            return;

        if (_config.BlockMovement)
        {
            _rmiWalkHook?.Enable();
            _rmiFlyHook?.Enable();
        }

        if (_config.BlockCamera && _getCameraControlTypeHook?.Address != IntPtr.Zero)
        {
            _getCameraControlTypeHook?.Enable();
        }

        if (_config.BlockButtons)
        {
            TryEnablePadDeviceHook();
        }

        _isEnabled = true;

        if (_config.EnableDebugLogging && _log != null)
            _log.Information("[ElliCon] InputBlocker enabled");
    }

    public void Disable()
    {
        if (!_isEnabled)
            return;

        _rmiWalkHook?.Disable();
        _rmiFlyHook?.Disable();
        _getCameraControlTypeHook?.Disable();
        _padDevicePollHook?.Disable();

        _isEnabled = false;

        if (_config.EnableDebugLogging && _log != null)
            _log.Information("[ElliCon] InputBlocker disabled");
    }

    /// <summary>
    /// Registers a window that should block game input when focused.
    /// Only registered windows will cause input blocking.
    /// This ensures ElliCon only affects YOUR plugin's windows, not other plugins.
    /// </summary>
    /// <param name="windowName">The ImGui window name.</param>
    public void RegisterBlockingWindow(string windowName)
    {
        _blockingWindows.Add(windowName);
        
        if (_config.EnableDebugLogging && _log != null)
            _log.Debug($"[ElliCon] Registered blocking window: {windowName}");
    }
    
    public void UnregisterBlockingWindow(string windowName)
    {
        _blockingWindows.Remove(windowName);
        
        if (_config.EnableDebugLogging && _log != null)
            _log.Debug($"[ElliCon] Unregistered blocking window: {windowName}");
    }
    
    /// <summary>
    /// Registers a window as non-blocking. When this window has focus,
    /// game input will NOT be blocked even though ImGui navigation is active.
    /// Use this for automation/status windows that should remain interactable
    /// but allow background processes (like navmesh movement) to continue.
    /// </summary>
    /// <param name="windowName">The ImGui window name.</param>
    public void RegisterNonBlockingWindow(string windowName)
    {
        _nonBlockingWindows.Add(windowName);
        
        if (_config.EnableDebugLogging && _log != null)
            _log.Debug($"[ElliCon] Registered non-blocking window: {windowName}");
    }
    
    public void UnregisterNonBlockingWindow(string windowName)
    {
        _nonBlockingWindows.Remove(windowName);
        
        if (_config.EnableDebugLogging && _log != null)
            _log.Debug($"[ElliCon] Unregistered non-blocking window: {windowName}");
    }
    
    /// <summary>
    /// Updates the currently focused window. Call this from your plugin's Draw() method
    /// to track which window has focus for input blocking.
    /// </summary>
    /// <param name="windowName">The window name, or null if no window is focused.</param>
    public void UpdateFocusedWindow(string? windowName)
    {
        _lastFocusedWindow = windowName;
    }

    public void Dispose()
    {
        _rmiWalkHook?.Dispose();
        _rmiFlyHook?.Dispose();
        _getCameraControlTypeHook?.Dispose();
        _padDevicePollHook?.Dispose();
    }

    private void TryEnablePadDeviceHook()
    {
        if (_padDevicePollHook != null)
        {
            _padDevicePollHook.Enable();
            return;
        }

        try
        {
            var inputDeviceManager = InputDeviceManager.Instance();
            if (inputDeviceManager != null)
            {
                var padDevice = inputDeviceManager->PadDevice;
                if (padDevice != null)
                {
                    var vtable = *(nint**)padDevice;
                    var pollAddress = vtable[2];
                    _padDevicePollHook = _gameInterop.HookFromAddress<PadDevicePollDelegate>(pollAddress, PadDevicePollDetour);
                    _padDevicePollHook?.Enable();

                    if (_config.EnableDebugLogging && _log != null)
                        _log.Information($"[ElliCon] PadDevice.Poll hooked at 0x{pollAddress:X}");
                }
            }
        }
        catch (Exception ex)
        {
            if (_log != null)
                _log.Warning($"[ElliCon] Failed to hook PadDevice.Poll: {ex.Message}");
        }
    }

    private void RMIWalkDetour(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
    {
        _rmiWalkHook.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);

        if (ShouldBlockGamepadInput())
        {
            *sumLeft = 0;
            *sumForward = 0;
            *sumTurnLeft = 0;
            *haveBackwardOrStrafe = 0;
        }
    }

    private void RMIFlyDetour(void* self, PlayerMoveControllerFlyInput* result)
    {
        _rmiFlyHook.Original(self, result);

        if (ShouldBlockGamepadInput())
        {
            result->Forward = 0;
            result->Left = 0;
            result->Up = 0;
            result->Turn = 0;
        }
    }

    private CameraControlType GetCameraControlTypeDetour()
    {
        var cameraControlType = _getCameraControlTypeHook!.Original();

        if (ShouldBlockGamepadInput() && cameraControlType == CameraControlType.Gamepad)
        {
            return CameraControlType.None;
        }

        return cameraControlType;
    }

    private nint PadDevicePollDetour(PadDevice* self)
    {
        var result = _padDevicePollHook!.Original(self);

        if (ShouldBlockGamepadInput())
        {
            self->GamepadInputData.Buttons = GamepadButtonsFlags.None;
            self->GamepadInputData.ButtonsPressed = GamepadButtonsFlags.None;
            self->GamepadInputData.ButtonsReleased = GamepadButtonsFlags.None;
            self->GamepadInputData.ButtonsRepeat = GamepadButtonsFlags.None;

            self->GamepadInputData.Square = 0;
            self->GamepadInputData.Cross = 0;
            self->GamepadInputData.Circle = 0;
            self->GamepadInputData.Triangle = 0;
            self->GamepadInputData.L1 = 0;
            self->GamepadInputData.R1 = 0;
            self->GamepadInputData.L2 = 0;
            self->GamepadInputData.R2 = 0;
            self->GamepadInputData.Start = 0;
            self->GamepadInputData.Select = 0;
            self->GamepadInputData.L3 = 0;
            self->GamepadInputData.R3 = 0;
            self->GamepadInputData.DPadLeft = 0;
            self->GamepadInputData.DPadRight = 0;
            self->GamepadInputData.DPadUp = 0;
            self->GamepadInputData.DPadDown = 0;
        }

        return result;
    }

    private bool ShouldBlockGamepadInput()
    {
        var io = ImGui.GetIO();
        if (!io.NavActive || (io.ConfigFlags & ImGuiConfigFlags.NavEnableGamepad) == 0)
        {
            // If nav is not active, clear any tracked focus
            _lastFocusedWindow = null;
            return false;
        }
        
        if (_blockingWindows.Count == 0)
            return false;
        
        if (string.IsNullOrEmpty(_lastFocusedWindow))
            return false;
        
        if (_nonBlockingWindows.Contains(_lastFocusedWindow))
            return false;
        
        return _blockingWindows.Contains(_lastFocusedWindow);
    }
}
