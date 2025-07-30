using System.Linq;
using System.Text;

namespace PEAK_Menu.Commands
{
    public class HelpCommand : BaseCommand
    {
        private readonly CommandManager _commandManager;

        public override string Name => "help";
        public override string Description => "Shows available commands and their descriptions";

        public HelpCommand(CommandManager commandManager)
        {
            _commandManager = commandManager;
        }

        public override void Execute(string[] parameters)
        {
            var availableCommands = _commandManager.GetAvailableCommands().ToList();
            
            if (parameters.Length > 0)
            {
                var commandName = parameters[0];
                var command = _commandManager.GetCommand(commandName);
                
                if (command != null)
                {
                    LogInfo($"{command.Name}: {command.Description}");
                }
                else
                {
                    LogError($"Command '{commandName}' not found");
                }
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Available commands:");
            
            foreach (var command in availableCommands.OrderBy(c => c.Name))
            {
                sb.AppendLine($"  {command.Name} - {command.Description}");
            }
            
            LogInfo(sb.ToString());
        }
    }
}