using UnityEngine;

namespace PEAK_Menu.Menu
{
    public class MenuUI
    {
        private readonly MenuManager _menuManager;
        private Rect _windowRect;
        private string _consoleInput = "";
        private Vector2 _scrollPosition;
        private readonly System.Collections.Generic.List<string> _consoleOutput;
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Console", "Player", "Environment" };

        public MenuUI(MenuManager menuManager)
        {
            _menuManager = menuManager;
            _windowRect = new Rect(50, 50, 700, 500);
            _consoleOutput = new System.Collections.Generic.List<string>();
        }

        public void OnGUI()
        {
            if (!_menuManager.IsMenuOpen)
                return;

            GUI.matrix = Matrix4x4.Scale(Vector3.one * Plugin.PluginConfig.MenuScale.Value);
            _windowRect = GUI.Window(0, _windowRect, DrawWindow, $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}");
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            // Tab selection
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);

            switch (_selectedTab)
            {
                case 0:
                    DrawConsoleTab();
                    break;
                case 1:
                    DrawPlayerTab();
                    break;
                case 2:
                    DrawEnvironmentTab();
                    break;
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void DrawConsoleTab()
        {
            // Console output area
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(350));
            foreach (var line in _consoleOutput)
            {
                GUILayout.Label(line);
            }
            GUILayout.EndScrollView();

            // Input area
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("ConsoleInput");
            _consoleInput = GUILayout.TextField(_consoleInput);
            
            if (GUILayout.Button("Execute", GUILayout.Width(80)) || 
                (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "ConsoleInput"))
            {
                ExecuteCommand();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawPlayerTab()
        {
            var character = Character.localCharacter;
            if (character == null)
            {
                GUILayout.Label("No character found");
                return;
            }

            GUILayout.Label("=== Player Information ===");
            GUILayout.Label($"Position: {character.Center}");
            GUILayout.Label($"Health: {(1f - character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury)) * 100:F1}%");
            GUILayout.Label($"Stamina: {character.GetTotalStamina() * 100:F1}%");
            
            if (GUILayout.Button("Randomize Appearance"))
            {
                character.refs.customization.RandomizeCosmetics();
            }
        }

        private void DrawEnvironmentTab()
        {
            GUILayout.Label("=== Environment ===");
            
            if (DayNightManager.instance != null)
            {
                GUILayout.Label($"Day Progress: {DayNightManager.instance.isDay * 100:F1}%");
            }
            
            GUILayout.Label($"Night Cold Active: {Ascents.isNightCold}");
        }

        private void ExecuteCommand()
        {
            if (string.IsNullOrWhiteSpace(_consoleInput))
                return;

            AddToConsole($"> {_consoleInput}");
            _menuManager.ExecuteCommand(_consoleInput);
            _consoleInput = "";
        }

        public void AddToConsole(string message)
        {
            _consoleOutput.Add(message);
            if (_consoleOutput.Count > 100) // Limit console history
            {
                _consoleOutput.RemoveAt(0);
            }
            _scrollPosition = new Vector2(0, float.MaxValue); // Auto-scroll to bottom
        }

        public void ClearConsole()
        {
            _consoleOutput.Clear();
        }
    }
}