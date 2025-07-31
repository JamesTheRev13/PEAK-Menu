using UnityEngine;
using System.Linq;

namespace PEAK_Menu.Commands
{
    public class TeleportCommand : BaseCommand
    {
        public override string Name => "teleport";
        public override string Description => "Teleport to coordinates (x y z) or to another player";
        
        public override string DetailedHelp =>
@"=== TELEPORT Command Help ===
Teleport to specific coordinates or to another player

Usage: 
  teleport <x> <y> <z>     - Teleport to coordinates
  teleport <playername>    - Teleport to player
  teleport list            - List all players

Parameters:
  x, y, z     - Coordinates (float)
  playername  - Name of player to teleport to

Examples:
  teleport 0 100 0
  teleport -50.5 25.3 100.7
  teleport player2
  teleport ""John Doe""
  teleport list

Notes:
  - Cannot teleport while dead
  - Player names are case-insensitive
  - Use quotes for names with spaces
  - Teleports slightly offset to avoid collision";

        public override void Execute(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                LogError("Missing parameters");
                LogInfo("Use 'help teleport' for usage information");
                return;
            }

            var character = Character.localCharacter;
            if (character == null)
            {
                LogError("No local character found");
                return;
            }

            // Special command: list players
            if (parameters.Length == 1 && parameters[0].ToLower() == "list")
            {
                ListPlayers();
                return;
            }

            // Try coordinate teleport first (3 parameters)
            if (parameters.Length == 3)
            {
                TeleportToCoordinates(parameters);
                return;
            }

            // Try player teleport (1 parameter or multiple for names with spaces)
            if (parameters.Length >= 1)
            {
                // Join all parameters to handle names with spaces
                var playerName = string.Join(" ", parameters);
                TeleportToPlayer(playerName);
                return;
            }

            LogError("Invalid parameters");
            LogInfo("Use 'help teleport' for usage information");
        }

        private void TeleportToCoordinates(string[] parameters)
        {
            if (!float.TryParse(parameters[0], out float x) ||
                !float.TryParse(parameters[1], out float y) ||
                !float.TryParse(parameters[2], out float z))
            {
                LogError("Invalid coordinates - must be numbers");
                LogInfo("Use 'help teleport' for usage information");
                return;
            }

            var character = Character.localCharacter;
            var targetPosition = new Vector3(x, y, z);
            
            character.refs.view.RPC("WarpPlayerRPC", Photon.Pun.RpcTarget.All, targetPosition, true);
            LogInfo($"Teleported to coordinates {targetPosition}");
        }

        private void TeleportToPlayer(string playerName)
        {
            var targetCharacter = FindPlayerByName(playerName);
            
            if (targetCharacter == null)
            {
                LogError($"Player '{playerName}' not found");
                LogInfo("Use 'teleport list' to see available players");
                return;
            }

            if (targetCharacter.data.dead)
            {
                LogWarning($"Player '{targetCharacter.characterName}' is dead");
                LogInfo("Teleporting to their last known position...");
            }

            // Calculate safe teleport position (slightly offset to avoid collision)
            var targetPosition = targetCharacter.Center;
            var offset = targetCharacter.data.lookDirection_Right * 2f; // 2 units to the right
            targetPosition += offset;
            
            // Ensure we're above ground
            targetPosition.y += 1f;

            var localCharacter = Character.localCharacter;
            localCharacter.refs.view.RPC("WarpPlayerRPC", Photon.Pun.RpcTarget.All, targetPosition, true);
            
            LogInfo($"Teleported to player '{targetCharacter.characterName}' at {targetPosition}");
        }

        private Character FindPlayerByName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return null;

            // Get all characters
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

        private void ListPlayers()
        {
            var allCharacters = Character.AllCharacters;
            var localCharacter = Character.localCharacter;
            
            if (allCharacters == null || allCharacters.Count == 0)
            {
                LogInfo("No players found");
                return;
            }

            LogInfo("=== Available Players ===");
            
            int playerCount = 0;
            foreach (var character in allCharacters.OrderBy(c => c.characterName))
            {
                if (character == null) continue;
                
                playerCount++;
                var status = "";
                
                // Add status indicators
                if (character == localCharacter)
                    status += " (YOU)";
                if (character.data.dead)
                    status += " [DEAD]";
                else if (character.data.passedOut)
                    status += " [PASSED OUT]";
                else if (character.data.isClimbingAnything)
                    status += " [CLIMBING]";
                else if (!character.data.isGrounded)
                    status += " [AIRBORNE]";
                
                var position = character.Center;
                LogInfo($"  {character.characterName}{status}");
                LogInfo($"    Position: ({position.x:F1}, {position.y:F1}, {position.z:F1})");
            }
            
            LogInfo($"Total: {playerCount} players");
            LogInfo("Use 'teleport <playername>' to teleport to a player");
        }

        public override bool CanExecute()
        {
            var character = Character.localCharacter;
            return character != null && !character.data.dead;
        }
    }
}