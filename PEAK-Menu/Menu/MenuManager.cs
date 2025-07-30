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
            _commandManager = new CommandManager();
            _menuUI = new MenuUI(this);
            Plugin.Log.LogInfo("Menu system initialized");
        }

        public void Update()
        {
            if (Input.GetKeyDown(Plugin.PluginConfig.MenuToggleKey.Value))
            {
                ToggleMenu();
            }
        }

        public void OnGUI()
        {
            _menuUI?.OnGUI();
        }

        public void ToggleMenu()
        {
            _isMenuOpen = !_isMenuOpen;
            Plugin.Log.LogDebug($"Menu toggled: {_isMenuOpen}");
        }

        public bool ExecuteCommand(string commandLine)
        {
            return _commandManager.ExecuteCommand(commandLine);
        }

        public void AddToConsole(string message)
        {
            _menuUI?.AddToConsole(message);
        }

        public void Cleanup()
        {
            _commandManager?.Cleanup();
        }
    }
}