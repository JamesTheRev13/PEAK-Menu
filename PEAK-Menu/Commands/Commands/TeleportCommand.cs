using UnityEngine;

namespace PEAK_Menu.Commands
{
    public class TeleportCommand : BaseCommand
    {
        public override string Name => "teleport";
        public override string Description => "Teleport to coordinates (x y z)";
        
        public override string DetailedHelp =>
@"=== TELEPORT Command Help ===
Teleport to specific coordinates

Usage: teleport <x> <y> <z>

Parameters:
  x - X coordinate (float)
  y - Y coordinate (float)
  z - Z coordinate (float)

Examples:
  teleport 0 100 0
  teleport -50.5 25.3 100.7

Note: Cannot teleport while dead";

        public override void Execute(string[] parameters)
        {
            if (parameters.Length != 3)
            {
                LogError("Invalid number of parameters");
                LogInfo("Use 'help teleport' for usage information");
                return;
            }

            if (!float.TryParse(parameters[0], out float x) ||
                !float.TryParse(parameters[1], out float y) ||
                !float.TryParse(parameters[2], out float z))
            {
                LogError("Invalid coordinates - must be numbers");
                LogInfo("Use 'help teleport' for usage information");
                return;
            }

            var character = Character.localCharacter;
            if (character == null)
            {
                LogError("No local character found");
                return;
            }

            var targetPosition = new Vector3(x, y, z);
            character.refs.view.RPC("WarpPlayerRPC", Photon.Pun.RpcTarget.All, targetPosition, true);
            LogInfo($"Teleported to {targetPosition}");
        }

        public override bool CanExecute()
        {
            var character = Character.localCharacter;
            return character != null && !character.data.dead;
        }
    }
}