using UnityEngine;
using PEAK_Menu.Commands;
using PEAK_Menu.Utils;

namespace PEAK_Menu.Menu
{
    public class MenuManager
    {
        private bool _isMenuOpen;
        private CommandManager _commandManager;
        private MenuUI _menuUI;
        private RainbowManager _rainbowManager;
        private NoClipManager _noClipManager;

        public bool IsMenuOpen => _isMenuOpen;

        public void Initialize()
        {
            try
            {
                _commandManager = new CommandManager();
                _menuUI = new MenuUI(this);
                _rainbowManager = new RainbowManager();
                _noClipManager = new NoClipManager();
                Plugin.Log.LogInfo("Menu system initialized");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Failed to initialize menu system: {ex.Message}");
            }
        }

        public void Update()
        {
            try
            {
                // Handle menu toggle
                if (Plugin.PluginConfig?.MenuToggleKey?.Value != null && 
                    Input.GetKeyDown(Plugin.PluginConfig.MenuToggleKey.Value))
                {
                    ToggleMenu();
                }

                // Handle NoClip toggle hotkey
                if (Plugin.PluginConfig?.NoClipToggleKey?.Value != null && 
                    Input.GetKeyDown(Plugin.PluginConfig.NoClipToggleKey.Value))
                {
                    if (_noClipManager != null)
                    {
                        _noClipManager.ToggleNoClip();
                        AddToConsole($"[HOTKEY] NoClip {(_noClipManager.IsNoClipEnabled ? "enabled" : "disabled")}");
                    }
                }

                // Update effects
                _rainbowManager?.Update();
                _noClipManager?.Update();
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error in MenuManager.Update: {ex.Message}");
            }
        }

        public void OnGUI()
        {
            try
            {
                _menuUI?.OnGUI();
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error in MenuManager.OnGUI: {ex.Message}");
            }
        }

        public void ToggleMenu()
        {
            _isMenuOpen = !_isMenuOpen;
            Plugin.Log.LogDebug($"Menu toggled: {_isMenuOpen}");
            
            if (_isMenuOpen)
            {
                AddToConsole($"=== {MyPluginInfo.PLUGIN_NAME} Menu Opened ===");
                AddToConsole("Type 'help' for available commands");
                AddToConsole("Press ESC to close menu");
            }
        }

        public bool ExecuteCommand(string commandLine)
        {
            try
            {
                return _commandManager?.ExecuteCommand(commandLine) ?? false;
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error executing command: {ex.Message}");
                AddToConsole($"[ERROR] Failed to execute command: {ex.Message}");
                return false;
            }
        }

        public void AddToConsole(string message)
        {
            _menuUI?.AddToConsole(message);
        }

        public void ClearConsole()
        {
            _menuUI?.ClearConsole();
        }

        public RainbowManager GetRainbowManager()
        {
            return _rainbowManager;
        }

        public NoClipManager GetNoClipManager()
        {
            return _noClipManager;
        }

        public void Cleanup()
        {
            try
            {
                _rainbowManager?.DisableRainbow();
                _noClipManager?.DisableNoClip();
                _commandManager?.Cleanup();
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error during cleanup: {ex.Message}");
            }
        }
    }
}