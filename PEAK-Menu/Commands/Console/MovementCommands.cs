using UnityEngine;
using Zorro.Core.CLI;

namespace PEAK_Menu.Commands.Console
{
    [ConsoleClassCustomizer("Movement")]
    public static class MovementCommands
    {
        [ConsoleCommand]
        public static void NoClip(bool enabled)
        {
            var noClipManager = Plugin.Instance?._debugConsoleManager?.GetNoClipManager();
            if (noClipManager == null)
            {
                Debug.LogError("[PEAK] NoClip manager not available");
                return;
            }

            if (enabled && !noClipManager.IsNoClipEnabled)
            {
                noClipManager.EnableNoClip();
            }
            else if (!enabled && noClipManager.IsNoClipEnabled)
            {
                noClipManager.DisableNoClip();
            }
            Debug.Log($"[PEAK] NoClip {(enabled ? "enabled" : "disabled")}");
        }

        [ConsoleCommand]
        public static void Speed(float multiplier)
        {
            var playerManager = Plugin.Instance?._debugConsoleManager?.GetPlayerManager();
            if (playerManager == null)
            {
                Debug.LogError("[PEAK] Player manager not available");
                return;
            }

            playerManager.SetMovementSpeedMultiplier(multiplier);
            Plugin.PluginConfig.MovementSpeedMultiplier.Value = multiplier;
            Debug.Log($"[PEAK] Movement speed set to {multiplier}x");
        }

        [ConsoleCommand]
        public static void Jump(float multiplier)
        {
            var playerManager = Plugin.Instance?._debugConsoleManager?.GetPlayerManager();
            if (playerManager == null)
            {
                Debug.LogError("[PEAK] Player manager not available");
                return;
            }

            playerManager.SetJumpHeightMultiplier(multiplier);
            Plugin.PluginConfig.JumpHeightMultiplier.Value = multiplier;
            Debug.Log($"[PEAK] Jump height set to {multiplier}x");
        }

        [ConsoleCommand]
        public static void Climb(float multiplier)
        {
            var playerManager = Plugin.Instance?._debugConsoleManager?.GetPlayerManager();
            if (playerManager == null)
            {
                Debug.LogError("[PEAK] Player manager not available");
                return;
            }

            playerManager.SetClimbSpeedMultiplier(multiplier);
            Plugin.PluginConfig.ClimbSpeedMultiplier.Value = multiplier;
            Debug.Log($"[PEAK] Climb speed set to {multiplier}x");
        }

        [ConsoleCommand]
        public static void NoClipSpeed(float speed)
        {
            var noClipManager = Plugin.Instance?._debugConsoleManager?.GetNoClipManager();
            if (noClipManager == null)
            {
                Debug.LogError("[PEAK] NoClip manager not available");
                return;
            }

            noClipManager.SetNoClipSpeed(speed);
            Debug.Log($"[PEAK] NoClip speed set to: {speed:F1}");
        }

        [ConsoleCommand]
        public static void NoClipFastSpeed(float fastSpeed)
        {
            var noClipManager = Plugin.Instance?._debugConsoleManager?.GetNoClipManager();
            if (noClipManager == null)
            {
                Debug.LogError("[PEAK] NoClip manager not available");
                return;
            }

            noClipManager.SetNoClipFastSpeed(fastSpeed);
            Debug.Log($"[PEAK] NoClip fast speed set to: {fastSpeed:F1}");
        }

        [ConsoleCommand]
        public static void TeleportCoords(float x, float y, float z)
        {
            var localPlayer = Character.localCharacter;
            if (localPlayer == null)
            {
                Debug.LogError("[PEAK] Cannot teleport: Local player not found");
                return;
            }

            try
            {
                Vector3 targetPosition = new Vector3(x, y, z);
                if (localPlayer.photonView != null)
                {
                    localPlayer.photonView.RPC("WarpPlayerRPC", Photon.Pun.RpcTarget.All, targetPosition, true);
                    Debug.Log($"[PEAK] Teleported to coordinates: {targetPosition}");
                }
                else
                {
                    Debug.LogError("[PEAK] Could not teleport: PhotonView not found");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PEAK] Failed to teleport to coordinates: {ex.Message}");
            }
        }
    }
}