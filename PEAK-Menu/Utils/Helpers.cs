using UnityEngine;

namespace PEAK_Menu.Utils
{
    public static class Helpers
    {
        public static bool IsKeyPressed(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        public static string FormatTime(float seconds)
        {
            var minutes = Mathf.FloorToInt(seconds / 60);
            var remainingSeconds = Mathf.FloorToInt(seconds % 60);
            return $"{minutes:00}:{remainingSeconds:00}";
        }

        public static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;
            return Color.white;
        }
    }
}