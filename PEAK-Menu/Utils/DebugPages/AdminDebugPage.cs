using UnityEngine;
using UnityEngine.UIElements;
using Zorro.Core.CLI;
using System.Linq;
using PEAK_Menu.Utils;

namespace PEAK_Menu.Utils.DebugPages
{
    public class AdminDebugPage : BaseCustomDebugPage
    {
        private FloatField _teleportX, _teleportY, _teleportZ;
        private DropdownField _playerDropdown;
        private DropdownField _itemDropdown;

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

            // Live coordinate display
            section.Add(CreateLiveLabel("Current Position: ", () => {
                var character = Character.localCharacter;
                return character?.Center.ToString("F1") ?? "N/A";
            }));

            // Coordinate input fields in a row
            var coordContainer = CreateRowContainer();
            
            var xContainer = new VisualElement();
            xContainer.style.flexDirection = FlexDirection.Row;
            xContainer.style.alignItems = Align.Center;
            xContainer.style.marginRight = 10;
            var xLabel = CreateLabel("X:");
            xLabel.style.width = 20;
            xLabel.style.marginRight = 5;
            _teleportX = new FloatField() { value = 0f };
            _teleportX.style.width = 80;
            _teleportX.style.color = Color.white;
            _teleportX.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            xContainer.Add(xLabel);
            xContainer.Add(_teleportX);

            var yContainer = new VisualElement();
            yContainer.style.flexDirection = FlexDirection.Row;
            yContainer.style.alignItems = Align.Center;
            yContainer.style.marginRight = 10;
            var yLabel = CreateLabel("Y:");
            yLabel.style.width = 20;
            yLabel.style.marginRight = 5;
            _teleportY = new FloatField() { value = 0f };
            _teleportY.style.width = 80;
            _teleportY.style.color = Color.white;
            _teleportY.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            yContainer.Add(yLabel);
            yContainer.Add(_teleportY);

            var zContainer = new VisualElement();
            zContainer.style.flexDirection = FlexDirection.Row;
            zContainer.style.alignItems = Align.Center;
            var zLabel = CreateLabel("Z:");
            zLabel.style.width = 20;
            zLabel.style.marginRight = 5;
            _teleportZ = new FloatField() { value = 0f };
            _teleportZ.style.width = 80;
            _teleportZ.style.color = Color.white;
            _teleportZ.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            zContainer.Add(zLabel);
            zContainer.Add(_teleportZ);

            coordContainer.Add(xContainer);
            coordContainer.Add(yContainer);
            coordContainer.Add(zContainer);
            section.Add(coordContainer);

            // Teleport buttons
            var buttonContainer = CreateRowContainer();

            var teleportButton = CreateButton("Teleport to Coords", () =>
            {
                AdminUIHelper.TeleportToCoordinates(_teleportX.value, _teleportY.value, _teleportZ.value);
                AddToConsole($"[ADMIN] Teleported to coordinates: {_teleportX.value:F1}, {_teleportY.value:F1}, {_teleportZ.value:F1}");
            });
            teleportButton.style.width = 150;

            var getCurrentPosButton = CreateButton("Get Current Position", () =>
            {
                var localCharacter = Character.localCharacter;
                if (localCharacter != null)
                {
                    var pos = localCharacter.Center;
                    _teleportX.value = pos.x;
                    _teleportY.value = pos.y;
                    _teleportZ.value = pos.z;
                    AddToConsole($"[ADMIN] Current position: {pos.x:F1}, {pos.y:F1}, {pos.z:F1}");
                }
            });
            getCurrentPosButton.style.width = 150;

            buttonContainer.Add(teleportButton);
            buttonContainer.Add(getCurrentPosButton);
            section.Add(buttonContainer);
            
            _scrollView.Add(section);
        }

        private void BuildPlayerManagementSection()
        {
            var section = CreateSection("Player Management");

            // Live player count
            section.Add(CreateLiveLabel("Players Online: ", () => {
                var playerCount = Character.AllCharacters?.Count() ?? 0;
                return playerCount.ToString();
            }));

            // Live reactive player dropdown
            _playerDropdown = CreateLiveDropdown("Select Player:", () => {
                var players = Character.AllCharacters?.ToArray() ?? new Character[0];
                return players.Select(p => $"{p.characterName} {(p.data.dead ? "[DEAD]" : p.data.passedOut ? "[OUT]" : "[OK]")}").ToList();
            }, 0, (selectedPlayerName) => {
                // Player selection changed - could add logic here if needed
            });

            // Player action buttons
            var actionContainer = CreateRowContainer();

            var actions = new[] { "Heal", "Kill", "Revive", "Goto", "Bring" };
            foreach (var action in actions)
            {
                var button = CreateButton(action, () => {
                    var players = Character.AllCharacters?.ToArray() ?? new Character[0];
                    if (_playerDropdown.index >= 0 && _playerDropdown.index < players.Length)
                    {
                        var selectedPlayer = players[_playerDropdown.index];
                        AdminUIHelper.ExecuteQuickAction(action.ToLower(), selectedPlayer.characterName);
                        AddToConsole($"[ADMIN] {action} action for {selectedPlayer.characterName}");
                    }
                    else
                    {
                        AddToConsole("No player selected or invalid selection");
                    }
                });
                button.style.width = 70;
                button.style.marginRight = 5;
                actionContainer.Add(button);
            }

            section.Add(actionContainer);
            _scrollView.Add(section);
        }

