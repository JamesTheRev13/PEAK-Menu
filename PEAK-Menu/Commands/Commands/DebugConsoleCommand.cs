using PEAK_Menu.Utils;

namespace PEAK_Menu.Commands
{
    public class DebugConsoleCommand : BaseCommand
    {
        public override string Name => "debug-console";
        public override string Description => "Control the built-in debug console";

        public override string DetailedHelp =>
@"=== DEBUG-CONSOLE Command Help ===
Control the game's built-in debug console system (separate from this menu)

Usage: debug-console <action>

Actions:
  toggle      - Toggle the debug console visibility
  open        - Open the debug console
  close       - Close the debug console
  enable      - Enable debug console access
  disable     - Disable debug console access
  status      - Show current debug console status

Examples:
  debug-console toggle
  debug-console open
  debug-console status

Hotkey: Press HOME to toggle debug console

The debug console provides:
- Full command history and autocomplete
- Multiple debug pages (Console, Hotkeys, etc.)
- Real-time log filtering and muting
- Hotkey configuration for quick commands
- Direct access to game's built-in commands";

        public override void Execute(string[] parameters)
        {
            var debugConsoleManager = Plugin.Instance._debugConsoleManager;
            if (debugConsoleManager == null)
            {
                LogError("Debug Console Manager not available");
                return;
            }

            if (parameters.Length == 0)
            {
                debugConsoleManager.ToggleDebugConsole();
                LogInfo($"Debug console {(debugConsoleManager.IsDebugConsoleOpen ? "opened" : "closed")}");
                return;
            }

            var action = parameters[0].ToLower();

            try
            {
                switch (action)
                {
                    case "toggle":
                        debugConsoleManager.ToggleDebugConsole();
                        LogInfo($"Debug console {(debugConsoleManager.IsDebugConsoleOpen ? "opened" : "closed")}");
                        break;

                    case "open":
                        debugConsoleManager.EnableDebugConsole();
                        LogInfo("Debug console opened");
                        break;

                    case "close":
                        debugConsoleManager.DisableDebugConsole();
                        LogInfo("Debug console closed");
                        break;

                    case "enable":
                        debugConsoleManager.EnableDebugConsole();
                        LogInfo("Debug console enabled");
                        break;

                    case "disable":
                        debugConsoleManager.DisableDebugConsole();
                        LogInfo("Debug console disabled");
                        break;

                    case "status":
                        LogInfo(debugConsoleManager.GetStatus());
                        break;

                    default:
                        LogError($"Unknown action: {action}");
                        LogInfo("Valid actions: toggle, open, close, enable, disable, status");
                        break;
                }
            }
            catch (System.Exception ex)
            {
                LogError($"Error controlling debug console: {ex.Message}");
            }
        }

        public override bool CanExecute()
        {
            return Plugin.Instance._debugConsoleManager != null;
        }
    }
}