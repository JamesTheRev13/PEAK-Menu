using UnityEngine;
using System;
using PEAK_Menu.Config;

namespace PEAK_Menu.Menu.UI.Sections
{
    public class PlayerModificationsSection
    {
        private readonly MenuManager _menuManager;

        public PlayerModificationsSection(MenuManager menuManager)
        {
            _menuManager = menuManager;
        }

        public void Draw(Action<string> addToConsole)
        {
            GUILayout.Space(UIConstants.STANDARD_SPACING);
            GUILayout.Label("=== Player Modifications ===");

            var playerManager = _menuManager.GetPlayerManager();
            if (playerManager != null)
            {
                DrawProtectionSettings(playerManager, addToConsole);
                DrawMovementEnhancements(playerManager, addToConsole);
            }
            else
            {
                GUILayout.Label("Player manager not available");
            }
        }

        private void DrawProtectionSettings(object playerManager, Action<string> addToConsole)
        {
            GUILayout.Label("Protection Settings:");

            DrawProtectionToggle("NoFallDamageEnabled", "No Fall Damage", playerManager, addToConsole, 101);
            DrawProtectionToggle("NoWeightEnabled", "No Weight", playerManager, addToConsole, 102);
            DrawProtectionToggle("AfflictionImmunityEnabled", "Affliction Immunity", playerManager, addToConsole, 103);
        }

        private void DrawProtectionToggle(string propertyName, string displayName, object playerManager, Action<string> addToConsole, int buttonId)
        {
            var property = playerManager.GetType().GetProperty(propertyName);
            var currentState = (bool)(property?.GetValue(playerManager) ?? false);

            var toggleColor = GUI.backgroundColor;
            GUI.backgroundColor = currentState ? Color.green : Color.gray;

            if (GUILayout.Button($"{displayName}: {(currentState ? "ON" : "OFF")}"))
            {
                var setMethod = playerManager.GetType().GetMethod($"Set{propertyName.Replace("Enabled", "")}");
                setMethod?.Invoke(playerManager, new object[] { !currentState });
                addToConsole($"[PLAYER] {displayName} {(!currentState ? "enabled" : "disabled")}");

                if (displayName == "No Weight" && !currentState)
                {
                    addToConsole("[INFO] Inventory weight penalties disabled via Harmony patches");
                    addToConsole("[INFO] You can now carry unlimited weight without speed penalties");
                }
            }

            GUI.backgroundColor = toggleColor;
        }

        private void DrawMovementEnhancements(object playerManager, Action<string> addToConsole)
        {
            GUILayout.Space(UIConstants.STANDARD_SPACING);
            GUILayout.Label("=== Movement Enhancement ===");

            DrawMovementSlider("Speed", "MovementSpeedMultiplier", UIConstants.MOVEMENT_SPEED_MIN, UIConstants.MOVEMENT_SPEED_MAX, playerManager, addToConsole);
            DrawMovementSlider("Jump", "JumpHeightMultiplier", UIConstants.JUMP_HEIGHT_MIN, UIConstants.JUMP_HEIGHT_MAX, playerManager, addToConsole);
            DrawMovementSlider("Climb", "ClimbSpeedMultiplier", UIConstants.CLIMB_SPEED_MIN, UIConstants.CLIMB_SPEED_MAX, playerManager, addToConsole);

            DrawMovementPresets(playerManager, addToConsole);
        }

        private void DrawMovementSlider(string label, string configPropertyName, float min, float max, object playerManager, Action<string> addToConsole)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}:", GUILayout.Width(60));

            try
            {
                var configProperty = Plugin.PluginConfig.GetType().GetProperty(configPropertyName);
                var configEntry = configProperty?.GetValue(Plugin.PluginConfig);
                
                // Fix: Get the Value property from the ConfigEntry
                var currentValue = 1.0f;
                if (configEntry != null)
                {
                    var valueProperty = configEntry.GetType().GetProperty("Value");
                    currentValue = (float)(valueProperty?.GetValue(configEntry) ?? 1.0f);
                }

                var newValue = GUILayout.HorizontalSlider(currentValue, min, max, GUILayout.Width(150));

                if (Mathf.Abs(newValue - currentValue) > 0.01f)
                {
                    // Fix: Set the Value property of the ConfigEntry
                    if (configEntry != null)
                    {
                        var valueProperty = configEntry.GetType().GetProperty("Value");
                        valueProperty?.SetValue(configEntry, newValue);
                    }
                    
                    var setMethod = playerManager.GetType().GetMethod($"Set{configPropertyName}");
                    setMethod?.Invoke(playerManager, new object[] { newValue });
                    addToConsole($"[PLAYER] {label}: {newValue:F2}x");
                }

                GUILayout.Label($"{newValue:F2}x", GUILayout.Width(50));
            }
            catch (System.Exception ex)
            {
                GUILayout.Label("Error", GUILayout.Width(50));
                Plugin.Log?.LogError($"Error in DrawMovementSlider: {ex.Message}");
            }

            GUILayout.EndHorizontal(); // Fix: Ensure this is always called
        }

        private void DrawMovementPresets(object playerManager, Action<string> addToConsole)
        {
            GUILayout.Space(UIConstants.SMALL_SPACING);
            GUILayout.Label("Movement Presets:");
            GUILayout.BeginHorizontal();

            var presets = new[]
            {
                ("Normal", 1.0f, 1.0f, 1.0f),
                ("Enhanced", 2.0f, 1.5f, 2.0f),
                ("Super", 4.0f, 3.0f, 4.0f),
                ("Extreme", 8.0f, 5.0f, 8.0f)
            };

            foreach (var (name, speed, jump, climb) in presets)
            {
                if (GUILayout.Button(name, GUILayout.Width(UIConstants.BUTTON_MEDIUM_WIDTH)))
                {
                    SetMovementValues(speed, jump, climb, playerManager);
                    addToConsole($"[PLAYER] Movement preset: {name}");
                }
            }

            GUILayout.EndHorizontal(); // Fix: Ensure this is always called
        }

        private void SetMovementValues(float speed, float jump, float climb, object playerManager)
        {
            try
            {
                // Fix: Properly set ConfigEntry values
                Plugin.PluginConfig.MovementSpeedMultiplier.Value = speed;
                Plugin.PluginConfig.JumpHeightMultiplier.Value = jump;
                Plugin.PluginConfig.ClimbSpeedMultiplier.Value = climb;

                var type = playerManager.GetType();
                type.GetMethod("SetMovementSpeedMultiplier")?.Invoke(playerManager, new object[] { speed });
                type.GetMethod("SetJumpHeightMultiplier")?.Invoke(playerManager, new object[] { jump });
                type.GetMethod("SetClimbSpeedMultiplier")?.Invoke(playerManager, new object[] { climb });
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogError($"Error in SetMovementValues: {ex.Message}");
            }
        }
    }
}