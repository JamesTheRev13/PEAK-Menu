namespace PEAK_Menu.Commands
{
    public class InventoryCommand : BaseCommand
    {
        public override string Name => "inventory";
        public override string Description => "Shows current inventory contents";

        public override void Execute(string[] parameters)
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
                    LogInfo($"Slot {i}: {slot.prefab.UIData.itemName}");
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
    }
}