using UnityEngine.UIElements;

namespace PEAK_Menu.Utils.DebugPages
{
    public class PlayerDebugPage : BaseCustomDebugPage
    {
        private PlayerManager _playerManager;
        private NoClipManager _noClipManager;
        private RainbowManager _rainbowManager;
        private Character _currentCharacter;

        public PlayerDebugPage()
        {
            _playerManager = Plugin.Instance?._menuManager?.GetPlayerManager();
            _noClipManager = Plugin.Instance?._menuManager?.GetNoClipManager();
            _rainbowManager = Plugin.Instance?._menuManager?.GetRainbowManager();
        }

        protected override void BuildContent()
        {
            RefreshPlayerInfo();
        }

        private void RefreshPlayerInfo()
        {
            _scrollView.Clear();
            _currentCharacter = Character.localCharacter;

            if (_currentCharacter == null)
            {
                _scrollView.Add(CreateLabel("No character found"));
                return;
            }

            BuildPlayerInfoSection();
            BuildHealthSection();
            BuildAppearanceSection();
            BuildMovementSection();
            BuildProtectionSection();
            BuildNoClipSection();
        }

        private void BuildPlayerInfoSection()
        {
            var section = CreateSection("Player Information");
            
            section.Add(CreateLabel($"Name: {_currentCharacter.characterName}"));
            
            // Use correct property access for health/stamina
            var health = (1f - _currentCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury)) * 100f;
            var stamina = _currentCharacter.GetTotalStamina() * 100f;
            
            section.Add(CreateLabel($"Health: {health:F1}%"));
            section.Add(CreateLabel($"Stamina: {stamina:F1}%"));
            section.Add(CreateLabel($"Position: {_currentCharacter.Center:F1}"));
            section.Add(CreateLabel($"Status: {(_currentCharacter.data.dead ? "Dead" : _currentCharacter.data.passedOut ? "Passed Out" : "Alive")}"));

            // Refresh button
            section.Add(CreateButton("Refresh Info", RefreshPlayerInfo));

            _scrollView.Add(section);
        }

