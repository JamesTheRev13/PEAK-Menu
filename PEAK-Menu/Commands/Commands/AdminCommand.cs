using System.Linq;
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

Status Management:
  hunger <player> <0-1>     - Set hunger level (0=none, 1=max)
  stamina <player> <0-1>    - Set stamina level
  health <player> <0-1>     - Set health level
  clear-status <player>     - Clear all negative status effects

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
  admin revive ""John Doe""
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

            var command = parameters[0].ToLower();
            
            switch (command)
            {
                case "heal":
                    HandleHealCommand(parameters);
                    break;
                case "revive":
                    HandleReviveCommand(parameters);
                    break;
                case "rescue":
                    HandleRescueCommand(parameters);
                    break;
                case "goto":
                    HandleGotoCommand(parameters);
                    break;
                case "hunger":
                    HandleHungerCommand(parameters);
                    break;
                case "stamina":
                    HandleStaminaCommand(parameters);
                    break;
                case "health":
                    HandleHealthCommand(parameters);
                    break;
                case "clear-status":
                    HandleClearStatusCommand(parameters);
                    break;
                case "infinite-stamina":
                    HandleInfiniteStaminaCommand(parameters);
                    break;
                case "god-mode":
                    HandleGodModeCommand(parameters);
                    break;
                case "no-hunger":
                    HandleNoHungerCommand(parameters);
                    break;
                case "spectate":
                    HandleSpectateCommand(parameters);
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
                default:
                    LogError($"Unknown admin command: {command}");
                    LogInfo("Use 'help admin' for available options");
                    break;
            }
        }

        private void HandleHealCommand(string[] parameters)
        {
            if (parameters.Length < 2)
            {
                LogError("Usage: admin heal <player>");
                return;
            }

            var playerName = string.Join(" ", parameters.Skip(1));
            var targets = GetTargetPlayers(playerName);

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

        private void HandleReviveCommand(string[] parameters)
        {
            if (parameters.Length < 2)
            {
                LogError("Usage: admin revive <player>");
                return;
            }

            var playerName = string.Join(" ", parameters.Skip(1));
            var targets = GetTargetPlayers(playerName);

            foreach (var character in targets)
            {
                if (character.data.dead || character.data.fullyPassedOut)
                {
                    // Revive at safe location (near local player)
                    var safePos = Character.localCharacter.Center + Vector3.up * 2f;
                    character.refs.view.RPC("RPCA_ReviveAtPosition", Photon.Pun.RpcTarget.All, safePos, true);
                    LogInfo($"Revived player: {character.characterName}");
                }
                else
                {
                    LogWarning($"Player {character.characterName} is not dead or passed out");
                }
            }
        }

        private void HandleRescueCommand(string[] parameters)
        {
            if (parameters.Length < 2)
            {
                LogError("Usage: admin rescue <player>");
                return;
            }

            var playerName = string.Join(" ", parameters.Skip(1));
            var targets = GetTargetPlayers(playerName);
            var localCharacter = Character.localCharacter;

            foreach (var character in targets)
            {
                var rescuePos = localCharacter.Center + localCharacter.data.lookDirection_Right * 3f;
                rescuePos.y += 1f; // Ensure above ground
                
                character.refs.view.RPC("WarpPlayerRPC", Photon.Pun.RpcTarget.All, rescuePos, true);
                LogInfo($"Rescued player {character.characterName} to your location");
            }
        }

        private void HandleGotoCommand(string[] parameters)
        {
            if (parameters.Length < 2)
            {
                LogError("Usage: admin goto <player>");
                return;
            }

            var playerName = string.Join(" ", parameters.Skip(1));
            var character = FindPlayerByName(playerName);
            
            if (character == null)
            {
                LogError($"Player '{playerName}' not found");
                return;
            }

            var targetPos = character.Center + character.data.lookDirection_Right * 2f;
            targetPos.y += 1f;
            
            var localCharacter = Character.localCharacter;
            localCharacter.refs.view.RPC("WarpPlayerRPC", Photon.Pun.RpcTarget.All, targetPos, true);
            LogInfo($"Teleported to player: {character.characterName}");
        }

        private void HandleStaminaCommand(string[] parameters)
        {
            if (parameters.Length < 3)
            {
                LogError("Usage: admin stamina <player> <0-1>");
                return;
            }

            if (!float.TryParse(parameters[parameters.Length - 1], out float staminaLevel))
            {
                LogError("Invalid stamina level. Use 0-1 (0=empty, 1=full)");
                return;
            }

            var playerName = string.Join(" ", parameters.Skip(1).Take(parameters.Length - 2));
            var targets = GetTargetPlayers(playerName);

            staminaLevel = UnityEngine.Mathf.Clamp01(staminaLevel);

            foreach (var character in targets)
            {
                character.data.currentStamina = staminaLevel;
                character.ClampStamina();
                LogInfo($"Set {character.characterName}'s stamina to {staminaLevel * 100:F0}%");
            }
        }

        private void HandleHealthCommand(string[] parameters)
        {
            if (parameters.Length < 3)
            {
                LogError("Usage: admin health <player> <0-1>");
                return;
            }

            if (!float.TryParse(parameters[parameters.Length - 1], out float healthLevel))
            {
                LogError("Invalid health level. Use 0-1 (0=dead, 1=full)");
                return;
            }

            var playerName = string.Join(" ", parameters.Skip(1).Take(parameters.Length - 2));
            var targets = GetTargetPlayers(playerName);

            healthLevel = UnityEngine.Mathf.Clamp01(healthLevel);

            foreach (var character in targets)
            {
                var injuryLevel = 1f - healthLevel;
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Injury, injuryLevel);
                LogInfo($"Set {character.characterName}'s health to {healthLevel * 100:F0}%");
            }
        }

        private void HandleHungerCommand(string[] parameters)
        {
            if (parameters.Length < 3)
            {
                LogError("Usage: admin hunger <player> <0-1>");
                return;
            }

            if (!float.TryParse(parameters[parameters.Length - 1], out float hungerLevel))
            {
                LogError("Invalid hunger level. Use 0-1 (0=none, 1=starving)");
                return;
            }

            var playerName = string.Join(" ", parameters.Skip(1).Take(parameters.Length - 2));
            var targets = GetTargetPlayers(playerName);

            hungerLevel = UnityEngine.Mathf.Clamp01(hungerLevel);

            foreach (var character in targets)
            {
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Hunger, hungerLevel);
                LogInfo($"Set {character.characterName}'s hunger to {hungerLevel * 100:F0}%");
            }
        }

        private void HandleClearStatusCommand(string[] parameters)
        {
            if (parameters.Length < 2)
            {
                LogError("Usage: admin clear-status <player>");
                return;
            }

            var playerName = string.Join(" ", parameters.Skip(1));
            var targets = GetTargetPlayers(playerName);

            foreach (var character in targets)
            {
                character.refs.afflictions.ClearAllStatus(excludeCurse: false);
                LogInfo($"Cleared all status effects for: {character.characterName}");
            }
        }

        private void HandleInfiniteStaminaCommand(string[] parameters)
        {
            if (parameters.Length < 2)
            {
                LogError("Usage: admin infinite-stamina <player> [on/off]");
                return;
            }

            var enable = true;
            var playerName = string.Join(" ", parameters.Skip(1));

            // Check if last parameter is on/off
            if (parameters.Length > 2)
            {
                var lastParam = parameters[parameters.Length - 1].ToLower();
                if (lastParam == "off" || lastParam == "false" || lastParam == "0")
                {
                    enable = false;
                    playerName = string.Join(" ", parameters.Skip(1).Take(parameters.Length - 2));
                }
                else if (lastParam == "on" || lastParam == "true" || lastParam == "1")
                {
                    enable = true;
                    playerName = string.Join(" ", parameters.Skip(1).Take(parameters.Length - 2));
                }
            }

            var targets = GetTargetPlayers(playerName);

            foreach (var character in targets)
            {
                // Use the existing console command method instead of reflection
                if (character.IsLocal)
                {
                    // For local character, we can use the static method directly
                    var currentState = character.infiniteStam;
                    if (enable != currentState)
                    {
                        Character.InfiniteStamina(); // This toggles the state
                    }
                }
                else
                {
                    // For remote characters, we need to use RPC or direct access
                    // Since infiniteStam has a private setter, we'll try the console command approach
                    LogWarning($"Cannot set infinite stamina for remote player: {character.characterName}");
                    LogInfo("This feature only works for the local player");
                    continue;
                }

                LogInfo($"{(enable ? "Enabled" : "Disabled")} infinite stamina for: {character.characterName}");
            }
        }

        private void HandleGodModeCommand(string[] parameters)
        {
            if (parameters.Length < 2)
            {
                LogError("Usage: admin god-mode <player> [on/off]");
                return;
            }

            var enable = true;
            var playerName = string.Join(" ", parameters.Skip(1));

            if (parameters.Length > 2)
            {
                var lastParam = parameters[parameters.Length - 1].ToLower();
                if (lastParam == "off" || lastParam == "false" || lastParam == "0")
                {
                    enable = false;
                    playerName = string.Join(" ", parameters.Skip(1).Take(parameters.Length - 2));
                }
                else if (lastParam == "on" || lastParam == "true" || lastParam == "1")
                {
                    enable = true;
                    playerName = string.Join(" ", parameters.Skip(1).Take(parameters.Length - 2));
                }
            }

            var targets = GetTargetPlayers(playerName);

            foreach (var character in targets)
            {
                // Use the existing console command method
                if (character.IsLocal)
                {
                    var currentState = character.statusesLocked;
                    if (enable != currentState)
                    {
                        Character.LockStatuses(); // This toggles the state
                    }
                }
                else
                {
                    LogWarning($"Cannot set god mode for remote player: {character.characterName}");
                    LogInfo("This feature only works for the local player");
                    continue;
                }

                LogInfo($"{(enable ? "Enabled" : "Disabled")} god mode for: {character.characterName}");
            }
        }

        private void HandleNoHungerCommand(string[] parameters)
        {
            // For now, we'll just set hunger to 0 and advise to use god-mode for permanent effect
            if (parameters.Length < 2)
            {
                LogError("Usage: admin no-hunger <player>");
                return;
            }

            var playerName = string.Join(" ", parameters.Skip(1));
            var targets = GetTargetPlayers(playerName);

            foreach (var character in targets)
            {
                character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Hunger, 0f);
                LogInfo($"Removed hunger for: {character.characterName}");
                LogInfo("Tip: Use 'admin god-mode' for permanent immunity");
            }
        }

        private void HandleSpectateCommand(string[] parameters)
        {
            LogInfo("Spectate mode not implemented yet");
            LogInfo("Use 'admin goto <player>' to follow players manually");
        }

        private void HandleListPlayersCommand()
        {
            var allCharacters = Character.AllCharacters;
            
            LogInfo("=== Admin Player List ===");
            
            foreach (var character in allCharacters.OrderBy(c => c.characterName))
            {
                if (character == null) continue;
                
                var status = character.data.dead ? "DEAD" : 
                           character.data.passedOut ? "PASSED OUT" : 
                           character.data.isClimbingAnything ? "CLIMBING" : "OK";
                
                var health = (1f - character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury)) * 100;
                var stamina = character.GetTotalStamina() * 100;
                var hunger = character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hunger) * 100;
                
                LogInfo($"{character.characterName} [{status}]");
                LogInfo($"  Health: {health:F0}% | Stamina: {stamina:F0}% | Hunger: {hunger:F0}%");
                LogInfo($"  Pos: ({character.Center.x:F1}, {character.Center.y:F1}, {character.Center.z:F1})");
                LogInfo($"  God: {character.statusesLocked} | InfStam: {character.infiniteStam}");
            }
        }

        private void HandleEmergencyHealAllCommand()
        {
            var allCharacters = Character.AllCharacters;
            int healedCount = 0;
            
            foreach (var character in allCharacters)
            {
                if (character == null) continue;
                
                // Full heal
                character.refs.afflictions.ClearAllStatus(excludeCurse: false);
                character.AddStamina(1f);
                healedCount++;
            }
            
            LogInfo($"EMERGENCY: Healed all {healedCount} players");
        }

        private void HandleResetWorldCommand()
        {
            LogInfo("World reset capabilities limited");
            LogInfo("Available resets:");
            LogInfo("- Use 'admin emergency-heal-all' to heal everyone");
            LogInfo("- Individual player management available");
        }

        private System.Collections.Generic.List<Character> GetTargetPlayers(string playerName)
        {
            var results = new System.Collections.Generic.List<Character>();
            
            // Remove surrounding quotes if they exist
            playerName = playerName.Trim();
            if (playerName.StartsWith("\"") && playerName.EndsWith("\""))
            {
                playerName = playerName.Substring(1, playerName.Length - 2);
            }
            else if (playerName.StartsWith("'") && playerName.EndsWith("'"))
            {
                playerName = playerName.Substring(1, playerName.Length - 2);
            }
            
            if (playerName.ToLower() == "all")
            {
                results.AddRange(Character.AllCharacters.Where(c => c != null));
            }
            else
            {
                var character = FindPlayerByName(playerName);
                if (character != null)
                {
                    results.Add(character);
                }
                else
                {
                    LogError($"Player '{playerName}' not found");
                }
            }
            
            return results;
        }

        private Character FindPlayerByName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return null;

            // Remove surrounding quotes if they exist
            playerName = playerName.Trim();
            if (playerName.StartsWith("\"") && playerName.EndsWith("\""))
            {
                playerName = playerName.Substring(1, playerName.Length - 2);
            }
            else if (playerName.StartsWith("'") && playerName.EndsWith("'"))
            {
                playerName = playerName.Substring(1, playerName.Length - 2);
            }

            var allCharacters = Character.AllCharacters;
            
            // Try exact match first (case-insensitive)
            var exactMatch = allCharacters.FirstOrDefault(c => 
                string.Equals(c.characterName, playerName, System.StringComparison.OrdinalIgnoreCase));
            
            if (exactMatch != null)
                return exactMatch;

            // Try partial match (contains)
            var partialMatch = allCharacters.FirstOrDefault(c => 
                c.characterName.ToLower().Contains(playerName.ToLower()));
            
            if (partialMatch != null)
            {
                LogInfo($"Found partial match: '{partialMatch.characterName}'");
                return partialMatch;
            }

            return null;
        }

        public override bool CanExecute()
        {
            // Could add additional permission checks here if needed
            return Character.localCharacter != null;
        }
    }
}