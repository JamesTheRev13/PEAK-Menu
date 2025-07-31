using System.Linq;
using UnityEngine;
using PEAK_Menu.Utils;

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

        // Player selection state for admin tab
        private Vector2 _playerListScrollPosition;
        private int _selectedPlayerIndex = -1;
        private Character _selectedPlayer = null;

        public MenuUI(MenuManager menuManager)
        {
            _menuManager = menuManager;
            _windowRect = new Rect(50, 50, 700, 500);
            _consoleOutput = new System.Collections.Generic.List<string>();
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

            // Player Management Section - UPDATED (removed local-only commands)
            DrawPlayerManagementSection();

            // Note about RPC limitations
            GUILayout.Space(10);
            GUILayout.Label("=== Development Notes ===");
            GUILayout.Label("• Some multi-player actions may require RPC calls");
            GUILayout.Label("• Remote player modifications under investigation");
            GUILayout.Label("• God Mode, Infinite Stamina only work for local player");
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
            
            var allCharacters = Character.AllCharacters?.ToList();
            if (allCharacters == null || allCharacters.Count == 0)
            {
                GUILayout.Label("No players found");
                return;
            }

            // Player Selection List
            GUILayout.Label("Select Player:");
            _playerListScrollPosition = GUILayout.BeginScrollView(_playerListScrollPosition, GUILayout.Height(120));
            
            for (int i = 0; i < allCharacters.Count; i++)
            {
                var character = allCharacters[i];
                if (character == null) continue;
                
                // Player status with color coding
                var status = character.data.dead ? "[DEAD]" : 
                            character.data.passedOut ? "[OUT]" : "[OK]";
        
                var statusColor = character.data.dead ? Color.red :
                                 character.data.passedOut ? Color.yellow : Color.green;
        
                // Selection button with status
                var isSelected = _selectedPlayerIndex == i;
                var buttonColor = isSelected ? Color.cyan : Color.white;
                
                var originalColor = GUI.backgroundColor;
                GUI.backgroundColor = buttonColor;
                
                if (GUILayout.Button($"{character.characterName} {status}", GUILayout.Height(25)))
                {
                    _selectedPlayerIndex = i;
                    _selectedPlayer = character;
                    AddToConsole($"[ADMIN] Selected player: {character.characterName}");
                }
                
                GUI.backgroundColor = originalColor;
            }
            
            GUILayout.EndScrollView();

            // Selected Player Actions - UPDATED (removed local-only commands)
            if (_selectedPlayer != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"=== Actions for: {_selectedPlayer.characterName} ===");
                
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
                
                // REMOVED: God Mode, Infinite Stamina, Clear Status buttons
                // These are local-only and have been moved to Player tab
                
                GUILayout.Space(5);
                GUILayout.Label("Note: God Mode and Infinite Stamina are in Player tab");
                GUILayout.Label("(These features only work for the local player)");
            }
            else
            {
                GUILayout.Space(10);
                GUILayout.Label("Select a player above to perform actions");
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
    }
}