using BepInEx.Configuration;
using UnityEngine;

namespace PEAK_Menu.Config
{
    public class PluginConfig
    {
        public ConfigEntry<KeyCode> MenuToggleKey { get; }
        public ConfigEntry<KeyCode> NoClipToggleKey { get; }
        public ConfigEntry<bool> EnableDebugMode { get; }
        public ConfigEntry<float> MenuScale { get; }

        public PluginConfig(ConfigFile config)
        {
            MenuToggleKey = config.Bind("General", "MenuToggleKey", KeyCode.Insert, 
                "Key to toggle the menu");
            
            NoClipToggleKey = config.Bind("General", "NoClipToggleKey", KeyCode.Delete, 
                "Key to toggle NoClip mode");
            
            EnableDebugMode = config.Bind("Debug", "EnableDebugMode", false, 
                "Enable debug logging and features");
            
            MenuScale = config.Bind("UI", "MenuScale", 1.0f, 
                new ConfigDescription("UI scale factor", new AcceptableValueRange<float>(0.5f, 2.0f)));
        }
    }
}