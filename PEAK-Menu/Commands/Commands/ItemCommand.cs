using PEAK_Menu.Utils;
using System.Linq;
using UnityEngine;

namespace PEAK_Menu.Commands
{
    public class ItemCommand : BaseCommand
    {
        public override string Name => "item";
        public override string Description => "Item management and spawning commands";
        
        public override string DetailedHelp =>
@"=== ITEM Command Help ===
Item management and spawning commands

Usage: item [subcommand] [parameters]

Subcommands:
  list                          - List all available items
  search <partial_name>         - Search for items by name
  give <player> <item> [qty]    - Give item to player (default qty: 1)
  spawn <item> [qty]            - Spawn item near you (default qty: 1)
  drop <item> [qty]             - Drop item at your feet (default qty: 1)
  
Examples:
  item list
  item search rope
  item give ""Player1"" rope 3
  item spawn bugle 5
  item drop rope 2

Note: Player names with spaces must be quoted.
'give' adds to inventory for local player, spawns for remote players.
'spawn' and 'drop' always spawn items in the world.
Item spawning requires Master Client privileges for remote players.";

        public override void Execute(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                HandleListItems();
                return;
            }

            var parsed = ParameterParser.ParseSubCommand(parameters);

            switch (parsed.Action.ToLower())
            {
                case "list":
                    HandleListItems();
                    break;

                case "search":
                    if (parsed.RemainingParameters.Length > 0)
                    {
                        HandleSearchItems(parsed.RemainingParameters[0]);
                    }
                    else
                    {
                        LogError("Search requires a search term");
                        LogInfo("Usage: item search <partial_name>");
                    }
                    break;

                case "give":
                    if (parsed.RemainingParameters.Length >= 2)
                    {
                        var playerName = parsed.RemainingParameters[0];
                        var itemName = parsed.RemainingParameters[1];
                        var quantity = 1;
                        
                        if (parsed.RemainingParameters.Length > 2 && int.TryParse(parsed.RemainingParameters[2], out int qty))
                        {
                            quantity = qty;
                        }
                        
                        HandleGiveItem(playerName, itemName, quantity);
                    }
                    else
                    {
                        LogError("Give requires player name and item name");
                        LogInfo("Usage: item give <player> <item> [quantity]");
                    }
                    break;

                case "spawn":
                    if (parsed.RemainingParameters.Length > 0)
                    {
                        var itemName = parsed.RemainingParameters[0];
                        var quantity = 1;
                        
                        if (parsed.RemainingParameters.Length > 1 && int.TryParse(parsed.RemainingParameters[1], out int qty))
                        {
                            quantity = qty;
                        }
                        
                        HandleSpawnItem(itemName, quantity);
                    }
                    else
                    {
                        LogError("Spawn requires an item name");
                        LogInfo("Usage: item spawn <item> [quantity]");
                    }
                    break;

                case "drop":
                    if (parsed.RemainingParameters.Length > 0)
                    {
                        var itemName = parsed.RemainingParameters[0];
                        var quantity = 1;
                        
                        if (parsed.RemainingParameters.Length > 1 && int.TryParse(parsed.RemainingParameters[1], out int qty))
                        {
                            quantity = qty;
                        }
                        
                        HandleDropItem(itemName, quantity);
                    }
                    else
                    {
                        LogError("Drop requires an item name");
                        LogInfo("Usage: item drop <item> [quantity]");
                    }
                    break;

                default:
                    LogError($"Unknown item command: {parsed.Action}");
                    LogInfo("Available subcommands: list, search, give, spawn, drop");
                    LogInfo("Use 'help item' for detailed help");
                    break;
            }
        }

        private void HandleListItems()
        {
            var itemHelper = ItemDiscoveryHelper.Instance;
            var allItems = itemHelper.GetAllAvailableItems();
            
            if (allItems.Count == 0)
            {
                LogWarning("No items found. Items are discovered dynamically when the game loads.");
                return;
            }

            LogInfo("=== Available Items ===");
            LogInfo($"Found {allItems.Count} items:");
            
            foreach (var kvp in allItems)
            {
                var displayName = kvp.Value?.UIData?.itemName ?? kvp.Key;
                LogInfo($"  {kvp.Key} -> {displayName}");
            }
        }

