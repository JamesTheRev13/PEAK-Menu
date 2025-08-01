using UnityEngine;
using System.Collections.Generic;
using PEAK_Menu.Config;

namespace PEAK_Menu.Menu.UI.Tabs
{
    public class EnvironmentTab : BaseTab
    {
        public EnvironmentTab(MenuManager menuManager, List<string> consoleOutput) 
            : base(menuManager, consoleOutput) { }

        public override void Draw()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(UIConstants.TAB_HEIGHT));
            
            GUILayout.Label("=== Environment ===");
            
            DrawDayNightInfo();
            DrawEnvironmentalSettings();
            DrawCharacterEnvironmentInfo();

            GUILayout.Space(UIConstants.LARGE_SPACING);
            GUILayout.EndScrollView();
        }

        private void DrawDayNightInfo()
        {
            if (DayNightManager.instance != null)
            {
                GUILayout.Label($"Day Progress: {DayNightManager.instance.isDay * 100:F1}%");
            }
            else
            {
                GUILayout.Label("Day/Night Manager: Not available");
            }
        }

        private void DrawEnvironmentalSettings()
        {
            GUILayout.Label($"Night Cold Active: {Ascents.isNightCold}");
            GUILayout.Label($"Hunger Rate Multiplier: {Ascents.hungerRateMultiplier:F2}");
            GUILayout.Label($"Fall Damage Multiplier: {Ascents.fallDamageMultiplier:F2}");
            GUILayout.Label($"Climb Stamina Multiplier: {Ascents.climbStaminaMultiplier:F2}");
        }

        private void DrawCharacterEnvironmentInfo()
        {
            var character = Character.localCharacter;
            if (character != null)
            {
                GUILayout.Space(UIConstants.STANDARD_SPACING);
                GUILayout.Label("=== Character Environment ===");
                GUILayout.Label($"In Fog: {character.data.isInFog}");
                GUILayout.Label($"Grounded For: {character.data.groundedFor:F1}s");
                GUILayout.Label($"Since Grounded: {character.data.sinceGrounded:F1}s");
                GUILayout.Label($"Fall Seconds: {character.data.fallSeconds:F1}s");
            }
        }
    }
}