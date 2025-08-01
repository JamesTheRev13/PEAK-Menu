using PEAK_Menu.Config;
using System.Collections.Generic;
using UnityEngine;

namespace PEAK_Menu.Menu.UI.Tabs
{
    public class ConsoleTab : BaseTab
    {
        private string _consoleInput = "";

        public ConsoleTab(MenuManager menuManager, List<string> consoleOutput)
            : base(menuManager, consoleOutput) { }

        public override void Draw()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(UIConstants.CONSOLE_HEIGHT));
            foreach (var line in _consoleOutput)
            {
                GUILayout.Label(line);
            }
            GUILayout.EndScrollView();

            DrawInputArea();
        }

        private void DrawInputArea()
        {
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("ConsoleInput");

            _consoleInput = GUILayout.TextField(_consoleInput);

            bool shouldExecute = CheckForExecuteCommand();

            if (GUILayout.Button("Execute", GUILayout.Width(UIConstants.BUTTON_EXECUTE_WIDTH)))
            {
                shouldExecute = true;
            }

            if (shouldExecute)
            {
                ExecuteCommand();
            }

            GUILayout.EndHorizontal();
        }

        private bool CheckForExecuteCommand()
        {
            if (Event.current.type == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                if (GUI.GetNameOfFocusedControl() == "ConsoleInput" ||
                    string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()))
                {
                    Event.current.Use();
                    return true;
                }
            }
            return false;
        }

        private void ExecuteCommand()
        {
            if (string.IsNullOrWhiteSpace(_consoleInput))
                return;

            AddToConsole($"> {_consoleInput}");
            _menuManager.ExecuteCommand(_consoleInput);
            _consoleInput = "";
        }

        public void SetScrollToBottom()
        {
            _scrollPosition = new Vector2(0, float.MaxValue);
        }
    }
}