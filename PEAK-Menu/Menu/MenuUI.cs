using System.Linq;
using UnityEngine;
using PEAK_Menu.Utils;
using Zorro.Core.Serizalization;

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
        private readonly string[] _tabNames = { "Console", "Player", "Environment", "Admin" };

        // Add separate scroll positions for each tab
        private Vector2 _playerTabScrollPosition;
        private Vector2 _environmentTabScrollPosition;
        private Vector2 _adminTabScrollPosition;

        // Add persistent state for teleport coordinates
        private static float _teleportX = 0f;
        private static float _teleportY = 0f;
        private static float _teleportZ = 0f;
        
        // Add persistent state for advanced player management - MOVED TO PLAYER TAB
        private static float _selfStatusValue = 0.5f;

        // For tracking hover states - improved approach
        private int _hoveredButtonId = -1;

        // Player selection state for admin tab - UPDATED for dropdown style
        private bool _showPlayerDropdown = false;
        private Vector2 _playerDropdownScrollPosition;
        private int _selectedPlayerIndex = -1;
        private Character _selectedPlayer = null;
        private string _selectedPlayerName = "Select Player...";

        // Item management state for admin tab
        private bool _showItemDropdown = false;
        private Vector2 _itemDropdownScrollPosition;
        private int _selectedItemIndex = 0;
        private string _selectedItemName = "Select Item...";
        
        // Predefined item list - this is temporary, will be dynamic
        private string[] _availableItems = { "Select Item..." };
        private bool _itemsInitialized = false;

        public MenuUI(MenuManager menuManager)
        {
            _menuManager = menuManager;
            _windowRect = new Rect(50, 50, 700, 500);
            _consoleOutput = new System.Collections.Generic.List<string>();
        }

        private void DrawPlayerDropdown()
        {
            var allCharacters = Character.AllCharacters?.ToList();
            if (allCharacters == null || allCharacters.Count == 0)
            {
                GUILayout.Label("No players found");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Player:", GUILayout.Width(50));
            
            // Dropdown button
            if (GUILayout.Button(_selectedPlayerName, GUILayout.Width(200)))
            {
                _showPlayerDropdown = !_showPlayerDropdown;
            }
            
            GUILayout.EndHorizontal();
            
            // Dropdown menu
            if (_showPlayerDropdown)
            {
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(250), GUILayout.MaxHeight(150));
                
                _playerDropdownScrollPosition = GUILayout.BeginScrollView(_playerDropdownScrollPosition, GUILayout.Height(140));
                
                // Add "Select Player..." option at the top
                var originalColor = GUI.backgroundColor;
                if (_selectedPlayerIndex == -1)
                {
                    GUI.backgroundColor = Color.cyan;
                }
                
                if (GUILayout.Button("Select Player...", GUILayout.Height(25)))
                {
                    _selectedPlayerIndex = -1;
                    _selectedPlayer = null;
                    _selectedPlayerName = "Select Player...";
                    _showPlayerDropdown = false;
                    AddToConsole("[ADMIN] Player selection cleared");
                }
                GUI.backgroundColor = originalColor;
                
                // Add all players
                for (int i = 0; i < allCharacters.Count; i++)
                {
                    var character = allCharacters[i];
                    if (character == null) continue;
                    
                    // Player status with color coding
                    var status = character.data.dead ? "[DEAD]" : 
                                character.data.passedOut ? "[OUT]" : "[OK]";
                    
                    var displayName = $"{character.characterName} {status}";
                    var isSelected = _selectedPlayerIndex == i;
                    
                    // Set background color based on selection and status
                    originalColor = GUI.backgroundColor;
                    if (isSelected)
                    {
                        GUI.backgroundColor = Color.cyan;
                    }
                    else if (character.data.dead)
                    {
                        GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f); // Light red for dead
                    }
                    else if (character.data.passedOut)
                    {
                        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.4f); // Light yellow for passed out
                    }
                    
                    if (GUILayout.Button(displayName, GUILayout.Height(25)))
                    {
                        _selectedPlayerIndex = i;
                        _selectedPlayer = character;
                        _selectedPlayerName = displayName;
                        _showPlayerDropdown = false;
                        AddToConsole($"[ADMIN] Selected player: {character.characterName}");
                    }
                    
                    GUI.backgroundColor = originalColor;
                }
                
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
        }

        // Standardized toggle button method - FIXED
        private bool DrawToggleButton(string featureName, bool isEnabled, float width = 0, int buttonId = -1)
        {
            // Check hover state properly
            var controlRect = width > 0 ? GUILayoutUtility.GetRect(width, 20) : GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.button);
            var isHovered = controlRect.Contains(Event.current.mousePosition);
            
            // Update hover tracking
            if (isHovered && Event.current.type == EventType.Repaint)
            {
                _hoveredButtonId = buttonId;
            }
            
            // Determine button text based on hover state
            string buttonText;
            if (isHovered)
            {
                buttonText = isEnabled ? $"{featureName} - DISABLE" : $"{featureName} - ENABLE";
            }
            else
            {
                buttonText = isEnabled ? $"{featureName} - ON" : $"{featureName} - OFF";
            }

            // Set button color
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = isEnabled ? Color.green : Color.gray;

            // Draw the button using the calculated rect
            bool clicked = GUI.Button(controlRect, buttonText);

            GUI.backgroundColor = originalColor;
            return clicked;
        }

        // Standardized toggle button with status label method - FIXED
        private bool DrawToggleButtonWithStatus(string featureName, bool isEnabled, float buttonWidth = 130, float statusWidth = 120, int buttonId = -1)
        {
            GUILayout.BeginHorizontal();
            
            bool clicked = DrawToggleButton(featureName, isEnabled, buttonWidth, buttonId);
            
            var statusText = isEnabled ? "ENABLED" : "Disabled";
            var statusColor = GUI.color;
            GUI.color = isEnabled ? Color.green : Color.gray;
            GUILayout.Label($"{featureName}: {statusText}", GUILayout.Width(statusWidth));
            GUI.color = statusColor;
            
            GUILayout.EndHorizontal();
            
            return clicked;
        }

        private void InitializeItemsList()
        {
            try
            {
                // Delay initialization to ensure game is loaded
                var itemHelper = ItemDiscoveryHelper.Instance;
                _availableItems = itemHelper.GetItemNamesArray();
                _itemsInitialized = true;
                AddToConsole($"[INIT] Discovered {_availableItems.Length - 1} items for admin panel");
            }
            catch (System.Exception ex)
            {
                AddToConsole($"[ERROR] Failed to initialize items list: {ex.Message}");
                // Keep the default array as fallback
            }
        }

        // NEW: Custom dropdown method for item selection
        private void DrawItemDropdown()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Item:", GUILayout.Width(40));

            // Dropdown button
            if (GUILayout.Button(_selectedItemName, GUILayout.Width(150)))
            {
                _showItemDropdown = !_showItemDropdown;
            }

            // Refresh button
            if (GUILayout.Button("Load Items", GUILayout.Width(75)))
            {
                RefreshItemsList();
            }

            GUILayout.EndHorizontal();

            // Dropdown menu - WIDENED for better comfort
            if (_showItemDropdown)
            {
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(250), GUILayout.MaxHeight(150));

                _itemDropdownScrollPosition = GUILayout.BeginScrollView(_itemDropdownScrollPosition, GUILayout.Height(140));

                for (int i = 0; i < _availableItems.Length; i++)
                {
                    var item = _availableItems[i];
                    var isSelected = _selectedItemIndex == i;

                    // Highlight selected item
                    var originalColor = GUI.backgroundColor;
                    if (isSelected)
                    {
                        GUI.backgroundColor = Color.cyan;
                    }

                    if (GUILayout.Button(item, GUILayout.Height(20)))
                    {
                        _selectedItemIndex = i;
                        _selectedItemName = item;
                        _showItemDropdown = false;

                        // Don't log for "Select Item..." option
                        if (i > 0)
                        {
                            AddToConsole($"[ADMIN] Selected item: {item}");
                        }
                    }

                    GUI.backgroundColor = originalColor;
                }

                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                // Show item count
                if (_itemsInitialized)
                {
                    GUILayout.Label($"{_availableItems.Length - 1} items available", GUI.skin.box);
                }
                else
                {
                    GUILayout.Label("Items loading...", GUI.skin.box);
                }
            }
        }

        private void RefreshItemsList()
        {
            try
            {
                var itemHelper = ItemDiscoveryHelper.Instance;
                itemHelper.RefreshItems();
                _availableItems = itemHelper.GetItemNamesArray();
                _itemsInitialized = true;

                // Reset selection
                _selectedItemIndex = 0;
                _selectedItemName = "Select Item...";

                AddToConsole($"[ADMIN] Refreshed items list - found {_availableItems.Length - 1} items");
            }
            catch (System.Exception ex)
            {
                AddToConsole($"[ERROR] Failed to refresh items: {ex.Message}");
            }
        }

        public void OnGUI()
        {
            if (!_menuManager.IsMenuOpen)
                return;

            // Handle global key events for the menu
            HandleGlobalKeyEvents();
            
            // Close dropdowns if clicking outside
            if (Event.current.type == EventType.MouseDown)
            {
                if (_showItemDropdown)
                {
                    _showItemDropdown = false;
                }
                if (_showPlayerDropdown)
                {
                    _showPlayerDropdown = false;
                }
            }

            GUI.matrix = Matrix4x4.Scale(Vector3.one * Plugin.PluginConfig.MenuScale.Value);
            _windowRect = GUI.Window(0, _windowRect, DrawWindow, $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}");
        }

        private void HandleGlobalKeyEvents()
        {
            // Handle Escape key to close menu or dropdowns
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                if (_showItemDropdown)
                {
                    _showItemDropdown = false;
                    Event.current.Use();
                }
                else if (_showPlayerDropdown)
                {
                    _showPlayerDropdown = false;
                    Event.current.Use();
                }
                else
                {
                    _menuManager.ToggleMenu();
                    Event.current.Use();
                }
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
                case 3:
                    DrawAdminTab();
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
        }

        private void DrawPlayerTab()
        {
            // Make Player tab scrollable
            _playerTabScrollPosition = GUILayout.BeginScrollView(_playerTabScrollPosition, GUILayout.Height(420));
            
            var character = Character.localCharacter;
            if (character == null)
            {
                GUILayout.Label("No character found");
                GUILayout.EndScrollView();
                return;
            }

            // === PLAYER INFORMATION SECTION ===
            GUILayout.Label("=== Player Information ===");
            GUILayout.Label($"Name: {character.characterName}");
            GUILayout.Label($"Position: {character.Center}");
            GUILayout.Label($"Health: {(1f - character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury)) * 100:F1}%");
            GUILayout.Label($"Stamina: {character.GetTotalStamina() * 100:F1}%");
            GUILayout.Label($"Hunger: {character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hunger) * 100:F1}%");
            GUILayout.Label($"Cold: {character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Cold) * 100:F1}%");
            GUILayout.Label($"Hot: {character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hot) * 100:F1}%");
            GUILayout.Label($"Grounded: {character.data.isGrounded}");
            GUILayout.Label($"Climbing: {character.data.isClimbingAnything}");
            
            GUILayout.Space(10);
            
            // === HEALTH & STATUS MANAGEMENT ===
            GUILayout.Label("=== Health & Status Management ===");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Full Heal", GUILayout.Width(100)))
            {
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Injury, 0f);
                character.AddStamina(1f);
                AddToConsole("[PLAYER] Player fully healed");
            }
            if (GUILayout.Button("Clear All Status Effects", GUILayout.Width(160)))
            {
                character.refs.afflictions.ClearAllStatus(excludeCurse: false);
                AddToConsole("[PLAYER] All status effects cleared");
            }
            GUILayout.EndHorizontal();

            // MOVED: Advanced Status Management from Admin Tab
            GUILayout.Space(5);
            GUILayout.Label("Advanced Status Control:");
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Value:", GUILayout.Width(45));
            _selfStatusValue = GUILayout.HorizontalSlider(_selfStatusValue, 0f, 1f, GUILayout.Width(100));
            GUILayout.Label($"{_selfStatusValue:F2}", GUILayout.Width(40));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Health", GUILayout.Width(80)))
            {
                AdminUIHelper.SetPlayerStatus(character.characterName, "health", _selfStatusValue);
                AddToConsole($"[PLAYER] Set health to {_selfStatusValue * 100:F0}%");
            }
            if (GUILayout.Button("Set Stamina", GUILayout.Width(80)))
            {
                AdminUIHelper.SetPlayerStatus(character.characterName, "stamina", _selfStatusValue);
                AddToConsole($"[PLAYER] Set stamina to {_selfStatusValue * 100:F0}%");
            }
            if (GUILayout.Button("Set Hunger", GUILayout.Width(80)))
            {
                AdminUIHelper.SetPlayerStatus(character.characterName, "hunger", _selfStatusValue);
                AddToConsole($"[PLAYER] Set hunger to {_selfStatusValue * 100:F0}%");
            }
            GUILayout.EndHorizontal();

            // === ADMIN FEATURES ===
            GUILayout.Space(10);
            GUILayout.Label("=== Admin Features ===");
            
            // God Mode Toggle Button
            var isGodModeEnabled = character.statusesLocked;
            if (DrawToggleButtonWithStatus("God Mode", isGodModeEnabled, 130, 120, 301))
            {
                AdminUIHelper.ExecuteQuickAction("god-mode", character.characterName);
                AddToConsole($"[PLAYER] God mode {(!isGodModeEnabled ? "enabled" : "disabled")}");
            }
            
            // Infinite Stamina Toggle Button
            var isInfiniteStamEnabled = character.infiniteStam;
            if (DrawToggleButtonWithStatus("Infinite Stamina", isInfiniteStamEnabled, 160, 140, 302))
            {
                AdminUIHelper.ExecuteQuickAction("infinite-stamina", character.characterName);
                AddToConsole($"[PLAYER] Infinite stamina {(!isInfiniteStamEnabled ? "enabled" : "disabled")}");
            }

            // Teleport to Ping Toggle
            var isTeleportToPingEnabled = Plugin.PluginConfig?.TeleportToPingEnabled?.Value ?? false;
            if (DrawToggleButton("Teleport to Ping", isTeleportToPingEnabled, 0, 305))
            {
                Plugin.PluginConfig.TeleportToPingEnabled.Value = !isTeleportToPingEnabled;
                AddToConsole($"[PLAYER] Teleport to ping {(!isTeleportToPingEnabled ? "enabled" : "disabled")}");
                
                if (!isTeleportToPingEnabled)
                {
                    AddToConsole("[INFO] You will now teleport to locations when you ping them");
                    AddToConsole("[INFO] Hold ping key and click to place marker and teleport");
                }
                else
                {
                    AddToConsole("[INFO] Ping will work normally without teleporting");
                }
            }

            // NoClip controls
            DrawNoClipControls();

            if (GUILayout.Button("Full Self Heal (Admin)", GUILayout.Width(160)))
            {
                AdminUIHelper.ExecuteQuickAction("heal", character.characterName);
                AddToConsole("[PLAYER] Admin self heal executed");
            }
            
            GUILayout.Space(10);
            
            // === APPEARANCE & CUSTOMIZATION ===
            GUILayout.Label("=== Appearance & Customization ===");
            
            if (GUILayout.Button("Randomize Appearance", GUILayout.Width(160)))
            {
                character.refs.customization.RandomizeCosmetics();
                AddToConsole("[PLAYER] Character appearance randomized");
            }

            // RAINBOW CONTROLS SECTION
            var rainbowManager = _menuManager.GetRainbowManager();
            if (rainbowManager != null)
            {
                var isRainbowEnabled = rainbowManager.IsRainbowEnabled;
                
                // Rainbow toggle with standardized button and unique ID
                if (DrawToggleButton("Rainbow Effect", isRainbowEnabled, 0, 104))
                {
                    rainbowManager.ToggleRainbow();
                    AddToConsole($"[PLAYER] Rainbow effect {(rainbowManager.IsRainbowEnabled ? "enabled" : "disabled")}");
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
                        AddToConsole("[PLAYER] Rainbow speed: Slow");
                    }
                    if (GUILayout.Button("Normal", GUILayout.Width(60)))
                    {
                        rainbowManager.SetRainbowSpeed(1.0f);
                        AddToConsole("[PLAYER] Rainbow speed: Normal");
                    }
                    if (GUILayout.Button("Fast", GUILayout.Width(60)))
                    {
                        rainbowManager.SetRainbowSpeed(2.0f);
                        AddToConsole("[PLAYER] Rainbow speed: Fast");
                    }
                    if (GUILayout.Button("CRAZY!", GUILayout.Width(60)))
                    {
                        rainbowManager.SetRainbowSpeed(5.0f);
                        AddToConsole("[PLAYER] Rainbow speed: CRAZY!");
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("Rainbow manager not available");
            }
            
            GUILayout.Space(10);
            
            // === PLAYER MODIFICATIONS ===
            GUILayout.Label("=== Player Modifications ===");
            
            var playerManager = _menuManager.GetPlayerManager();
            if (playerManager != null)
            {
                // Protection Toggles
                GUILayout.Label("Protection Settings:");
                
                // No Fall Damage - Updated with standardized toggle and unique ID
                var isNoFallEnabled = playerManager.NoFallDamageEnabled;
                if (DrawToggleButton("No Fall Damage", isNoFallEnabled, 0, 101))
                {
                    playerManager.SetNoFallDamage(!isNoFallEnabled);
                    AddToConsole($"[PLAYER] No fall damage {(!isNoFallEnabled ? "enabled" : "disabled")}");
                }
                
                // No Weight - Updated with standardized toggle and unique ID
                var isNoWeightEnabled = playerManager.NoWeightEnabled;
                if (DrawToggleButton("No Weight", isNoWeightEnabled, 0, 102))
                {
                    playerManager.SetNoWeight(!isNoWeightEnabled);
                    AddToConsole($"[PLAYER] No weight {(!isNoWeightEnabled ? "enabled" : "disabled")}");
                    
                    if (!isNoWeightEnabled)
                    {
                        AddToConsole("[INFO] Inventory weight penalties disabled via Harmony patches");
                        AddToConsole("[INFO] You can now carry unlimited weight without speed penalties");
                    }
                    else
                    {
                        AddToConsole("[INFO] Normal weight mechanics restored");
                    }
                }
                
                // Affliction Immunity - Updated with standardized toggle and unique ID
                var isAfflictionImmune = playerManager.AfflictionImmunityEnabled;
                if (DrawToggleButton("Affliction Immunity", isAfflictionImmune, 0, 103))
                {
                    playerManager.SetAfflictionImmunity(!isAfflictionImmune);
                    AddToConsole($"[PLAYER] Affliction immunity {(!isAfflictionImmune ? "enabled" : "disabled")}");
                }
                
                GUILayout.Space(10);
                
                // === MOVEMENT ENHANCEMENT ===
                GUILayout.Label("=== Movement Enhancement ===");
                
                // Movement Speed Controls
                GUILayout.BeginHorizontal();
                GUILayout.Label("Speed:", GUILayout.Width(60));
                var currentSpeed = Plugin.PluginConfig.MovementSpeedMultiplier.Value;
                var newSpeed = GUILayout.HorizontalSlider(currentSpeed, 0.1f, 20f, GUILayout.Width(150));
                if (Mathf.Abs(newSpeed - currentSpeed) > 0.01f)
                {
                    Plugin.PluginConfig.MovementSpeedMultiplier.Value = newSpeed;
                    playerManager.SetMovementSpeedMultiplier(newSpeed);
                    AddToConsole($"[PLAYER] Movement speed: {newSpeed:F2}x");
                }
                GUILayout.Label($"{newSpeed:F2}x", GUILayout.Width(50));
                GUILayout.EndHorizontal();
                
                // Jump Height Controls
                GUILayout.BeginHorizontal();
                GUILayout.Label("Jump:", GUILayout.Width(60));
                var currentJump = Plugin.PluginConfig.JumpHeightMultiplier.Value;
                var newJump = GUILayout.HorizontalSlider(currentJump, 0.1f, 10f, GUILayout.Width(150));
                if (Mathf.Abs(newJump - currentJump) > 0.01f)
                {
                    Plugin.PluginConfig.JumpHeightMultiplier.Value = newJump;
                    playerManager.SetJumpHeightMultiplier(newJump);
                    AddToConsole($"[PLAYER] Jump height: {newJump:F2}x");
                }
                GUILayout.Label($"{newJump:F2}x", GUILayout.Width(50));
                GUILayout.EndHorizontal();
                
                // Climb Speed Controls
                GUILayout.BeginHorizontal();
                GUILayout.Label("Climb:", GUILayout.Width(60));
                var currentClimb = Plugin.PluginConfig.ClimbSpeedMultiplier.Value;
                var newClimb = GUILayout.HorizontalSlider(currentClimb, 0.1f, 20f, GUILayout.Width(150));
                if (Mathf.Abs(newClimb - currentClimb) > 0.01f)
                {
                    Plugin.PluginConfig.ClimbSpeedMultiplier.Value = newClimb;
                    playerManager.SetClimbSpeedMultiplier(newClimb);
                    AddToConsole($"[PLAYER] Climb speed: {newClimb:F2}x");
                }
                GUILayout.Label($"{newClimb:F2}x", GUILayout.Width(50));
                GUILayout.EndHorizontal();
                
                // Movement preset buttons for quick setup
                GUILayout.Space(5);
                GUILayout.Label("Movement Presets:");
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Normal", GUILayout.Width(60)))
                {
                    Plugin.PluginConfig.MovementSpeedMultiplier.Value = 1.0f;
                    Plugin.PluginConfig.JumpHeightMultiplier.Value = 1.0f;
                    Plugin.PluginConfig.ClimbSpeedMultiplier.Value = 1.0f;
                    playerManager.SetMovementSpeedMultiplier(1.0f);
                    playerManager.SetJumpHeightMultiplier(1.0f);
                    playerManager.SetClimbSpeedMultiplier(1.0f);
                    AddToConsole("[PLAYER] Movement preset: Normal (1x)");
                }
                
                if (GUILayout.Button("Enhanced", GUILayout.Width(60)))
                {
                    Plugin.PluginConfig.MovementSpeedMultiplier.Value = 2.0f;
                    Plugin.PluginConfig.JumpHeightMultiplier.Value = 1.5f;
                    Plugin.PluginConfig.ClimbSpeedMultiplier.Value = 2.0f;
                    playerManager.SetMovementSpeedMultiplier(2.0f);
                    playerManager.SetJumpHeightMultiplier(1.5f);
                    playerManager.SetClimbSpeedMultiplier(2.0f);
                    AddToConsole("[PLAYER] Movement preset: Enhanced");
                }
                
                if (GUILayout.Button("Super", GUILayout.Width(60)))
                {
                    Plugin.PluginConfig.MovementSpeedMultiplier.Value = 4.0f;
                    Plugin.PluginConfig.JumpHeightMultiplier.Value = 3.0f;
                    Plugin.PluginConfig.ClimbSpeedMultiplier.Value = 4.0f;
                    playerManager.SetMovementSpeedMultiplier(4.0f);
                    playerManager.SetJumpHeightMultiplier(3.0f);
                    playerManager.SetClimbSpeedMultiplier(4.0f);
                    AddToConsole("[PLAYER] Movement preset: Super");
                }
                
                if (GUILayout.Button("Extreme", GUILayout.Width(60)))
                {
                    Plugin.PluginConfig.MovementSpeedMultiplier.Value = 8.0f;
                    Plugin.PluginConfig.JumpHeightMultiplier.Value = 5.0f;
                    Plugin.PluginConfig.ClimbSpeedMultiplier.Value = 8.0f;
                    playerManager.SetMovementSpeedMultiplier(8.0f);
                    playerManager.SetJumpHeightMultiplier(5.0f);
                    playerManager.SetClimbSpeedMultiplier(8.0f);
                    AddToConsole("[PLAYER] Movement preset: Extreme");
                }
                
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Player manager not available");
            }

            // Add some extra space at the bottom for better scrolling
            GUILayout.Space(20);
            
            GUILayout.EndScrollView();
        }

        private void DrawEnvironmentTab()
        {
            // Make Environment tab scrollable
            _environmentTabScrollPosition = GUILayout.BeginScrollView(_environmentTabScrollPosition, GUILayout.Height(420));
            
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

            // Add some extra space at the bottom for better scrolling
            GUILayout.Space(20);
            
            GUILayout.EndScrollView();
        }

        private void DrawAdminTab()
        {
            // Make Admin tab scrollable
            _adminTabScrollPosition = GUILayout.BeginScrollView(_adminTabScrollPosition, GUILayout.Height(420));
            
            GUILayout.Label("=== Admin Panel ===");
            GUILayout.Label("Administrative tools for player management");
            
            var localCharacter = Character.localCharacter;
            if (localCharacter == null)
            {
                GUILayout.Label("No character found");
                GUILayout.EndScrollView();
                return;
            }

            // Teleport Coordinates Section
            GUILayout.Space(10);
            GUILayout.Label("=== Teleport to Coordinates ===");
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("X:", GUILayout.Width(20));
            if (float.TryParse(GUILayout.TextField(_teleportX.ToString("F1"), GUILayout.Width(80)), out float newX))
                _teleportX = newX;
            GUILayout.Label("Y:", GUILayout.Width(20));
            if (float.TryParse(GUILayout.TextField(_teleportY.ToString("F1"), GUILayout.Width(80)), out float newY))
                _teleportY = newY;
            GUILayout.Label("Z:", GUILayout.Width(20));
            if (float.TryParse(GUILayout.TextField(_teleportZ.ToString("F1"), GUILayout.Width(80)), out float newZ))
                _teleportZ = newZ;
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Teleport to Coords", GUILayout.Width(150)))
            {
                AdminUIHelper.TeleportToCoordinates(_teleportX, _teleportY, _teleportZ);
                AddToConsole($"[ADMIN] Teleported to coordinates: {_teleportX:F1}, {_teleportY:F1}, {_teleportZ:F1}");
            }
            if (GUILayout.Button("Get Current Position", GUILayout.Width(150)))
            {
                var pos = localCharacter.Center;
                _teleportX = pos.x;
                _teleportY = pos.y;
                _teleportZ = pos.z;
                AddToConsole($"[ADMIN] Current position: {pos.x:F1}, {pos.y:F1}, {pos.z:F1}");
            }
            GUILayout.EndHorizontal();

            // Player Management Section - UPDATED with Item Management
            DrawPlayerManagementSection();

            // Note about RPC limitations
            GUILayout.Space(10);
            GUILayout.Label("=== Development Notes ===");
            GUILayout.Label("• Some multi-player actions may require RPC calls");
            GUILayout.Label("• Remote player modifications under investigation");
            GUILayout.Label("• God Mode, Infinite Stamina only work for local player");
            GUILayout.Label("• Item giving may spawn items at player's feet for remote players");
            GUILayout.Label("• Use console for advanced admin commands");
            
            var hotkeyText = Plugin.PluginConfig?.MenuToggleKey?.Value.ToString() ?? "Insert";
            var noClipHotkey = Plugin.PluginConfig?.NoClipToggleKey?.Value.ToString() ?? "Delete";
            GUILayout.Label($"• Hotkeys: Menu ({hotkeyText}), NoClip ({noClipHotkey})");

            // Add extra space at the bottom for better scrolling
            GUILayout.Space(20);
            
            GUILayout.EndScrollView();
        }

        private void DrawNoClipControls()
        {
            GUILayout.Space(10);
            GUILayout.Label("=== NoClip Controls ===");

            var noClipManager = _menuManager.GetNoClipManager();
            if (noClipManager != null)
            {
                var isNoClipEnabled = noClipManager.IsNoClipEnabled;
                
                // NoClip toggle with standardized button and status
                if (DrawToggleButtonWithStatus("NoClip", isNoClipEnabled, 100, 150, 303))
                {
                    noClipManager.ToggleNoClip();
                    AddToConsole($"[PLAYER] NoClip {(noClipManager.IsNoClipEnabled ? "enabled" : "disabled")}");
                }
                
                var hotkeyText = Plugin.PluginConfig?.NoClipToggleKey?.Value.ToString() ?? "Delete";
                GUILayout.Label($"Hotkey: {hotkeyText}");
                
                // Enhanced controls when enabled
                if (isNoClipEnabled)
                {
                    GUILayout.Space(5);
                    GUILayout.Label("NoClip Force Controls:");
                    
                    // Base Force Control
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Base Force: {noClipManager.VerticalForce:F0}", GUILayout.Width(100));
                    
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        var newForce = Mathf.Max(100f, noClipManager.VerticalForce - 100f);
                        noClipManager.SetVerticalForce(newForce);
                        AddToConsole($"[PLAYER] NoClip base force: {newForce:F0}");
                    }
                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        var newForce = Mathf.Min(2000f, noClipManager.VerticalForce + 100f);
                        noClipManager.SetVerticalForce(newForce);
                        AddToConsole($"[PLAYER] NoClip base force: {newForce:F0}");
                    }
                    GUILayout.EndHorizontal();
                    
                    // Sprint Multiplier Control
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Sprint Mult: {noClipManager.SprintMultiplier:F1}x", GUILayout.Width(100));
                    
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        var newMult = Mathf.Max(1f, noClipManager.SprintMultiplier - 0.5f);
                        noClipManager.SetSprintMultiplier(newMult);
                        AddToConsole($"[PLAYER] NoClip sprint multiplier: {newMult:F1}x");
                    }
                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        var newMult = Mathf.Min(10f, noClipManager.SprintMultiplier + 0.5f);
                        noClipManager.SetSprintMultiplier(newMult);
                        AddToConsole($"[PLAYER] NoClip sprint multiplier: {newMult:F1}x");
                    }
                    GUILayout.EndHorizontal();
                    
                    // Preset buttons
                    GUILayout.Space(5);
                    GUILayout.Label("Presets:");
                    GUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button("Slow", GUILayout.Width(50)))
                    {
                        noClipManager.SetVerticalForce(400f);
                        noClipManager.SetSprintMultiplier(2f);
                        AddToConsole("[PLAYER] NoClip preset: Slow");
                    }
                    if (GUILayout.Button("Normal", GUILayout.Width(50)))
                    {
                        noClipManager.SetVerticalForce(800f);
                        noClipManager.SetSprintMultiplier(4f);
                        AddToConsole("[PLAYER] NoClip preset: Normal");
                    }
                    if (GUILayout.Button("Fast", GUILayout.Width(50)))
                    {
                        noClipManager.SetVerticalForce(1200f);
                        noClipManager.SetSprintMultiplier(6f);
                        AddToConsole("[PLAYER] NoClip preset: Fast");
                    }
                    if (GUILayout.Button("Turbo", GUILayout.Width(50)))
                    {
                        noClipManager.SetVerticalForce(1600f);
                        noClipManager.SetSprintMultiplier(8f);
                        AddToConsole("[PLAYER] NoClip preset: Turbo");
                    }
                    
                    GUILayout.EndHorizontal();
                    
                    GUILayout.Space(5);
                    GUILayout.Label("Controls: WASD + Space/Ctrl + Shift");
                }
            }
            else
            {
                GUILayout.Label("NoClip manager not available");
            }
        }

        private void DrawPlayerManagementSection()
        {
            GUILayout.Space(10);
            GUILayout.Label("=== Player Management ===");
            
            // Player Selection Dropdown - UPDATED
            GUILayout.Label("Select Player:");
            DrawPlayerDropdown();

            // Selected Player Actions - UPDATED with Item Management
            if (_selectedPlayer != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"=== Actions for: {_selectedPlayer.characterName} ===");
                
                // Player info display
                var status = _selectedPlayer.data.dead ? "[DEAD]" : 
                            _selectedPlayer.data.passedOut ? "[OUT]" : "[OK]";
                var isLocal = _selectedPlayer.IsLocal ? " (Local)" : " (Remote)";
                GUILayout.Label($"Status: {status}{isLocal}", GUI.skin.box);
                
                // Basic Actions Row 1
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Heal", GUILayout.Width(60)))
                {
                    AdminUIHelper.ExecuteQuickAction("heal", _selectedPlayer.characterName);
                    AddToConsole($"[ADMIN] Healed {_selectedPlayer.characterName}");
                }
                if (GUILayout.Button("Goto", GUILayout.Width(60)))
                {
                    AdminUIHelper.ExecuteQuickAction("goto", _selectedPlayer.characterName);
                    AddToConsole($"[ADMIN] Teleported to {_selectedPlayer.characterName}");
                }
                if (GUILayout.Button("Bring", GUILayout.Width(60)))
                {
                    AdminUIHelper.ExecuteQuickAction("bring", _selectedPlayer.characterName);
                    AddToConsole($"[ADMIN] Brought {_selectedPlayer.characterName} to you");
                }
                
                // Conditional action
                if (_selectedPlayer.data.dead || _selectedPlayer.data.fullyPassedOut)
                {
                    if (GUILayout.Button("Revive", GUILayout.Width(60)))
                    {
                        AdminUIHelper.ExecuteQuickAction("revive", _selectedPlayer.characterName);
                        AddToConsole($"[ADMIN] Revived {_selectedPlayer.characterName}");
                    }
                }
                else
                {
                    if (GUILayout.Button("Kill", GUILayout.Width(60)))
                    {
                        AdminUIHelper.ExecuteQuickAction("kill", _selectedPlayer.characterName);
                        AddToConsole($"[ADMIN] Killed {_selectedPlayer.characterName}");
                    }
                }
                GUILayout.EndHorizontal();
                
                // NEW: Item Management Section
                GUILayout.Space(10);
                GUILayout.Label("=== Item Management ===");
                
                // Item selection dropdown
                DrawItemDropdown();
                
                GUILayout.Space(5);

                // Give item button
                GUILayout.BeginHorizontal();

                // Button state logic
                bool canGiveItem = _selectedItemIndex > 0; // Index 0 is "Select Item..."
                var buttonColor = GUI.backgroundColor;

                if (!canGiveItem)
                {
                    GUI.backgroundColor = Color.gray;
                }

                if (GUILayout.Button("Give Item", GUILayout.Width(100)) && canGiveItem)
                {
                    var itemName = _availableItems[_selectedItemIndex];
                    GiveItemToPlayer(_selectedPlayer, itemName, 1);
                }

                if (GUILayout.Button("Drop Item", GUILayout.Width(100)) && canGiveItem)
                {
                    var itemName = _availableItems[_selectedItemIndex];
                    DropItemNearPlayer(_selectedPlayer, itemName, 1);
                }

                GUI.backgroundColor = buttonColor;
                GUILayout.EndHorizontal();

                // Item management help text - UPDATED
                if (!canGiveItem)
                {
                    GUILayout.Label("Select an item from the dropdown to enable item actions", GUI.skin.box);
                }
                else
                {
                    var isLocalPlayer = _selectedPlayer.IsLocal;
                    var giveMethodText = isLocalPlayer ? "Direct inventory addition" : "Spawn at player location";
                    GUILayout.Label($"Give: {giveMethodText}", GUI.skin.box);
                    GUILayout.Label("Drop: Always spawns at player's feet", GUI.skin.box);

                    if (!isLocalPlayer && !Photon.Pun.PhotonNetwork.IsMasterClient)
                    {
                        var statusColor = GUI.color;
                        GUI.color = Color.yellow;
                        GUILayout.Label("Warning: Not Master Client - spawning may fail", GUI.skin.box);
                        GUI.color = statusColor;
                    }
                }

                GUILayout.Space(5);
                GUILayout.Label("Note: God Mode and Infinite Stamina are in Player tab");
                GUILayout.Label("(These features only work for the local player)");
            }
            else
            {
                GUILayout.Space(10);
                GUILayout.Label("Select a player from the dropdown above to perform actions", GUI.skin.box);
            }
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

        private void GiveItemToPlayer(Character targetPlayer, string itemName, int quantity)
        {
            if (targetPlayer == null || string.IsNullOrEmpty(itemName))
            {
                AddToConsole("[ERROR] Invalid player or item name");
                return;
            }

            try
            {
                // Find the item prefab by name
                var itemPrefab = FindItemPrefabByName(itemName);
                if (itemPrefab == null)
                {
                    AddToConsole($"[ERROR] Item '{itemName}' not found in resources");
                    return;
                }

                if (targetPlayer.IsLocal)
                {
                    // For local player, add directly to inventory
                    GiveItemToLocalPlayer(itemPrefab, quantity);
                }
                else
                {
                    // For remote players, spawn items near them
                    SpawnItemsNearPlayer(targetPlayer, itemPrefab, quantity);
                }
                
                AddToConsole($"[ADMIN] Gave {quantity}x {itemName} to {targetPlayer.characterName}");
            }
            catch (System.Exception ex)
            {
                AddToConsole($"[ERROR] Failed to give item: {ex.Message}");
            }
        }

        private void DropItemNearPlayer(Character targetPlayer, string itemName, int quantity)
        {
            if (targetPlayer == null || string.IsNullOrEmpty(itemName))
            {
                AddToConsole("[ERROR] Invalid player or item name");
                return;
            }

            try
            {
                // Find the item prefab by name
                var itemPrefab = FindItemPrefabByName(itemName);
                if (itemPrefab == null)
                {
                    AddToConsole($"[ERROR] Item '{itemName}' not found in resources");
                    return;
                }

                // Always spawn items at player's feet regardless of local/remote
                SpawnItemsNearPlayer(targetPlayer, itemPrefab, quantity);

                AddToConsole($"[ADMIN] Dropped {quantity}x {itemName} near {targetPlayer.characterName}");
            }
            catch (System.Exception ex)
            {
                AddToConsole($"[ERROR] Failed to drop item: {ex.Message}");
            }
        }

        private Item FindItemPrefabByName(string itemName)
        {
            try
            {
                var itemHelper = ItemDiscoveryHelper.Instance;
                return itemHelper.FindItemByName(itemName);
            }
            catch (System.Exception ex)
            {
                AddToConsole($"[ERROR] Exception while searching for item '{itemName}': {ex.Message}");
                return null;
            }
        }

        private void GiveItemToLocalPlayer(Item itemPrefab, int quantity)
        {
            var localPlayer = Player.localPlayer;
            if (localPlayer == null)
            {
                AddToConsole("[ERROR] Local player not found");
                return;
            }

            try
            {
                for (int i = 0; i < quantity; i++)
                {
                    // Find an empty slot
                    for (byte slotIndex = 0; slotIndex < localPlayer.itemSlots.Length; slotIndex++)
                    {
                        var slot = localPlayer.itemSlots[slotIndex];
                        if (slot.IsEmpty())
                        {
                            // Create new item instance data
                            var itemData = new ItemInstanceData(System.Guid.NewGuid());
                            ItemInstanceDataHandler.AddInstanceData(itemData);
                            
                            // Set the slot data
                            slot.prefab = itemPrefab;
                            slot.data = itemData;
                            
                            // Sync inventory with other players
                            var syncData = IBinarySerializable.ToManagedArray<InventorySyncData>(
                                new InventorySyncData(
                                    localPlayer.itemSlots,
                                    localPlayer.backpackSlot,
                                    localPlayer.tempFullSlot
                                )
                            );
                            
                            localPlayer.photonView.RPC("SyncInventoryRPC", Photon.Pun.RpcTarget.Others, syncData, true);
                            
                            AddToConsole($"[ADMIN] Added {itemPrefab.UIData?.itemName ?? itemPrefab.gameObject.name} to inventory slot {slotIndex}");
                            break;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                AddToConsole($"[ERROR] Failed to add item to local inventory: {ex.Message}");
            }
        }

        private void SpawnItemsNearPlayer(Character targetPlayer, Item itemPrefab, int quantity)
        {
            if (!Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                AddToConsole("[WARNING] Item spawning requires Master Client privileges");
                AddToConsole("[INFO] Attempting to spawn anyway...");
            }

            try
            {
                for (int i = 0; i < quantity; i++)
                {
                    // Calculate spawn position near the player
                    Vector3 basePosition = targetPlayer.Center;
                    Vector3 randomOffset = new Vector3(
                        UnityEngine.Random.Range(-2f, 2f),
                        UnityEngine.Random.Range(1f, 3f),
                        UnityEngine.Random.Range(-2f, 2f)
                    );
                    Vector3 spawnPosition = basePosition + randomOffset;
                    
                    // Spawn the item using PhotonNetwork
                    var spawnedObject = Photon.Pun.PhotonNetwork.Instantiate(
                        "0_Items/" + itemPrefab.gameObject.name, 
                        spawnPosition, 
                        UnityEngine.Quaternion.identity
                    );
                    
                    if (spawnedObject != null)
                    {
                        var spawnedItem = spawnedObject.GetComponent<Item>();
                        if (spawnedItem != null)
                        {
                            // Set the item to non-kinematic so it falls naturally
                            spawnedItem.SetKinematicNetworked(false, spawnPosition, UnityEngine.Quaternion.identity);
                            
                            AddToConsole($"[ADMIN] Spawned {itemPrefab.UIData?.itemName ?? itemPrefab.gameObject.name} near {targetPlayer.characterName}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                AddToConsole($"[ERROR] Failed to spawn item near player: {ex.Message}");
                
                // Fallback: Try using CharacterItems.SpawnItemInHand via reflection
                try
                {
                    if (targetPlayer.refs?.items != null)
                    {
                        // Use reflection to access the internal SpawnItemInHand method
                        var spawnItemMethod = typeof(CharacterItems).GetMethod("SpawnItemInHand", 
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        
                        if (spawnItemMethod != null)
                        {
                            for (int i = 0; i < quantity; i++)
                            {
                                spawnItemMethod.Invoke(targetPlayer.refs.items, new object[] { itemPrefab.gameObject.name });
                            }
                            AddToConsole($"[ADMIN] Used fallback method to spawn items for {targetPlayer.characterName}");
                        }
                        else
                        {
                            AddToConsole("[ERROR] SpawnItemInHand method not found via reflection");
                        }
                    }
                }
                catch (System.Exception fallbackEx)
                {
                    AddToConsole($"[ERROR] Fallback method also failed: {fallbackEx.Message}");
                }
            }
        }
    }
}