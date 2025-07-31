using PEAK_Menu.Utils;

namespace PEAK_Menu.Commands
{
    public class InventoryCommand : BaseCommand
    {
        public override string Name => "inventory";
        public override string Description => "Inventory management and information commands";
        
        public override string DetailedHelp =>
@"=== INVENTORY Command Help ===
Inventory management and information commands

Usage: inventory [option]

Options:
  (no option)       - Show current inventory contents
  list              - Show current inventory contents
  info              - Show detailed inventory information
  clear             - Clear current held item (if possible)
  stats             - Show inventory statistics

Examples:
  inventory
  inventory list
  inventory clear
  inventory stats

Shows which items are currently in your inventory
and whether you have a backpack equipped.";

        public override void Execute(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                HandleListInventory();
                return;
            }

            var parsed = ParameterParser.ParseSubCommand(parameters);

            switch (parsed.Action)
            {
                case "list":
                    HandleListInventory();
                    break;

                case "info":
                    HandleInventoryInfo();
                    break;

                case "clear":
                    HandleClearInventory();
                    break;

                case "stats":
                    HandleInventoryStats();
                    break;

                default:
                    LogError($"Unknown inventory command: {parsed.Action}");
                    LogInfo("Available options: list, info, clear, stats");
                    break;
            }
        }

        private void HandleListInventory()
        {
            var player = Player.localPlayer;
            if (player == null)
            {
                LogError("No local player found");
                return;
            }

            LogInfo("=== Inventory ===");
            
            for (int i = 0; i < player.itemSlots.Length; i++)
            {
                var slot = player.itemSlots[i];
                if (!slot.IsEmpty())
                {
                    LogInfo($"Slot {i}: {slot.prefab.UIData.itemName} - ID: {slot.prefab.itemID}");
                }
                else
                {
                    LogInfo($"Slot {i}: Empty");
                }
            }

            // Backpack
            if (!player.backpackSlot.IsEmpty())
            {
                LogInfo("Backpack: Equipped");
            }
            else
            {
                LogInfo("Backpack: None");
            }

            // Temp slot
            if (!player.tempFullSlot.IsEmpty())
            {
                LogInfo($"Temp Slot: {player.tempFullSlot.prefab.UIData.itemName}");
            }
            else
            {
                LogInfo("Temp Slot: Empty");
            }
        }
        // for testing
        private void HandleInventoryInfo()
        {
            var player = Player.localPlayer;
            if (player == null)
            {
                LogError("No local player found");
                return;
            }

            LogInfo("=== Detailed Inventory Information ===");
            
            // Count items
            int itemCount = 0;
            int totalSlots = player.itemSlots.Length;
            
            for (int i = 0; i < player.itemSlots.Length; i++)
            {
                var slot = player.itemSlots[i];
                if (!slot.IsEmpty())
                {
                    itemCount++;
                    var item = slot.prefab;
                    LogInfo($"Slot {i}: {item.UIData.itemName}");
                    LogInfo($"  - Type: {item.GetType().Name}");
                    
                    // Try to get additional item info
                    try
                    {
                        if (item.UIData != null)
                        {
                            LogInfo($"  - UI Name: {item.UIData.itemName}");                           
                            LogInfo($"  - Description: {item.itemID}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        LogInfo($"  - Additional info unavailable: {ex.Message}");
                    }
                }
            }

            LogInfo($"Items: {itemCount}/{totalSlots} slots used");
            LogInfo($"Backpack: {(!player.backpackSlot.IsEmpty() ? "Equipped" : "Not equipped")}");
            LogInfo($"Temp Slot: {(!player.tempFullSlot.IsEmpty() ? "Occupied" : "Empty")}");
        }

        private void HandleClearInventory()
        {
            var character = Character.localCharacter;
            if (character?.data?.currentItem != null)
            {
                try
                {
                    // Try to drop/clear current item
                    LogInfo($"Attempting to clear current item: {character.data.currentItem.name}");
                    
                    // TODO
                }
                catch (System.Exception ex)
                {
                    LogError($"Failed to clear inventory: {ex.Message}");
                }
            }
            else
            {
                LogInfo("No current item to clear");
            }
        }
        // for testing
        private void HandleInventoryStats()
        {
            var player = Player.localPlayer;
            if (player == null)
            {
                LogError("No local player found");
                return;
            }

            LogInfo("=== Inventory Statistics ===");
            
            int usedSlots = 0;
            int totalSlots = player.itemSlots.Length;
            
            foreach (var slot in player.itemSlots)
            {
                if (!slot.IsEmpty())
                    usedSlots++;
            }
            
            float usagePercentage = totalSlots > 0 ? (float)usedSlots / totalSlots * 100f : 0f;
            
            LogInfo($"Slot Usage: {usedSlots}/{totalSlots} ({usagePercentage:F1}%)");
            LogInfo($"Available Slots: {totalSlots - usedSlots}");
            LogInfo($"Backpack Equipped: {(!player.backpackSlot.IsEmpty() ? "Yes" : "No")}");
            LogInfo($"Temp Slot Used: {(!player.tempFullSlot.IsEmpty() ? "Yes" : "No")}");
            
            // Character item info
            var character = Character.localCharacter;
            if (character?.data?.currentItem != null)
            {
                LogInfo($"Currently Holding: {character.data.currentItem.name}");
            }
            else
            {
                LogInfo("Currently Holding: Nothing");
            }
        }

        public override bool CanExecute()
        {
            return Player.localPlayer != null;
        }
    }
}