using UnityEngine;
using Zorro.Core.CLI;
using PEAK_Menu.Utils;
using System.Linq;

namespace PEAK_Menu.Commands.Console
{
    [ConsoleClassCustomizer("Item")]
    public static class ItemCommands
    {
        [ConsoleCommand]
        public static void ListItems()
        {
            var itemHelper = ItemDiscoveryHelper.Instance;
            var allItems = itemHelper.GetAllAvailableItems();
            
            if (allItems.Count == 0)
            {
                Debug.LogWarning("[PEAK] No items found. Items are discovered dynamically when the game loads.");
                return;
            }

            Debug.Log("[PEAK] === Available Items ===");
            Debug.Log($"[PEAK] Found {allItems.Count} items:");
            
            foreach (var kvp in allItems)
            {
                var displayName = kvp.Value?.UIData?.itemName ?? kvp.Key;
                Debug.Log($"[PEAK]   {kvp.Key} -> {displayName}");
            }
        }

        [ConsoleCommand]
        public static void SearchItems(string searchTerm)
        {
            var itemHelper = ItemDiscoveryHelper.Instance;
            var matchingItems = itemHelper.SearchItems(searchTerm);
            
            if (matchingItems.Count == 0)
            {
                Debug.Log($"[PEAK] No items found matching '{searchTerm}'");
                return;
            }

            Debug.Log($"[PEAK] === Items matching '{searchTerm}' ===");
            foreach (var kvp in matchingItems)
            {
                var displayName = kvp.Value?.UIData?.itemName ?? kvp.Key;
                Debug.Log($"[PEAK]   {kvp.Key} -> {displayName}");
            }
        }

        [ConsoleCommand]
        public static void GiveItem(string playerName, string itemName, int quantity = 1)
        {
            // Find the target player
            var targetPlayer = Character.AllCharacters?.FirstOrDefault(c => 
                c?.characterName?.ToLower() == playerName.ToLower());
            
            if (targetPlayer == null)
            {
                Debug.LogError($"[PEAK] Player '{playerName}' not found");
                return;
            }

            // Find the item
            var itemHelper = ItemDiscoveryHelper.Instance;
            var itemPrefab = itemHelper.FindItemByName(itemName);
            
            if (itemPrefab == null)
            {
                Debug.LogError($"[PEAK] Item '{itemName}' not found");
                Debug.Log("[PEAK] Use 'searchitems <name>' to find available items");
                return;
            }

            // Use the existing item giving logic
            try
            {
                if (targetPlayer.IsLocal)
                {
                    GiveItemToLocalPlayer(itemPrefab, quantity);
                }
                else
                {
                    SpawnItemsNearPlayer(targetPlayer, itemPrefab, quantity);
                }
                
                var displayName = itemPrefab.UIData?.itemName ?? itemPrefab.gameObject.name;
                Debug.Log($"[PEAK] Gave {quantity}x {displayName} to {targetPlayer.characterName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PEAK] Failed to give item: {ex.Message}");
            }
        }

        [ConsoleCommand]
        public static void SpawnItem(string itemName, int quantity = 1)
        {
            var localCharacter = Character.localCharacter;
            if (localCharacter == null)
            {
                Debug.LogError("[PEAK] No local character found");
                return;
            }

            GiveItem(localCharacter.characterName, itemName, quantity);
        }

        [ConsoleCommand]
        public static void DropItem(string itemName, int quantity = 1)
        {
            var localCharacter = Character.localCharacter;
            if (localCharacter == null)
            {
                Debug.LogError("[PEAK] No local character found");
                return;
            }

            // Find the item
            var itemHelper = ItemDiscoveryHelper.Instance;
            var itemPrefab = itemHelper.FindItemByName(itemName);
            
            if (itemPrefab == null)
            {
                Debug.LogError($"[PEAK] Item '{itemName}' not found");
                Debug.Log("[PEAK] Use 'searchitems <name>' to find available items");
                return;
            }

            try
            {
                // Spawn the item at the player's position
                var displayName = itemPrefab.UIData?.itemName ?? itemPrefab.gameObject.name;

                // Log the drop action
                Debug.Log($"[PEAK] Dropped {quantity}x {displayName} at {localCharacter.characterName}'s feet");

                // Use the spawn method to drop the item
                SpawnItemsNearPlayer(localCharacter, itemPrefab, quantity);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PEAK] Failed to drop item: {ex.Message}");
            }
        }

