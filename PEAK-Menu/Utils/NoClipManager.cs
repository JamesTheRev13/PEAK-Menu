using UnityEngine;

namespace PEAK_Menu.Utils
{
    public class NoClipManager
    {
        private bool _noClipEnabled = false;
        private float _noClipSpeed = 800f;
        private float _noClipFastSpeed = 3200f;
        private float _verticalForce = 800f;
        private float _sprintMultiplier = 4f;
        private float _maxClamp = 4000f;
        
        // Enhanced state tracking for collision management
        private bool _wasAnimationEnabled = false;
        private Vector3 _targetPosition;
        private bool _positionInitialized = false;
        
        // Collision state management
        private System.Collections.Generic.Dictionary<Collider, bool> _originalColliderStates;
        
        // Reflection field for CanDoInput method
        private static System.Reflection.MethodInfo _canDoInputMethod;

        public bool IsNoClipEnabled => _noClipEnabled;
        public float NoClipSpeed => _noClipSpeed;
        public float NoClipFastSpeed => _noClipFastSpeed;

        static NoClipManager()
        {
            try
            {
                _canDoInputMethod = typeof(Character).GetMethod("CanDoInput", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning($"Could not find CanDoInput method: {ex.Message}");
            }
        }

        public NoClipManager()
        {
            _originalColliderStates = new System.Collections.Generic.Dictionary<Collider, bool>();
        }

        public void EnableNoClip()
        {
            if (_noClipEnabled) return;

            var character = Character.localCharacter;
            if (character == null)
            {
                Plugin.Log.LogWarning("Cannot enable NoClip: No local character found");
                return;
            }

            _noClipEnabled = true;
            _positionInitialized = false;
            
            // Disable collisions for true NoClip behavior
            DisableCharacterCollisions(character);
            
            Plugin.Log.LogInfo("Enhanced NoClip enabled (FlyMod style with collision disabled)");
        }

        public void DisableNoClip()
        {
            if (!_noClipEnabled) return;

            var character = Character.localCharacter;
            if (character == null)
            {
                Plugin.Log.LogWarning("Cannot disable NoClip: No local character found");
                return;
            }

            _noClipEnabled = false;
            _positionInitialized = false;
            
            // Re-enable collisions
            RestoreCharacterCollisions(character);
            
            Plugin.Log.LogInfo("Enhanced NoClip disabled");
        }

        private void DisableCharacterCollisions(Character character)
        {
            _originalColliderStates.Clear();
            
            // Disable collisions on all character colliders
            foreach (var bodypart in character.refs.ragdoll.partList)
            {
                if (bodypart?.Rig == null) continue;
                
                var colliders = bodypart.Rig.GetComponents<Collider>();
                foreach (var collider in colliders)
                {
                    if (collider != null)
                    {
                        _originalColliderStates[collider] = collider.enabled;
                        collider.enabled = false;
                    }
                }
            }
            
            // Try to get additional colliders using reflection
            try
            {
                var ragdollType = typeof(CharacterRagdoll);
                var colliderListField = ragdollType.GetField("colliderList", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                
                if (colliderListField != null)
                {
                    var colliderList = colliderListField.GetValue(character.refs.ragdoll) as System.Collections.Generic.List<Collider>;
                    if (colliderList != null)
                    {
                        foreach (var collider in colliderList)
                        {
                            if (collider != null && !_originalColliderStates.ContainsKey(collider))
                            {
                                _originalColliderStates[collider] = collider.enabled;
                                collider.enabled = false;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogDebug($"Could not access ragdoll collider list via reflection: {ex.Message}");
                // Continue without the additional colliders - the bodypart colliders should be sufficient
            }
            
            Plugin.Log.LogDebug($"Disabled {_originalColliderStates.Count} colliders for NoClip");
        }

        private void RestoreCharacterCollisions(Character character)
        {
            // Restore all collider states
            foreach (var kvp in _originalColliderStates)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.enabled = kvp.Value;
                }
            }
            
            Plugin.Log.LogDebug($"Restored {_originalColliderStates.Count} colliders from NoClip");
            _originalColliderStates.Clear();
        }

        public void ToggleNoClip()
        {
            if (_noClipEnabled)
                DisableNoClip();
            else
                EnableNoClip();
        }

        public void SetNoClipSpeed(float speed)
        {
            _noClipSpeed = Mathf.Clamp(speed, 100f, 2000f);
        }

        public void SetNoClipFastSpeed(float fastSpeed)
        {
            _noClipFastSpeed = Mathf.Clamp(fastSpeed, 500f, 8000f);
        }

        public void Update()
        {
            if (!_noClipEnabled) return;

            var character = Character.localCharacter;
            if (character == null) return;

            // Handle input and movement using FlyMod approach
            HandleNoClipMovement(character);
        }

        private bool CanProcessInput(Character character)
        {
            try
            {
                // Try using reflection to call CanDoInput
                if (_canDoInputMethod != null)
                {
                    return (bool)_canDoInputMethod.Invoke(character, null);
                }
                
                // Fallback to manual check
                if (GUIManager.instance != null)
                {
                    if (GUIManager.instance.windowBlockingInput || GUIManager.instance.wheelActive)
                    {
                        return false;
                    }
                }
                
                return true;
            }
            catch (System.Exception)
            {
                return true; // Default to allowing input if check fails
            }
        }

        private void HandleNoClipMovement(Character character)
        {
            if (!CanProcessInput(character)) return;

            // Set character as grounded to enable creative fly mode
            character.data.isGrounded = true;
            
            // Base movement force starts with look direction
            Vector3 flyForce = character.data.lookDirection;

            // Forward/Backward movement (W/S keys)
            if (Input.GetKey(KeyCode.W))
            {
                flyForce *= _verticalForce;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                flyForce *= -_verticalForce;
            }
            else
            {
                flyForce = Vector3.zero; // No forward/back input
            }

            // Left/Right movement (A/D keys)
            Vector3 right = Vector3.Cross(Vector3.up, character.data.lookDirection).normalized * _verticalForce;
            
            if (Input.GetKey(KeyCode.D))
            {
                flyForce += right;
            }
            
            if (Input.GetKey(KeyCode.A))
            {
                flyForce -= right;
            }

            // Vertical movement (Space/Ctrl keys)
            if (Input.GetKey(KeyCode.Space))
            {
                flyForce += Vector3.up * _verticalForce;
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                flyForce += Vector3.down * _verticalForce;
            }

            // Sprint multiplier
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // Restore stamina while flying to prevent drain
                character.AddStamina(character.refs.movement.sprintStaminaUsage * Time.deltaTime);
                flyForce *= _sprintMultiplier;
            }

            // Add slight downward force to counter gravity (creative mode style)
            flyForce += Vector3.down * 100f;

            // Clamp forces to prevent excessive speeds
            flyForce.x = Mathf.Clamp(flyForce.x, -_maxClamp, _maxClamp);
            flyForce.y = Mathf.Clamp(flyForce.y, -_maxClamp, _maxClamp);
            flyForce.z = Mathf.Clamp(flyForce.z, -_maxClamp, _maxClamp);

            // Apply forces to all body parts
            foreach (var part in character.refs.ragdoll.partList)
            {
                if (part?.Rig != null)
                {
                    part.AddForce(flyForce, ForceMode.Force);
                }
            }
        }

        // Additional configuration methods for advanced users
        public void SetVerticalForce(float force)
        {
            _verticalForce = Mathf.Clamp(force, 100f, 2000f);
        }

        public void SetSprintMultiplier(float multiplier)
        {
            _sprintMultiplier = Mathf.Clamp(multiplier, 1f, 10f);
        }

        public void SetMaxClamp(float clamp)
        {
            _maxClamp = Mathf.Clamp(clamp, 1000f, 10000f);
        }

        // Property getters for UI display
        public float VerticalForce => _verticalForce;
        public float SprintMultiplier => _sprintMultiplier;
        public float MaxClamp => _maxClamp;
    }
}