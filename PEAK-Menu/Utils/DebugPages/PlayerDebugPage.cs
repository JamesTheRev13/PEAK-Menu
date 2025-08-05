using UnityEngine;
using UnityEngine.UIElements;
using Zorro.Core.CLI;
using PEAK_Menu.Utils;

namespace PEAK_Menu.Utils.DebugPages
{
    public class PlayerDebugPage : BaseCustomDebugPage
    {
        private PlayerManager _playerManager;
        private NoClipManager _noClipManager;
        private RainbowManager _rainbowManager;
        private VisualElement _rainbowSpeedContainer;
        private VisualElement _noClipControls;

        // Flags to track if sections need rebuilding
        private bool _managersInitialized = false;
        private bool _sectionsBuilt = false;

        public PlayerDebugPage()
        {
            // Don't try to get managers here - they might not be initialized yet
            AddToConsole("PlayerDebugPage created - managers will be initialized later");
        }

        protected override void BuildContent()
        {
            // Always build the basic sections
            BuildPlayerInfoSection();
            BuildHealthSection();
            
            // Try to get managers and build dependent sections
            TryInitializeManagers();
            BuildManagerDependentSections();
        }

        private void TryInitializeManagers()
        {
            if (_managersInitialized) return;

            try
            {
                _playerManager = Plugin.Instance?._menuManager?.GetPlayerManager();
                _noClipManager = Plugin.Instance?._menuManager?.GetNoClipManager();
                _rainbowManager = Plugin.Instance?._menuManager?.GetRainbowManager();

                if (_playerManager != null && _noClipManager != null && _rainbowManager != null)
                {
                    _managersInitialized = true;
                    AddToConsole("All managers initialized successfully");
                }
                else
                {
                    AddToConsole($"Managers status - Player: {(_playerManager != null ? "OK" : "NULL")}, " +
                                $"NoClip: {(_noClipManager != null ? "OK" : "NULL")}, " +
                                $"Rainbow: {(_rainbowManager != null ? "OK" : "NULL")}");
                }
            }
            catch (System.Exception ex)
            {
                AddToConsole($"Error initializing managers: {ex.Message}");
            }
        }

        private void BuildManagerDependentSections()
        {
            if (!_managersInitialized)
            {
                // Build placeholder sections that will be replaced when managers are ready
                BuildPlaceholderSections();
                return;
            }

            // Build actual sections with manager functionality
            BuildAppearanceSection();
            BuildMovementSection();
            BuildProtectionSection();
            BuildNoClipSection();
            _sectionsBuilt = true;
        }

        private void BuildPlaceholderSections()
        {
            // Appearance placeholder
            var appearanceSection = CreateSection("Appearance & Customization");
            appearanceSection.Add(CreateLabel("Waiting for managers to initialize..."));
            _scrollView.Add(appearanceSection);

            // Movement placeholder  
            var movementSection = CreateSection("Movement Enhancement");
            movementSection.Add(CreateLabel("Waiting for managers to initialize..."));
            _scrollView.Add(movementSection);

            // Protection placeholder
            var protectionSection = CreateSection("Protection Settings");
            protectionSection.Add(CreateLabel("Waiting for managers to initialize..."));
            _scrollView.Add(protectionSection);

            // NoClip placeholder
            var noClipSection = CreateSection("NoClip Controls");
            noClipSection.Add(CreateLabel("Waiting for managers to initialize..."));
            _scrollView.Add(noClipSection);
        }

        public override void UpdateContent()
        {
            // First, try to initialize managers if not done yet
            if (!_managersInitialized)
            {
                TryInitializeManagers();
                
                // If managers are now ready and sections haven't been built, rebuild everything
                if (_managersInitialized && !_sectionsBuilt)
                {
                    AddToConsole("Managers ready - rebuilding sections with full functionality");
                    RebuildManagerDependentSections();
                }
            }

            // Continue with normal reactive updates
            base.UpdateContent();
        }

