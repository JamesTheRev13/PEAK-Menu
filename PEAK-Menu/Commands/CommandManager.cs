using System;
using System.Collections.Generic;
using System.Linq;

namespace PEAK_Menu.Commands
{
    public class CommandManager
    {
        private readonly Dictionary<string, ICommand> _commands;

        public CommandManager()
        {
            _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);
            RegisterDefaultCommands();
        }

        public void RegisterCommand(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            _commands[command.Name] = command;
            Plugin.Log.LogDebug($"Registered command: {command.Name}");
        }

        public bool ExecuteCommand(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                return false;

            var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var commandName = parts[0];
            var parameters = parts.Skip(1).ToArray();

            if (_commands.TryGetValue(commandName, out var command))
            {
                if (command.CanExecute())
                {
                    try
                    {
                        command.Execute(parameters);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogError($"Error executing command '{commandName}': {ex.Message}");
                    }
                }
                else
                {
                    Plugin.Log.LogWarning($"Command '{commandName}' cannot be executed at this time");
                }
            }
            else
            {
                Plugin.Log.LogWarning($"Unknown command: {commandName}");
            }

            return false;
        }

        public IEnumerable<ICommand> GetAvailableCommands()
        {
            return _commands.Values.Where(cmd => cmd.CanExecute());
        }

        public ICommand? GetCommand(string name)
        {
            _commands.TryGetValue(name, out var command);
            return command;
        }

        private void RegisterDefaultCommands()
        {
            RegisterCommand(new HelpCommand(this));
            RegisterCommand(new ClearCommand());
            RegisterCommand(new VersionCommand());
        }

        public void Cleanup()
        {
            _commands.Clear();
            Plugin.Log.LogDebug("Command manager cleaned up");
        }
    }
}