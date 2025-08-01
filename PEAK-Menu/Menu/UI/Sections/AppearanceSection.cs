using UnityEngine;
using System;
using PEAK_Menu.Config;

namespace PEAK_Menu.Menu.UI.Sections
{
    public class AppearanceSection
    {
        private readonly MenuManager _menuManager;

        public AppearanceSection(MenuManager menuManager)
        {
            _menuManager = menuManager;
        }

        public void Draw(Character character, Action<string> addToConsole)
        {
            GUILayout.Space(UIConstants.STANDARD_SPACING);
            GUILayout.Label("=== Appearance & Customization ===");
            
            DrawRandomizeButton(character, addToConsole);
            DrawRainbowControls(addToConsole);
        }

        private void DrawRandomizeButton(Character character, Action<string> addToConsole)
        {
            if (GUILayout.Button("Randomize Appearance", GUILayout.Width(160)))
            {
                character.refs.customization.RandomizeCosmetics();
                addToConsole("[PLAYER] Character appearance randomized");
            }
        }

        private void DrawRainbowControls(Action<string> addToConsole)
        {
            var rainbowManager = _menuManager.GetRainbowManager();
            if (rainbowManager != null)
            {
                var isRainbowEnabled = rainbowManager.IsRainbowEnabled;
                
                if (GUILayout.Button(isRainbowEnabled ? "Rainbow Effect - ON" : "Rainbow Effect - OFF"))
                {
                    rainbowManager.ToggleRainbow();
                    addToConsole($"[PLAYER] Rainbow effect {(rainbowManager.IsRainbowEnabled ? "enabled" : "disabled")}");
                }
                
                if (isRainbowEnabled)
                {
                    DrawRainbowSpeedControls(rainbowManager, addToConsole);
                }
            }
            else
            {
                GUILayout.Label("Rainbow manager not available");
            }
        }

        private void DrawRainbowSpeedControls(object rainbowManager, Action<string> addToConsole)
        {
            GUILayout.Space(UIConstants.SMALL_SPACING);
            GUILayout.Label("Rainbow Speed:");
            GUILayout.BeginHorizontal();
            
            var speedButtons = new[]
            {
                ("Slow", 0.5f),
                ("Normal", 1.0f),
                ("Fast", 2.0f),
                ("CRAZY!", 5.0f)
            };

            foreach (var (label, speed) in speedButtons)
            {
                if (GUILayout.Button(label, GUILayout.Width(UIConstants.BUTTON_MEDIUM_WIDTH)))
                {
                    var setSpeedMethod = rainbowManager.GetType().GetMethod("SetRainbowSpeed");
                    setSpeedMethod?.Invoke(rainbowManager, new object[] { speed });
                    addToConsole($"[PLAYER] Rainbow speed: {label}");
                }
            }
            
            GUILayout.EndHorizontal();
        }
    }
}