        private void RebuildManagerDependentSections()
        {
            // Remove placeholder sections (keep player info and health)
            var elementsToRemove = new System.Collections.Generic.List<VisualElement>();
            
            // Find sections to remove (anything after the health section)
            bool foundHealthSection = false;
            foreach (var child in _scrollView.Children())
            {
                if (child is VisualElement section)
                {
                    // Look for the health section marker
                    var firstChild = section.childCount > 0 ? section[0] as Label : null;
                    if (firstChild?.text?.Contains("Health Management") == true)
                    {
                        foundHealthSection = true;
                        continue;
                    }
                    
                    // Remove everything after health section
                    if (foundHealthSection)
                    {
                        elementsToRemove.Add(section);
                    }
                }
            }

            // Remove the placeholder sections
            foreach (var element in elementsToRemove)
            {
                _scrollView.Remove(element);
            }

            // Build real sections
            BuildAppearanceSection();
            BuildMovementSection();
            BuildProtectionSection();
            BuildNoClipSection();
            _sectionsBuilt = true;
        }

        private void BuildPlayerInfoSection()
        {
            var section = CreateSection("Player Information");
            
            // Live reactive labels that update automatically
            section.Add(CreateLiveLabel("Name: ", () => {
                var character = Character.localCharacter;
                return character?.characterName ?? "No character";
            }));
            
            section.Add(CreateLiveLabel("Health: ", () => {
                var character = Character.localCharacter;
                if (character == null) return "N/A";
                var health = (1f - character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury)) * 100f;
                return $"{health:F1}%";
            }));
            
            section.Add(CreateLiveLabel("Stamina: ", () => {
                var character = Character.localCharacter;
                if (character == null) return "N/A";
                var stamina = character.GetTotalStamina() * 100f;
                return $"{stamina:F1}%";
            }));
            
            section.Add(CreateLiveLabel("Position: ", () => {
                var character = Character.localCharacter;
                return character?.Center.ToString("F1") ?? "N/A";
            }));
            
            section.Add(CreateLiveLabel("Status: ", () => {
                var character = Character.localCharacter;
                if (character == null) return "N/A";
                return character.data.dead ? "Dead" : character.data.passedOut ? "Passed Out" : "Alive";
            }));

            _scrollView.Add(section);
        }

