using UnityEngine;
using System.Collections.Generic;
using PEAK_Menu.Config;
using PEAK_Menu.Menu.UI.Tabs;

namespace PEAK_Menu.Menu
{
    public class MenuUI
    {
        private readonly MenuManager _menuManager;
        private Rect _windowRect;
        private readonly List<string> _consoleOutput;
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Console", "Player", "Environment", "Admin" };

        private readonly ConsoleTab _consoleTab;
        private readonly PlayerTab _playerTab;
        private readonly EnvironmentTab _environmentTab;
        private readonly AdminTab _adminTab;

        public MenuUI(MenuManager menuManager)
        {
            _menuManager = menuManager;
            _windowRect = new Rect(50, 50, UIConstants.WINDOW_DEFAULT_WIDTH, UIConstants.WINDOW_DEFAULT_HEIGHT);
            _consoleOutput = new List<string>();

            // Initialize tabs
            _consoleTab = new ConsoleTab(menuManager, _consoleOutput);
            _playerTab = new PlayerTab(menuManager, _consoleOutput);
            _environmentTab = new EnvironmentTab(menuManager, _consoleOutput);
            _adminTab = new AdminTab(menuManager, _consoleOutput);
        }

        public void OnGUI()
        {
            if (!_menuManager.IsMenuOpen)
                return;

            HandleGlobalKeyEvents();
            HandleDropdownClicks();

            GUI.matrix = Matrix4x4.Scale(Vector3.one * Plugin.PluginConfig.MenuScale.Value);
            _windowRect = GUI.Window(0, _windowRect, DrawWindow, 
                $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}");
        }

        private void HandleGlobalKeyEvents()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                _menuManager.ToggleMenu();
                Event.current.Use();
            }
        }

        private void HandleDropdownClicks()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                _adminTab.HandleDropdownClicks();
            }
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);

            switch (_selectedTab)
            {
                case 0: _consoleTab.Draw(); break;
                case 1: _playerTab.Draw(); break;
                case 2: _environmentTab.Draw(); break;
                case 3: _adminTab.Draw(); break;
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public void AddToConsole(string message)
        {
            _consoleOutput.Add(message);
            if (_consoleOutput.Count > UIConstants.CONSOLE_HISTORY_LIMIT)
            {
                _consoleOutput.RemoveAt(0);
            }
            _consoleTab.SetScrollToBottom();
        }

        public void ClearConsole()
        {
            _consoleOutput.Clear();
        }
    }
}