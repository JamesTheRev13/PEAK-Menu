using UnityEngine;
using UnityEngine.UIElements;
using Zorro.Core.CLI;
using System.Linq;
using PEAK_Menu.Utils;

namespace PEAK_Menu.Utils.DebugPages
{
    public class AdminDebugPage : BaseCustomDebugPage
    {
        private DropdownField _playerDropdown;
        private DropdownField _itemDropdown;
        private FloatField _teleportX, _teleportY, _teleportZ;
        private Character[] _allPlayers;

        protected override void BuildContent()
        {
            BuildTeleportSection();
            BuildPlayerManagementSection();
            BuildItemManagementSection();
            BuildDebugConsoleSection();
            BuildEmergencyActionsSection();
        }

        private void BuildTeleportSection()
        {
            var section = CreateSection("Teleport Controls");

            // Coordinate input fields
            var coordContainer = new VisualElement();
            coordContainer.style.flexDirection = FlexDirection.Row;
            coordContainer.style.marginBottom = 10;

            _teleportX = new FloatField("X:") { value = 0f };
            _teleportX.style.width = 80;
            _teleportX.style.marginRight = 5;

            _teleportY = new FloatField("Y:") { value = 0f };
            _teleportY.style.width = 80;
            _teleportY.style.marginRight = 5;

            _teleportZ = new FloatField("Z:") { value = 0f };
            _teleportZ.style.width = 80;

            coordContainer.Add(_teleportX);
            coordContainer.Add(_teleportY);
            coordContainer.Add(_teleportZ);

            section.Add(coordContainer);

            // Teleport buttons
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;

            buttonContainer.Add(CreateButton("Teleport to Coords", () =>
            {
                AdminUIHelper.TeleportToCoordinates(_teleportX.value, _teleportY.value, _teleportZ.value);
                AddToConsole($"[ADMIN] Teleported to coordinates: {_teleportX.value:F1}, {_teleportY.value:F1}, {_teleportZ.value:F1}");
            }));

            buttonContainer.Add(CreateButton("Get Current Position", () =>
            {
                var localCharacter = Character.localCharacter;
                var pos = localCharacter.Center;
                _teleportX.value = pos.x;
                _teleportY.value = pos.y;
                _teleportZ.value = pos.z;
                AddToConsole($"[ADMIN] Current position: {pos.x:F1}, {pos.y:F1}, {pos.z:F1}");
            }));

            section.Add(buttonContainer);
            _scrollView.Add(section);
        }

        private void BuildPlayerManagementSection()
        {
            var section = CreateSection("Player Management");

            // Player dropdown
            RefreshPlayerList();
            if (_allPlayers.Length > 0)
            {
                var playerNames = _allPlayers.Select(p => p.characterName).ToList();
                _playerDropdown = new DropdownField("Select Player:", playerNames, 0);
                section.Add(_playerDropdown);

                // Player action buttons
                var actionContainer = new VisualElement();
                actionContainer.style.flexDirection = FlexDirection.Row;
                actionContainer.style.marginTop = 10;

                actionContainer.Add(CreateButton("Heal", () => ExecutePlayerAction("heal")));
                actionContainer.Add(CreateButton("Kill", () => ExecutePlayerAction("kill")));
                actionContainer.Add(CreateButton("Revive", () => ExecutePlayerAction("revive")));
                actionContainer.Add(CreateButton("Goto", () => ExecutePlayerAction("goto")));
                actionContainer.Add(CreateButton("Bring", () => ExecutePlayerAction("bring")));

                section.Add(actionContainer);
            }
            else
            {
                section.Add(CreateLabel("No players found"));
            }

            section.Add(CreateButton("Refresh Player List", RefreshPlayerList));
            _scrollView.Add(section);
        }

        private void BuildItemManagementSection()
        {
            var section = CreateSection("Item Management");

            // Item dropdown
            var itemHelper = ItemDiscoveryHelper.Instance;
            var allItemNames = itemHelper.GetItemNamesArray();

            if (allItemNames.Length > 1) // More than just "Select Item..."
            {
                _itemDropdown = new DropdownField("Select Item:", allItemNames.ToList(), 0);
                section.Add(_itemDropdown);

                // Item action buttons
                var itemActionContainer = new VisualElement();
                itemActionContainer.style.flexDirection = FlexDirection.Row;
                itemActionContainer.style.marginTop = 10;

                itemActionContainer.Add(CreateButton("Give Item", () => GiveItemToSelectedPlayer()));
                itemActionContainer.Add(CreateButton("Drop Item", () => DropItemNearSelectedPlayer()));

                section.Add(itemActionContainer);
            }
            else
            {
                section.Add(CreateLabel("No items discovered yet"));
                section.Add(CreateButton("Refresh Items", () =>
                {
                    itemHelper.RefreshItems();
                    AddToConsole("Item list refreshed");
                    BuildContent();
                }));
            }

            _scrollView.Add(section);
        }

