using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PEAK_Menu.Menu.UI.Components
{
    public class PlayerDropdown
    {
        private bool _showDropdown = false;
        private Vector2 _scrollPosition;
        private int _selectedIndex = -1;
        private Character _selectedPlayer = null;
        private string _selectedPlayerName = "Select Player...";

        public Character SelectedPlayer => _selectedPlayer;
        public string SelectedPlayerName => _selectedPlayerName;

        public void Draw(Action<string> addToConsole)
        {
            var allCharacters = Character.AllCharacters?.ToList();
            if (allCharacters == null || allCharacters.Count == 0)
            {
                GUILayout.Label("No players found");
                return;
            }

            DrawDropdownButton();
            
            if (_showDropdown)
            {
                DrawDropdownMenu(allCharacters, addToConsole);
            }
        }

        private void DrawDropdownButton()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Player:", GUILayout.Width(50));
            
            if (GUILayout.Button(_selectedPlayerName, GUILayout.Width(200)))
            {
                _showDropdown = !_showDropdown;
            }
            
            GUILayout.EndHorizontal();
        }

        private void DrawDropdownMenu(List<Character> characters, Action<string> addToConsole)
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(250), GUILayout.MaxHeight(150));
            
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(140));
            
            DrawClearOption(addToConsole);
            DrawPlayerOptions(characters, addToConsole);
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawClearOption(Action<string> addToConsole)
        {
            var originalColor = GUI.backgroundColor;
            if (_selectedIndex == -1)
            {
                GUI.backgroundColor = Color.cyan;
            }
            
            if (GUILayout.Button("Select Player...", GUILayout.Height(25)))
            {
                ClearSelection(addToConsole);
            }
            GUI.backgroundColor = originalColor;
        }

        private void DrawPlayerOptions(List<Character> characters, Action<string> addToConsole)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                var character = characters[i];
                if (character == null) continue;
                
                DrawPlayerOption(character, i, addToConsole);
            }
        }

        private void DrawPlayerOption(Character character, int index, Action<string> addToConsole)
        {
            var status = character.data.dead ? "[DEAD]" : 
                        character.data.passedOut ? "[OUT]" : "[OK]";
            
            var displayName = $"{character.characterName} {status}";
            var isSelected = _selectedIndex == index;
            
            var originalColor = GUI.backgroundColor;
            SetPlayerStatusColor(character, isSelected);
            
            if (GUILayout.Button(displayName, GUILayout.Height(25)))
            {
                SelectPlayer(character, index, displayName, addToConsole);
            }
            
            GUI.backgroundColor = originalColor;
        }

        private void SetPlayerStatusColor(Character character, bool isSelected)
        {
            if (isSelected)
            {
                GUI.backgroundColor = Color.cyan;
            }
            else if (character.data.dead)
            {
                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
            }
            else if (character.data.passedOut)
            {
                GUI.backgroundColor = new Color(0.8f, 0.8f, 0.4f);
            }
        }

        private void SelectPlayer(Character character, int index, string displayName, Action<string> addToConsole)
        {
            _selectedIndex = index;
            _selectedPlayer = character;
            _selectedPlayerName = displayName;
            _showDropdown = false;
            addToConsole($"[ADMIN] Selected player: {character.characterName}");
        }

        private void ClearSelection(Action<string> addToConsole)
        {
            _selectedIndex = -1;
            _selectedPlayer = null;
            _selectedPlayerName = "Select Player...";
            _showDropdown = false;
            addToConsole("[ADMIN] Player selection cleared");
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