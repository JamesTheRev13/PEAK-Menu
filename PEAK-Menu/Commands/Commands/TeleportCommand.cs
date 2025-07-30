using UnityEngine;

namespace PEAK_Menu.Commands
{
    public class TeleportCommand : BaseCommand
    {
        public override string Name => "teleport";
        public override string Description => "Teleport to coordinates (x y z)";

        public override void Execute(string[] parameters)
        {
            if (parameters.Length != 3)
            {
                LogError("Usage: teleport <x> <y> <z>");
                return;
            }

            if (!float.TryParse(parameters[0], out float x) ||
                !float.TryParse(parameters[1], out float y) ||
                !float.TryParse(parameters[2], out float z))
            {
                LogError("Invalid coordinates. Use numbers only.");
                return;
            }

            var character = Character.localCharacter;
            if (character == null)
            {
                LogError("No local character found");
                return;
            }

            var targetPosition = new Vector3(x, y, z);
            
            // Use the game's warp system
            character.refs.view.RPC("WarpPlayerRPC", Photon.Pun.RpcTarget.All, targetPosition, true);
            LogInfo($"Teleported to {targetPosition}");
        }

        public override bool CanExecute()
        {
            // Only allow teleporting if character exists and is not dead
            var character = Character.localCharacter;
            return character != null && !character.data.dead;
        }
    }
}