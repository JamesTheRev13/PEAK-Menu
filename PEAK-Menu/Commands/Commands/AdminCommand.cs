using PEAK_Menu.Utils;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PEAK_Menu.Commands
{
    public class AdminCommand : BaseCommand
    {
        public override string Name => "admin";
        public override string Description => "Administrative commands for game moderation";

        public override string DetailedHelp =>
@"=== ADMIN Command Help ===
Administrative commands for game moderation and assistance

Usage: admin <option> [player] [value]

Player Management:
  heal <player>         - Fully heal a player
  revive <player>       - Revive a dead player at safe location
  rescue <player>       - Teleport player to you for rescue
  goto <player>         - Teleport to a player's location
  bring <player>        - Teleport player to your location  
  kill <player>         - Kill a player instantly

Status Management:
  hunger <player> <0-1>     - Set hunger level (0=none, 1=max)
  stamina <player> <0-1>    - Set stamina level
  health <player> <0-1>     - Set health level
  clear-status <player>     - Clear all negative status effects

Movement Tools:
  noclip [on/off]           - Toggle noclip mode (fly through walls)
  noclip speed <value>      - Set noclip speed (100-2000)
  noclip fast <value>       - Set noclip fast speed (1-10)
  teleport-coords <x> <y> <z> - Teleport to coordinates

Moderation Tools:
  infinite-stamina <player> [on/off] - Toggle infinite stamina
  god-mode <player> [on/off]         - Toggle god mode (no damage)
  no-hunger <player> [on/off]        - Toggle hunger immunity
  spectate <player>                  - Enter spectator mode for player

Utility:
  list-players          - Show all players with detailed status
  emergency-heal-all    - Emergency heal all players
  reset-world          - Reset environmental conditions

Examples:
  admin heal player1
  admin kill ""John Doe""
  admin bring player1
  admin noclip on
  admin noclip speed 1500
  admin teleport-coords 100 50 200
  admin infinite-stamina player1 on
  admin clear-status all
  admin list-players

Note: Use 'all' as player name to affect all players";

        public override void Execute(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                LogError("Missing admin command");
                LogInfo("Use 'help admin' for available options");
                return;
            }

            var parsed = ParameterParser.ParseSubCommand(parameters);
            
            switch (parsed.Action)
            {
                case "heal":
                    HandleHealCommand(parsed);
                    break;
                case "revive":
                    HandleReviveCommand(parsed);
                    break;
                case "rescue":
                    HandleRescueCommand(parsed);
                    break;
                case "goto":
                    HandleGotoCommand(parsed);
                    break;
                case "hunger":
                    HandleHungerCommand(parsed);
                    break;
                case "stamina":
                    HandleStaminaCommand(parsed);
                    break;
                case "health":
                    HandleHealthCommand(parsed);
                    break;
                case "clear-status":
                    HandleClearStatusCommand(parsed);
                    break;
                case "infinite-stamina":
                    HandleInfiniteStaminaCommand(parsed);
                    break;
                case "god-mode":
                    HandleGodModeCommand(parsed);
                    break;
                case "no-hunger":
                    HandleNoHungerCommand(parsed);
                    break;
                case "spectate":
                    HandleSpectateCommand(parsed);
                    break;
                case "list-players":
                    HandleListPlayersCommand();
                    break;
                case "emergency-heal-all":
                    HandleEmergencyHealAllCommand();
                    break;
                case "reset-world":
                    HandleResetWorldCommand();
                    break;
                case "noclip":
                    HandleNoClipCommand(parsed);
                    break;
                case "kill":
                    HandleKillCommand(parsed);
                    break;
                case "bring":
                    HandleBringCommand(parsed);
                    break;
                case "teleport-coords":
                    HandleTeleportCoordsCommand(parsed);
                    break;
                default:
                    LogError($"Unknown admin command: {parsed.Action}");
                    LogInfo("Use 'help admin' for available options");
                    break;
            }
        }

        private void HandleStaminaCommand(ParameterParser.ParsedParameters parsed)
        {
            if (string.IsNullOrEmpty(parsed.PlayerName))
            {
                LogError("Usage: admin stamina <player> <0-1>");
                return;
            }

            if (!ParameterParser.ValidateNumericRange(parsed.NumericValue, 0f, 1f, out string error))
            {
                LogError($"Invalid stamina level: {error}");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string playerError);
            if (!string.IsNullOrEmpty(playerError))
            {
                LogError(playerError);
                return;
            }

            var staminaLevel = parsed.NumericValue.Value;

            foreach (var character in targets)
            {
                character.data.currentStamina = staminaLevel;
                character.ClampStamina();
                LogInfo($"Set {character.characterName}'s stamina to {staminaLevel * 100:F0}%");
            }
        }

        private void HandleHealthCommand(ParameterParser.ParsedParameters parsed)
        {
            if (string.IsNullOrEmpty(parsed.PlayerName))
            {
                LogError("Usage: admin health <player> <0-1>");
                return;
            }

            if (!ParameterParser.ValidateNumericRange(parsed.NumericValue, 0f, 1f, out string error))
            {
                LogError($"Invalid health level: {error}");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string playerError);
            if (!string.IsNullOrEmpty(playerError))
            {
                LogError(playerError);
                return;
            }

            var healthLevel = parsed.NumericValue.Value;
            var injuryLevel = 1f - healthLevel;

            foreach (var character in targets)
            {
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Injury, injuryLevel);
                LogInfo($"Set {character.characterName}'s health to {healthLevel * 100:F0}%");
            }
        }

        private void HandleHungerCommand(ParameterParser.ParsedParameters parsed)
        {
            if (string.IsNullOrEmpty(parsed.PlayerName))
            {
                LogError("Usage: admin hunger <player> <0-1>");
                return;
            }

            if (!ParameterParser.ValidateNumericRange(parsed.NumericValue, 0f, 1f, out string error))
            {
                LogError($"Invalid hunger level: {error}");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string playerError);
            if (!string.IsNullOrEmpty(playerError))
            {
                LogError(playerError);
                return;
            }

            var hungerLevel = parsed.NumericValue.Value;

            foreach (var character in targets)
            {
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Hunger, hungerLevel);
                LogInfo($"Set {character.characterName}'s hunger to {hungerLevel * 100:F0}%");
            }
        }

        private void HandleInfiniteStaminaCommand(ParameterParser.ParsedParameters parsed)
        {
            if (string.IsNullOrEmpty(parsed.PlayerName))
            {
                LogError("Usage: admin infinite-stamina <player> [on/off]");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string playerError);
            if (!string.IsNullOrEmpty(playerError))
            {
                LogError(playerError);
                return;
            }

            // Default to enabling if no boolean specified
            bool enable = parsed.BooleanValue ?? true;

            foreach (var character in targets)
            {
                if (character.IsLocal)
                {
                    var currentState = character.infiniteStam;
                    if (enable != currentState)
                    {
                        Character.InfiniteStamina(); // This toggles the state
                    }
                    LogInfo($"{(enable ? "Enabled" : "Disabled")} infinite stamina for: {character.characterName}");
                }
                else
                {
                    LogWarning($"Cannot set infinite stamina for remote player: {character.characterName}");
                    LogInfo("This feature only works for the local player");
                }
            }
        }

        private void HandleGodModeCommand(ParameterParser.ParsedParameters parsed)
        {
            if (string.IsNullOrEmpty(parsed.PlayerName))
            {
                LogError("Usage: admin god-mode <player> [on/off]");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string playerError);
            if (!string.IsNullOrEmpty(playerError))
            {
                LogError(playerError);
                return;
            }

            bool enable = parsed.BooleanValue ?? true;

            foreach (var character in targets)
            {
                if (character.IsLocal)
                {
                    var currentState = character.statusesLocked;
                    if (enable != currentState)
                    {
                        Character.LockStatuses(); // This toggles the state
                    }
                    LogInfo($"{(enable ? "Enabled" : "Disabled")} god mode for: {character.characterName}");
                }
                else
                {
                    LogWarning($"Cannot set god mode for remote player: {character.characterName}");
                    LogInfo("This feature only works for the local player");
                }
            }
        }

        private void HandleHealCommand(ParameterParser.ParsedParameters parsed)
        {
            if (string.IsNullOrEmpty(parsed.PlayerName))
            {
                LogError("Usage: admin heal <player>");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                LogError(error);
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
                
                LogInfo($"Fully healed player: {character.characterName}");
            }
        }

        private void HandleReviveCommand(ParameterParser.ParsedParameters parsed)
        {
            if (string.IsNullOrEmpty(parsed.PlayerName))
            {
                LogError("Usage: admin revive <player>");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                LogError(error);
                return;
            }

            foreach (var character in targets)
            {
                if (character.data.dead || character.data.fullyPassedOut)
                {
                    try
                    {
                        // Get safe revive position near local player
                        var localPlayer = Character.localCharacter;
                        Vector3 revivePos = localPlayer != null ? localPlayer.Center + Vector3.forward * 2f : character.Center;
                        
                        // Use reflection to call RPCA_ReviveAtPosition if available
                        var reviveMethod = typeof(Character).GetMethod("RPCA_ReviveAtPosition", 
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        
                        if (reviveMethod != null)
                        {
                            reviveMethod.Invoke(character, new object[] { revivePos, true });
                        }
                        else
                        {
                            // Fallback to basic revive
                            var basicReviveMethod = typeof(Character).GetMethod("RPCA_Revive", 
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                            
                            if (basicReviveMethod != null)
                            {
                                basicReviveMethod.Invoke(character, new object[] { true });
                            }
                        }
                        
                        LogInfo($"Revived player: {character.characterName}");
                    }
                    catch (System.Exception ex)
                    {
                        LogError($"Failed to revive {character.characterName}: {ex.Message}");
                    }
                }
                else
                {
                    LogWarning($"Player {character.characterName} is not dead or passed out");
                }
            }
        }

        private void HandleRescueCommand(ParameterParser.ParsedParameters parsed)
        {
            if (string.IsNullOrEmpty(parsed.PlayerName))
            {
                LogError("Usage: admin rescue <player>");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                LogError(error);
                return;
            }

            var localPlayer = Character.localCharacter;
            if (localPlayer == null)
            {
                LogError("Cannot rescue: Local player not found");
                return;
            }

            Vector3 rescuePosition = localPlayer.Center + localPlayer.data.lookDirection * 3f;

            foreach (var character in targets)
            {
                if (character == localPlayer)
                {
                    LogWarning("Cannot rescue yourself");
                    continue;
                }

                try
                {
                    // Teleport player to rescue position
                    var warpMethod = typeof(Character).GetMethod("WarpPlayerRPC", 
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    
                    if (warpMethod != null)
                    {
                        warpMethod.Invoke(character, new object[] { rescuePosition, true });
                        LogInfo($"Rescued player {character.characterName} to your location");
                    }
                    else
                    {
                        LogError($"Could not rescue {character.characterName}: Teleport method not found");
                    }
                }
                catch (System.Exception ex)
                {
                    LogError($"Failed to rescue {character.characterName}: {ex.Message}");
                }
            }
        }

        private void HandleGotoCommand(ParameterParser.ParsedParameters parsed)
        {
            if (string.IsNullOrEmpty(parsed.PlayerName))
            {
                LogError("Usage: admin goto <player>");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                LogError(error);
                return;
            }

            var localPlayer = Character.localCharacter;
            if (localPlayer == null)
            {
                LogError("Cannot teleport: Local player not found");
                return;
            }

            var target = targets.FirstOrDefault();
            if (target == null)
            {
                LogError("No target player found");
                return;
            }

            if (target == localPlayer)
            {
                LogWarning("Cannot teleport to yourself");
                return;
            }

            try
            {
                Vector3 targetPosition = target.Center + Vector3.back * 2f; // Position slightly behind target
                
                var warpMethod = typeof(Character).GetMethod("WarpPlayerRPC", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                
                if (warpMethod != null)
                {
                    warpMethod.Invoke(localPlayer, new object[] { targetPosition, true });
                    LogInfo($"Teleported to {target.characterName}");
                }
                else
                {
                    LogError("Could not teleport: Teleport method not found");
                }
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to teleport to {target.characterName}: {ex.Message}");
            }
        }

        private void HandleClearStatusCommand(ParameterParser.ParsedParameters parsed)
        {
            if (string.IsNullOrEmpty(parsed.PlayerName))
            {
                LogError("Usage: admin clear-status <player>");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                LogError(error);
                return;
            }

            foreach (var character in targets)
            {
                try
                {
                    character.refs.afflictions.ClearAllStatus(excludeCurse: false);
                    LogInfo($"Cleared all status effects for: {character.characterName}");
                }
                catch (System.Exception ex)
                {
                    LogError($"Failed to clear status for {character.characterName}: {ex.Message}");
                }
            }
        }

        private void HandleNoHungerCommand(ParameterParser.ParsedParameters parsed)
        {
            LogWarning("No-hunger command is not implemented yet");
            LogInfo("This feature would require additional game modifications");
        }

        private void HandleSpectateCommand(ParameterParser.ParsedParameters parsed)
        {
            LogWarning("Spectate command is not implemented yet");
            LogInfo("This feature would require camera system modifications");
        }

        private void HandleListPlayersCommand()
        {
            var allCharacters = Character.AllCharacters;
            if (allCharacters == null || !allCharacters.Any())
            {
                LogInfo("No players found");
                return;
            }

            LogInfo("=== All Players ===");
            for (int i = 0; i < allCharacters.Count; i++)
            {
                var character = allCharacters[i];
                if (character == null) continue;

                var status = character.data.dead ? "DEAD" : 
                           character.data.passedOut ? "PASSED OUT" : 
                           character.data.fullyPassedOut ? "UNCONSCIOUS" : "ALIVE";
                
                var health = (1f - character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury)) * 100f;
                var stamina = character.GetTotalStamina() * 100f;
                
                LogInfo($"{i + 1}. {character.characterName} - {status}");
                LogInfo($"   Health: {health:F0}% | Stamina: {stamina:F0}% | Position: {character.Center}");
            }
        }

        private void HandleEmergencyHealAllCommand()
        {
            var allCharacters = Character.AllCharacters;
            if (allCharacters == null || !allCharacters.Any())
            {
                LogInfo("No players found to heal");
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
                        var reviveMethod = typeof(Character).GetMethod("RPCA_Revive", 
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        
                        if (reviveMethod != null)
                        {
                            reviveMethod.Invoke(character, new object[] { true });
                        }
                    }
                    
                    healedCount++;
                }
                catch (System.Exception ex)
                {
                    LogError($"Failed to heal {character.characterName}: {ex.Message}");
                }
            }

            LogInfo($"Emergency heal completed: {healedCount} players healed");
        }

        private void HandleResetWorldCommand()
        {
            // TODO
            LogWarning("Reset-world command is not implemented yet");
        }

        private void HandleNoClipCommand(ParameterParser.ParsedParameters parsed)
        {
            var noClipManager = Plugin.Instance?._menuManager?.GetNoClipManager();
            if (noClipManager == null)
            {
                LogError("NoClip manager not available");
                return;
            }

            if (parsed.RemainingParameters.Length == 0)
            {
                // Just "admin noclip" - toggle
                noClipManager.ToggleNoClip();
                LogInfo($"NoClip {(noClipManager.IsNoClipEnabled ? "enabled" : "disabled")}");
                return;
            }

            var subCommand = parsed.RemainingParameters[0].ToLower();
            
            switch (subCommand)
            {
                case "on":
                case "enable":
                case "true":
                case "1":
                    if (!noClipManager.IsNoClipEnabled)
                    {
                        noClipManager.EnableNoClip();
                        LogInfo("NoClip enabled");
                    }
                    else
                    {
                        LogInfo("NoClip already enabled");
                    }
                    break;
                    
                case "off":
                case "disable":
                case "false":
                case "0":
                    if (noClipManager.IsNoClipEnabled)
                    {
                        noClipManager.DisableNoClip();
                        LogInfo("NoClip disabled");
                    }
                    else
                    {
                        LogInfo("NoClip already disabled");
                    }
                    break;
                    
                case "speed":
                    if (parsed.RemainingParameters.Length < 2)
                    {
                        LogError("Usage: admin noclip speed <value>");
                        LogInfo($"Current speed: {noClipManager.VerticalForce:F1}");
                        return;
                    }
                    
                    if (float.TryParse(parsed.RemainingParameters[1], out float speed))
                    {
                        noClipManager.SetVerticalForce(speed);
                        LogInfo($"NoClip speed set to: {speed:F1}");
                    }
                    else
                    {
                        LogError("Invalid speed value. Use a number between 100-2000");
                    }
                    break;
                    
                case "fast":
                    if (parsed.RemainingParameters.Length < 2)
                    {
                        LogError("Usage: admin noclip fast <value>");
                        LogInfo($"Current fast speed: {noClipManager.SprintMultiplier:F1}x");
                        return;
                    }
                    
                    if (float.TryParse(parsed.RemainingParameters[1], out float fastMult))
                    {
                        noClipManager.SetSprintMultiplier(fastMult);
                        LogInfo($"NoClip fast multiplier set to: {fastMult:F1}x");
                    }
                    else
                    {
                        LogError("Invalid multiplier value. Use a number between 1-10");
                    }
                    break;
                    
                case "status":
                case "info":
                    LogInfo($"=== NoClip Status ===");
                    LogInfo($"Enabled: {noClipManager.IsNoClipEnabled}");
                    LogInfo($"Base Force: {noClipManager.VerticalForce:F1}");
                    LogInfo($"Sprint Multiplier: {noClipManager.SprintMultiplier:F1}x");
                    LogInfo($"Controls: WASD to move, Space/Ctrl for up/down, Shift for fast mode");
                    break;
                    
                default:
                    LogError($"Unknown noclip command: {subCommand}");
                    LogInfo("Available options: on, off, speed <value>, fast <value>, status");
                    break;
            }
        }

        private void HandleKillCommand(ParameterParser.ParsedParameters parsed)
        {
            if (string.IsNullOrEmpty(parsed.PlayerName))
            {
                LogError("Usage: admin kill <player>");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                LogError(error);
                return;
            }

            var playerManager = Plugin.Instance?._menuManager?.GetPlayerManager();
            if (playerManager == null)
            {
                LogError("Player manager not available");
                return;
            }

            foreach (var character in targets)
            {
                playerManager.KillPlayer(character);
                LogInfo($"Killed player: {character.characterName}");
            }
        }

        private void HandleBringCommand(ParameterParser.ParsedParameters parsed)
        {
            if (string.IsNullOrEmpty(parsed.PlayerName))
            {
                LogError("Usage: admin bring <player>");
                return;
            }

            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                LogError(error);
                return;
            }

            var localPlayer = Character.localCharacter;
            if (localPlayer == null)
            {
                LogError("Cannot bring player: Local player not found");
                return;
            }

            var playerManager = Plugin.Instance?._menuManager?.GetPlayerManager();
            if (playerManager == null)
            {
                LogError("Player manager not available");
                return;
            }

            Vector3 bringPosition = localPlayer.Center + localPlayer.data.lookDirection * 2f;

            foreach (var character in targets)
            {
                if (character == localPlayer)
                {
                    LogWarning("Cannot bring yourself");
                    continue;
                }

                playerManager.BringPlayer(character, bringPosition);
                LogInfo($"Brought player {character.characterName} to your location");
            }
        }

        private void HandleTeleportCoordsCommand(ParameterParser.ParsedParameters parsed)
        {
            if (parsed.RemainingParameters.Length < 3)
            {
                LogError("Usage: admin teleport-coords <x> <y> <z>");
                return;
            }

            if (!float.TryParse(parsed.RemainingParameters[0], out float x) ||
                !float.TryParse(parsed.RemainingParameters[1], out float y) ||
                !float.TryParse(parsed.RemainingParameters[2], out float z))
            {
                LogError("Invalid coordinates. Please provide numeric values.");
                return;
            }

            var localPlayer = Character.localCharacter;
            if (localPlayer == null)
            {
                LogError("Cannot teleport: Local player not found");
                return;
            }

            try
            {
                Vector3 targetPosition = new Vector3(x, y, z);
                var warpMethod = typeof(Character).GetMethod("WarpPlayerRPC",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (warpMethod != null)
                {
                    warpMethod.Invoke(localPlayer, new object[] { targetPosition, true });
                    LogInfo($"Teleported to coordinates: {targetPosition}");
                }
                else
                {
                    LogError("Could not teleport: Teleport method not found");
                }
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to teleport to coordinates: {ex.Message}");
            }
        }

        public override bool CanExecute()
        {
            return Character.localCharacter != null;
        }
    }
}