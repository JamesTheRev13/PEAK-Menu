using UnityEngine;
using System.Collections.Generic;
using PEAK_Menu.Config;

namespace PEAK_Menu.Menu.UI
{
    public abstract class BaseTab
    {
        protected readonly MenuManager _menuManager;
        protected Vector2 _scrollPosition;
        protected readonly List<string> _consoleOutput;

        protected BaseTab(MenuManager menuManager, List<string> consoleOutput)
        {
            _menuManager = menuManager;
            _consoleOutput = consoleOutput;
            _scrollPosition = Vector2.zero;
        }

        public abstract void Draw();

        protected void AddToConsole(string message)
        {
            _consoleOutput.Add(message);
            if (_consoleOutput.Count > UIConstants.CONSOLE_HISTORY_LIMIT)
            {
                _consoleOutput.RemoveAt(0);
            }
        }

        protected bool DrawToggleButton(string featureName, bool isEnabled, float width = 0, int buttonId = -1)
        {
            var controlRect = width > 0 ?
                GUILayoutUtility.GetRect(width, 20) :
                GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.button);

            var isHovered = controlRect.Contains(Event.current.mousePosition);

            string buttonText = isHovered ?
                (isEnabled ? $"{featureName} - DISABLE" : $"{featureName} - ENABLE") :
                (isEnabled ? $"{featureName} - ON" : $"{featureName} - OFF");

            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = isEnabled ? Color.green : Color.gray;

            bool clicked = GUI.Button(controlRect, buttonText);

            GUI.backgroundColor = originalColor;
            return clicked;
        }

        protected bool DrawToggleButtonWithStatus(string featureName, bool isEnabled,
            float buttonWidth = UIConstants.BUTTON_TOGGLE_WIDTH,
            float statusWidth = UIConstants.STATUS_LABEL_WIDTH, int buttonId = -1)
        {
            GUILayout.BeginHorizontal();

            bool clicked = DrawToggleButton(featureName, isEnabled, buttonWidth, buttonId);

            var statusText = isEnabled ? "ENABLED" : "Disabled";
            var statusColor = GUI.color;
            GUI.color = isEnabled ? Color.green : Color.gray;
            GUILayout.Label($"{featureName}: {statusText}", GUILayout.Width(statusWidth));
            GUI.color = statusColor;

            GUILayout.EndHorizontal();

            return clicked;
        }
    }
}