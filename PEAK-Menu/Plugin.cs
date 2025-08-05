using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using PEAK_Menu.Config;
using PEAK_Menu.Utils;
using UnityEngine;

namespace PEAK_Menu
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }
        public static PluginConfig PluginConfig { get; private set; }
        
        private Harmony _harmony;
        internal DebugConsoleManager _debugConsoleManager; // Primary system

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            
            // Initialize configuration
            PluginConfig = new PluginConfig(Config);
            
            // Initialize both systems
            _debugConsoleManager = new DebugConsoleManager(); // Primary system with all managers
            
            // Apply Harmony patches
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);
            
            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
        }

        private void Start()
        {
            _debugConsoleManager.Initialize(); // Primary system
            
            // Register console commands
            StartCoroutine(DelayedConsoleRegistration());
        }
        
        private System.Collections.IEnumerator DelayedConsoleRegistration()
        {
            yield return new WaitForSeconds(1f);
            
            try 
            {
                _debugConsoleManager.RegisterConsoleCommands();
                
                Log.LogInfo("Console commands registered and available in the native debug console");
                Log.LogInfo($"Open the Debug Menu with {PluginConfig.DebugConsoleToggleKey.Value} ( or ` ) and go to Console tab for native autocomplete");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Failed to register console commands: {ex.Message}");
            }
        }

        private void Update()
        {
            try
            {
                HandleInput();
                
                // Update only the debug console system (which owns all managers)
                _debugConsoleManager?.Update();
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Error in Plugin.Update: {ex.Message}");
            }
        }

        private void HandleInput()
        {
            // Debug menu toggle (Home key) - PRIMARY SYSTEM
            if (PluginConfig?.DebugConsoleToggleKey?.Value != null && 
                Input.GetKeyDown(PluginConfig.DebugConsoleToggleKey.Value))
            {
                _debugConsoleManager?.ToggleDebugConsole();
            }

            // NoClip toggle (Delete key) - Uses debug console manager
            if (PluginConfig?.NoClipToggleKey?.Value != null && 
                Input.GetKeyDown(PluginConfig.NoClipToggleKey.Value))
            {
                var noClipManager = _debugConsoleManager?.GetNoClipManager();
                if (noClipManager != null)
                {
                    noClipManager.ToggleNoClip();
                }
            }
        }

        private void OnGUI()
        {
        }

        private void OnDestroy()
        {
            _debugConsoleManager?.RestoreOriginalState();
            _harmony?.UnpatchSelf();
            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} is unloaded!");
        }
    }
}
