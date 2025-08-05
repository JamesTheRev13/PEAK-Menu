using UnityEngine;
using Zorro.Core.CLI;

namespace PEAK_Menu.Commands.Console
{
    [ConsoleClassCustomizer("Customization")]
    public static class CustomizationCommands
    {
        [ConsoleCommand]
        public static void Rainbow(bool enabled)
        {
            var rainbowManager = Plugin.Instance?._debugConsoleManager?.GetRainbowManager();
            if (rainbowManager == null)
            {
                Debug.LogError("[PEAK] Rainbow manager not available");
                return;
            }

            if (enabled)
            {
                rainbowManager.EnableRainbow();
            }
            else
            {
                rainbowManager.DisableRainbow();
            }
            Debug.Log($"[PEAK] Rainbow effect {(enabled ? "enabled" : "disabled")}");
        }

        [ConsoleCommand]
        public static void Randomize()
        {
            try
            {
                CharacterCustomization.Randomize();
                Debug.Log("[PEAK] Character appearance randomized");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PEAK] Failed to randomize appearance: {ex.Message}");
            }
        }
    }
}