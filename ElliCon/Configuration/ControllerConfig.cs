using Dalamud.Game.ClientState.GamePad;

namespace ElliCon.Configuration;

public class ControllerConfig
{
    public bool EnableInputBlocking { get; set; } = true;

    public bool BlockMovement { get; set; } = true;

    public bool BlockCamera { get; set; } = true;

    public bool BlockButtons { get; set; } = true;

    public GamepadButtons TabPreviousButton { get; set; } = GamepadButtons.L1;

    public GamepadButtons TabNextButton { get; set; } = GamepadButtons.R1;

    // Default is West (X on Xbox / Square on PlayStation) to match FFXIV's default.
    public GamepadButtons ContextMenuButton { get; set; } = GamepadButtons.West;

    public bool EnableDebugLogging { get; set; } = false;
}
