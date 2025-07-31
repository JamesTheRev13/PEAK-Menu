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
        public ConfigEntry<bool> NoFallDamage { get; }
        public ConfigEntry<bool> NoWeight { get; }
        public ConfigEntry<bool> AfflictionImmunity { get; }
        public ConfigEntry<float> MovementSpeedMultiplier { get; }
        public ConfigEntry<float> JumpHeightMultiplier { get; }
        public ConfigEntry<float> ClimbSpeedMultiplier { get; }
        public ConfigEntry<bool> TeleportToPingEnabled { get; private set; }

        public PluginConfig(ConfigFile config)
        {
            MenuToggleKey = config.Bind("Controls", "MenuToggleKey", KeyCode.Insert, "Key to toggle the menu");
            NoClipToggleKey = config.Bind("Controls", "NoClipToggleKey", KeyCode.Delete, "Key to toggle NoClip");
            EnableDebugMode = config.Bind("General", "EnableDebugMode", false, "Enable debug logging");
            MenuScale = config.Bind("UI", "MenuScale", 1.0f, new ConfigDescription("UI scale factor", new AcceptableValueRange<float>(0.5f, 2.0f)));
            NoFallDamage = config.Bind("Player", "NoFallDamage", false, "Disable fall damage");
            NoWeight = config.Bind("Player", "NoWeight", false, "Disable weight penalties from items");
            AfflictionImmunity = config.Bind("Player", "AfflictionImmunity", false, "Immunity to all afflictions");
            MovementSpeedMultiplier = config.Bind("Player", "MovementSpeedMultiplier", 1.0f, new ConfigDescription("Movement speed multiplier", new AcceptableValueRange<float>(0.1f, 20.0f)));
            JumpHeightMultiplier = config.Bind("Player", "JumpHeightMultiplier", 1.0f, new ConfigDescription("Jump height multiplier", new AcceptableValueRange<float>(0.1f, 10.0f)));
            ClimbSpeedMultiplier = config.Bind("Player", "ClimbSpeedMultiplier", 1.0f, new ConfigDescription("Climb speed multiplier", new AcceptableValueRange<float>(0.1f, 20.0f)));
            TeleportToPingEnabled = config.Bind("Features", "TeleportToPing", false, "Enable teleporting to marker points when you ping them");
        }
    }
}