        private void BuildHealthSection()
        {
            var section = CreateSection("Health Management");

            var buttonRow = CreateRowContainer();
            buttonRow.Add(CreateButton("Full Heal", () => 
            {
                var character = Character.localCharacter;
                if (character != null)
                {
                    ExecuteMenuCommand($"admin heal \"{character.characterName}\"");
                    AddToConsole("Player fully healed");
                }
            }));

            buttonRow.Add(CreateButton("Clear Status Effects", () => 
            {
                var character = Character.localCharacter;
                if (character != null)
                {
                    ExecuteMenuCommand($"admin clear-status \"{character.characterName}\"");
                    AddToConsole("All status effects cleared");
                }
            }));

            buttonRow.Add(CreateButton("Revive", () => 
            {
                var character = Character.localCharacter;
                if (character != null)
                {
                    ExecuteMenuCommand($"admin revive \"{character.characterName}\"");
                    AddToConsole("Player revived");
                }
            }));

            section.Add(buttonRow);

            // Advanced Status Control Section
            var statusSection = CreateSection("Advanced Status Control");
            
            // Status value slider
            var statusSliderContainer = new VisualElement();
            statusSliderContainer.style.flexDirection = FlexDirection.Row;
            statusSliderContainer.style.marginBottom = 8;
            statusSliderContainer.style.alignItems = Align.Center;

            var statusLabel = CreateLabel("Status Value:");
            statusLabel.style.width = 100;
            
            var statusSlider = new Slider(0f, 1f) { value = 0.5f };
            statusSlider.style.flexGrow = 1;
            statusSlider.style.marginLeft = 10;
            statusSlider.style.marginRight = 10;
            
            var statusValueLabel = CreateLabel("0.50");
            statusValueLabel.style.width = 50;
            statusSlider.RegisterValueChangedCallback(evt => statusValueLabel.text = $"{evt.newValue:F2}");

            statusSliderContainer.Add(statusLabel);
            statusSliderContainer.Add(statusSlider);
            statusSliderContainer.Add(statusValueLabel);
            statusSection.Add(statusSliderContainer);

            // Status action buttons
            var statusButtonRow = CreateRowContainer();
            
            var setHealthButton = CreateButton("Set Health", () =>
            {
                var character = Character.localCharacter;
                if (character != null)
                {
                    ExecuteMenuCommand($"admin health \"{character.characterName}\" {statusSlider.value}");
                    AddToConsole($"Set health to {statusSlider.value * 100:F0}%");
                }
            });
            setHealthButton.style.width = 90;
            setHealthButton.style.marginRight = 5;

            var setStaminaButton = CreateButton("Set Stamina", () =>
            {
                var character = Character.localCharacter;
                if (character != null)
                {
                    ExecuteMenuCommand($"admin stamina \"{character.characterName}\" {statusSlider.value}");
                    AddToConsole($"Set stamina to {statusSlider.value * 100:F0}%");
                }
            });
            setStaminaButton.style.width = 90;
            setStaminaButton.style.marginRight = 5;

            var setHungerButton = CreateButton("Set Hunger", () =>
            {
                var character = Character.localCharacter;
                if (character != null)
                {
                    ExecuteMenuCommand($"admin hunger \"{character.characterName}\" {statusSlider.value}");
                    AddToConsole($"Set hunger to {statusSlider.value * 100:F0}%");
                }
            });
            setHungerButton.style.width = 90;

            statusButtonRow.Add(setHealthButton);
            statusButtonRow.Add(setStaminaButton);
            statusButtonRow.Add(setHungerButton);
            statusSection.Add(statusButtonRow);

            section.Add(statusSection);

            // Live reactive toggles that sync with actual game state
            section.Add(CreateLiveToggle("God Mode", 
                () => Character.localCharacter?.statusesLocked ?? false,
                (enabled) => {
                    var character = Character.localCharacter;
                    if (character != null)
                    {
                        AdminUIHelper.ExecuteQuickAction("god-mode", character.characterName);
                        AddToConsole($"God mode {(enabled ? "enabled" : "disabled")}");
                    }
                }));

            section.Add(CreateLiveToggle("Infinite Stamina", 
                () => Character.localCharacter?.infiniteStam ?? false,
                (enabled) => {
                    var character = Character.localCharacter;
                    if (character != null)
                    {
                        AdminUIHelper.ExecuteQuickAction("infinite-stamina", character.characterName);
                        AddToConsole($"Infinite stamina {(enabled ? "enabled" : "disabled")}");
                    }
                }));

            _scrollView.Add(section);
        }

