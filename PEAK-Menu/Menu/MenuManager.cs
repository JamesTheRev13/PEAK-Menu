using UnityEngine;
using PEAK_Menu.Commands;
using PEAK_Menu.Menu;

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
                
                Plugin.Log.LogInfo("Legacy menu system initialized");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Failed to initialize legacy menu system: {ex.Message}");
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
            Plugin.Log.LogDebug($"Legacy Menu toggled: {_isMenuOpen}");
            
            if (_isMenuOpen)
            {
                AddToConsole("Type 'help' for available commands");
                AddToConsole("Press ESC to close menu");
                AddToConsole($"Press {Plugin.PluginConfig.DebugConsoleToggleKey.Value} to toggle debug console");
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