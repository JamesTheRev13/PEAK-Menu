using UnityEngine;
using PEAK_Menu.Config;
using PEAK_Menu.Utils;

namespace PEAK_Menu.Menu.UI.Components
{
    public class ItemDropdown
    {
        private bool _showDropdown = false;
        private Vector2 _scrollPosition;
        private int _selectedIndex = 0;
        private string _selectedItemName = "Select Item...";
        private string[] _availableItems = { "Select Item..." };
        private bool _itemsInitialized = false;

        public int SelectedItemIndex => _selectedIndex;
        public string SelectedItemName => _selectedItemName;

        public void Draw()
        {
            if (!_itemsInitialized)
            {
                InitializeItemsList();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Item:", GUILayout.Width(40));

            if (GUILayout.Button(_selectedItemName, GUILayout.Width(150)))
            {
                _showDropdown = !_showDropdown;
            }

            if (GUILayout.Button("Load Items", GUILayout.Width(75)))
            {
                RefreshItemsList();
            }

            GUILayout.EndHorizontal();

            if (_showDropdown)
            {
                DrawDropdownMenu();
            }
        }

        private void DrawDropdownMenu()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(UIConstants.DROPDOWN_WIDTH), 
                GUILayout.MaxHeight(UIConstants.DROPDOWN_MAX_HEIGHT));

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, 
                GUILayout.Height(UIConstants.DROPDOWN_HEIGHT));

            for (int i = 0; i < _availableItems.Length; i++)
            {
                var item = _availableItems[i];
                var isSelected = _selectedIndex == i;

                var originalColor = GUI.backgroundColor;
                if (isSelected)
                {
                    GUI.backgroundColor = Color.cyan;
                }

                if (GUILayout.Button(item, GUILayout.Height(20)))
                {
                    _selectedIndex = i;
                    _selectedItemName = item;
                    _showDropdown = false;
                }

                GUI.backgroundColor = originalColor;
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (_itemsInitialized)
            {
                GUILayout.Label($"{_availableItems.Length - 1} items available", GUI.skin.box);
            }
            else
            {
                GUILayout.Label("Items loading...", GUI.skin.box);
            }
        }

        private void InitializeItemsList()
        {
            try
            {
                var itemHelper = ItemDiscoveryHelper.Instance;
                _availableItems = itemHelper.GetItemNamesArray();
                _itemsInitialized = true;
            }
            catch (System.Exception)
            {
                // Keep default array as fallback
            }
        }

        private void RefreshItemsList()
        {
            try
            {
                var itemHelper = ItemDiscoveryHelper.Instance;
                itemHelper.RefreshItems();
                _availableItems = itemHelper.GetItemNamesArray();
                _itemsInitialized = true;

                _selectedIndex = 0;
                _selectedItemName = "Select Item...";
            }
            catch (System.Exception)
            {
                // Handle error silently
            }
        }

        public void HandleClickOutside()
        {
            if (Event.current.type == EventType.MouseDown && _showDropdown)
            {
                _showDropdown = false;
            }
        }
    }
}