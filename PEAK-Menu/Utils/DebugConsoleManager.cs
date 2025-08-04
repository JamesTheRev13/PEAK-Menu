using UnityEngine;
using Zorro.Core.CLI;

// Manager for handling the built-in debug console in PEAK Menu

namespace PEAK_Menu.Utils
{
    public class DebugConsoleManager
    {
        private bool _allowOpenOriginal;
        private bool _wasInitialized = false;

        public bool IsDebugConsoleOpen => DebugUIHandler.IsOpen;
        public bool IsDebugConsoleAllowed => DebugUIHandler.AllowOpen;

        public void Initialize()
        {
            if (_wasInitialized) return;

            try
            {
                // Store original AllowOpen state
                _allowOpenOriginal = DebugUIHandler.AllowOpen;
                _wasInitialized = true;
                
                Plugin.Log.LogInfo("Debug Console Manager initialized");
                Plugin.Log.LogInfo($"Original AllowOpen state: {_allowOpenOriginal}");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Failed to initialize Debug Console Manager: {ex.Message}");
            }
        }

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
                    // Close the debug console
                    debugHandler.Hide();
                    Plugin.Log.LogInfo("Built-in debug console closed");
                }
                else
                {
                    // Enable and open the debug console
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
                return $"Debug Console Status:\n" +
                       $"  Available: {(DebugUIHandler.Instance != null ? "Yes" : "No")}\n" +
                       $"  Allowed: {DebugUIHandler.AllowOpen}\n" +
                       $"  Open: {DebugUIHandler.IsOpen}\n" +
                       $"  Paused: {(DebugUIHandler.Instance?.Paused ?? false)}";
            }
            catch (System.Exception ex)
            {
                return $"Error getting debug console status: {ex.Message}";
            }
        }
    }
}