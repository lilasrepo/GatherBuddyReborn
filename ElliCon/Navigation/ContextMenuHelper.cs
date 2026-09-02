using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.GamePad;
using Dalamud.Plugin.Services;
using ElliCon.Configuration;
using ElliCon.Core;

namespace ElliCon.Navigation;

/// <summary>
/// Handles controller button (Triangle/Y) context menu activation.
/// </summary>
public class ContextMenuHelper
{
    private readonly ControllerConfig _config;
    private readonly ButtonStateTracker _buttonTracker;
    private readonly IPluginLog? _log;

    public ContextMenuHelper(ControllerConfig config, ButtonStateTracker buttonTracker, IPluginLog? log = null)
    {
        _config = config;
        _buttonTracker = buttonTracker;
        _log = log;
    }

    /// <summary>
    /// Begins a context menu popup with gamepad support.
    /// Use this instead of ImGui.BeginPopupContextItem() for controller support.
    /// </summary>
    public bool BeginPopupContextItemWithGamepad(string popupId, IGamepadState gamepad, ImGuiPopupFlags flags = ImGuiPopupFlags.MouseButtonRight)
    {
        var isPopupOpen = ImGui.BeginPopupContextItem(popupId, flags);

        if (!isPopupOpen)
        {
            var io = ImGui.GetIO();
            var isGamepadNavActive = io.NavActive && (io.ConfigFlags & ImGuiConfigFlags.NavEnableGamepad) != 0;
            var isItemFocused = ImGui.IsItemActive() || ImGui.IsItemHovered();

            if (isGamepadNavActive && isItemFocused)
            {
                if (_buttonTracker.JustPressed(gamepad, _config.ContextMenuButton))
                {
                    ImGui.OpenPopup(popupId);
                    isPopupOpen = ImGui.BeginPopup(popupId);

                    if (_config.EnableDebugLogging && _log != null)
                        _log.Debug($"[ElliCon] Context menu opened: {popupId}");
                }
            }
        }

        return isPopupOpen;
    }

    /// <summary>
    /// Begins a context menu popup with gamepad support using ImGuiKey detection.
    /// Alternative implementation using ImGui's key system.
    /// </summary>
    public bool BeginPopupContextItemWithGamepadKey(string popupId, ImGuiPopupFlags flags = ImGuiPopupFlags.MouseButtonRight)
    {
        var isPopupOpen = ImGui.BeginPopupContextItem(popupId, flags);

        if (!isPopupOpen)
        {
            var io = ImGui.GetIO();
            var isGamepadNavActive = io.NavActive && (io.ConfigFlags & ImGuiConfigFlags.NavEnableGamepad) != 0;
            var isItemFocused = ImGui.IsItemActive() || ImGui.IsItemHovered();

            if (isGamepadNavActive && isItemFocused)
            {
                // Use ImGui's GamepadFaceLeft key (Square/X button) as fallback
                var inputPressed = ImGui.IsKeyPressed(ImGuiKey.GamepadFaceLeft);

                if (inputPressed)
                {
                    ImGui.OpenPopup(popupId);
                    isPopupOpen = ImGui.BeginPopup(popupId);

                    if (_config.EnableDebugLogging && _log != null)
                        _log.Debug($"[ElliCon] Context menu opened (key): {popupId}");
                }
            }
        }

        return isPopupOpen;
    }
}