        // Copy the item giving methods from existing implementation
        private static void GiveItemToLocalPlayer(Item itemPrefab, int quantity)
        {
            var localPlayer = Player.localPlayer;
            if (localPlayer == null)
            {
                Debug.LogError("[PEAK] Local player not found");
                return;
            }

            try
            {
                int itemsGiven = 0;
                for (int i = 0; i < quantity; i++)
                {
                    // Find an empty slot
                    bool slotFound = false;
                    for (byte slotIndex = 0; slotIndex < localPlayer.itemSlots.Length; slotIndex++)
                    {
                        var slot = localPlayer.itemSlots[slotIndex];
                        if (slot.IsEmpty())
                        {
                            // Create new item instance data
                            var itemData = new ItemInstanceData(System.Guid.NewGuid());
                            ItemInstanceDataHandler.AddInstanceData(itemData);
                            
                            // Set the slot data
                            slot.prefab = itemPrefab;
                            slot.data = itemData;
                            
                            // Sync inventory with other players
                            var syncData = Zorro.Core.Serizalization.IBinarySerializable.ToManagedArray<InventorySyncData>(
                                new InventorySyncData(
                                    localPlayer.itemSlots,
                                    localPlayer.backpackSlot,
                                    localPlayer.tempFullSlot
                                )
                            );
                            
                            localPlayer.photonView.RPC("SyncInventoryRPC", Photon.Pun.RpcTarget.Others, syncData, true);
                            
                            itemsGiven++;
                            slotFound = true;
                            break;
                        }
                    }
                    
                    if (!slotFound)
                    {
                        Debug.LogWarning($"[PEAK] Inventory full. Only gave {itemsGiven} out of {quantity} items");
                        break;
                    }
                }
                
                var displayName = itemPrefab.UIData?.itemName ?? itemPrefab.gameObject.name;
                Debug.Log($"[PEAK] Added {itemsGiven}x {displayName} to local inventory");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PEAK] Failed to add item to local inventory: {ex.Message}");
            }
        }

        private static void SpawnItemsNearPlayer(Character targetPlayer, Item itemPrefab, int quantity)
        {
            if (!Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[PEAK] Item spawning requires Master Client privileges");
                Debug.Log("[PEAK] Attempting to spawn anyway...");
            }

            try
            {
                for (int i = 0; i < quantity; i++)
                {
                    // Calculate spawn position near the player
                    Vector3 basePosition = targetPlayer.Center;
                    Vector3 randomOffset = new Vector3(
                        UnityEngine.Random.Range(-2f, 2f),
                        UnityEngine.Random.Range(1f, 3f),
                        UnityEngine.Random.Range(-2f, 2f)
                    );
                    Vector3 spawnPosition = basePosition + randomOffset;
                    
                    // Spawn the item using PhotonNetwork
                    var spawnedObject = Photon.Pun.PhotonNetwork.Instantiate(
                        "0_Items/" + itemPrefab.gameObject.name, 
                        spawnPosition, 
                        UnityEngine.Quaternion.identity
                    );
                    
                    if (spawnedObject != null)
                    {
                        var spawnedItem = spawnedObject.GetComponent<Item>();
                        if (spawnedItem != null)
                        {
                            // Set the item to non-kinematic so it falls naturally
                            spawnedItem.SetKinematicNetworked(false, spawnPosition, UnityEngine.Quaternion.identity);
                        }
                    }
                }
                
                var displayName = itemPrefab.UIData?.itemName ?? itemPrefab.gameObject.name;
                Debug.Log($"[PEAK] Spawned {quantity}x {displayName} near {targetPlayer.characterName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PEAK] Failed to spawn item near player: {ex.Message}");
                
                // Fallback: Try using CharacterItems.SpawnItemInHand via reflection
                try
                {
                    if (targetPlayer.refs?.items != null)
                    {
                        var spawnItemMethod = typeof(CharacterItems).GetMethod("SpawnItemInHand", 
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        
                        if (spawnItemMethod != null)
                        {
                            for (int i = 0; i < quantity; i++)
                            {
                                spawnItemMethod.Invoke(targetPlayer.refs.items, new object[] { itemPrefab.gameObject.name });
                            }
                            Debug.Log($"[PEAK] Used fallback method to spawn items for {targetPlayer.characterName}");
                        }
                        else
                        {
                            Debug.LogError("[PEAK] SpawnItemInHand method not found via reflection");
                        }
                    }
                }
                catch (System.Exception fallbackEx)
                {
                    Debug.LogError($"[PEAK] Fallback method also failed: {fallbackEx.Message}");
                }
            }
        }
    }
}