        private void BuildItemManagementSection()
        {
            var section = CreateSection("Item Management");

            // Live item count
            section.Add(CreateLiveLabel("Items Available: ", () => {
                var itemHelper = ItemDiscoveryHelper.Instance;
                var itemCount = itemHelper.GetItemNamesArray().Length - 1; // Subtract "Select Item..."
                return itemCount.ToString();
            }));

            // Live reactive item dropdown
            _itemDropdown = CreateLiveDropdown("Select Item:", () => {
                var itemHelper = ItemDiscoveryHelper.Instance;
                return itemHelper.GetItemNamesArray().ToList();
            }, 0, (selectedItemName) => {
                // Item selection changed - could add logic here if needed
            });

            // Item action buttons
            var itemActionContainer = CreateRowContainer();

            var giveButton = CreateButton("Give Item", () => {
                var players = Character.AllCharacters?.ToArray() ?? new Character[0];
                
                if (_playerDropdown != null && _itemDropdown != null &&
                    _playerDropdown.index >= 0 && _playerDropdown.index < players.Length &&
                    _itemDropdown.index > 0) // Skip "Select Item..."
                {
                    var selectedPlayer = players[_playerDropdown.index];
                    var itemName = _itemDropdown.value;
                    GiveItemToPlayer(selectedPlayer, itemName, 1);
                }
                else
                {
                    AddToConsole("Please select both a player and an item");
                }
            });
            giveButton.style.width = 100;
            giveButton.style.marginRight = 5;

            var dropButton = CreateButton("Drop Item", () => {
                var players = Character.AllCharacters?.ToArray() ?? new Character[0];
                
                if (_playerDropdown != null && _itemDropdown != null &&
                    _playerDropdown.index >= 0 && _playerDropdown.index < players.Length &&
                    _itemDropdown.index > 0) // Skip "Select Item..."
                {
                    var selectedPlayer = players[_playerDropdown.index];
                    var itemName = _itemDropdown.value;
                    DropItemNearPlayer(selectedPlayer, itemName, 1);
                }
                else
                {
                    AddToConsole("Please select both a player and an item");
                }
            });
            dropButton.style.width = 100;

            itemActionContainer.Add(giveButton);
            itemActionContainer.Add(dropButton);
            section.Add(itemActionContainer);

            _scrollView.Add(section);
        }

        private void BuildDebugConsoleSection()
        {
            var section = CreateSection("Debug Console");

            var debugConsoleManager = Plugin.Instance?._menuManager?.GetDebugConsoleManager();
            if (debugConsoleManager != null)
            {
                // Live reactive toggle for debug console state
                section.Add(CreateLiveToggle("Debug Console Open", 
                    () => debugConsoleManager.IsDebugConsoleOpen,
                    (enabled) => {
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

            var buttonContainer = CreateRowContainer();

            var healAllButton = CreateButton("Emergency Heal All", () =>
            {
                AdminUIHelper.ExecuteQuickAction("heal-all");
                AddToConsole("[ADMIN] Emergency heal all executed");
            });
            healAllButton.style.width = 150;

            var listPlayersButton = CreateButton("List All Players", () =>
            {
                AdminUIHelper.ExecuteQuickAction("list-all");
                AddToConsole("[ADMIN] Listed all players");
            });
            listPlayersButton.style.width = 150;

            buttonContainer.Add(healAllButton);
            buttonContainer.Add(listPlayersButton);
            section.Add(buttonContainer);

            _scrollView.Add(section);
        }

        // Keep the existing item methods from AdminTab
        private void GiveItemToPlayer(Character targetPlayer, string itemName, int quantity)
        {
            if (targetPlayer == null || string.IsNullOrEmpty(itemName))
            {
                AddToConsole("[ERROR] Invalid player or item name");
                return;
            }

            try
            {
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

        private void DropItemNearPlayer(Character targetPlayer, string itemName, int quantity)
        {
            if (targetPlayer == null || string.IsNullOrEmpty(itemName))
            {
                AddToConsole("[ERROR] Invalid player or item name");
                return;
            }

            try
            {
                var itemHelper = ItemDiscoveryHelper.Instance;
                var itemPrefab = itemHelper.FindItemByName(itemName);
                
                if (itemPrefab == null)
                {
                    AddToConsole($"[ERROR] Item '{itemName}' not found");
                    return;
                }

                SpawnItemsNearPlayer(targetPlayer, itemPrefab, quantity);
                AddToConsole($"[ADMIN] Dropped {quantity}x {itemName} near {targetPlayer.characterName}");
            }
            catch (System.Exception ex)
            {
                AddToConsole($"[ERROR] Failed to drop item: {ex.Message}");
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
                    Vector3 basePosition = targetPlayer.Center;
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-2f, 2f),
                        Random.Range(1f, 3f),
                        Random.Range(-2f, 2f)
                    );
                    Vector3 spawnPosition = basePosition + randomOffset;
                    
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
            }
        }

        public override VisualElement FocusOnDefault()
        {
            return _scrollView;
        }
    }
}