        private void BuildDebugConsoleSection()
        {
            var section = CreateSection("Debug Console");

            var debugConsoleManager = Plugin.Instance?._menuManager?.GetDebugConsoleManager();
            if (debugConsoleManager != null)
            {
                var isOpen = debugConsoleManager.IsDebugConsoleOpen;

                section.Add(CreateToggle("Debug Console Open", isOpen, (enabled) =>
                {
                    debugConsoleManager.ToggleDebugConsole();
                    AddToConsole($"Debug console {(debugConsoleManager.IsDebugConsoleOpen ? "opened" : "closed")}");
                }));

                section.Add(CreateLabel($"Hotkey: {Plugin.PluginConfig.DebugConsoleToggleKey.Value}"));
                section.Add(CreateLabel("Access full game console with command history & autocomplete"));
            }

            _scrollView.Add(section);
        }

        private void BuildEmergencyActionsSection()
        {
            var section = CreateSection("Emergency Actions");

            section.Add(CreateButton("Emergency Heal All", () =>
            {
                AdminUIHelper.ExecuteQuickAction("heal-all");
                AddToConsole("[ADMIN] Emergency heal all executed");
            }));

            section.Add(CreateButton("List All Players", () =>
            {
                AdminUIHelper.ExecuteQuickAction("list-all");
                AddToConsole("[ADMIN] Listed all players");
            }));

            _scrollView.Add(section);
        }

        private void RefreshPlayerList()
        {
            _allPlayers = Character.AllCharacters?.ToArray() ?? new Character[0];

            if (_playerDropdown != null && _allPlayers.Length > 0)
            {
                _playerDropdown.choices = _allPlayers.Select(p => p.characterName).ToList();
            }
        }

        private void ExecutePlayerAction(string action)
        {
            if (_playerDropdown != null && _allPlayers != null && _playerDropdown.index >= 0 && _playerDropdown.index < _allPlayers.Length)
            {
                var selectedPlayer = _allPlayers[_playerDropdown.index];
                AdminUIHelper.ExecuteQuickAction(action, selectedPlayer.characterName);
                AddToConsole($"[ADMIN] {action} action for {selectedPlayer.characterName}");
            }
            else
            {
                AddToConsole("No player selected or invalid selection");
            }
        }

        private void GiveItemToSelectedPlayer()
        {
            if (_playerDropdown != null && _itemDropdown != null && _allPlayers != null &&
                _playerDropdown.index >= 0 && _playerDropdown.index < _allPlayers.Length &&
                _itemDropdown.index > 0) // Skip "Select Item..."
            {
                var selectedPlayer = _allPlayers[_playerDropdown.index];
                var itemName = _itemDropdown.value;

                // Use the EXACT same logic as AdminTab.GiveItemToPlayer
                GiveItemToPlayer(selectedPlayer, itemName, 1);
            }
            else
            {
                AddToConsole("Please select both a player and an item");
            }
        }

        private void DropItemNearSelectedPlayer()
        {
            if (_playerDropdown != null && _itemDropdown != null && _allPlayers != null &&
                _playerDropdown.index >= 0 && _playerDropdown.index < _allPlayers.Length &&
                _itemDropdown.index > 0) // Skip "Select Item..."
            {
                var selectedPlayer = _allPlayers[_playerDropdown.index];
                var itemName = _itemDropdown.value;

                // Use the EXACT same logic as AdminTab.DropItemNearPlayer
                DropItemNearPlayer(selectedPlayer, itemName, 1);
            }
            else
            {
                AddToConsole("Please select both a player and an item");
            }
        }

        // COPIED DIRECTLY FROM AdminTab.cs - EXACT SAME IMPLEMENTATION
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

        // COPIED DIRECTLY FROM AdminTab.cs - EXACT SAME IMPLEMENTATION
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

        // COPIED DIRECTLY FROM AdminTab.cs - EXACT SAME IMPLEMENTATION
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

        public override VisualElement FocusOnDefault()
        {
            return _scrollView;
        }
    }
}