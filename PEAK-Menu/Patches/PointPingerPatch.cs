using HarmonyLib;
using UnityEngine;
using System;
using Photon.Pun;

namespace PEAK_Menu.Patches
{
    [HarmonyPatch]
    public class PointPingerPatch
    {
        [HarmonyPatch(typeof(PointPinger), "ReceivePoint_Rpc")]
        [HarmonyPostfix]
        static void ReceivePoint_Rpc_Postfix(Vector3 point, Vector3 hitNormal, PointPinger __instance)
        {
            try
            {
                // Check if teleport-to-ping is enabled in config
                if (!Plugin.PluginConfig?.TeleportToPingEnabled?.Value == true)
                    return;

                var owner = __instance.character?.photonView?.Owner;
                if (owner != null && owner == PhotonNetwork.LocalPlayer)
                {
                    if (Character.localCharacter != null && !Character.localCharacter.data.dead)
                    {
                        Vector3 safePoint = point + Vector3.up;
                        Character.localCharacter.photonView.RPC("WarpPlayerRPC", RpcTarget.All, new object[] {
                            safePoint, true
                        });

                        Plugin.Log?.LogInfo($"[TeleportToPing] Teleported to ping at: {safePoint}");
                        
                        // Also add to console if menu manager is available
                        Plugin.Instance?._menuManager?.AddToConsole($"[INFO] Teleported to ping at: {safePoint}");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[TeleportToPing] Exception: {ex}");
            }
        }
    }
}