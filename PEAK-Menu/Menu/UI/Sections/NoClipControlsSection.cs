using UnityEngine;
using System;
using PEAK_Menu.Config;
using PEAK_Menu.Menu.UI.Tabs;

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

            var noClipManager = Plugin.Instance?._menuManager?.GetNoClipManager();
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

        private void DrawNoClipToggle(object noClipManager, Action<string> addToConsole)
        {
            var isEnabledProperty = noClipManager.GetType().GetProperty("IsNoClipEnabled");
            var isEnabled = (bool)(isEnabledProperty?.GetValue(noClipManager) ?? false);
            
            if (_parentTab.DrawToggleButtonWithStatus("NoClip", isEnabled, 
                UIConstants.BUTTON_LARGE_WIDTH, 150, 303))
            {
                var toggleMethod = noClipManager.GetType().GetMethod("ToggleNoClip");
                toggleMethod?.Invoke(noClipManager, null);
                addToConsole($"[PLAYER] NoClip {(!isEnabled ? "enabled" : "disabled")}");
            }
            
            var hotkeyText = Plugin.PluginConfig?.NoClipToggleKey?.Value.ToString() ?? "Delete";
            GUILayout.Label($"Hotkey: {hotkeyText}");
            
            if (isEnabled)
            {
                DrawNoClipControls(noClipManager, addToConsole);
            }
        }

        private void DrawNoClipSettings(object noClipManager, Action<string> addToConsole)
        {
            var isEnabledProperty = noClipManager.GetType().GetProperty("IsNoClipEnabled");
            var isEnabled = (bool)(isEnabledProperty?.GetValue(noClipManager) ?? false);
            
            if (!isEnabled) return;
            
            GUILayout.Space(UIConstants.SMALL_SPACING);
            GUILayout.Label("NoClip Force Controls:");
            
            DrawForceControl(noClipManager, addToConsole);
            DrawSprintControl(noClipManager, addToConsole);
            DrawNoClipPresets(noClipManager, addToConsole);
        }

        private void DrawNoClipControls(object noClipManager, Action<string> addToConsole)
        {
            GUILayout.Space(UIConstants.SMALL_SPACING);
            GUILayout.Label("Controls: WASD + Space/Ctrl + Shift");
        }

        private void DrawForceControl(object noClipManager, Action<string> addToConsole)
        {
            var forceProperty = noClipManager.GetType().GetProperty("VerticalForce");
            var currentForce = (float)(forceProperty?.GetValue(noClipManager) ?? UIConstants.NOCLIP_FORCE_DEFAULT);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Base Force: {currentForce:F0}", GUILayout.Width(UIConstants.BUTTON_LARGE_WIDTH));
            
            if (GUILayout.Button("-", GUILayout.Width(UIConstants.BUTTON_SMALL_WIDTH)))
            {
                var newForce = Mathf.Max(UIConstants.NOCLIP_FORCE_MIN, currentForce - 100f);
                SetNoClipForce(noClipManager, newForce);
                addToConsole($"[PLAYER] NoClip base force: {newForce:F0}");
            }
            if (GUILayout.Button("+", GUILayout.Width(UIConstants.BUTTON_SMALL_WIDTH)))
            {
                var newForce = Mathf.Min(UIConstants.NOCLIP_FORCE_MAX, currentForce + 100f);
                SetNoClipForce(noClipManager, newForce);
                addToConsole($"[PLAYER] NoClip base force: {newForce:F0}");
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSprintControl(object noClipManager, Action<string> addToConsole)
        {
            var sprintProperty = noClipManager.GetType().GetProperty("SprintMultiplier");
            var currentSprint = (float)(sprintProperty?.GetValue(noClipManager) ?? UIConstants.NOCLIP_SPRINT_DEFAULT);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Sprint Mult: {currentSprint:F1}x", GUILayout.Width(UIConstants.BUTTON_LARGE_WIDTH));
            
            if (GUILayout.Button("-", GUILayout.Width(UIConstants.BUTTON_SMALL_WIDTH)))
            {
                var newMult = Mathf.Max(UIConstants.NOCLIP_SPRINT_MIN, currentSprint - 0.5f);
                SetNoClipSprint(noClipManager, newMult);
                addToConsole($"[PLAYER] NoClip sprint multiplier: {newMult:F1}x");
            }
            if (GUILayout.Button("+", GUILayout.Width(UIConstants.BUTTON_SMALL_WIDTH)))
            {
                var newMult = Mathf.Min(UIConstants.NOCLIP_SPRINT_MAX, currentSprint + 0.5f);
                SetNoClipSprint(noClipManager, newMult);
                addToConsole($"[PLAYER] NoClip sprint multiplier: {newMult:F1}x");
            }
            GUILayout.EndHorizontal();
        }

        private void DrawNoClipPresets(object noClipManager, Action<string> addToConsole)
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
                    SetNoClipForce(noClipManager, force);
                    SetNoClipSprint(noClipManager, sprint);
                    addToConsole($"[PLAYER] NoClip preset: {name}");
                }
            }
            
            GUILayout.EndHorizontal();
        }

        private void SetNoClipForce(object noClipManager, float force)
        {
            var setMethod = noClipManager.GetType().GetMethod("SetVerticalForce");
            setMethod?.Invoke(noClipManager, new object[] { force });
        }

        private void SetNoClipSprint(object noClipManager, float sprint)
        {
            var setMethod = noClipManager.GetType().GetMethod("SetSprintMultiplier");
            setMethod?.Invoke(noClipManager, new object[] { sprint });
        }
    }
}