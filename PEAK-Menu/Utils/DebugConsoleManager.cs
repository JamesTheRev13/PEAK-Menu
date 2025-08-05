using PEAK_Menu.Commands;
using PEAK_Menu.Utils.CLI;
using PEAK_Menu.Utils.DebugPages;
using System.Reflection;
using UnityEngine;
using Zorro.Core.CLI;

namespace PEAK_Menu.Utils
{
    public class DebugConsoleManager
    {
        private bool _allowOpenOriginal;
        private bool _wasInitialized = false;
        private static FieldInfo _currentPageField;

        // Managers owned by debug console system
        private RainbowManager _rainbowManager;
        private NoClipManager _noClipManager;
        private PlayerManager _playerManager;
        private CommandManager _commandManager = new CommandManager();

        static DebugConsoleManager()
        {
            try
            {
                _currentPageField = typeof(DebugUIHandler).GetField("m_currentPage", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning($"Could not find m_currentPage field: {ex.Message}");
            }
        }

        public bool IsDebugConsoleOpen => DebugUIHandler.IsOpen;
        public bool IsDebugConsoleAllowed => DebugUIHandler.AllowOpen;

        public void Initialize()
        {
            if (_wasInitialized) return;

            try
            {
                _allowOpenOriginal = DebugUIHandler.AllowOpen;
                _wasInitialized = true;

                // Initialize managers here
                _rainbowManager = new RainbowManager();
                _noClipManager = new NoClipManager();
                _playerManager = new PlayerManager();
                
                RegisterCustomPages();
                
                Plugin.Log.LogInfo("Debug Console Manager initialized with all managers");
                Plugin.Log.LogInfo($"Original AllowOpen state: {_allowOpenOriginal}");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Failed to initialize Debug Console Manager: {ex.Message}");
            }
        }

        public void RegisterCustomPages()
        {
            try
            {
                var debugHandler = DebugUIHandler.Instance;
                if (debugHandler == null)
                {
                    Plugin.Log.LogWarning("DebugUIHandler not available for page registration");
                    return;
                }

                debugHandler.RegisterPage("PEAK Player", () => new PlayerDebugPage());
                debugHandler.RegisterPage("PEAK Admin", () => new AdminDebugPage());
                debugHandler.RegisterPage("PEAK Environment", () => new EnvironmentDebugPage());

                Plugin.Log.LogInfo("Custom debug pages registered successfully");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error registering custom debug pages: {ex.Message}");
            }
        }

        public void RegisterConsoleCommands()
        {
            try
            {
                ConsoleCommandRegistry.RegisterPEAKCommands();
                Plugin.Log.LogInfo("Console commands registered with native debug console");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Failed to register console commands: {ex.Message}");
            }
        }

        public void Update()
        {
            // Update all managers
            _rainbowManager?.Update();
            _noClipManager?.Update();

            // Update custom debug pages if the console is open
            if (DebugUIHandler.IsOpen)
            {
                try
                {
                    var debugHandler = DebugUIHandler.Instance;
                    if (debugHandler != null && _currentPageField != null)
                    {
                        var currentPage = _currentPageField.GetValue(debugHandler);
                        
                        if (currentPage is BaseCustomDebugPage customPage)
                        {
                            customPage.UpdateContent();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    if (Plugin.PluginConfig?.EnableDebugMode?.Value == true)
                    {
                        Plugin.Log?.LogWarning($"Error updating debug page: {ex.Message}");
                    }
                }
            }
        }

        // Expose managers for debug pages and commands
        public RainbowManager GetRainbowManager() => _rainbowManager;
        public NoClipManager GetNoClipManager() => _noClipManager;
        public PlayerManager GetPlayerManager() => _playerManager;

        public void ToggleDebugConsole()
        {
            try
            {
                var debugHandler = DebugUIHandler.Instance;
                if (debugHandler == null)
                {
                    Plugin.Log.LogWarning("DebugUIHandler instance not available");
                    return;
                }

                if (DebugUIHandler.IsOpen)
                {
                    debugHandler.Hide();
                    Plugin.Log.LogInfo("Built-in debug console closed");
                }
                else
                {
                    DebugUIHandler.AllowOpen = true;
                    debugHandler.Show();
                    Plugin.Log.LogInfo("Built-in debug console opened");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error toggling debug console: {ex.Message}");
            }
        }

        public void EnableDebugConsole()
        {
            try
            {
                var debugHandler = DebugUIHandler.Instance;
                if (debugHandler == null)
                {
                    Plugin.Log.LogWarning("DebugUIHandler instance not available");
                    return;
                }

                DebugUIHandler.AllowOpen = true;
                debugHandler.Show();
                Plugin.Log.LogInfo("Built-in debug console enabled and opened");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error enabling debug console: {ex.Message}");
            }
        }

        public void DisableDebugConsole()
        {
            try
            {
                var debugHandler = DebugUIHandler.Instance;
                if (debugHandler == null)
                {
                    Plugin.Log.LogWarning("DebugUIHandler instance not available");
                    return;
                }

                debugHandler.Hide();
                DebugUIHandler.AllowOpen = false;
                Plugin.Log.LogInfo("Built-in debug console disabled and closed");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error disabling debug console: {ex.Message}");
            }
        }

        public void RestoreOriginalState()
        {
            try
            {
                if (_wasInitialized)
                {
                    // Cleanup managers
                    _rainbowManager?.DisableRainbow();
                    _noClipManager?.DisableNoClip();
                    
                    DebugUIHandler.AllowOpen = _allowOpenOriginal;
                    
                    if (DebugUIHandler.IsOpen)
                    {
                        DebugUIHandler.Instance?.Hide();
                    }
                    
                    Plugin.Log.LogInfo($"Debug console restored to original state (AllowOpen: {_allowOpenOriginal})");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error restoring debug console state: {ex.Message}");
            }
        }

        public string GetStatus()
        {
            try
            {
                var currentPageInfo = "Unknown";
                
                if (_currentPageField != null)
                {
                    var debugHandler = DebugUIHandler.Instance;
                    if (debugHandler != null)
                    {
                        var currentPage = _currentPageField.GetValue(debugHandler);
                        if (currentPage != null)
                        {
                            currentPageInfo = currentPage.GetType().Name;
                            if (currentPage is BaseCustomDebugPage)
                            {
                                currentPageInfo += " (PEAK Custom)";
                            }
                        }
                        else
                        {
                            currentPageInfo = "None";
                        }
                    }
                }

                return $"Debug Console Status:\n" +
                       $"  Available: {(DebugUIHandler.Instance != null ? "Yes" : "No")}\n" +
                       $"  Allowed: {DebugUIHandler.AllowOpen}\n" +
                       $"  Open: {DebugUIHandler.IsOpen}\n" +
                       $"  Paused: {(DebugUIHandler.Instance?.Paused ?? false)}\n" +
                       $"  Current Page: {currentPageInfo}\n" +
                       $"  Rainbow: {(_rainbowManager?.IsRainbowEnabled ?? false)}\n" +
                       $"  NoClip: {(_noClipManager?.IsNoClipEnabled ?? false)}";
            }
            catch (System.Exception ex)
            {
                return $"Error getting debug console status: {ex.Message}";
            }
        }
        // LEGACY - Needs to be replaced by CLI command once commands are migrated properly and all work
        public bool ExecuteCommand(string commandLine)
        {
            try
            {
                return _commandManager?.ExecuteCommand(commandLine) ?? false;
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error executing command: {ex.Message}");
                return false;
            }
        }
    }
}