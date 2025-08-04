using UnityEngine;
using System;
using PEAK_Menu.Config;
using PEAK_Menu.Utils;
using PEAK_Menu.Menu.UI.Tabs;

namespace PEAK_Menu.Menu.UI.Sections
{
    public class AdminFeaturesSection
    {
        private readonly PlayerTab _parentTab;

        public AdminFeaturesSection(PlayerTab parentTab)
        {
            _parentTab = parentTab;
        }

        public void Draw(Character character, Action<string> addToConsole)
        {
            GUILayout.Space(UIConstants.STANDARD_SPACING);
            GUILayout.Label("=== Admin Features ===");
            
            DrawGodModeToggle(character, addToConsole);
            DrawInfiniteStaminaToggle(character, addToConsole);
            DrawTeleportToPingToggle(addToConsole);
            DrawDebugConsoleToggle(addToConsole);
            DrawNoClipControls(addToConsole);
            DrawAdminHealButton(character, addToConsole);
        }

        private void DrawGodModeToggle(Character character, Action<string> addToConsole)
        {
            var isGodModeEnabled = character.statusesLocked;
            if (_parentTab.DrawToggleButtonWithStatus("God Mode", isGodModeEnabled, 
                UIConstants.BUTTON_TOGGLE_WIDTH, UIConstants.STATUS_LABEL_WIDTH, 301))
            {
                AdminUIHelper.ExecuteQuickAction("god-mode", character.characterName);
                addToConsole($"[PLAYER] God mode {(!isGodModeEnabled ? "enabled" : "disabled")}");
            }
        }

        private void DrawInfiniteStaminaToggle(Character character, Action<string> addToConsole)
        {
            var isInfiniteStamEnabled = character.infiniteStam;
            if (_parentTab.DrawToggleButtonWithStatus("Infinite Stamina", isInfiniteStamEnabled, 
                160, 140, 302))
            {
                AdminUIHelper.ExecuteQuickAction("infinite-stamina", character.characterName);
                addToConsole($"[PLAYER] Infinite stamina {(!isInfiniteStamEnabled ? "enabled" : "disabled")}");
            }
        }

        private void DrawTeleportToPingToggle(Action<string> addToConsole)
        {
            var isTeleportToPingEnabled = Plugin.PluginConfig?.TeleportToPingEnabled?.Value ?? false;
            if (_parentTab.DrawToggleButton("Teleport to Ping", isTeleportToPingEnabled, 0, 305))
            {
                Plugin.PluginConfig.TeleportToPingEnabled.Value = !isTeleportToPingEnabled;
                addToConsole($"[PLAYER] Teleport to ping {(!isTeleportToPingEnabled ? "enabled" : "disabled")}");
                
                if (!isTeleportToPingEnabled)
                {
                    addToConsole("[INFO] You will now teleport to locations when you ping them");
                    addToConsole("[INFO] Hold ping key and click to place marker and teleport");
                }
                else
                {
                    addToConsole("[INFO] Ping will work normally without teleporting");
                }
            }
        }

        private void DrawDebugConsoleToggle(Action<string> addToConsole)
        {
            var debugConsoleManager = Plugin.Instance._menuManager.GetDebugConsoleManager();
            if (debugConsoleManager != null)
            {
                var isDebugOpen = debugConsoleManager.IsDebugConsoleOpen;
                var buttonColor = GUI.backgroundColor;
                GUI.backgroundColor = isDebugOpen ? Color.red : Color.green;
                
                if (GUILayout.Button(isDebugOpen ? "Close Debug Console" : "Open Debug Console", GUILayout.Width(180)))
                {
                    debugConsoleManager.ToggleDebugConsole();
                    addToConsole($"[ADMIN] Debug console {(debugConsoleManager.IsDebugConsoleOpen ? "opened" : "closed")}");
                    
                    if (debugConsoleManager.IsDebugConsoleOpen)
                    {
                        addToConsole($"[INFO] Or use {Plugin.PluginConfig.DebugConsoleToggleKey.Value} hotkey to toggle");
                        addToConsole("[INFO] Access full game console with command history & autocomplete");
                    }
                }
                
                GUI.backgroundColor = buttonColor;
                
                // Status indicator
                GUILayout.BeginHorizontal();
                var statusColor = isDebugOpen ? Color.green : Color.gray;
                var originalColor = GUI.color;
                GUI.color = statusColor;
                GUILayout.Label($"Status: {(isDebugOpen ? "OPEN" : "CLOSED")}", GUILayout.Width(100));
                GUI.color = originalColor;
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Debug Console Manager not available", GUILayout.Width(200));
            }
        }

        private void DrawNoClipControls(Action<string> addToConsole)
        {
            var noClipSection = new NoClipControlsSection(_parentTab);
            noClipSection.Draw(addToConsole);
        }

        private void DrawAdminHealButton(Character character, Action<string> addToConsole)
        {
            if (GUILayout.Button("Full Self Heal (Admin)", GUILayout.Width(160)))
            {
                AdminUIHelper.ExecuteQuickAction("heal", character.characterName);
                addToConsole("[PLAYER] Admin self heal executed");
            }
        }
    }
}