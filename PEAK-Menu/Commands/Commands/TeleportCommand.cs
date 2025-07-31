using PEAK_Menu.Utils;
using System.Linq;
using UnityEngine;

namespace PEAK_Menu.Commands
{
    public class TeleportCommand : BaseCommand
    {
        public override string Name => "teleport";
        public override string Description => "Teleport to coordinates or players";

        public override string DetailedHelp =>
@"=== TELEPORT Command Help ===
Teleport to specific locations or players

Usage:
  teleport <x> <y> <z>     - Teleport to coordinates
  teleport <player>        - Teleport to player
  teleport here <player>   - Teleport player to you

Examples:
  teleport 100 50 200
  teleport ""Player Name""
  teleport here john";

        public override void Execute(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                LogError("Usage: teleport <x> <y> <z> OR teleport <player> OR teleport here <player>");
                return;
            }

            var localPlayer = Character.localCharacter;
            if (localPlayer == null)
            {
                LogError("No local character found");
                return;
            }

            // Check for "here" subcommand
            if (parameters[0].ToLower() == "here")
            {
                HandleTeleportHere(parameters);
                return;
            }

            // Try to parse as coordinates (3 numbers)
            if (parameters.Length >= 3 && 
                float.TryParse(parameters[0], out float x) &&
                float.TryParse(parameters[1], out float y) &&
                float.TryParse(parameters[2], out float z))
            {
                HandleTeleportToCoordinates(x, y, z);
                return;
            }

            // Parse as player name
            var parsed = ParameterParser.ParsePlayerAndValue(parameters, 0);
            if (!string.IsNullOrEmpty(parsed.PlayerName))
            {
                HandleTeleportToPlayer(parsed);
            }
            else
            {
                LogError("Invalid teleport parameters. Use coordinates or player name.");
            }
        }

        private void HandleTeleportHere(string[] parameters)
        {
            if (parameters.Length < 2)
            {
                LogError("Usage: teleport here <player>");
                return;
            }

            var playerName = string.Join(" ", parameters.Skip(1));
            var targets = ParameterParser.GetTargetPlayers(playerName, out string error);
            
            if (!string.IsNullOrEmpty(error))
            {
                LogError(error);
                return;
            }

            var localPlayer = Character.localCharacter;
            Vector3 teleportPos = localPlayer.Center + localPlayer.data.lookDirection * 2f;

            foreach (var character in targets)
            {
                if (character == localPlayer)
                {
                    LogWarning("Cannot teleport yourself to yourself");
                    continue;
                }

                TeleportCharacter(character, teleportPos);
                LogInfo($"Teleported {character.characterName} to your location");
            }
        }

        private void HandleTeleportToCoordinates(float x, float y, float z)
        {
            var position = new Vector3(x, y, z);
            var localPlayer = Character.localCharacter;
            
            TeleportCharacter(localPlayer, position);
            LogInfo($"Teleported to coordinates: {position}");
        }

        private void HandleTeleportToPlayer(ParameterParser.ParsedParameters parsed)
        {
            var targets = ParameterParser.GetTargetPlayers(parsed.PlayerName, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                LogError(error);
                return;
            }

            var target = targets.FirstOrDefault();
            if (target == null)
            {
                LogError("Player not found");
                return;
            }

            var localPlayer = Character.localCharacter;
            if (target == localPlayer)
            {
                LogWarning("Cannot teleport to yourself");
                return;
            }

            Vector3 targetPos = target.Center + Vector3.back * 2f;
            TeleportCharacter(localPlayer, targetPos);
            LogInfo($"Teleported to {target.characterName}");
        }

        private void TeleportCharacter(Character character, Vector3 position)
        {
            try
            {
                var warpMethod = typeof(Character).GetMethod("WarpPlayerRPC", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                
                if (warpMethod != null)
                {
                    warpMethod.Invoke(character, new object[] { position, true });
                }
                else
                {
                    LogError("Teleport method not found");
                }
            }
            catch (System.Exception ex)
            {
                LogError($"Teleport failed: {ex.Message}");
            }
        }

        public override bool CanExecute()
        {
            return Character.localCharacter != null;
        }
    }
}