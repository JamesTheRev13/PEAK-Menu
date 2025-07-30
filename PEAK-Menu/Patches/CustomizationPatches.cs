using HarmonyLib;
using UnityEngine;

namespace PEAK_Menu.Patches
{
    [HarmonyPatch]
    public class CustomizationPatches
    {
        // Patch to log customization changes
        [HarmonyPatch(typeof(CharacterCustomization), "OnPlayerDataChange")]
        [HarmonyPostfix]
        public static void OnPlayerDataChange_Postfix(CharacterCustomization __instance, PersistentPlayerData playerData)
        {
            if (Plugin.PluginConfig?.EnableDebugMode?.Value == true)
            {
                Plugin.Log?.LogInfo($"Character customization updated for player");
            }
        }

        // Patch to allow unlimited customization changes - simplified version
        [HarmonyPatch(typeof(CharacterCustomization), "SetCharacterSkinColor")]
        [HarmonyPrefix]
        public static bool SetCharacterSkinColor_Prefix(int index)
        {
            // Simple bounds check without accessing Singleton directly
            if (index < 0)
            {
                Plugin.Log?.LogWarning($"Skin index {index} is negative");
                return false; // Skip original method
            }
            
            // Let the original method handle its own bounds checking
            // We're just adding extra logging and validation here
            if (Plugin.PluginConfig?.EnableDebugMode?.Value == true)
            {
                Plugin.Log?.LogInfo($"Setting character skin color to index: {index}");
            }
            
            return true; // Run original method
        }

        // Alternative approach - patch the bounds check itself
        [HarmonyPatch(typeof(CharacterCustomization), "SetCharacterSkinColor")]
        [HarmonyPostfix]
        public static void SetCharacterSkinColor_Postfix(int index)
        {
            if (Plugin.PluginConfig?.EnableDebugMode?.Value == true)
            {
                Plugin.Log?.LogInfo($"Character skin color set to index: {index}");
            }
        }
    }
}