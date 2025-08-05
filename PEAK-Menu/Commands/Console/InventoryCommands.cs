using UnityEngine;
using Zorro.Core.CLI;

namespace PEAK_Menu.Commands.Console
{
    [ConsoleClassCustomizer("Inventory")]
    public static class InventoryCommands
    {
        [ConsoleCommand]
        public static void ShowInventory()
        {
            var player = Player.localPlayer;
            if (player == null)
            {
                Debug.LogError("[PEAK] No local player found");
                return;
            }

            Debug.Log("[PEAK] === Inventory ===");
            
            for (int i = 0; i < player.itemSlots.Length; i++)
            {
                var slot = player.itemSlots[i];
                if (!slot.IsEmpty())
                {
                    Debug.Log($"[PEAK] Slot {i}: {slot.prefab.UIData.itemName} - ID: {slot.prefab.itemID}");
                }
                else
                {
                    Debug.Log($"[PEAK] Slot {i}: Empty");
                }
            }

            // Backpack
            if (!player.backpackSlot.IsEmpty())
            {
                Debug.Log("[PEAK] Backpack: Equipped");
            }
            else
            {
                Debug.Log("[PEAK] Backpack: None");
            }

            // Temp slot
            if (!player.tempFullSlot.IsEmpty())
            {
                Debug.Log($"[PEAK] Temp Slot: {player.tempFullSlot.prefab.UIData.itemName}");
            }
            else
            {
                Debug.Log("[PEAK] Temp Slot: Empty");
            }
        }

        [ConsoleCommand]
        public static void InventoryStats()
        {
            var player = Player.localPlayer;
            if (player == null)
            {
                Debug.LogError("[PEAK] No local player found");
                return;
            }

            Debug.Log("[PEAK] === Inventory Statistics ===");
            
            int usedSlots = 0;
            int totalSlots = player.itemSlots.Length;
            
            foreach (var slot in player.itemSlots)
            {
                if (!slot.IsEmpty())
                    usedSlots++;
            }
            
            float usagePercentage = totalSlots > 0 ? (float)usedSlots / totalSlots * 100f : 0f;
            
            Debug.Log($"[PEAK] Slot Usage: {usedSlots}/{totalSlots} ({usagePercentage:F1}%)");
            Debug.Log($"[PEAK] Available Slots: {totalSlots - usedSlots}");
            Debug.Log($"[PEAK] Backpack Equipped: {(!player.backpackSlot.IsEmpty() ? "Yes" : "No")}");
            Debug.Log($"[PEAK] Temp Slot Used: {(!player.tempFullSlot.IsEmpty() ? "Yes" : "No")}");
            
            // Character item info
            var character = Character.localCharacter;
            if (character?.data?.currentItem != null)
            {
                Debug.Log($"[PEAK] Currently Holding: {character.data.currentItem.name}");
            }
            else
            {
                Debug.Log("[PEAK] Currently Holding: Nothing");
            }
        }

        [ConsoleCommand]
        public static void ClearInventory()
        {
            var character = Character.localCharacter;
            if (character?.data?.currentItem != null)
            {
                try
                {
                    // Try to drop/clear current item
                    Debug.Log($"[PEAK] Attempting to clear current item: {character.data.currentItem.name}");
                    
                    // Use reflection to access EquipSlot method if needed
                    var equipSlotMethod = typeof(CharacterItems).GetMethod("EquipSlot", 
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    
                    if (equipSlotMethod != null && character.refs?.items != null)
                    {
                        // Create an "None" optionable byte to clear the slot
                        var optionableType = typeof(Zorro.Core.Optionable<byte>);
                        var noneProperty = optionableType.GetProperty("None", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                        
                        if (noneProperty != null)
                        {
                            var noneValue = noneProperty.GetValue(null);
                            equipSlotMethod.Invoke(character.refs.items, new object[] { noneValue });
                            Debug.Log("[PEAK] Successfully cleared current item");
                        }
                        else
                        {
                            Debug.LogError("[PEAK] Could not access Optionable.None property");
                        }
                    }
                    else
                    {
                        Debug.LogError("[PEAK] Could not access EquipSlot method or CharacterItems reference");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[PEAK] Failed to clear inventory: {ex.Message}");
                }
            }
            else
            {
                Debug.Log("[PEAK] No current item to clear");
            }
        }
    }
}