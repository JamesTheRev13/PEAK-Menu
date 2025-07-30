using BepInEx.Logging;

namespace PEAK_Menu.Utils
{
    public static class Logger
    {
        public static void Info(string message) => Plugin.Log.LogInfo(message);
        public static void Warning(string message) => Plugin.Log.LogWarning(message);
        public static void Error(string message) => Plugin.Log.LogError(message);
        public static void Debug(string message)
        {
            if (Plugin.PluginConfig.EnableDebugMode.Value)
                Plugin.Log.LogDebug(message);
        }
    }
}