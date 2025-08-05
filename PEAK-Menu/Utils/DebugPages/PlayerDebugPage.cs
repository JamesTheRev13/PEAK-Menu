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

        public PlayerDebugPage()
        {
            _playerManager = Plugin.Instance?._menuManager?.GetPlayerManager();
            _noClipManager = Plugin.Instance?._menuManager?.GetNoClipManager();
            _rainbowManager = Plugin.Instance?._menuManager?.GetRainbowManager();
        }

        protected override void BuildContent()
        {
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
                    ExecuteMenuCommand($"no-weight {(enabled ? "on" : "off")}");
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