using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.GamePad;
using Dalamud.Plugin.Services;
using ElliCon.Configuration;
using ElliCon.Core;

namespace ElliCon.Navigation;

/// <summary>
/// Handles shoulder button (L1/R1) tab navigation for ImGui tab bars.
/// </summary>
public class TabNavigationHelper
{
    private readonly ControllerConfig _config;
    private readonly ButtonStateTracker _buttonTracker;
    private readonly IPluginLog? _log;
    
    private int _currentTabIndex;
    private int _pendingTabSwitch = -1;

    public int CurrentTabIndex => _currentTabIndex;

    public TabNavigationHelper(ControllerConfig config, ButtonStateTracker buttonTracker, IPluginLog? log = null)
    {
        _config = config;
        _buttonTracker = buttonTracker;
        _log = log;
    }

    /// <summary>
    /// Updates tab navigation state. Should be called once per frame before drawing tabs.
    /// </summary>
    public void Update(IGamepadState gamepad, int tabCount)
    {
        var io = ImGui.GetIO();
        if (!io.NavActive || (io.ConfigFlags & ImGuiConfigFlags.NavEnableGamepad) == 0)
            return;

        if (tabCount <= 1)
            return;

        if (_buttonTracker.JustPressed(gamepad, _config.TabPreviousButton))
        {
            _pendingTabSwitch = (_currentTabIndex - 1 + tabCount) % tabCount;
            
            if (_config.EnableDebugLogging && _log != null)
                _log.Debug($"[ElliCon] Tab navigation: {_currentTabIndex} -> {_pendingTabSwitch} (previous)");
        }
        else if (_buttonTracker.JustPressed(gamepad, _config.TabNextButton))
        {
            _pendingTabSwitch = (_currentTabIndex + 1) % tabCount;
            
            if (_config.EnableDebugLogging && _log != null)
                _log.Debug($"[ElliCon] Tab navigation: {_currentTabIndex} -> {_pendingTabSwitch} (next)");
        }
    }

    public bool ShouldSelectTab(int tabIndex)
    {
        return _pendingTabSwitch == tabIndex;
    }

    /// <summary>
    /// Notifies the helper that a tab is currently active.
    /// Should be called inside the tab content rendering.
    /// </summary>
    public void NotifyTabActive(int tabIndex)
    {
        _currentTabIndex = tabIndex;
        
        if (_pendingTabSwitch == tabIndex)
            _pendingTabSwitch = -1;
    }

    /// <summary>
    /// Creates an ImGui tab item with automatic controller navigation support.
    /// </summary>
    /// <param name="totalTabs">Total number of tabs (used for wrapping).</param>
    /// <returns>A disposable tab item handle.</returns>
    public unsafe TabItemHandle TabItem(string label, int tabIndex, int totalTabs)
    {
        return new TabItemHandle(this, label, tabIndex);
    }

    public unsafe struct TabItemHandle : IDisposable
    {
        private readonly TabNavigationHelper _helper;
        private readonly int _tabIndex;
        private bool _success;
        private bool _disposed;

        internal TabItemHandle(TabNavigationHelper helper, string label, int tabIndex)
        {
            _helper = helper;
            _tabIndex = tabIndex;
            _disposed = false;

            if (_helper.ShouldSelectTab(tabIndex))
            {
                fixed (byte* labelPtr = System.Text.Encoding.UTF8.GetBytes(label + "\0"))
                {
                    _success = ImGui.BeginTabItem(labelPtr, ImGuiTabItemFlags.SetSelected);
                }
                _helper._pendingTabSwitch = -1;
            }
            else
            {
                _success = ImGui.BeginTabItem(label);
            }

            if (_success)
                _helper.NotifyTabActive(tabIndex);
        }

        public bool Success => _success;

        public static implicit operator bool(TabItemHandle handle) => handle._success;

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_success)
                ImGui.EndTabItem();

            _disposed = true;
        }
    }
}
