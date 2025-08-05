using UnityEngine;
using System;
using PEAK_Menu.Config;
using PEAK_Menu.Menu.UI.Tabs;
using PEAK_Menu.Utils;

namespace PEAK_Menu.Menu.UI.Sections
{
    public class NoClipControlsSection
    {
        private readonly PlayerTab _parentTab;

        public NoClipControlsSection(PlayerTab parentTab)
        {
            _parentTab = parentTab;
        }

        public void Draw(Action<string> addToConsole)
        {
            GUILayout.Space(UIConstants.STANDARD_SPACING);
            GUILayout.Label("=== NoClip Controls ===");

            // Get noClip manager from debug console manager
            var noClipManager = Plugin.Instance?._debugConsoleManager?.GetNoClipManager();
            if (noClipManager != null)
            {
                DrawNoClipToggle(noClipManager, addToConsole);
                DrawNoClipSettings(noClipManager, addToConsole);
            }
            else
            {
                GUILayout.Label("NoClip manager not available");
            }
        }

        private void DrawNoClipToggle(NoClipManager noClipManager, Action<string> addToConsole)
        {
            var isEnabled = noClipManager.IsNoClipEnabled;
            
            if (_parentTab.DrawToggleButtonWithStatus("NoClip", isEnabled, 
                UIConstants.BUTTON_LARGE_WIDTH, 150, 303))
            {
                noClipManager.ToggleNoClip();
                addToConsole($"[PLAYER] NoClip {(!isEnabled ? "enabled" : "disabled")}");
            }
            
            var hotkeyText = Plugin.PluginConfig?.NoClipToggleKey?.Value.ToString() ?? "Delete";
            GUILayout.Label($"Hotkey: {hotkeyText}");
            
            if (isEnabled)
            {
                DrawNoClipControls(noClipManager, addToConsole);
            }
        }

        private void DrawNoClipSettings(NoClipManager noClipManager, Action<string> addToConsole)
        {
            var isEnabled = noClipManager.IsNoClipEnabled;
            
            if (!isEnabled) return;
            
            GUILayout.Space(UIConstants.SMALL_SPACING);
            GUILayout.Label("NoClip Force Controls:");
            
            DrawForceControl(noClipManager, addToConsole);
            DrawSprintControl(noClipManager, addToConsole);
            DrawNoClipPresets(noClipManager, addToConsole);
        }

        private void DrawNoClipControls(NoClipManager noClipManager, Action<string> addToConsole)
        {
            GUILayout.Space(UIConstants.SMALL_SPACING);
            GUILayout.Label("Controls: WASD + Space/Ctrl + Shift");
        }

        private void DrawForceControl(NoClipManager noClipManager, Action<string> addToConsole)
        {
            var currentForce = noClipManager.VerticalForce;
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Base Force: {currentForce:F0}", GUILayout.Width(UIConstants.BUTTON_LARGE_WIDTH));
            
            if (GUILayout.Button("-", GUILayout.Width(UIConstants.BUTTON_SMALL_WIDTH)))
            {
                var newForce = Mathf.Max(UIConstants.NOCLIP_FORCE_MIN, currentForce - 100f);
                noClipManager.SetVerticalForce(newForce);
                addToConsole($"[PLAYER] NoClip base force: {newForce:F0}");
            }
            if (GUILayout.Button("+", GUILayout.Width(UIConstants.BUTTON_SMALL_WIDTH)))
            {
                var newForce = Mathf.Min(UIConstants.NOCLIP_FORCE_MAX, currentForce + 100f);
                noClipManager.SetVerticalForce(newForce);
                addToConsole($"[PLAYER] NoClip base force: {newForce:F0}");
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSprintControl(NoClipManager noClipManager, Action<string> addToConsole)
        {
            var currentSprint = noClipManager.SprintMultiplier;
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Sprint Mult: {currentSprint:F1}x", GUILayout.Width(UIConstants.BUTTON_LARGE_WIDTH));
            
            if (GUILayout.Button("-", GUILayout.Width(UIConstants.BUTTON_SMALL_WIDTH)))
            {
                var newMult = Mathf.Max(UIConstants.NOCLIP_SPRINT_MIN, currentSprint - 0.5f);
                noClipManager.SetSprintMultiplier(newMult);
                addToConsole($"[PLAYER] NoClip sprint multiplier: {newMult:F1}x");
            }
            if (GUILayout.Button("+", GUILayout.Width(UIConstants.BUTTON_SMALL_WIDTH)))
            {
                var newMult = Mathf.Min(UIConstants.NOCLIP_SPRINT_MAX, currentSprint + 0.5f);
                noClipManager.SetSprintMultiplier(newMult);
                addToConsole($"[PLAYER] NoClip sprint multiplier: {newMult:F1}x");
            }
            GUILayout.EndHorizontal();
        }

        private void DrawNoClipPresets(NoClipManager noClipManager, Action<string> addToConsole)
        {
            GUILayout.Space(UIConstants.SMALL_SPACING);
            GUILayout.Label("Presets:");
            GUILayout.BeginHorizontal();
            
            var presets = new[]
            {
                ("Slow", 400f, 2f),
                ("Normal", 800f, 4f),
                ("Fast", 1200f, 6f),
                ("Turbo", 1600f, 8f)
            };

            foreach (var (name, force, sprint) in presets)
            {
                if (GUILayout.Button(name, GUILayout.Width(50)))
                {
                    noClipManager.SetVerticalForce(force);
                    noClipManager.SetSprintMultiplier(sprint);
                    addToConsole($"[PLAYER] NoClip preset: {name}");
                }
            }
            
            GUILayout.EndHorizontal();
        }
    }
}