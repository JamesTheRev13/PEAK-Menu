using PEAK_Menu.Utils;

namespace PEAK_Menu.Commands
{
    public class NoWeightCommand : BaseCommand
    {
        public override string Name => "no-weight";
        public override string Description => "Toggle no weight penalties";

        public override string DetailedHelp =>
@"=== NO-WEIGHT Command Help ===
Toggle weight penalties from inventory items

Usage:
  no-weight [on/off]    - Toggle or set no-weight
  no-weight status      - Show current status

Examples:
  no-weight on
  no-weight off
  no-weight
  no-weight status

When enabled, inventory weight will not slow you down or affect movement.
This is implemented via Harmony patches for maximum compatibility.";

        public override void Execute(string[] parameters)
        {
            var playerManager = Plugin.Instance?._debugConsoleManager?.GetPlayerManager();
            if (playerManager == null)
            {
                LogError("Player manager not available");
                return;
            }

            if (parameters.Length == 0)
            {
                // Toggle current state
                var currentState = playerManager.NoWeightEnabled;
                playerManager.SetNoWeight(!currentState);
                return;
            }

            var parameter = parameters[0].ToLower();
            
            switch (parameter)
            {
                case "on":
                case "enable":
                case "true":
                case "1":
                    playerManager.SetNoWeight(true);
                    LogInfo("No weight enabled - inventory weight penalties disabled");
                    break;
                    
                case "off":
                case "disable":
                case "false":
                case "0":
                    playerManager.SetNoWeight(false);
                    LogInfo("No weight disabled - normal weight mechanics restored");
                    break;
                    
                case "status":
                case "info":
                    var isEnabled = playerManager.NoWeightEnabled;
                    LogInfo($"=== No Weight Status ===");
                    LogInfo($"Enabled: {isEnabled}");
                    LogInfo($"Effect: {(isEnabled ? "No weight penalties applied" : "Normal weight mechanics")}");
                    LogInfo($"Implementation: Harmony patches on weight calculation");
                    break;
                    
                default:
                    LogError($"Unknown parameter: {parameter}");
                    LogInfo("Use: no-weight [on/off/status]");
                    break;
            }
        }

        public override bool CanExecute()
        {
            return Character.localCharacter != null;
        }
    }
}