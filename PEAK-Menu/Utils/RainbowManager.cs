using UnityEngine;
using System.Reflection;
using System;

namespace PEAK_Menu.Utils
{
    public class RainbowManager
    {
        private bool _rainbowEnabled = false;
        private float _rainbowSpeed = 1.0f;
        private float _rainbowTime = 0f;
        private int _maxSkinIndex = 10;
        private int _originalSkinIndex = 0;
        private int _lastSkinIndex = -1;

        public bool IsRainbowEnabled => _rainbowEnabled;

        public void EnableRainbow()
        {
            if (!_rainbowEnabled)
            {
                // Store original skin index
                var character = Character.localCharacter;
                if (character?.refs?.customization != null)
                {
                    try
                    {
                        // Try to get current skin index (this might fail due to Singleton access)
                        _originalSkinIndex = 0; // Default fallback
                    }
                    catch
                    {
                        _originalSkinIndex = 0;
                    }
                }
                
                _rainbowEnabled = true;
                _rainbowTime = 0f;
                _lastSkinIndex = -1; // Reset tracking
                
                // Determine max skin index by trying different values
                DetermineMaxSkinIndex();
                
                Plugin.Log.LogInfo($"Rainbow skin effect enabled (using {_maxSkinIndex} skins)");
            }
        }

        public void DisableRainbow()
        {
            if (_rainbowEnabled)
            {
                _rainbowEnabled = false;
                
                // Restore original skin color
                try
                {
                    CharacterCustomization.SetCharacterSkinColor(_originalSkinIndex);
                }
                catch (System.Exception ex)
                {
                    Plugin.Log.LogWarning($"Could not restore original skin: {ex.Message}");
                }
                
                Plugin.Log.LogInfo("Rainbow skin effect disabled");
            }
        }

        public void ToggleRainbow()
        {
            if (_rainbowEnabled)
                DisableRainbow();
            else
                EnableRainbow();
        }

        public void SetRainbowSpeed(float speed)
        {
            _rainbowSpeed = Mathf.Clamp(speed, 0.1f, 10f);
        }

        public void Update()
        {
            if (!_rainbowEnabled || _maxSkinIndex <= 0)
                return;

            _rainbowTime += Time.deltaTime * _rainbowSpeed;

            // Create rainbow effect using HSV color space
            // Convert time to hue (0-1 range, cycling)
            float hue = (_rainbowTime * 0.5f) % 1f;
            
            // Map hue to skin index - FIXED CALCULATION
            int skinIndex = Mathf.FloorToInt(hue * _maxSkinIndex);
            skinIndex = Mathf.Clamp(skinIndex, 0, _maxSkinIndex - 1);

            // Only change if it's different to avoid spam and errors
            if (skinIndex != _lastSkinIndex)
            {
                try
                {
                    CharacterCustomization.SetCharacterSkinColor(skinIndex);
                    _lastSkinIndex = skinIndex;
                }
                catch (System.Exception ex)
                {
                    Plugin.Log.LogError($"Rainbow skin update failed at index {skinIndex}: {ex.Message}");
                    Plugin.Log.LogWarning($"Adjusting max skin index from {_maxSkinIndex} to {skinIndex}");
                    
                    // Adjust max skin index if we hit an error
                    if (skinIndex > 0)
                    {
                        _maxSkinIndex = skinIndex;
                    }
                    else
                    {
                        // If we can't even set index 0, disable rainbow
                        DisableRainbow();
                    }
                }
            }
        }

        private void DetermineMaxSkinIndex()
        {
            // Try reflection approach first
            if (TryGetSkinCountViaReflection(out int reflectionCount))
            {
                _maxSkinIndex = reflectionCount;
                Plugin.Log.LogInfo($"Found {_maxSkinIndex} skins via reflection");
                return;
            }
            
            // Fallback to trial-and-error method
            Plugin.Log.LogInfo("Reflection failed, using trial-and-error method");
            
            _maxSkinIndex = 1;
            
            for (int i = 0; i < 30; i++) // Reduced range for safety
            {
                try
                {
                    CharacterCustomization.SetCharacterSkinColor(i);
                    _maxSkinIndex = i + 1;
                }
                catch
                {
                    break;
                }
            }
            
            if (_maxSkinIndex < 2)
            {
                _maxSkinIndex = 8; // Safe fallback
            }
            
            Plugin.Log.LogInfo($"Determined max skin index via testing: {_maxSkinIndex}");
            
            // Restore original skin
            try
            {
                CharacterCustomization.SetCharacterSkinColor(_originalSkinIndex);
            }
            catch { }
        }

        private bool TryGetSkinCountViaReflection(out int skinCount)
        {
            skinCount = 0;
            
            try
            {
                // Try to find the Customization singleton and get skins array length
                var customizationType = Type.GetType("Customization");
                if (customizationType == null)
                {
                    Plugin.Log.LogDebug("Could not find Customization type");
                    return false;
                }

                // Look for Instance property or field
                var instanceProperty = customizationType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var instanceField = customizationType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                
                object instance = null;
                if (instanceProperty != null)
                {
                    instance = instanceProperty.GetValue(null);
                }
                else if (instanceField != null)
                {
                    instance = instanceField.GetValue(null);
                }

                if (instance == null)
                {
                    Plugin.Log.LogDebug("Could not get Customization instance");
                    return false;
                }

                // Look for skins field/property
                var skinsField = customizationType.GetField("skins", BindingFlags.Public | BindingFlags.Instance);
                var skinsProperty = customizationType.GetProperty("skins", BindingFlags.Public | BindingFlags.Instance);
                
                object skins = null;
                if (skinsField != null)
                {
                    skins = skinsField.GetValue(instance);
                }
                else if (skinsProperty != null)
                {
                    skins = skinsProperty.GetValue(instance);
                }

                if (skins is Array skinsArray)
                {
                    skinCount = skinsArray.Length;
                    return skinCount > 0;
                }
                
                Plugin.Log.LogDebug("Skins is not an array or is null");
                return false;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogDebug($"Reflection failed: {ex.Message}");
                return false;
            }
        }
    }
}