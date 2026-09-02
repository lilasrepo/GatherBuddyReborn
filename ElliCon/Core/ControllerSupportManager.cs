using Dalamud.Game.ClientState.GamePad;
using Dalamud.Plugin.Services;
using ElliCon.Configuration;
using ElliCon.Navigation;

namespace ElliCon.Core;

/// <summary>
/// Main entry point for controller support functionality.
/// Provides a unified interface for input blocking and navigation helpers.
/// </summary>
public class ControllerSupportManager : IDisposable
{
    private readonly ControllerConfig _config;
    private readonly IGamepadState _gamepadState;
    private readonly IPluginLog? _log;
    
    private InputBlocker? _inputBlocker;
    private readonly ButtonStateTracker _buttonTracker;
    private readonly TabNavigationHelper _tabNavigation;
    private readonly ContextMenuHelper _contextMenu;

    public TabNavigationHelper TabNavigation => _tabNavigation;

    public ContextMenuHelper ContextMenu => _contextMenu;

    public ButtonStateTracker ButtonTracker => _buttonTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerSupportManager"/> class.
    /// </summary>
    /// <param name="gamepadState">Dalamud gamepad state service.</param>
    /// <param name="gameInteropProvider">Dalamud game interop provider for hooking.</param>
    /// <param name="config">Configuration for controller support. If null, uses defaults.</param>
    /// <param name="log">Optional logger for debug output.</param>
    public ControllerSupportManager(
        IGamepadState gamepadState,
        IGameInteropProvider gameInteropProvider,
        ControllerConfig? config = null,
        IPluginLog? log = null)
    {
        _config = config ?? new ControllerConfig();
        _gamepadState = gamepadState;
        _log = log;

        _buttonTracker = new ButtonStateTracker();
        _tabNavigation = new TabNavigationHelper(_config, _buttonTracker, log);
        _contextMenu = new ContextMenuHelper(_config, _buttonTracker, log);

        if (_config.EnableInputBlocking)
        {
            _inputBlocker = new InputBlocker(gameInteropProvider, _config, log);
        }

        if (_config.EnableDebugLogging && _log != null)
            _log.Information("[ElliCon] ControllerSupportManager initialized");
    }

    /// <summary>
    /// Enables input blocking. Prevents game input when ImGui has focus.
    /// </summary>
    public void EnableInputBlocking()
    {
        if (_inputBlocker == null)
        {
            if (_log != null)
                _log.Warning("[ElliCon] Cannot enable input blocking - InputBlocker was not initialized");
            return;
        }

        _inputBlocker.Enable();
    }

    public void DisableInputBlocking()
    {
        _inputBlocker?.Disable();
    }
    
    /// <summary>
    /// Configures what types of input should be blocked.
    /// </summary>
    /// <param name="blockMovement">Whether to block movement (left stick).</param>
    /// <param name="blockCamera">Whether to block camera (right stick).</param>
    /// <param name="blockButtons">Whether to block buttons (face buttons, dpad, triggers).</param>
    public void SetBlockingMode(bool blockMovement, bool blockCamera, bool blockButtons)
    {
        _config.BlockMovement = blockMovement;
        _config.BlockCamera = blockCamera;
        _config.BlockButtons = blockButtons;
        
        if (_inputBlocker != null && _inputBlocker.IsEnabled)
        {
            // Re-enable to apply new settings
            _inputBlocker.Disable();
            _inputBlocker.Enable();
        }
    }

    public bool IsInputBlockingEnabled => _inputBlocker?.IsEnabled ?? false;
    
    /// <summary>
    /// Registers a window that should block game input when focused.
    /// Only registered windows will cause input blocking.
    /// This ensures ElliCon only affects YOUR plugin's windows, not other plugins.
    /// </summary>
    /// <param name="windowName">The ImGui window name.</param>
    public void RegisterBlockingWindow(string windowName)
    {
        _inputBlocker?.RegisterBlockingWindow(windowName);
    }
    
    /// <summary>
    /// Unregisters a window from blocking.
    /// </summary>
    /// <param name="windowName">The ImGui window name to unregister.</param>
    public void UnregisterBlockingWindow(string windowName)
    {
        _inputBlocker?.UnregisterBlockingWindow(windowName);
    }
    
    /// <summary>
    /// Registers a window as non-blocking. When this window has focus,
    /// game input will NOT be blocked even though ImGui navigation is active.
    /// This is useful for automation/status windows that should remain interactable
    /// while allowing background automation to continue.
    /// </summary>
    /// <param name="windowName">The ImGui window name.</param>
    public void RegisterNonBlockingWindow(string windowName)
    {
        _inputBlocker?.RegisterNonBlockingWindow(windowName);
    }
    
    public void UnregisterNonBlockingWindow(string windowName)
    {
        _inputBlocker?.UnregisterNonBlockingWindow(windowName);
    }

    /// <summary>
    /// Updates the currently focused window for input blocking.
    /// Call this from your Draw() method when a window is focused.
    /// </summary>
    /// <param name="windowName">The window name, or null if no managed window is focused.</param>
    public void UpdateFocusedWindow(string? windowName)
    {
        _inputBlocker?.UpdateFocusedWindow(windowName);
    }
    
    /// <summary>
    /// Updates controller state at the END of the frame.
    /// Call this AFTER all your ImGui drawing and button checks.
    /// Typically place at the very end of your Draw() method.
    /// </summary>
    public void UpdateEndOfFrame()
    {
        _buttonTracker.UpdateEndOfFrame(_gamepadState);
    }

    public void Dispose()
    {
        _inputBlocker?.Dispose();
        
        if (_config.EnableDebugLogging && _log != null)
            _log.Information("[ElliCon] ControllerSupportManager disposed");
    }
}
