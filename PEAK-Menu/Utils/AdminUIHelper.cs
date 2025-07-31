using UnityEngine;

namespace PEAK_Menu.Utils
{
    public static class AdminUIHelper
    {
        public static void ExecuteQuickAction(string action, string playerName = null)
        {
            var menuManager = Plugin.Instance?._menuManager;
            if (menuManager == null) return;

            string command = action.ToLower() switch
            {
                "heal-all" => "admin emergency-heal-all",
                "list-all" => "admin list-players",
                "god-mode" => $"admin god-mode \"{playerName ?? Character.localCharacter?.characterName}\"",
                "infinite-stamina" => $"admin infinite-stamina \"{playerName ?? Character.localCharacter?.characterName}\"",
                "kill" => $"admin kill \"{playerName}\"",
                "bring" => $"admin bring \"{playerName}\"",
                "revive" => $"admin revive \"{playerName}\"",
                "heal" => $"admin heal \"{playerName}\"",
                "goto" => $"admin goto \"{playerName}\"",
                "clear-status" => $"admin clear-status \"{playerName}\"",
                _ => null
            };

            if (!string.IsNullOrEmpty(command))
            {
                menuManager.ExecuteCommand(command);
            }
        }

        public static void SetPlayerStatus(string playerName, string statusType, float value)
        {
            var menuManager = Plugin.Instance?._menuManager;
            if (menuManager == null) return;

            var command = $"admin {statusType} \"{playerName}\" {value:F2}";
            menuManager.ExecuteCommand(command);
        }

        public static void TeleportToCoordinates(float x, float y, float z)
        {
            var menuManager = Plugin.Instance?._menuManager;
            if (menuManager == null) return;

            var command = $"admin teleport-coords {x:F1} {y:F1} {z:F1}";
            menuManager.ExecuteCommand(command);
        }
    }
}