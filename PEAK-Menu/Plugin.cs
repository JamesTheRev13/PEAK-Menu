using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using PEAK_Menu.Config;
using PEAK_Menu.Menu;
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
        internal MenuManager _menuManager; // Legacy menu only
        internal DebugConsoleManager _debugConsoleManager; // Primary system

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            
            // Initialize configuration
            PluginConfig = new PluginConfig(Config);
            
            // Initialize both systems
            _menuManager = new MenuManager(); // Legacy menu only
            _debugConsoleManager = new DebugConsoleManager(); // Primary system with all managers
            
            // Apply Harmony patches
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);
            
            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
        }

        private void Start()
        {
            _menuManager.Initialize(); // Legacy menu
            _debugConsoleManager.Initialize(); // Primary system
            
            // Register console commands
            StartCoroutine(DelayedConsoleRegistration());
        }
        
        private System.Collections.IEnumerator DelayedConsoleRegistration()
        {
            yield return new UnityEngine.WaitForSeconds(1f);
            
            try 
            {
                _debugConsoleManager.RegisterConsoleCommands();
                
                Log.LogInfo("Console commands registered and available in the native debug console");
                Log.LogInfo("Available commands: heal, kill, revive, teleport, goto, bring, godmode, infinitestamina");
                Log.LogInfo("                   noclip, speed, jump, climb, listitems, giveitem, rainbow, showinventory");
                Log.LogInfo($"Open the Debug Menu with {PluginConfig.DebugConsoleToggleKey.Value} and go to Console tab for native autocomplete");
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
            // Legacy menu toggle (Insert key)
            if (PluginConfig?.MenuToggleKey?.Value != null && 
                Input.GetKeyDown(PluginConfig.MenuToggleKey.Value))
            {
                _menuManager?.ToggleMenu();
            }

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
                    _menuManager?.AddToConsole($"[HOTKEY] NoClip {(noClipManager.IsNoClipEnabled ? "enabled" : "disabled")}");
                }
            }
        }

        private void OnGUI()
        {
            _menuManager?.OnGUI(); // Only legacy menu uses OnGUI
        }

        private void OnDestroy()
        {
            _menuManager?.Cleanup();
            _debugConsoleManager?.RestoreOriginalState();
            _harmony?.UnpatchSelf();
            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} is unloaded!");
        }
    }
}
