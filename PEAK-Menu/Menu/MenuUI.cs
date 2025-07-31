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
        
        // Add persistent state for advanced player management
        private static string _targetPlayerName = "";
        private static float _statusValue = 0.5f;

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
            
            // Auto-focus console input when console tab is active
            //if (_selectedTab == 0)
            //{
            //    GUI.FocusControl("ConsoleInput");
            //}
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
            
            // Enhanced Player Modifications Section
            GUILayout.Label("=== Player Modifications ===");
            
            var playerManager = _menuManager.GetPlayerManager();
            if (playerManager != null)
            {
                // No Fall Damage
                var originalColor = GUI.backgroundColor;
                var isNoFallEnabled = playerManager.NoFallDamageEnabled;
                GUI.backgroundColor = isNoFallEnabled ? Color.green : Color.red;
                
                if (GUILayout.Button(isNoFallEnabled ? "No Fall Damage OFF" : "No Fall Damage ON"))
                {
                    playerManager.SetNoFallDamage(!isNoFallEnabled);
                    AddToConsole($"[PLAYER] No fall damage {(!isNoFallEnabled ? "enabled" : "disabled")}");
                }
                GUI.backgroundColor = originalColor;
                
                // No Weight
                var isNoWeightEnabled = playerManager.NoWeightEnabled;
                GUI.backgroundColor = isNoWeightEnabled ? Color.green : Color.red;
                
                if (GUILayout.Button(isNoWeightEnabled ? "No Weight OFF" : "No Weight ON"))
                {
                    playerManager.SetNoWeight(!isNoWeightEnabled);
                    AddToConsole($"[PLAYER] No weight {(!isNoWeightEnabled ? "enabled" : "disabled")}");
                }
                GUI.backgroundColor = originalColor;
                
                // Affliction Immunity
                var isAfflictionImmune = playerManager.AfflictionImmunityEnabled;
                GUI.backgroundColor = isAfflictionImmune ? Color.green : Color.red;
                
                if (GUILayout.Button(isAfflictionImmune ? "Affliction Immunity OFF" : "Affliction Immunity ON"))
                {
                    playerManager.SetAfflictionImmunity(!isAfflictionImmune);
                    AddToConsole($"[PLAYER] Affliction immunity {(!isAfflictionImmune ? "enabled" : "disabled")}");
                }
                GUI.backgroundColor = originalColor;
                
                GUILayout.Space(10);
                
                // Movement Speed Controls
                GUILayout.Label("=== Movement Controls ===");
                
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
            }
            
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
            // Make Admin tab scrollable - this is the most important one due to content size
            _adminTabScrollPosition = GUILayout.BeginScrollView(_adminTabScrollPosition, GUILayout.Height(420));
            
            GUILayout.Label("=== Admin Panel ===");
            GUILayout.Label("Administrative tools for moderation");
            
            var localCharacter = Character.localCharacter;
            if (localCharacter == null)
            {
                GUILayout.Label("No character found");
                GUILayout.EndScrollView();
                return;
            }

            // Quick Actions Section - ENHANCED using AdminUIHelper
            GUILayout.Space(10);
            GUILayout.Label("=== Quick Actions ===");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Emergency Heal All", GUILayout.Width(150)))
            {
                AdminUIHelper.ExecuteQuickAction("heal-all");
                AddToConsole("[ADMIN] Emergency heal all executed");
            }
            if (GUILayout.Button("List All Players", GUILayout.Width(150)))
            {
                AdminUIHelper.ExecuteQuickAction("list-all");
                AddToConsole("[ADMIN] Player list requested");
            }
            GUILayout.EndHorizontal();

            // Teleport Coordinates Section - ENHANCED using AdminUIHelper
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

            // Self Admin Section - ENHANCED using AdminUIHelper
            GUILayout.Space(10);
            GUILayout.Label("=== Self Administration ===");
            
            // God Mode Toggle Button
            GUILayout.BeginHorizontal();
            var isGodModeEnabled = localCharacter.statusesLocked;
            var godModeButtonText = isGodModeEnabled ? "God Mode OFF" : "God Mode ON";
            var godModeButtonColor = isGodModeEnabled ? Color.red : Color.green;
            
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = godModeButtonColor;
            
            if (GUILayout.Button(godModeButtonText, GUILayout.Width(130)))
            {
                AdminUIHelper.ExecuteQuickAction("god-mode", localCharacter.characterName);
                AddToConsole($"[ADMIN] God mode {(!isGodModeEnabled ? "enabled" : "disabled")}");
            }
            
            GUI.backgroundColor = originalColor;
            
            var godModeStatus = isGodModeEnabled ? "ENABLED" : "Disabled";
            GUILayout.Label($"God Mode: {godModeStatus}", GUILayout.Width(120));
            
            GUILayout.EndHorizontal();
            
            // Infinite Stamina Toggle Button
            GUILayout.BeginHorizontal();
            var isInfiniteStamEnabled = localCharacter.infiniteStam;
            var infiniteStamButtonText = isInfiniteStamEnabled ? "Infinite Stamina OFF" : "Infinite Stamina ON";
            var infiniteStamButtonColor = isInfiniteStamEnabled ? Color.red : Color.green;
            
            GUI.backgroundColor = infiniteStamButtonColor;
            
            if (GUILayout.Button(infiniteStamButtonText, GUILayout.Width(160)))
            {
                AdminUIHelper.ExecuteQuickAction("infinite-stamina", localCharacter.characterName);
                AddToConsole($"[ADMIN] Infinite stamina {(!isInfiniteStamEnabled ? "enabled" : "disabled")}");
            }
            
            GUI.backgroundColor = originalColor;
            
            var infiniteStamStatus = isInfiniteStamEnabled ? "ENABLED" : "Disabled";
            GUILayout.Label($"Infinite Stamina: {infiniteStamStatus}", GUILayout.Width(120));
            
            GUILayout.EndHorizontal();

            // NoClip controls - existing implementation stays the same
            DrawNoClipControls();

            if (GUILayout.Button("Full Self Heal"))
            {
                AdminUIHelper.ExecuteQuickAction("heal", localCharacter.characterName);
                AddToConsole("[ADMIN] Self heal executed");
            }

            // Player Management Section - ENHANCED using AdminUIHelper
            DrawPlayerManagementSection(localCharacter);

            // Advanced Player Management Section - ENHANCED using AdminUIHelper
            DrawAdvancedPlayerManagement();

            // Status Display and Footer - existing implementation
            DrawAdminStatusDisplay(localCharacter);

            // Add extra space at the bottom for better scrolling
            GUILayout.Space(20);
            
            GUILayout.EndScrollView();
        }

        private void DrawNoClipControls()
        {
            GUILayout.Space(10);
            GUILayout.Label("=== Movement Controls ===");

            var noClipManager = _menuManager.GetNoClipManager();
            if (noClipManager != null)
            {
                var isNoClipEnabled = noClipManager.IsNoClipEnabled;
                
                // NoClip toggle with hotkey info
                GUILayout.BeginHorizontal();
                var noClipButtonText = isNoClipEnabled ? "NoClip OFF" : "NoClip ON";
                var noClipButtonColor = isNoClipEnabled ? Color.red : Color.green;
                
                var originalColor = GUI.backgroundColor;
                GUI.backgroundColor = noClipButtonColor;
                
                if (GUILayout.Button(noClipButtonText, GUILayout.Width(100)))
                {
                    noClipManager.ToggleNoClip();
                    AddToConsole($"[ADMIN] NoClip {(noClipManager.IsNoClipEnabled ? "enabled" : "disabled")}");
                }
                
                GUI.backgroundColor = originalColor;
                
                // Status display with hotkey
                var statusText = isNoClipEnabled ? "ENABLED" : "Disabled";
                var hotkeyText = Plugin.PluginConfig?.NoClipToggleKey?.Value.ToString() ?? "Delete";
                GUILayout.Label($"NoClip: {statusText} (Hotkey: {hotkeyText})", GUILayout.Width(200));
                
                GUILayout.EndHorizontal();
                
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
                        AddToConsole($"[ADMIN] NoClip base force: {newForce:F0}");
                    }
                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        var newForce = Mathf.Min(2000f, noClipManager.VerticalForce + 100f);
                        noClipManager.SetVerticalForce(newForce);
                        AddToConsole($"[ADMIN] NoClip base force: {newForce:F0}");
                    }
                    GUILayout.EndHorizontal();
                    
                    // Sprint Multiplier Control
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Sprint Mult: {noClipManager.SprintMultiplier:F1}x", GUILayout.Width(100));
                    
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        var newMult = Mathf.Max(1f, noClipManager.SprintMultiplier - 0.5f);
                        noClipManager.SetSprintMultiplier(newMult);
                        AddToConsole($"[ADMIN] NoClip sprint multiplier: {newMult:F1}x");
                    }
                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        var newMult = Mathf.Min(10f, noClipManager.SprintMultiplier + 0.5f);
                        noClipManager.SetSprintMultiplier(newMult);
                        AddToConsole($"[ADMIN] NoClip sprint multiplier: {newMult:F1}x");
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
                        AddToConsole("[ADMIN] NoClip preset: Slow");
                    }
                    if (GUILayout.Button("Normal", GUILayout.Width(50)))
                    {
                        noClipManager.SetVerticalForce(800f);
                        noClipManager.SetSprintMultiplier(4f);
                        AddToConsole("[ADMIN] NoClip preset: Normal");
                    }
                    if (GUILayout.Button("Fast", GUILayout.Width(50)))
                    {
                        noClipManager.SetVerticalForce(1200f);
                        noClipManager.SetSprintMultiplier(6f);
                        AddToConsole("[ADMIN] NoClip preset: Fast");
                    }
                    if (GUILayout.Button("Turbo", GUILayout.Width(50)))
                    {
                        noClipManager.SetVerticalForce(1600f);
                        noClipManager.SetSprintMultiplier(8f);
                        AddToConsole("[ADMIN] NoClip preset: Turbo");
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

        private void DrawPlayerManagementSection(Character localCharacter)
        {
            GUILayout.Space(10);
            GUILayout.Label("=== Player Management ===");
            
            var allCharacters = Character.AllCharacters?.Take(8); // Limit display
            if (allCharacters != null)
            {
                foreach (var character in allCharacters)
                {
                    if (character == null) continue;
                    
                    GUILayout.BeginHorizontal();
                    
                    // Player name and status with color coding
                    var status = character.data.dead ? "[DEAD]" : 
                                character.data.passedOut ? "[OUT]" : "[OK]";
            
                    var statusColor = character.data.dead ? Color.red :
                                     character.data.passedOut ? Color.yellow : Color.green;
            
                    var textColor = GUI.color;
                    GUI.color = statusColor;
                    GUILayout.Label($"{character.characterName} {status}", GUILayout.Width(120));
                    GUI.color = textColor;
                    
                    // Quick action buttons - Row 1 using AdminUIHelper
                    if (GUILayout.Button("Heal", GUILayout.Width(40)))
                    {
                        AdminUIHelper.ExecuteQuickAction("heal", character.characterName);
                        AddToConsole($"[ADMIN] Healed {character.characterName}");
                    }
                    if (GUILayout.Button("Goto", GUILayout.Width(40)))
                    {
                        AdminUIHelper.ExecuteQuickAction("goto", character.characterName);
                        AddToConsole($"[ADMIN] Teleported to {character.characterName}");
                    }
                    if (GUILayout.Button("Bring", GUILayout.Width(40)))
                    {
                        AdminUIHelper.ExecuteQuickAction("bring", character.characterName);
                        AddToConsole($"[ADMIN] Brought {character.characterName} to you");
                    }
                    
                    // Conditional buttons using AdminUIHelper
                    if (character.data.dead || character.data.fullyPassedOut)
                    {
                        if (GUILayout.Button("Revive", GUILayout.Width(50)))
                        {
                            AdminUIHelper.ExecuteQuickAction("revive", character.characterName);
                            AddToConsole($"[ADMIN] Revived {character.characterName}");
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Kill", GUILayout.Width(50)))
                        {
                            AdminUIHelper.ExecuteQuickAction("kill", character.characterName);
                            AddToConsole($"[ADMIN] Killed {character.characterName}");
                        }
                    }
                    
                    GUILayout.EndHorizontal();
                    
                    // Player actions row 2 (status management) using AdminUIHelper
                    if (character != localCharacter) // Don't show for self
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(120); // Align with name column
                        
                        if (GUILayout.Button("God Mode", GUILayout.Width(70)))
                        {
                            AdminUIHelper.ExecuteQuickAction("god-mode", character.characterName);
                            AddToConsole($"[ADMIN] Toggled god mode for {character.characterName}");
                        }
                        if (GUILayout.Button("∞ Stamina", GUILayout.Width(70)))
                        {
                            AdminUIHelper.ExecuteQuickAction("infinite-stamina", character.characterName);
                            AddToConsole($"[ADMIN] Toggled infinite stamina for {character.characterName}");
                        }
                        if (GUILayout.Button("Clear Status", GUILayout.Width(80)))
                        {
                            AdminUIHelper.ExecuteQuickAction("clear-status", character.characterName);
                            AddToConsole($"[ADMIN] Cleared status for {character.characterName}");
                        }
                        
                        GUILayout.EndHorizontal();
                    }
                    
                    GUILayout.Space(5); // Spacing between players
                }
            }
        }

        private void DrawAdvancedPlayerManagement()
        {
            GUILayout.Space(10);
            GUILayout.Label("=== Advanced Player Management ===");
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Player:", GUILayout.Width(50));
            _targetPlayerName = GUILayout.TextField(_targetPlayerName, GUILayout.Width(120));
            GUILayout.Label("Value:", GUILayout.Width(45));
            _statusValue = GUILayout.HorizontalSlider(_statusValue, 0f, 1f, GUILayout.Width(80));
            GUILayout.Label($"{_statusValue:F2}", GUILayout.Width(40));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Health", GUILayout.Width(80)))
            {
                if (!string.IsNullOrEmpty(_targetPlayerName))
                {
                    AdminUIHelper.SetPlayerStatus(_targetPlayerName, "health", _statusValue);
                    AddToConsole($"[ADMIN] Set {_targetPlayerName}'s health to {_statusValue * 100:F0}%");
                }
            }
            if (GUILayout.Button("Set Stamina", GUILayout.Width(80)))
            {
                if (!string.IsNullOrEmpty(_targetPlayerName))
                {
                    AdminUIHelper.SetPlayerStatus(_targetPlayerName, "stamina", _statusValue);
                    AddToConsole($"[ADMIN] Set {_targetPlayerName}'s stamina to {_statusValue * 100:F0}%");
                }
            }
            if (GUILayout.Button("Set Hunger", GUILayout.Width(80)))
            {
                if (!string.IsNullOrEmpty(_targetPlayerName))
                {
                    AdminUIHelper.SetPlayerStatus(_targetPlayerName, "hunger", _statusValue);
                    AddToConsole($"[ADMIN] Set {_targetPlayerName}'s hunger to {_statusValue * 100:F0}%");
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawAdminStatusDisplay(Character localCharacter)
        {
            // Status Display
            GUILayout.Space(10);
            GUILayout.Label("=== Your Admin Status ===");
            
            var playerManager = _menuManager.GetPlayerManager();
            var noClipManager = _menuManager.GetNoClipManager();
            
            GUILayout.Label($"God Mode: {(localCharacter.statusesLocked ? "ON" : "OFF")}");
            GUILayout.Label($"Infinite Stamina: {(localCharacter.infiniteStam ? "ON" : "OFF")}");
            GUILayout.Label($"NoClip: {(noClipManager?.IsNoClipEnabled == true ? "ON" : "OFF")}");
            
            if (playerManager != null)
            {
                GUILayout.Label($"No Fall Damage: {(playerManager.NoFallDamageEnabled ? "ON" : "OFF")}");
                GUILayout.Label($"No Weight: {(playerManager.NoWeightEnabled ? "ON" : "OFF")}");
                GUILayout.Label($"Affliction Immunity: {(playerManager.AfflictionImmunityEnabled ? "ON" : "OFF")}");
            }
            
            GUILayout.Space(10);
            GUILayout.Label("=== Hotkeys ===");
            GUILayout.Label($"Toggle Menu: {Plugin.PluginConfig?.MenuToggleKey?.Value.ToString() ?? "Insert"}");
            GUILayout.Label($"Toggle NoClip: {Plugin.PluginConfig?.NoClipToggleKey?.Value.ToString() ?? "Delete"}");
            
            GUILayout.Space(10);
            GUILayout.Label("Use console for advanced admin commands");
            GUILayout.Label("Type 'help admin' for full command list");
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