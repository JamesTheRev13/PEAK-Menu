namespace PEAK_Menu.Utils
{
    public static class AdminUIHelper
    {
        public static void ExecuteQuickAction(string action, string playerName = null)
        {
            var menuManager = Plugin.Instance?._debugConsoleManager;
            if (menuManager == null) return;

            string command = action.ToLower() switch
            {
                "heal-all" => "admin emergency-heal-all",
                "list-all" => "admin list-players",
                
                // FIXED: God mode and infinite stamina now properly check current state
                "god-mode" => GenerateToggleCommand("god-mode", playerName),
                "infinite-stamina" => GenerateToggleCommand("infinite-stamina", playerName),
                
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

        private static string GenerateToggleCommand(string commandType, string playerName)
        {
            var character = Character.localCharacter;
            if (character == null) return null;

            string targetPlayerName = playerName ?? character.characterName;
            bool currentState = false;
            
            // Check current state based on command type
            switch (commandType)
            {
                case "god-mode":
                    currentState = character.statusesLocked;
                    break;
                case "infinite-stamina":
                    currentState = character.infiniteStam;
                    break;
            }
            
            // Generate command with opposite state
            string newState = currentState ? "off" : "on";
            return $"admin {commandType} \"{targetPlayerName}\" {newState}";
        }

        public static void SetPlayerStatus(string playerName, string statusType, float value)
        {
            var menuManager = Plugin.Instance?._debugConsoleManager;
            if (menuManager == null) return;

            var command = $"admin {statusType} \"{playerName}\" {value:F2}";
            menuManager.ExecuteCommand(command);
        }

        public static void TeleportToCoordinates(float x, float y, float z)
        {
            var menuManager = Plugin.Instance?._debugConsoleManager;
            if (menuManager == null) return;

            var command = $"admin teleport-coords {x:F1} {y:F1} {z:F1}";
            menuManager.ExecuteCommand(command);
        }
    }
}