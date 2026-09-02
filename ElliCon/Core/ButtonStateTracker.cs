using Dalamud.Game.ClientState.GamePad;
using Dalamud.Plugin.Services;

namespace ElliCon.Core;

public class ButtonStateTracker
{
    private readonly Dictionary<GamepadButtons, bool> _previousStates = new();

    // IMPORTANT: Should be called AFTER all button checks for the frame.
    // This saves the current state as "previous" for the next frame.
    public void UpdateEndOfFrame(IGamepadState gamepad)
    {
        var allButtons = Enum.GetValues<GamepadButtons>();
        foreach (var button in allButtons)
        {
            if (button == GamepadButtons.None)
                continue;

            var currentState = gamepad.Pressed(button) > 0;
            _previousStates[button] = currentState;
        }
    }

    // <param name="gamepad">Current gamepad state.</param>
    // <param name="button">Button to check.</param>
    // <returns>True if the button was just pressed.</returns>
    public bool JustPressed(IGamepadState gamepad, GamepadButtons button)
    {
        var currentState = gamepad.Pressed(button) > 0;
        var previousState = _previousStates.GetValueOrDefault(button, false);
        return currentState && !previousState;
    }

    public bool JustReleased(IGamepadState gamepad, GamepadButtons button)
    {
        var currentState = gamepad.Pressed(button) > 0;
        var previousState = _previousStates.GetValueOrDefault(button, false);
        return !currentState && previousState;
    }

    public bool IsHeld(IGamepadState gamepad, GamepadButtons button)
    {
        return gamepad.Pressed(button) > 0;
    }

    public void Reset()
    {
        _previousStates.Clear();
    }
}
