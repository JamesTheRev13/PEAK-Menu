using UnityEngine;
using Zorro.Core.CLI;
using PEAK_Menu.Utils;
using System.Linq;

namespace PEAK_Menu.Commands.Console
{
    [ConsoleClassCustomizer("Player")]
    public static class PlayerCommands
    {
        [ConsoleCommand]
        public static void Heal(string playerName)
        {
            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[PEAK] {error}");
                return;
            }

            foreach (var character in targets)
            {
                // Clear all negative status effects
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Injury, 0f);
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Hunger, 0f);
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Cold, 0f);
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Hot, 0f);
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Poison, 0f);
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Drowsy, 0f);
                
                // Restore stamina
                character.AddStamina(1f);
                
                Debug.Log($"[PEAK] Fully healed player: {character.characterName}");
            }
        }

        [ConsoleCommand]
        public static void Kill(string playerName)
        {
            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[PEAK] {error}");
                return;
            }

            var playerManager = Plugin.Instance?._debugConsoleManager?.GetPlayerManager();
            if (playerManager == null)
            {
                Debug.LogError("[PEAK] Player manager not available");
                return;
            }

            foreach (var character in targets)
            {
                playerManager.KillPlayer(character);
                Debug.Log($"[PEAK] Killed player: {character.characterName}");
            }
        }

        [ConsoleCommand]
        public static void Revive(string playerName)
        {
            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[PEAK] {error}");
                return;
            }

            foreach (var character in targets)
            {
                if (character.data.dead || character.data.fullyPassedOut)
                {
                    try
                    {
                        Vector3 revivePos = character.Ghost != null ? character.Ghost.transform.position : character.Head;

                        if (character.photonView != null)
                        {
                            character.photonView.RPC("RPCA_ReviveAtPosition", Photon.Pun.RpcTarget.All, revivePos, true);
                        }
                        else
                        {
                            // Fallback to basic revive
                            var basicReviveMethod = typeof(Character).GetMethod("RPCA_Revive", 
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                            basicReviveMethod?.Invoke(character, new object[] { true });
                        }
                        
                        Debug.Log($"[PEAK] Revived player: {character.characterName}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[PEAK] Failed to revive {character.characterName}: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[PEAK] Player {character.characterName} is not dead or passed out");
                }
            }
        }

        [ConsoleCommand]
        public static void Teleport(string playerName, float x, float y, float z)
        {
            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[PEAK] {error}");
                return;
            }

            var targetPosition = new Vector3(x, y, z);

            foreach (var character in targets)
            {
                try
                {
                    if (character.photonView != null)
                    {
                        character.photonView.RPC("WarpPlayerRPC", Photon.Pun.RpcTarget.All, targetPosition, true);
                        Debug.Log($"[PEAK] Teleported {character.characterName} to ({x}, {y}, {z})");
                    }
                    else
                    {
                        Debug.LogError($"[PEAK] Could not teleport {character.characterName}: PhotonView not found");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[PEAK] Failed to teleport {character.characterName}: {ex.Message}");
                }
            }
        }

        [ConsoleCommand]
        public static void Goto(string playerName)
        {
            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[PEAK] {error}");
                return;
            }

            var localPlayer = Character.localCharacter;
            if (localPlayer == null)
            {
                Debug.LogError("[PEAK] Cannot teleport: Local player not found");
                return;
            }

            var target = targets.FirstOrDefault();
            if (target == null)
            {
                Debug.LogError("[PEAK] No target player found");
                return;
            }

            if (target == localPlayer)
            {
                Debug.LogWarning("[PEAK] Cannot teleport to yourself");
                return;
            }

            try
            {
                Vector3 targetPosition = target.Center + Vector3.back * 2f;

                localPlayer.photonView.RPC("WarpPlayerRPC", Photon.Pun.RpcTarget.All, targetPosition, true);
                Debug.Log($"[PEAK] Teleported to {target.characterName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PEAK] Failed to teleport to {target.characterName}: {ex.Message}");
            }
        }

        [ConsoleCommand]
        public static void Bring(string playerName)
        {
            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[PEAK] {error}");
                return;
            }

            var localPlayer = Character.localCharacter;
            if (localPlayer == null)
            {
                Debug.LogError("[PEAK] Cannot bring player: Local player not found");
                return;
            }

            var playerManager = Plugin.Instance?._debugConsoleManager?.GetPlayerManager();
            if (playerManager == null)
            {
                Debug.LogError("[PEAK] Player manager not available");
                return;
            }

            Vector3 bringPosition = localPlayer.Center + localPlayer.data.lookDirection * 2f;

            foreach (var character in targets)
            {
                if (character == localPlayer)
                {
                    Debug.LogWarning("[PEAK] Cannot bring yourself");
                    continue;
                }

                playerManager.BringPlayer(character, bringPosition);
                Debug.Log($"[PEAK] Brought player {character.characterName} to your location");
            }
        }

        [ConsoleCommand]
        public static void GodMode(string playerName, bool enabled)
        {
            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[PEAK] {error}");
                return;
            }

            foreach (var character in targets)
            {
                if (character.IsLocal)
                {
                    var currentState = character.statusesLocked;
                    
                    // Only change if different from current state
                    if (enabled != currentState)
                    {
                        Character.LockStatuses(); // This toggles the state
                    }
                    
                    Debug.Log($"[PEAK] {(enabled ? "Enabled" : "Disabled")} god mode for: {character.characterName}");
                }
                else
                {
                    Debug.LogWarning($"[PEAK] Cannot set god mode for remote player: {character.characterName}");
                }
            }
        }

        [ConsoleCommand]
        public static void InfiniteStamina(string playerName, bool enabled)
        {
            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[PEAK] {error}");
                return;
            }

            foreach (var character in targets)
            {
                if (character.IsLocal)
                {
                    var currentState = character.infiniteStam;
                    
                    // Only change if different from current state
                    if (enabled != currentState)
                    {
                        Character.InfiniteStamina(); // This toggles the state
                    }
                    
                    Debug.Log($"[PEAK] {(enabled ? "Enabled" : "Disabled")} infinite stamina for: {character.characterName}");
                }
                else
                {
                    Debug.LogWarning($"[PEAK] Cannot set infinite stamina for remote player: {character.characterName}");
                }
            }
        }

        [ConsoleCommand]
        public static void SetHealth(string playerName, float healthLevel)
        {
            if (healthLevel < 0f || healthLevel > 1f)
            {
                Debug.LogError("[PEAK] Health level must be between 0 and 1");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[PEAK] {error}");
                return;
            }

            var injuryLevel = 1f - healthLevel;

            foreach (var character in targets)
            {
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Injury, injuryLevel);
                Debug.Log($"[PEAK] Set {character.characterName}'s health to {healthLevel * 100:F0}%");
            }
        }

        [ConsoleCommand]
        public static void SetStamina(string playerName, float staminaLevel)
        {
            if (staminaLevel < 0f || staminaLevel > 1f)
            {
                Debug.LogError("[PEAK] Stamina level must be between 0 and 1");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[PEAK] {error}");
                return;
            }

            foreach (var character in targets)
            {
                character.data.currentStamina = staminaLevel;
                character.ClampStamina();
                Debug.Log($"[PEAK] Set {character.characterName}'s stamina to {staminaLevel * 100:F0}%");
            }
        }

        [ConsoleCommand]
        public static void SetHunger(string playerName, float hungerLevel)
        {
            if (hungerLevel < 0f || hungerLevel > 1f)
            {
                Debug.LogError("[PEAK] Hunger level must be between 0 and 1");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[PEAK] {error}");
                return;
            }

            foreach (var character in targets)
            {
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Hunger, hungerLevel);
                Debug.Log($"[PEAK] Set {character.characterName}'s hunger to {hungerLevel * 100:F0}%");
            }
        }

        [ConsoleCommand]
        public static void ClearStatus(string playerName)
        {
            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[PEAK] {error}");
                return;
            }

            foreach (var character in targets)
            {
                try
                {
                    character.refs.afflictions.ClearAllStatus(excludeCurse: false);
                    Debug.Log($"[PEAK] Cleared all status effects for: {character.characterName}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[PEAK] Failed to clear status for {character.characterName}: {ex.Message}");
                }
            }
        }

        [ConsoleCommand]
        public static void ListPlayers()
        {
            var allCharacters = Character.AllCharacters;
            if (allCharacters == null || !allCharacters.Any())
            {
                Debug.Log("[PEAK] No players found");
                return;
            }

            Debug.Log("[PEAK] === All Players ===");
            for (int i = 0; i < allCharacters.Count; i++)
            {
                var character = allCharacters[i];
                if (character == null) continue;

                var status = character.data.dead ? "DEAD" : 
                           character.data.passedOut ? "PASSED OUT" : 
                           character.data.fullyPassedOut ? "UNCONSCIOUS" : "ALIVE";
                
                var health = (1f - character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury)) * 100f;
                var stamina = character.GetTotalStamina() * 100f;
                
                Debug.Log($"[PEAK] {i + 1}. {character.characterName} - {status}");
                Debug.Log($"[PEAK]    Health: {health:F0}% | Stamina: {stamina:F0}% | Position: {character.Center}");
            }
        }

        [ConsoleCommand]
        public static void EmergencyHealAll()
        {
            var allCharacters = Character.AllCharacters;
            if (allCharacters == null || !allCharacters.Any())
            {
                Debug.Log("[PEAK] No players found to heal");
                return;
            }

            int healedCount = 0;
            foreach (var character in allCharacters)
            {
                if (character == null) continue;

                try
                {
                    // Full heal
                    character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Injury, 0f);
                    character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Hunger, 0f);
                    character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Cold, 0f);
                    character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Hot, 0f);
                    character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Poison, 0f);
                    character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Drowsy, 0f);
                    character.AddStamina(1f);
                    
                    // Revive if dead
                    if (character.data.dead || character.data.fullyPassedOut)
                    {
                        if (character.photonView != null)
                        {
                            Vector3 revivePos = character.Ghost != null ? character.Ghost.transform.position : character.Head;
                            character.photonView.RPC("RPCA_ReviveAtPosition", Photon.Pun.RpcTarget.All, revivePos, true);
                        }
                    }
                    
                    healedCount++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[PEAK] Failed to heal {character.characterName}: {ex.Message}");
                }
            }

            Debug.Log($"[PEAK] Emergency heal completed: {healedCount} players healed");
        }
    }
}