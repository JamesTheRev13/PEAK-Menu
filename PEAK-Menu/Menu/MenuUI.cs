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

            // Handle global key events for the menu
            HandleGlobalKeyEvents();

            GUI.matrix = Matrix4x4.Scale(Vector3.one * Plugin.PluginConfig.MenuScale.Value);
            _windowRect = GUI.Window(0, _windowRect, DrawWindow, $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}");
        }

        private void HandleGlobalKeyEvents()
        {
            // Handle Escape key to close menu
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                _menuManager.ToggleMenu();
                Event.current.Use(); // Consume the event
            }
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
            
            // Store previous input to detect changes
            var prevInput = _consoleInput;
            _consoleInput = GUILayout.TextField(_consoleInput);
            
            // Handle Enter key for command execution - FIXED VERSION
            bool shouldExecute = false;
            
            // Check for Enter/Return key press globally in the window
            if (Event.current.type == EventType.KeyDown && 
                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                // Only execute if console input is focused or if we're in console tab
                if (_selectedTab == 0 && (GUI.GetNameOfFocusedControl() == "ConsoleInput" || string.IsNullOrEmpty(GUI.GetNameOfFocusedControl())))
                {
                    shouldExecute = true;
                    Event.current.Use(); // Consume the event to prevent other handlers
                }
            }
            
            // Execute button
            if (GUILayout.Button("Execute", GUILayout.Width(80)))
            {
                shouldExecute = true;
            }
            
            // Execute command if triggered by either method
            if (shouldExecute)
            {
                ExecuteCommand();
            }
            
            GUILayout.EndHorizontal();
            
            // Auto-focus console input when console tab is active
            //if (_selectedTab == 0)
            //{
            //    GUI.FocusControl("ConsoleInput");
            //}
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
            GUILayout.Label($"Hunger: {character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hunger) * 100:F1}%");
            GUILayout.Label($"Cold: {character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Cold) * 100:F1}%");
            GUILayout.Label($"Hot: {character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hot) * 100:F1}%");
            GUILayout.Label($"Grounded: {character.data.isGrounded}");
            GUILayout.Label($"Climbing: {character.data.isClimbingAnything}");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Randomize Appearance"))
            {
                character.refs.customization.RandomizeCosmetics();
                AddToConsole("[INFO] Character appearance randomized");
            }
            
            if (GUILayout.Button("Clear All Status Effects"))
            {
                character.refs.afflictions.ClearAllStatus(excludeCurse: false);
                AddToConsole("[INFO] All status effects cleared");
            }
            
            if (GUILayout.Button("Full Heal"))
            {
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Injury, 0f);
                character.AddStamina(1f);
                AddToConsole("[INFO] Player fully healed");
            }

            // RAINBOW CONTROLS SECTION - ADDED!
            GUILayout.Space(15);
            GUILayout.Label("=== Rainbow Effect ===");
            
            var rainbowManager = _menuManager.GetRainbowManager();
            if (rainbowManager != null)
            {
                var isRainbowEnabled = rainbowManager.IsRainbowEnabled;
                
                // Rainbow status
                GUILayout.Label($"Rainbow Mode: {(isRainbowEnabled ?  "ENABLED" : "Disabled")}");
                
                // Toggle button
                var buttonText = isRainbowEnabled ? "Disable Rainbow" : "Enable Rainbow";
                if (GUILayout.Button(buttonText))
                {
                    rainbowManager.ToggleRainbow();
                    AddToConsole($"[INFO] Rainbow effect {(rainbowManager.IsRainbowEnabled ? "enabled" : "disabled")}");
                }
                
                // Speed controls - show when enabled
                if (isRainbowEnabled)
                {
                    GUILayout.Space(5);
                    GUILayout.Label("Rainbow Speed:");
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Slow", GUILayout.Width(60)))
                    {
                        rainbowManager.SetRainbowSpeed(0.5f);
                        AddToConsole("[INFO] Rainbow speed: Slow");
                    }
                    if (GUILayout.Button("Normal", GUILayout.Width(60)))
                    {
                        rainbowManager.SetRainbowSpeed(1.0f);
                        AddToConsole("[INFO] Rainbow speed: Normal");
                    }
                    if (GUILayout.Button("Fast", GUILayout.Width(60)))
                    {
                        rainbowManager.SetRainbowSpeed(2.0f);
                        AddToConsole("[INFO] Rainbow speed: Fast");
                    }
                    if (GUILayout.Button("CRAZY!", GUILayout.Width(60)))
                    {
                        rainbowManager.SetRainbowSpeed(5.0f);
                        AddToConsole("[INFO] Rainbow speed: CRAZY!");
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("Rainbow manager not available");
            }
        }

        private void DrawEnvironmentTab()
        {
            GUILayout.Label("=== Environment ===");
            
            if (DayNightManager.instance != null)
            {
                GUILayout.Label($"Day Progress: {DayNightManager.instance.isDay * 100:F1}%");
            }
            else
            {
                GUILayout.Label("Day/Night Manager: Not available");
            }
            
            GUILayout.Label($"Night Cold Active: {Ascents.isNightCold}");
            GUILayout.Label($"Hunger Rate Multiplier: {Ascents.hungerRateMultiplier:F2}");
            GUILayout.Label($"Fall Damage Multiplier: {Ascents.fallDamageMultiplier:F2}");
            GUILayout.Label($"Climb Stamina Multiplier: {Ascents.climbStaminaMultiplier:F2}");
            
            var character = Character.localCharacter;
            if (character != null)
            {
                GUILayout.Space(10);
                GUILayout.Label("=== Character Environment ===");
                GUILayout.Label($"In Fog: {character.data.isInFog}");
                GUILayout.Label($"Grounded For: {character.data.groundedFor:F1}s");
                GUILayout.Label($"Since Grounded: {character.data.sinceGrounded:F1}s");
                GUILayout.Label($"Fall Seconds: {character.data.fallSeconds:F1}s");
            }
        }

        private void ExecuteCommand()
        {
            if (string.IsNullOrWhiteSpace(_consoleInput))
                return;

            AddToConsole($"> {_consoleInput}");
            _menuManager.ExecuteCommand(_consoleInput);
            _consoleInput = "";
            
            // Keep focus on console input after command execution
            //GUI.FocusControl("ConsoleInput");
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