        private void BuildHealthSection()
        {
            var section = CreateSection("Health Management");

            section.Add(CreateButton("Full Heal", () => 
            {
                if (_currentCharacter != null)
                {
                    // Use existing admin command instead of duplicating logic
                    ExecuteMenuCommand($"admin heal \"{_currentCharacter.characterName}\"");
                    AddToConsole("Player fully healed");
                    RefreshPlayerInfo();
                }
            }));

            section.Add(CreateButton("Clear All Status Effects", () => 
            {
                if (_currentCharacter != null)
                {
                    // Use existing admin command instead of duplicating logic
                    ExecuteMenuCommand($"admin clear-status \"{_currentCharacter.characterName}\"");
                    AddToConsole("All status effects cleared");
                    RefreshPlayerInfo();
                }
            }));

            section.Add(CreateButton("Revive", () => 
            {
                if (_currentCharacter != null)
                {
                    // Use existing admin command instead of duplicating logic
                    ExecuteMenuCommand($"admin revive \"{_currentCharacter.characterName}\"");
                    AddToConsole("Player revived");
                    RefreshPlayerInfo();
                }
            }));

            // Use AdminUIHelper for god mode and infinite stamina like the existing UI does
            section.Add(CreateToggle("God Mode", _currentCharacter?.statusesLocked ?? false, (enabled) =>
            {
                if (_currentCharacter != null)
                {
                    AdminUIHelper.ExecuteQuickAction("god-mode", _currentCharacter.characterName);
                    AddToConsole($"God mode {(enabled ? "enabled" : "disabled")}");
                }
            }));

            section.Add(CreateToggle("Infinite Stamina", _currentCharacter?.infiniteStam ?? false, (enabled) =>
            {
                if (_currentCharacter != null)
                {
                    AdminUIHelper.ExecuteQuickAction("infinite-stamina", _currentCharacter.characterName);
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
                // Use existing customize command instead of duplicating logic
                ExecuteMenuCommand("customize randomize");
                AddToConsole("Character appearance randomized");
            }));

            // Rainbow controls - THIS BELONGS IN PLAYER SECTION, NOT ENVIRONMENT
            if (_rainbowManager != null)
            {
                var isRainbowEnabled = _rainbowManager.IsRainbowEnabled;
                
                section.Add(CreateToggle("Rainbow Effect", isRainbowEnabled, (enabled) =>
                {
                    // Use existing customize command instead of duplicating logic
                    ExecuteMenuCommand($"customize rainbow {(enabled ? "on" : "off")}");
                    AddToConsole($"Rainbow effect {(enabled ? "enabled" : "disabled")}");
                }));

                if (isRainbowEnabled)
                {
                    // Rainbow speed presets like AppearanceSection
                    var speedContainer = new VisualElement();
                    speedContainer.style.flexDirection = FlexDirection.Row;
                    speedContainer.style.marginTop = 10;

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
                            // Use existing customize command instead of duplicating logic
                            ExecuteMenuCommand($"customize rainbow speed {speed}");
                            AddToConsole($"Rainbow speed: {label}");
                        });
                        speedButton.style.marginRight = 5;
                        speedContainer.Add(speedButton);
                    }

                    section.Add(speedContainer);
                }
            }

            _scrollView.Add(section);
        }

        private void BuildMovementSection()
        {
            var section = CreateSection("Movement Enhancement");

            if (_playerManager != null)
            {
                CreateSlider("Speed Multiplier", 
                    Plugin.PluginConfig.MovementSpeedMultiplier.Value, 
                    0.1f, 20f, 
                    (value) => 
                    {
                        Plugin.PluginConfig.MovementSpeedMultiplier.Value = value;
                        _playerManager.SetMovementSpeedMultiplier(value);
                        AddToConsole($"Movement speed: {value:F2}x");
                    });

                CreateSlider("Jump Multiplier", 
                    Plugin.PluginConfig.JumpHeightMultiplier.Value, 
                    0.1f, 10f, 
                    (value) => 
                    {
                        Plugin.PluginConfig.JumpHeightMultiplier.Value = value;
                        _playerManager.SetJumpHeightMultiplier(value);
                        AddToConsole($"Jump height: {value:F2}x");
                    });

                CreateSlider("Climb Multiplier", 
                    Plugin.PluginConfig.ClimbSpeedMultiplier.Value, 
                    0.1f, 20f, 
                    (value) => 
                    {
                        Plugin.PluginConfig.ClimbSpeedMultiplier.Value = value;
                        _playerManager.SetClimbSpeedMultiplier(value);
                        AddToConsole($"Climb speed: {value:F2}x");
                    });

                // Movement presets
                var presetsContainer = new VisualElement();
                presetsContainer.style.flexDirection = FlexDirection.Row;
                presetsContainer.style.marginTop = 10;

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
                        RefreshPlayerInfo();
                    });
                    presetButton.style.marginRight = 5;
                    presetsContainer.Add(presetButton);
                }

                section.Add(presetsContainer);
            }

            _scrollView.Add(section);
        }

        private void BuildProtectionSection()
        {
            var section = CreateSection("Protection Settings");

            section.Add(CreateToggle("No Fall Damage", 
                Plugin.PluginConfig.NoFallDamage.Value,
                (enabled) =>
                {
                    Plugin.PluginConfig.NoFallDamage.Value = enabled;
                    _playerManager?.SetNoFallDamage(enabled);
                    AddToConsole($"No fall damage {(enabled ? "enabled" : "disabled")}");
                }));

            section.Add(CreateToggle("No Weight Penalties", 
                Plugin.PluginConfig.NoWeight.Value,
                (enabled) =>
                {
                    // Use existing no-weight command instead of duplicating logic
                    ExecuteMenuCommand($"no-weight {(enabled ? "on" : "off")}");
                    AddToConsole($"No weight penalties {(enabled ? "enabled" : "disabled")}");
                }));

            section.Add(CreateToggle("Affliction Immunity", 
                Plugin.PluginConfig.AfflictionImmunity.Value,
                (enabled) =>
                {
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
                var isEnabled = _noClipManager.IsNoClipEnabled;

                section.Add(CreateToggle("NoClip Mode", isEnabled, (enabled) =>
                {
                    // Use existing admin command instead of duplicating logic
                    ExecuteMenuCommand($"admin noclip {(enabled ? "on" : "off")}");
                    AddToConsole($"NoClip {(enabled ? "enabled" : "disabled")}");
                    RefreshPlayerInfo();
                }));

                if (isEnabled)
                {
                    section.Add(CreateLabel($"Hotkey: {Plugin.PluginConfig.NoClipToggleKey.Value}"));
                    section.Add(CreateLabel("Controls: WASD + Space/Ctrl + Shift"));

                    CreateSlider("Base Force", _noClipManager.VerticalForce, 200f, 2000f, (value) =>
                    {
                        // Use existing admin command instead of duplicating logic
                        ExecuteMenuCommand($"admin noclip speed {value}");
                        AddToConsole($"NoClip base force: {value:F0}");
                    });

                    CreateSlider("Sprint Multiplier", _noClipManager.SprintMultiplier, 1f, 10f, (value) =>
                    {
                        // Use existing admin command instead of duplicating logic
                        ExecuteMenuCommand($"admin noclip fast {value}");
                        AddToConsole($"NoClip sprint multiplier: {value:F1}x");
                    });

                    // NoClip presets
                    var presetsContainer = new VisualElement();
                    presetsContainer.style.flexDirection = FlexDirection.Row;
                    presetsContainer.style.marginTop = 10;

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
                            // Use existing admin commands instead of duplicating logic
                            ExecuteMenuCommand($"admin noclip speed {force}");
                            ExecuteMenuCommand($"admin noclip fast {sprint}");
                            AddToConsole($"NoClip preset: {name}");
                            RefreshPlayerInfo();
                        });
                        presetButton.style.marginRight = 5;
                        presetsContainer.Add(presetButton);
                    }

                    section.Add(presetsContainer);
                }
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

        public override void Update()
        {
            // Optionally refresh info periodically or on specific events
        }

        public override VisualElement FocusOnDefault()
        {
            return _scrollView;
        }
    }
}