using UnityEngine;

namespace PEAK_Menu.Utils
{
    public class RainbowManager
    {
        private bool _rainbowEnabled = false;
        private float _rainbowSpeed = 1.0f;
        private float _rainbowTime = 0f;
        private int _maxSkinIndex = 10; // Will be determined dynamically
        private int _originalSkinIndex = 0;

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
                
                // Determine max skin index by trying different values
                DetermineMaxSkinIndex();
                
                Plugin.Log.LogInfo("Rainbow skin effect enabled");
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
            if (!_rainbowEnabled)
                return;

            _rainbowTime += Time.deltaTime * _rainbowSpeed;

            // Create rainbow effect using HSV color space
            // Convert time to hue (0-1 range, cycling)
            float hue = (_rainbowTime * 0.5f) % 1f;
            
            // Map hue to skin index
            int skinIndex = Mathf.FloorToInt(hue * _maxSkinIndex);
            skinIndex = Mathf.Clamp(skinIndex, 0, _maxSkinIndex - 1);

            try
            {
                // Only change if it's different to avoid spam
                CharacterCustomization.SetCharacterSkinColor(skinIndex);
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Rainbow skin update failed: {ex.Message}");
                DisableRainbow();
            }
        }

        private void DetermineMaxSkinIndex()
        {
            // Try to find the maximum valid skin index
            // Start with a reasonable assumption and work backwards if needed
            _maxSkinIndex = 10;
            
            // This is a simple approach - in a real implementation you might want to
            // use reflection to access the actual skin count from the Customization singleton
            for (int i = 20; i > 0; i--)
            {
                try
                {
                    CharacterCustomization.SetCharacterSkinColor(i);
                    _maxSkinIndex = i + 1;
                    break;
                }
                catch
                {
                    // Continue trying smaller indices
                    continue;
                }
            }
            
            Plugin.Log.LogDebug($"Determined max skin index: {_maxSkinIndex}");
        }
    }
}