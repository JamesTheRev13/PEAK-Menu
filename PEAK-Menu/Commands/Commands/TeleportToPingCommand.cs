namespace PEAK_Menu.Commands
{
    public class TeleportToPingCommand : BaseCommand
    {
        public override string Name => "teleport-to-ping";
        public override string Description => "Toggle teleporting to ping markers";

        public override string DetailedHelp =>
@"=== TELEPORT-TO-PING Command Help ===
Toggle the ability to teleport to your ping markers

Usage:
  teleport-to-ping [on/off]    - Toggle or set teleport-to-ping
  teleport-to-ping status      - Show current status

Examples:
  teleport-to-ping on
  teleport-to-ping off
  teleport-to-ping
  teleport-to-ping status

When enabled, pinging a location will automatically teleport you there.
This only works for your own pings, not other players' pings.";

        public override void Execute(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                // Toggle current state
                var currentState = Plugin.PluginConfig?.TeleportToPingEnabled?.Value ?? false;
                Plugin.PluginConfig.TeleportToPingEnabled.Value = !currentState;
                var newState = Plugin.PluginConfig.TeleportToPingEnabled.Value;
                
                LogInfo($"Teleport-to-ping {(newState ? "enabled" : "disabled")}");
                return;
            }

            var parameter = parameters[0].ToLower();
            
            switch (parameter)
            {
                case "on":
                case "enable":
                case "true":
                case "1":
                    Plugin.PluginConfig.TeleportToPingEnabled.Value = true;
                    LogInfo("Teleport-to-ping enabled");
                    LogInfo("You will now teleport to locations when you ping them");
                    break;
                    
                case "off":
                case "disable":
                case "false":
                case "0":
                    Plugin.PluginConfig.TeleportToPingEnabled.Value = false;
                    LogInfo("Teleport-to-ping disabled");
                    LogInfo("Pinging will work normally without teleporting");
                    break;
                    
                case "status":
                case "info":
                    var isEnabled = Plugin.PluginConfig?.TeleportToPingEnabled?.Value ?? false;
                    LogInfo($"=== Teleport-to-Ping Status ===");
                    LogInfo($"Enabled: {isEnabled}");
                    LogInfo($"Description: {(isEnabled ? "You will teleport to ping markers" : "Normal ping behavior")}");
                    LogInfo($"Usage: Hold ping key and click to place marker {(isEnabled ? "and teleport" : "")}");
                    break;
                    
                default:
                    LogError($"Unknown parameter: {parameter}");
                    LogInfo("Use: teleport-to-ping [on/off/status]");
                    break;
            }
        }

        public override bool CanExecute()
        {
            return true; // This command can always be executed
        }
    }
}