        private void HandleSearchItems(string searchTerm)
        {
            var itemHelper = ItemDiscoveryHelper.Instance;
            var matchingItems = itemHelper.SearchItems(searchTerm);
            
            if (matchingItems.Count == 0)
            {
                LogInfo($"No items found matching '{searchTerm}'");
                return;
            }

            LogInfo($"=== Items matching '{searchTerm}' ===");
            foreach (var kvp in matchingItems)
            {
                var displayName = kvp.Value?.UIData?.itemName ?? kvp.Key;
                LogInfo($"  {kvp.Key} -> {displayName}");
            }
        }

        private void HandleGiveItem(string playerName, string itemName, int quantity)
        {
            // Find the target player
            var targetPlayer = Character.AllCharacters?.FirstOrDefault(c => 
                c?.characterName?.ToLower() == playerName.ToLower());
            
            if (targetPlayer == null)
            {
                LogError($"Player '{playerName}' not found");
                return;
            }

            // Find the item
            var itemHelper = ItemDiscoveryHelper.Instance;
            var itemPrefab = itemHelper.FindItemByName(itemName);
            
            if (itemPrefab == null)
            {
                LogError($"Item '{itemName}' not found");
                LogInfo("Use 'item search <name>' to find available items");
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
                LogInfo($"[ITEM] Gave {quantity}x {displayName} to {targetPlayer.characterName}");
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to give item: {ex.Message}");
            }
        }

        private void HandleSpawnItem(string itemName, int quantity)
        {
            var localCharacter = Character.localCharacter;
            if (localCharacter == null)
            {
                LogError("No local character found");
                return;
            }

            HandleGiveItem(localCharacter.characterName, itemName, quantity);
        }

        private void HandleDropItem(string itemName, int quantity)
        {
            var localCharacter = Character.localCharacter;
            if (localCharacter == null)
            {
                LogError("No local character found");
                return;
            }

            // Find the item
            var itemHelper = ItemDiscoveryHelper.Instance;
            var itemPrefab = itemHelper.FindItemByName(itemName);
            
            if (itemPrefab == null)
            {
                LogError($"Item '{itemName}' not found");
                LogInfo("Use 'item search <name>' to find available items");
                return;
            }

            try
            {
                // Spawn the item at the player's position
                var displayName = itemPrefab.UIData?.itemName ?? itemPrefab.gameObject.name;

                // Log the drop action
                LogInfo($"[ITEM] Dropped {quantity}x {displayName} at {localCharacter.characterName}'s feet");

                // Use the spawn method to drop the item
                SpawnItemsNearPlayer(localCharacter, itemPrefab, quantity);
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to drop item: {ex.Message}");
            }
        }

        // Copy the item giving methods from MenuUI
        private void GiveItemToLocalPlayer(Item itemPrefab, int quantity)
        {
            var localPlayer = Player.localPlayer;
            if (localPlayer == null)
            {
                LogError("Local player not found");
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
                        LogWarning($"Inventory full. Only gave {itemsGiven} out of {quantity} items");
                        break;
                    }
                }
                
                var displayName = itemPrefab.UIData?.itemName ?? itemPrefab.gameObject.name;
                LogInfo($"Added {itemsGiven}x {displayName} to local inventory");
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to add item to local inventory: {ex.Message}");
            }
        }

        private void SpawnItemsNearPlayer(Character targetPlayer, Item itemPrefab, int quantity)
        {
            if (!Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                LogWarning("Item spawning requires Master Client privileges");
                LogInfo("Attempting to spawn anyway...");
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
                LogInfo($"Spawned {quantity}x {displayName} near {targetPlayer.characterName}");
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to spawn item near player: {ex.Message}");
                
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
                            LogInfo($"Used fallback method to spawn items for {targetPlayer.characterName}");
                        }
                        else
                        {
                            LogError("SpawnItemInHand method not found via reflection");
                        }
                    }
                }
                catch (System.Exception fallbackEx)
                {
                    LogError($"Fallback method also failed: {fallbackEx.Message}");
                }
            }
        }

        public override bool CanExecute()
        {
            return true; // Item commands can always be attempted
        }
    }
}