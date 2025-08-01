using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PEAK_Menu.Utils
{
    public class ItemDiscoveryHelper
    {
        private static ItemDiscoveryHelper _instance;
        public static ItemDiscoveryHelper Instance => _instance ??= new ItemDiscoveryHelper();

        private Dictionary<string, Item> _discoveredItems;
        private bool _hasScannedItems = false;

        private ItemDiscoveryHelper()
        {
            _discoveredItems = new Dictionary<string, Item>();
        }

        public Dictionary<string, Item> GetAllAvailableItems()
        {
            if (!_hasScannedItems)
            {
                ScanForItems();
            }
            return new Dictionary<string, Item>(_discoveredItems);
        }

        public Item FindItemByName(string itemName)
        {
            if (!_hasScannedItems)
            {
                ScanForItems();
            }

            // Try exact match first
            if (_discoveredItems.TryGetValue(itemName.ToLower(), out Item exactMatch))
            {
                return exactMatch;
            }

            // Try partial matches
            var partialMatches = _discoveredItems.Where(kvp => 
                kvp.Key.Contains(itemName.ToLower()) || 
                kvp.Value?.UIData?.itemName?.ToLower().Contains(itemName.ToLower()) == true);

            return partialMatches.FirstOrDefault().Value;
        }

        public Dictionary<string, Item> SearchItems(string searchTerm)
        {
            if (!_hasScannedItems)
            {
                ScanForItems();
            }

            var results = new Dictionary<string, Item>();
            var lowerSearchTerm = searchTerm.ToLower();

            foreach (var kvp in _discoveredItems)
            {
                var key = kvp.Key;
                var item = kvp.Value;
                var itemDisplayName = item?.UIData?.itemName?.ToLower() ?? "";

                if (key.Contains(lowerSearchTerm) || itemDisplayName.Contains(lowerSearchTerm))
                {
                    results[key] = item;
                }
            }

            return results;
        }

        public string[] GetItemNamesArray()
        {
            if (!_hasScannedItems)
            {
                ScanForItems();
            }

            var names = new List<string> { "Select Item..." };
            names.AddRange(_discoveredItems.Keys.OrderBy(name => name));
            return names.ToArray();
        }

        public void RefreshItems()
        {
            _hasScannedItems = false;
            _discoveredItems.Clear();
            ScanForItems();
        }

        private void ScanForItems()
        {
            try
            {
                Plugin.Log.LogInfo("Scanning for available items...");
                _discoveredItems.Clear();

                // Search through all Item prefabs in resources
                var allItems = UnityEngine.Resources.FindObjectsOfTypeAll<Item>();
                
                foreach (var item in allItems)
                {
                    // Check if this is a prefab (not instantiated in scene)
                    if (item != null && 
                        item.gameObject.scene.handle == 0 && 
                        string.IsNullOrEmpty(item.gameObject.scene.name))
                    {
                        // Use the GameObject name as the key (cleaned up)
                        var itemKey = item.gameObject.name
                            .ToLower()
                            .Replace("(clone)", "")
                            .Trim();

                        if (!string.IsNullOrEmpty(itemKey) && !_discoveredItems.ContainsKey(itemKey))
                        {
                            _discoveredItems[itemKey] = item;
                        }
                    }
                }

                _hasScannedItems = true;
                Plugin.Log.LogInfo($"Found {_discoveredItems.Count} available items");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error scanning for items: {ex.Message}");
                _hasScannedItems = true; // Prevent infinite retries
            }
        }
    }
}