using UnityEngine;
using System;
using PEAK_Menu.Utils;

namespace PEAK_Menu.Menu.UI
{
    public class HealthManagementSection
    {
        public void Draw(Character character, ref float statusValue, Action<string> addToConsole)
        {
            GUILayout.Label("=== Health & Status Management ===");
            
            DrawQuickActions(character, addToConsole);
            DrawAdvancedStatusControls(character, ref statusValue, addToConsole);
        }

        private void DrawQuickActions(Character character, Action<string> addToConsole)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Full Heal", GUILayout.Width(100)))
            {
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Injury, 0f);
                character.AddStamina(1f);
                addToConsole("[PLAYER] Player fully healed");
            }
            if (GUILayout.Button("Clear All Status Effects", GUILayout.Width(160)))
            {
                character.refs.afflictions.ClearAllStatus(excludeCurse: false);
                addToConsole("[PLAYER] All status effects cleared");
            }
            GUILayout.EndHorizontal();
        }

        private void DrawAdvancedStatusControls(Character character, ref float statusValue, Action<string> addToConsole)
        {
            GUILayout.Space(5);
            GUILayout.Label("Advanced Status Control:");
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Value:", GUILayout.Width(45));
            statusValue = GUILayout.HorizontalSlider(statusValue, 0f, 1f, GUILayout.Width(100));
            GUILayout.Label($"{statusValue:F2}", GUILayout.Width(40));
            GUILayout.EndHorizontal();
            
            DrawStatusButtons(character, statusValue, addToConsole);
        }

        private void DrawStatusButtons(Character character, float statusValue, Action<string> addToConsole)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Health", GUILayout.Width(80)))
            {
                AdminUIHelper.SetPlayerStatus(character.characterName, "health", statusValue);
                addToConsole($"[PLAYER] Set health to {statusValue * 100:F0}%");
            }
            if (GUILayout.Button("Set Stamina", GUILayout.Width(80)))
            {
                AdminUIHelper.SetPlayerStatus(character.characterName, "stamina", statusValue);
                addToConsole($"[PLAYER] Set stamina to {statusValue * 100:F0}%");
            }
            if (GUILayout.Button("Set Hunger", GUILayout.Width(80)))
            {
                AdminUIHelper.SetPlayerStatus(character.characterName, "hunger", statusValue);
                addToConsole($"[PLAYER] Set hunger to {statusValue * 100:F0}%");
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
    }
}