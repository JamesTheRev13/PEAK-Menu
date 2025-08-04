using PEAK_Menu.Config;
using PEAK_Menu.Menu.UI.Components;
using PEAK_Menu.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace PEAK_Menu.Menu.UI.Tabs
{
    public class AdminTab : BaseTab
    {
        private static float _teleportX = 0f;
        private static float _teleportY = 0f;
        private static float _teleportZ = 0f;

        private readonly PlayerDropdown _playerDropdown;
        private readonly ItemDropdown _itemDropdown;
        
        public AdminTab(MenuManager menuManager, List<string> consoleOutput) 
            : base(menuManager, consoleOutput)
        {
            _playerDropdown = new PlayerDropdown();
            _itemDropdown = new ItemDropdown();
        }

        public override void Draw()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(UIConstants.TAB_HEIGHT));
            
            GUILayout.Label("=== Admin Panel ===");
            GUILayout.Label("Administrative tools for player management");
            
            var localCharacter = Character.localCharacter;
            if (localCharacter == null)
            {
                GUILayout.Label("No character found");
                GUILayout.EndScrollView();
                return;
            }

            DrawTeleportCoordinatesSection();
            DrawPlayerManagementSection();
            DrawDevelopmentNotes();

            GUILayout.Space(UIConstants.LARGE_SPACING);
            GUILayout.EndScrollView();
        }

        // TODO: Move to player tab
        private void DrawTeleportCoordinatesSection()
        {
            GUILayout.Space(UIConstants.STANDARD_SPACING);
            GUILayout.Label("=== Teleport to Coordinates ===");
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("X:", GUILayout.Width(20));
            if (float.TryParse(GUILayout.TextField(_teleportX.ToString("F1"), GUILayout.Width(80)), out float newX))
                _teleportX = newX;
            GUILayout.Label("Y:", GUILayout.Width(20));
            if (float.TryParse(GUILayout.TextField(_teleportY.ToString("F1"), GUILayout.Width(80)), out float newY))
                _teleportY = newY;
            GUILayout.Label("Z:", GUILayout.Width(20));
            if (float.TryParse(GUILayout.TextField(_teleportZ.ToString("F1"), GUILayout.Width(80)), out float newZ))
                _teleportZ = newZ;
            GUILayout.EndHorizontal();
            
            DrawTeleportButtons();
        }
        private void DrawTeleportButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Teleport to Coords", GUILayout.Width(150)))
            {
                AdminUIHelper.TeleportToCoordinates(_teleportX, _teleportY, _teleportZ);
                AddToConsole($"[ADMIN] Teleported to coordinates: {_teleportX:F1}, {_teleportY:F1}, {_teleportZ:F1}");
            }
            if (GUILayout.Button("Get Current Position", GUILayout.Width(150)))
            {
                var localCharacter = Character.localCharacter;
                var pos = localCharacter.Center;
                _teleportX = pos.x;
                _teleportY = pos.y;
                _teleportZ = pos.z;
                AddToConsole($"[ADMIN] Current position: {pos.x:F1}, {pos.y:F1}, {pos.z:F1}");
            }
            GUILayout.EndHorizontal();
        }

        private void DrawPlayerManagementSection()
        {
            GUILayout.Space(UIConstants.STANDARD_SPACING);
            GUILayout.Label("=== Player Management ===");
            
            GUILayout.Label("Select Player:");
            _playerDropdown.Draw(AddToConsole);

            if (_playerDropdown.SelectedPlayer != null)
            {
                DrawSelectedPlayerActions();
            }
            else
            {
                GUILayout.Space(UIConstants.STANDARD_SPACING);
                GUILayout.Label("Select a player from the dropdown above to perform actions", GUI.skin.box);
            }
        }

        private void DrawSelectedPlayerActions()
        {
            var selectedPlayer = _playerDropdown.SelectedPlayer;
            
            GUILayout.Space(UIConstants.STANDARD_SPACING);
            GUILayout.Label($"=== Actions for: {selectedPlayer.characterName} ===");
            
            DrawPlayerStatus(selectedPlayer);
            DrawBasicActions(selectedPlayer);
            DrawItemManagementSection(selectedPlayer);
        }

        private void DrawPlayerStatus(Character player)
        {
            var status = player.data.dead ? "[DEAD]" : 
                        player.data.passedOut ? "[OUT]" : "[OK]";
            var isLocal = player.IsLocal ? " (Local)" : " (Remote)";
            GUILayout.Label($"Status: {status}{isLocal}", GUI.skin.box);
        }

        private void DrawBasicActions(Character player)
        {
            GUILayout.BeginHorizontal();
            
            var actions = new[]
            {
                ("Heal", "heal"),
                ("Goto", "goto"),
                ("Bring", "bring"),
                (player.data.dead || player.data.fullyPassedOut ? "Revive" : "Kill", 
                 player.data.dead || player.data.fullyPassedOut ? "revive" : "kill")
            };

            foreach (var (label, action) in actions)
            {
                if (GUILayout.Button(label, GUILayout.Width(UIConstants.BUTTON_MEDIUM_WIDTH)))
                {
                    AdminUIHelper.ExecuteQuickAction(action, player.characterName);
                    AddToConsole($"[ADMIN] {label} action for {player.characterName}");
                }
            }
            
            GUILayout.EndHorizontal();
        }

        private void DrawItemManagementSection(Character player)
        {
            GUILayout.Space(UIConstants.STANDARD_SPACING);
            GUILayout.Label("=== Item Management ===");
            
            _itemDropdown.Draw();
            
            GUILayout.Space(UIConstants.SMALL_SPACING);
            DrawItemActions(player);
            DrawItemManagementHelp(player);
        }

        private void DrawItemActions(Character player)
        {
            GUILayout.BeginHorizontal();

            bool canGiveItem = _itemDropdown.SelectedItemIndex > 0;
            var buttonColor = GUI.backgroundColor;

            if (!canGiveItem)
            {
                GUI.backgroundColor = Color.gray;
            }

            if (GUILayout.Button("Give Item", GUILayout.Width(UIConstants.BUTTON_LARGE_WIDTH)) && canGiveItem)
            {
                var itemName = _itemDropdown.SelectedItemName;
                GiveItemToPlayer(player, itemName, 1);
            }

            if (GUILayout.Button("Drop Item", GUILayout.Width(UIConstants.BUTTON_LARGE_WIDTH)) && canGiveItem)
            {
                var itemName = _itemDropdown.SelectedItemName;
                DropItemNearPlayer(player, itemName, 1);
            }

            GUI.backgroundColor = buttonColor;
            GUILayout.EndHorizontal();
        }

        private void DrawItemManagementHelp(Character player)
        {
            if (_itemDropdown.SelectedItemIndex <= 0)
            {
                GUILayout.Label("Select an item from the dropdown to enable item actions", GUI.skin.box);
            }
            else if (!player.IsLocal && !Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                var statusColor = GUI.color;
                GUI.color = Color.yellow;
                GUILayout.Label("Warning: Not Master Client - spawning may fail", GUI.skin.box);
                GUI.color = statusColor;
            }
            
        }

        private void DrawDevelopmentNotes()
        {
            GUILayout.Space(UIConstants.STANDARD_SPACING);
            GUILayout.Label("=== Development Notes ===");
            GUILayout.Label("• Use console for advanced admin commands");
            
            var hotkeyText = Plugin.PluginConfig?.MenuToggleKey?.Value.ToString() ?? "Insert";
            var noClipHotkey = Plugin.PluginConfig?.NoClipToggleKey?.Value.ToString() ?? "Delete";
            GUILayout.Label($"• Hotkeys: Menu ({hotkeyText}), NoClip ({noClipHotkey})");
        }

        private void GiveItemToPlayer(Character targetPlayer, string itemName, int quantity)
        {
            if (targetPlayer == null || string.IsNullOrEmpty(itemName))
            {
                AddToConsole("[ERROR] Invalid player or item name");
                return;
            }

            try
            {
                // Find the item prefab by name
                var itemHelper = ItemDiscoveryHelper.Instance;
                var itemPrefab = itemHelper.FindItemByName(itemName);
                
                if (itemPrefab == null)
                {
                    AddToConsole($"[ERROR] Item '{itemName}' not found");
                    return;
                }

                //if (targetPlayer.IsLocal)
                //{
                //    GiveItemToLocalPlayer(itemPrefab, quantity);
                //}
                //else
                //{
                //    SpawnItemsNearPlayer(targetPlayer, itemPrefab, quantity);
                //}

                if (targetPlayer.refs?.items != null)
                {
                    if (targetPlayer.photonView != null)
                    {
                        for (int i = 0; i < quantity; i++)
                        {
                            targetPlayer.photonView.RPC("RPC_SpawnItemInHandMaster", Photon.Pun.RpcTarget.All, itemPrefab.gameObject.name);
                        }
                    }
                    else
                    {
                        AddToConsole("[ERROR] SpawnItemInHand method not found");
                    }
                }

                AddToConsole($"[ADMIN] Gave {quantity}x {itemName} to {targetPlayer.characterName}");
            }
            catch (System.Exception ex)
            {
                AddToConsole($"[ERROR] Failed to give item: {ex.Message}");
            }
        }

        private void DropItemNearPlayer(Character targetPlayer, string itemName, int quantity)
        {
            if (targetPlayer == null || string.IsNullOrEmpty(itemName))
            {
                AddToConsole("[ERROR] Invalid player or item name");
                return;
            }

            try
            {
                // Find the item prefab by name
                var itemHelper = ItemDiscoveryHelper.Instance;
                var itemPrefab = itemHelper.FindItemByName(itemName);
                
                if (itemPrefab == null)
                {
                    AddToConsole($"[ERROR] Item '{itemName}' not found");
                    return;
                }

                // Always spawn items at player's feet regardless of local/remote
                SpawnItemsNearPlayer(targetPlayer, itemPrefab, quantity);

                AddToConsole($"[ADMIN] Dropped {quantity}x {itemName} near {targetPlayer.characterName}");
            }
            catch (System.Exception ex)
            {
                AddToConsole($"[ERROR] Failed to drop item: {ex.Message}");
            }
        }

        private void GiveItemToLocalPlayer(Item itemPrefab, int quantity)
        {
            var localPlayer = Player.localPlayer;
            if (localPlayer == null)
            {
                AddToConsole("[ERROR] Local player not found");
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
                        AddToConsole($"[WARNING] Inventory full. Only gave {itemsGiven} out of {quantity} items");
                        break;
                    }
                }
                
                var displayName = itemPrefab.UIData?.itemName ?? itemPrefab.gameObject.name;
                AddToConsole($"[ADMIN] Added {itemsGiven}x {displayName} to inventory");
            }
            catch (System.Exception ex)
            {
                AddToConsole($"[ERROR] Failed to add item to local inventory: {ex.Message}");
            }
        }

        private void SpawnItemsNearPlayer(Character targetPlayer, Item itemPrefab, int quantity)
        {
            if (!Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                AddToConsole("[WARNING] Item spawning requires Master Client privileges");
                AddToConsole("[INFO] Attempting to spawn anyway...");
            }

            try
            {
                for (int i = 0; i < quantity; i++)
                {
                    // Calculate spawn position near the player
                    Vector3 basePosition = targetPlayer.Center;
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-2f, 2f),
                        Random.Range(1f, 3f),
                        Random.Range(-2f, 2f)
                    );
                    Vector3 spawnPosition = basePosition + randomOffset;
                    
                    // Spawn the item using PhotonNetwork
                    var spawnedObject = Photon.Pun.PhotonNetwork.Instantiate(
                        "0_Items/" + itemPrefab.gameObject.name, 
                        spawnPosition, 
                        Quaternion.identity
                    );
                    
                    if (spawnedObject != null)
                    {
                        var spawnedItem = spawnedObject.GetComponent<Item>();
                        if (spawnedItem != null)
                        {
                            // Set the item to non-kinematic so it falls naturally
                            spawnedItem.SetKinematicNetworked(false, spawnPosition, Quaternion.identity);
                        }
                    }
                }
                
                var displayName = itemPrefab.UIData?.itemName ?? itemPrefab.gameObject.name;
                AddToConsole($"[ADMIN] Spawned {quantity}x {displayName} near {targetPlayer.characterName}");
            }
            catch (System.Exception ex)
            {
                AddToConsole($"[ERROR] Failed to spawn item near player: {ex.Message}");
                
                // Fallback: Try using CharacterItems.SpawnItemInHand via reflection
                try
                {
                    if (targetPlayer.refs?.items != null)
                    {
                        if (targetPlayer.photonView != null)
                        {
                            for (int i = 0; i < quantity; i++)
                            {
                                targetPlayer.photonView.RPC("RPC_SpawnItemInHandMaster", Photon.Pun.RpcTarget.All, itemPrefab.gameObject.name);
                            }
                        }
                        else
                        {
                            AddToConsole("[ERROR] SpawnItemInHand method not found");
                        }
                    }
                }
                catch (System.Exception fallbackEx)
                {
                    AddToConsole($"[ERROR] Fallback method also failed: {fallbackEx.Message}");
                }
            }
        }

        public void HandleDropdownClicks()
        {
            _playerDropdown.HandleClickOutside();
            _itemDropdown.HandleClickOutside();
        }
    }
}