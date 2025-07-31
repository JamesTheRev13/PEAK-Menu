using UnityEngine;

namespace PEAK_Menu.Utils
{
    public class NoClipManager
    {
        private bool _noClipEnabled = false;
        private float _noClipSpeed = 10f;
        private float _noClipFastSpeed = 25f;
        private bool _wasPreviouslyGrounded = false;
        private Vector3 _originalGravity;
        private bool _originalUseGravity = true;

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
            
            // Store original states
            _wasPreviouslyGrounded = character.data.isGrounded;
            
            // Disable collision for all body parts
            SetCollisionEnabled(character, false);
            
            // Disable gravity and physics
            SetPhysicsEnabled(character, false);
            
            Plugin.Log.LogInfo("NoClip enabled");
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
            
            // Re-enable collision for all body parts
            SetCollisionEnabled(character, true);
            
            // Re-enable gravity and physics
            SetPhysicsEnabled(character, true);
            
            Plugin.Log.LogInfo("NoClip disabled");
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
            _noClipSpeed = Mathf.Clamp(speed, 1f, 100f);
        }

        public void SetNoClipFastSpeed(float fastSpeed)
        {
            _noClipFastSpeed = Mathf.Clamp(fastSpeed, 5f, 200f);
        }

        public void Update()
        {
            if (!_noClipEnabled) return;

            var character = Character.localCharacter;
            if (character == null) return;

            // Handle NoClip movement
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

            var input = character.input;
            if (input == null) return;

            // Calculate movement direction based on camera look direction
            var lookDirection = character.data.lookDirection;
            var lookRight = character.data.lookDirection_Right;
            var lookUp = character.data.lookDirection_Up;

            // Get movement input
            var moveVector = Vector3.zero;
            
            // Forward/Backward (W/S)
            if (input.movementInput.y != 0)
            {
                moveVector += lookDirection * input.movementInput.y;
            }
            
            // Left/Right (A/D)
            if (input.movementInput.x != 0)
            {
                moveVector += lookRight * input.movementInput.x;
            }
            
            // Up/Down (Space/Ctrl)
            if (Input.GetKey(KeyCode.Space))
            {
                moveVector += Vector3.up;
            }
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                moveVector += Vector3.down;
            }

            // Normalize movement vector to prevent faster diagonal movement
            if (moveVector.magnitude > 1f)
            {
                moveVector = moveVector.normalized;
            }

            // Apply speed (check for sprint modifier)
            var currentSpeed = _noClipSpeed;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentSpeed = _noClipFastSpeed;
            }

            // Apply movement with delta time
            var movement = moveVector * currentSpeed * Time.deltaTime;
            
            if (movement != Vector3.zero)
            {
                // Move all body parts
                MoveCharacter(character, movement);
            }
        }

        private void MoveCharacter(Character character, Vector3 movement)
        {
            // Move all ragdoll parts
            foreach (var bodypart in character.refs.ragdoll.partList)
            {
                if (bodypart?.Rig != null)
                {
                    bodypart.Rig.position += movement;
                    bodypart.Rig.linearVelocity = Vector3.zero; // Stop any residual velocity
                }
            }
        }

        private void SetCollisionEnabled(Character character, bool enabled)
        {
            try
            {
                foreach (var bodypart in character.refs.ragdoll.partList)
                {
                    if (bodypart?.Rig != null)
                    {
                        var colliders = bodypart.Rig.GetComponentsInChildren<Collider>();
                        foreach (var collider in colliders)
                        {
                            if (collider != null)
                            {
                                collider.enabled = enabled;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error setting collision enabled={enabled}: {ex.Message}");
            }
        }

        private void SetPhysicsEnabled(Character character, bool enabled)
        {
            try
            {
                foreach (var bodypart in character.refs.ragdoll.partList)
                {
                    if (bodypart?.Rig != null)
                    {
                        var rigidbody = bodypart.Rig;
                        
                        if (enabled)
                        {
                            // Re-enable physics
                            rigidbody.useGravity = true;
                            rigidbody.isKinematic = false;
                            rigidbody.constraints = RigidbodyConstraints.None;
                        }
                        else
                        {
                            // Disable physics
                            rigidbody.useGravity = false;
                            rigidbody.isKinematic = true;
                            rigidbody.linearVelocity = Vector3.zero;
                            rigidbody.angularVelocity = Vector3.zero;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"Error setting physics enabled={enabled}: {ex.Message}");
            }
        }
    }
}