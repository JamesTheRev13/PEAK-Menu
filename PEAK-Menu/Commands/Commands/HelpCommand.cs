using System.Linq;

namespace PEAK_Menu.Commands
{
    public class HelpCommand : BaseCommand
    {
        private readonly CommandManager _commandManager;

        public override string Name => "help";
        public override string Description => "Shows available commands and their descriptions";
        
        public override string DetailedHelp => 
@"=== HELP Command Help ===
Shows command information and usage

Usage:
  help              - List all commands
  help <command>    - Detailed help for specific command

Examples:
  help
  help customize
  help teleport";

        public HelpCommand(CommandManager commandManager)
        {
            _commandManager = commandManager;
        }

        public override void Execute(string[] parameters)
        {
            var availableCommands = _commandManager.GetAvailableCommands().ToList();
            
            if (parameters.Length > 0)
            {
                var commandName = parameters[0].ToLower();
                var command = _commandManager.GetCommand(commandName);
                
                if (command != null)
                {
                    // Use the command's own detailed help
                    LogInfo(command.DetailedHelp);
                }
                else
                {
                    LogError($"Command '{commandName}' not found");
                }
                return;
            }

            LogInfo("Available commands:");
            
            foreach (var command in availableCommands.OrderBy(c => c.Name))
            {
                LogInfo($"  {command.Name} - {command.Description}");
            }
            
            LogInfo($"Total: {availableCommands.Count} commands");
            LogInfo("Use 'help <command>' for detailed information");
        }
    }
}