using HarmonyLib;
using System;

namespace PEAK_Menu.Patches
{
    [HarmonyPatch]
    public class NoWeightPatch
    {
        // Patch the weight calculation method to return 0 when no weight is enabled
        [HarmonyPatch(typeof(CharacterAfflictions), "UpdateWeight")]
        [HarmonyPrefix]
        public static bool UpdateWeight_Prefix(CharacterAfflictions __instance)
        {
            try
            {
                // Check if no weight is enabled and this is the local character
                var character = __instance.character;
                if (character != null && character.IsLocal)
                {
                    // Check the config directly to ensure sync with UI
                    if (Plugin.PluginConfig?.NoWeight?.Value == true)
                    {
                        // Skip the original weight update method entirely
                        if (Plugin.PluginConfig?.EnableDebugMode?.Value == true)
                        {
                            Plugin.Log?.LogDebug("[NoWeight] Skipping weight update - no weight enabled");
                        }
                        return false; // Skip original method
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[NoWeight] Error in UpdateWeight patch: {ex.Message}");
            }

            return true; // Run original method
        }
    }
}