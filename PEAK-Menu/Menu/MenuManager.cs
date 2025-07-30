using UnityEngine;
using PEAK_Menu.Commands;

namespace PEAK_Menu.Menu
{
    public class MenuManager
    {
        private bool _isMenuOpen;
        private CommandManager _commandManager;
        private MenuUI _menuUI;

        public bool IsMenuOpen => _isMenuOpen;

        public void Initialize()
        {
            try
            {
                _commandManager = new CommandManager();
                _menuUI = new MenuUI(this);
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
                if (Plugin.PluginConfig?.MenuToggleKey?.Value != null && 
                    Input.GetKeyDown(Plugin.PluginConfig.MenuToggleKey.Value))
                {
                    ToggleMenu();
                }
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
                return false;
            }
        }

        public void AddToConsole(string message)
        {
            _menuUI?.AddToConsole(message);
        }

        public void Cleanup()
        {
            try
            {
                _commandManager?.Cleanup();
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error during cleanup: {ex.Message}");
            }
        }
    }
}