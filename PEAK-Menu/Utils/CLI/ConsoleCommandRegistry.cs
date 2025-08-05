using System;
using System.Collections.Generic;
using System.Reflection;
using Zorro.Core.CLI;
using PEAK_Menu.Commands.Console;

namespace PEAK_Menu.Utils.CLI
{
    public static class ConsoleCommandRegistry
    {
        private static bool _isRegistered = false;
        private static readonly object _registrationLock = new object();
        
        public static void RegisterPEAKCommands()
        {
            lock (_registrationLock)
            {
                if (_isRegistered)
                {
                    Plugin.Log?.LogDebug("PEAK console commands already registered, skipping...");
                    return;
                }
                
                try
                {
                    Plugin.Log?.LogInfo("Registering PEAK console commands...");
                    
                    // Register our custom type parsers first
                    RegisterCustomTypeParsers();
                    
                    // Register our commands
                    RegisterCommands();
                    
                    _isRegistered = true;
                    Plugin.Log?.LogInfo("PEAK console commands registered successfully");
                }
                catch (Exception ex)
                {
                    Plugin.Log?.LogError($"Failed to register PEAK console commands: {ex.Message}");
                    _isRegistered = false; // Allow retry on failure
                }
            }
        }
        
        public static bool IsRegistered => _isRegistered;
        
        private static void RegisterCustomTypeParsers()
        {
            // Add our custom type parsers to the existing system
            var typeParsers = ConsoleCommands.TypeParsers;
            
            if (!typeParsers.ContainsKey(typeof(Player)))
            {
                typeParsers.Add(typeof(Player), new PlayerCLIParser());
                Plugin.Log?.LogDebug("Registered Player CLI parser");
            }
            
            if (!typeParsers.ContainsKey(typeof(string)))
            {
                typeParsers.Add(typeof(string), new StringCLIParser());
                Plugin.Log?.LogDebug("Registered String CLI parser");
            }
        }
        
        private static void RegisterCommands()
        {
            var commandMethods = ConsoleCommands.ConsoleCommandMethods;
            int commandsRegistered = 0;
            
            // Player commands
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.Heal)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.Kill)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.Revive)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.Teleport)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.Goto)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.Bring)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.GodMode)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.InfiniteStamina)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.SetHealth)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.SetStamina)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.SetHunger)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.ClearStatus)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.ListPlayers)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(PlayerCommands), nameof(PlayerCommands.EmergencyHealAll)) ? 1 : 0;
            
            // Movement commands
            commandsRegistered += AddCommand(commandMethods, typeof(MovementCommands), nameof(MovementCommands.NoClip)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(MovementCommands), nameof(MovementCommands.Speed)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(MovementCommands), nameof(MovementCommands.Jump)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(MovementCommands), nameof(MovementCommands.Climb)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(MovementCommands), nameof(MovementCommands.NoClipSpeed)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(MovementCommands), nameof(MovementCommands.NoClipFastSpeed)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(MovementCommands), nameof(MovementCommands.TeleportCoords)) ? 1 : 0;
            
            // Item commands
            commandsRegistered += AddCommand(commandMethods, typeof(ItemCommands), nameof(ItemCommands.ListItems)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(ItemCommands), nameof(ItemCommands.SearchItems)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(ItemCommands), nameof(ItemCommands.GiveItem)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(ItemCommands), nameof(ItemCommands.SpawnItem)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(ItemCommands), nameof(ItemCommands.DropItem)) ? 1 : 0;
            
            // Customization commands
            commandsRegistered += AddCommand(commandMethods, typeof(CustomizationCommands), nameof(CustomizationCommands.Rainbow)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(CustomizationCommands), nameof(CustomizationCommands.Randomize)) ? 1 : 0;
            
            // Inventory commands
            commandsRegistered += AddCommand(commandMethods, typeof(InventoryCommands), nameof(InventoryCommands.ShowInventory)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(InventoryCommands), nameof(InventoryCommands.InventoryStats)) ? 1 : 0;
            commandsRegistered += AddCommand(commandMethods, typeof(InventoryCommands), nameof(InventoryCommands.ClearInventory)) ? 1 : 0;
            
            Plugin.Log?.LogInfo($"Registered {commandsRegistered} PEAK console commands (Total: {commandMethods.Count})");
        }
        
        private static bool AddCommand(List<ConsoleCommand> commandList, Type classType, string methodName)
        {
            try
            {
                var method = classType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                {
                    var consoleCommand = new ConsoleCommand(method);
                    commandList.Add(consoleCommand);
                    Plugin.Log?.LogDebug($"Registered command: {classType.Name}.{methodName}");
                    return true;
                }
                else
                {
                    Plugin.Log?.LogWarning($"Method {methodName} not found in {classType.Name}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"Failed to register command {classType.Name}.{methodName}: {ex.Message}");
                return false;
            }
        }
    }
}