using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using PEAK_Menu.Config;
using PEAK_Menu.Menu;

// TODO: Investigate PointPinger (ReceivePoint_Rpc) for teleport to point functionality
// TODO: Investigate CharacterAfflictions (UpdateWeight) for potential weight management

namespace PEAK_Menu
{
    // Plugin by Bob Saget
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }
        public static PluginConfig PluginConfig { get; private set; }
        
        private Harmony _harmony;
        internal MenuManager _menuManager; // Made internal so commands can access it

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            
            // Initialize configuration
            PluginConfig = new PluginConfig(Config);
            
            // Initialize menu system
            _menuManager = new MenuManager();
            
            // Apply Harmony patches
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);
            
            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
        }

        private void Start()
        {
            _menuManager.Initialize();
        }

        private void Update()
        {
            _menuManager?.Update();
        }

        private void OnGUI()
        {
            _menuManager?.OnGUI();
        }

        private void OnDestroy()
        {
            _menuManager?.Cleanup();
            _harmony?.UnpatchSelf();
            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} is unloaded!");
        }
    }
}