        private void BuildAppearanceSection()
        {
            var section = CreateSection("Appearance & Customization");

            section.Add(CreateButton("Randomize Appearance", () =>
            {
                ExecuteMenuCommand("customize randomize");
                AddToConsole("Character appearance randomized");
            }));

            // Rainbow controls with live reactive updates
            if (_rainbowManager != null)
            {
                section.Add(CreateLiveToggle("Rainbow Effect",
                    () => _rainbowManager.IsRainbowEnabled,
                    (enabled) => {
                        ExecuteMenuCommand($"customize rainbow {(enabled ? "on" : "off")}");
                        AddToConsole($"Rainbow effect {(enabled ? "enabled" : "disabled")}");
                    }));

                // Speed container that shows/hides reactively
                _rainbowSpeedContainer = CreateRowContainer();
                var speedButtons = new[]
                {
                    ("Slow", 0.5f),
                    ("Normal", 1.0f),
                    ("Fast", 2.0f),
                    ("CRAZY!", 5.0f)
                };

                foreach (var (label, speed) in speedButtons)
                {
                    var speedButton = CreateButton(label, () =>
                    {
                        ExecuteMenuCommand($"customize rainbow speed {speed}");
                        AddToConsole($"Rainbow speed: {label}");
                    });
                    speedButton.style.marginRight = 5;
                    speedButton.style.width = 70;
                    _rainbowSpeedContainer.Add(speedButton);
                }

                section.Add(_rainbowSpeedContainer);

                // Register reactive visibility update
                _liveUpdateCallbacks.Add(() => {
                    if (_rainbowSpeedContainer != null && _rainbowManager != null)
                    {
                        var shouldShow = _rainbowManager.IsRainbowEnabled;
                        _rainbowSpeedContainer.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                });
            } else
            {
                section.Add(CreateLabel("Rainbow manager not available"));
            }

            _scrollView.Add(section);
        }

        private void BuildMovementSection()
        {
            var section = CreateSection("Movement Enhancement");

            if (_playerManager != null)
            {
                // Live reactive sliders that sync with config values
                CreateLiveSlider("Speed Multiplier", 
                    () => Plugin.PluginConfig.MovementSpeedMultiplier.Value,
                    (value) => {
                        Plugin.PluginConfig.MovementSpeedMultiplier.Value = value;
                        _playerManager.SetMovementSpeedMultiplier(value);
                        AddToConsole($"Movement speed: {value:F2}x");
                    }, 0.1f, 20f, section);

                CreateLiveSlider("Jump Multiplier", 
                    () => Plugin.PluginConfig.JumpHeightMultiplier.Value,
                    (value) => {
                        Plugin.PluginConfig.JumpHeightMultiplier.Value = value;
                        _playerManager.SetJumpHeightMultiplier(value);
                        AddToConsole($"Jump height: {value:F2}x");
                    }, 0.1f, 10f, section);

                CreateLiveSlider("Climb Multiplier", 
                    () => Plugin.PluginConfig.ClimbSpeedMultiplier.Value,
                    (value) => {
                        Plugin.PluginConfig.ClimbSpeedMultiplier.Value = value;
                        _playerManager.SetClimbSpeedMultiplier(value);
                        AddToConsole($"Climb speed: {value:F2}x");
                    }, 0.1f, 20f, section);

                // Movement presets
                var presetsContainer = CreateRowContainer();
                var presets = new[]
                {
                    ("Normal", 1.0f, 1.0f, 1.0f),
                    ("Enhanced", 2.0f, 1.5f, 2.0f),
                    ("Super", 4.0f, 3.0f, 4.0f),
                    ("Extreme", 8.0f, 5.0f, 8.0f)
                };

                foreach (var (name, speed, jump, climb) in presets)
                {
                    var presetButton = CreateButton(name, () => 
                    {
                        Plugin.PluginConfig.MovementSpeedMultiplier.Value = speed;
                        Plugin.PluginConfig.JumpHeightMultiplier.Value = jump;
                        Plugin.PluginConfig.ClimbSpeedMultiplier.Value = climb;

                        _playerManager.SetMovementSpeedMultiplier(speed);
                        _playerManager.SetJumpHeightMultiplier(jump);
                        _playerManager.SetClimbSpeedMultiplier(climb);

                        AddToConsole($"Movement preset: {name}");
                    });
                    presetButton.style.marginRight = 5;
                    presetButton.style.width = 80;
                    presetsContainer.Add(presetButton);
                }

                section.Add(presetsContainer);

                // Teleport-to-Ping Controls Section (using existing system)
                var teleportSection = CreateSection("Teleport-to-Ping Controls");
                
                // Use the existing teleport-to-ping toggle functionality
                teleportSection.Add(CreateLiveToggle("Auto Teleport to Ping", 
                    () => Plugin.PluginConfig.TeleportToPingEnabled.Value,
                    (enabled) => {
                        Plugin.PluginConfig.TeleportToPingEnabled.Value = enabled;
                        AddToConsole($"Auto teleport to ping {(enabled ? "enabled" : "disabled")}");
                        
                        if (enabled)
                        {
                            AddToConsole("[INFO] You will now teleport when you ping locations");
                            AddToConsole("[INFO] Hold ping key and click to place marker and teleport");
                        }
                        else
                        {
                            AddToConsole("[INFO] Ping will work normally without teleporting");
                        }
                    }));

                // Status display
                teleportSection.Add(CreateLiveLabel("Teleport Mode: ", () => {
                    return Plugin.PluginConfig.TeleportToPingEnabled.Value ? "Auto (when pinging)" : "Disabled";
                }));

                // Instructions
                teleportSection.Add(CreateLabel("How to use: Enable the toggle above, then use your normal ping"));
                teleportSection.Add(CreateLabel("controls in-game. You'll teleport automatically when you ping."));

                section.Add(teleportSection);
            }
            else
            {
                section.Add(CreateLabel("Player manager not available"));
            }

            _scrollView.Add(section);
        }

        private void BuildProtectionSection()
        {
            var section = CreateSection("Protection Settings");

            section.Add(CreateLiveToggle("No Fall Damage", 
                () => Plugin.PluginConfig.NoFallDamage.Value,
                (enabled) => {
                    Plugin.PluginConfig.NoFallDamage.Value = enabled;
                    _playerManager?.SetNoFallDamage(enabled);
                    AddToConsole($"No fall damage {(enabled ? "enabled" : "disabled")}");
                }));

            section.Add(CreateLiveToggle("No Weight Penalties", 
                () => Plugin.PluginConfig.NoWeight.Value,
                (enabled) => {
                    Plugin.PluginConfig.NoWeight.Value = enabled;
                    _playerManager?.SetNoWeight(enabled);
                    AddToConsole($"No weight penalties {(enabled ? "enabled" : "disabled")}");
                }));

            section.Add(CreateLiveToggle("Affliction Immunity", 
                () => Plugin.PluginConfig.AfflictionImmunity.Value,
                (enabled) => {
                    Plugin.PluginConfig.AfflictionImmunity.Value = enabled;
                    _playerManager?.SetAfflictionImmunity(enabled);
                    AddToConsole($"Affliction immunity {(enabled ? "enabled" : "disabled")}");
                }));

            _scrollView.Add(section);
        }

        private void BuildNoClipSection()
        {
            var section = CreateSection("NoClip Controls");

            if (_noClipManager != null)
            {
                section.Add(CreateLiveToggle("NoClip Mode", 
                    () => _noClipManager.IsNoClipEnabled,
                    (enabled) => {
                        _noClipManager.ToggleNoClip();
                        AddToConsole($"NoClip {(_noClipManager.IsNoClipEnabled ? "enabled" : "disabled")}");
                    }));

                // NoClip controls that show/hide reactively
                _noClipControls = new VisualElement();
                
                _noClipControls.Add(CreateLabel($"Hotkey: {Plugin.PluginConfig.NoClipToggleKey.Value}"));
                _noClipControls.Add(CreateLabel("Controls: WASD + Space/Ctrl + Shift"));

                CreateLiveSlider("Base Force", 
                    () => _noClipManager.VerticalForce,
                    (value) => {
                        _noClipManager.SetVerticalForce(value);
                        AddToConsole($"NoClip base force: {value:F0}");
                    }, 200f, 2000f, _noClipControls);

                CreateLiveSlider("Sprint Multiplier", 
                    () => _noClipManager.SprintMultiplier,
                    (value) => {
                        _noClipManager.SetSprintMultiplier(value);
                        AddToConsole($"NoClip sprint multiplier: {value:F1}x");
                    }, 1f, 10f, _noClipControls);

                // NoClip presets
                var presetsContainer = CreateRowContainer();
                var presets = new[]
                {
                    ("Slow", 400f, 2f),
                    ("Normal", 800f, 4f),
                    ("Fast", 1200f, 6f),
                    ("Turbo", 1600f, 8f)
                };

                foreach (var (name, force, sprint) in presets)
                {
                    var presetButton = CreateButton(name, () => 
                    {
                        _noClipManager.SetVerticalForce(force);
                        _noClipManager.SetSprintMultiplier(sprint);
                        AddToConsole($"NoClip preset: {name}");
                    });
                    presetButton.style.marginRight = 5;
                    presetButton.style.width = 70;
                    presetsContainer.Add(presetButton);
                }

                _noClipControls.Add(presetsContainer);
                section.Add(_noClipControls);
                
                // Register reactive visibility update
                _liveUpdateCallbacks.Add(() => {
                    if (_noClipControls != null && _noClipManager != null)
                    {
                        var shouldShow = _noClipManager.IsNoClipEnabled;
                        _noClipControls.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                });
            }
            else
            {
                section.Add(CreateLabel("NoClip manager not available"));
            }

            _scrollView.Add(section);
        }

        private void ExecuteMenuCommand(string command)
        {
            var menuManager = Plugin.Instance?._menuManager;
            if (menuManager != null)
            {
                menuManager.ExecuteCommand(command);
            }
        }

        public override VisualElement FocusOnDefault()
        {
            return _scrollView;
        }
    }
}