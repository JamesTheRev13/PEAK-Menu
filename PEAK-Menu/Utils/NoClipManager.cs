using UnityEngine;

namespace PEAK_Menu.Utils
{
    public class NoClipManager
    {
        private bool _noClipEnabled = false;
        private float _noClipSpeed = 10f;
        private float _noClipFastSpeed = 25f;
        private bool _wasPreviouslyGrounded = false;

        private static System.Reflection.MethodInfo _canDoInputMethod;

        // Store original physics states for restoration
        private System.Collections.Generic.Dictionary<Rigidbody, PhysicsState> _originalStates;
        
        // Track relative positions to maintain character shape
        private System.Collections.Generic.Dictionary<Rigidbody, Vector3> _relativePositions;
        private Vector3 _lastCenterPosition;
        
        // Smooth movement variables
        private Vector3 _accumulatedMovement = Vector3.zero;
        private float _fixedUpdateTimer = 0f;

        public bool IsNoClipEnabled => _noClipEnabled;
        public float NoClipSpeed => _noClipSpeed;
        public float NoClipFastSpeed => _noClipFastSpeed;

        private struct PhysicsState
        {
            public bool useGravity;
            public bool isKinematic;
            public RigidbodyConstraints constraints;
        }

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
            _originalStates = new System.Collections.Generic.Dictionary<Rigidbody, PhysicsState>();
            _relativePositions = new System.Collections.Generic.Dictionary<Rigidbody, Vector3>();
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
            StoreOriginalPhysicsStates(character);
            StoreRelativePositions(character);
            
            // Reset movement accumulation
            _accumulatedMovement = Vector3.zero;
            _fixedUpdateTimer = 0f;
            
            // Disable collision for all body parts
            SetCollisionEnabled(character, false);
            
            // Disable gravity and physics properly
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
            
            // Clear stored states
            _originalStates.Clear();
            _relativePositions.Clear();
            _accumulatedMovement = Vector3.zero;
            
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

            // Handle input gathering more frequently
            HandleNoClipInput(character);
            
            // Apply movement more frequently for smoother movement
            _fixedUpdateTimer += Time.deltaTime;
            if (_fixedUpdateTimer >= Time.fixedDeltaTime || _accumulatedMovement.magnitude > 0.001f)
            {
                if (_accumulatedMovement != Vector3.zero)
                {
                    MoveCharacterMaintainShape(character, _accumulatedMovement);
                    _accumulatedMovement = Vector3.zero;
                }
                _fixedUpdateTimer = 0f;
            }
        }

        private void StoreOriginalPhysicsStates(Character character)
        {
            _originalStates.Clear();
            
            foreach (var bodypart in character.refs.ragdoll.partList)
            {
                if (bodypart?.Rig != null)
                {
                    var rigidbody = bodypart.Rig;
                    _originalStates[rigidbody] = new PhysicsState
                    {
                        useGravity = rigidbody.useGravity,
                        isKinematic = rigidbody.isKinematic,
                        constraints = rigidbody.constraints
                    };
                }
            }
        }

        private void StoreRelativePositions(Character character)
        {
            _relativePositions.Clear();
            
            // Use hip as the center reference point
            var hipRig = character.refs.ragdoll.partDict[BodypartType.Hip].Rig;
            var centerPos = hipRig.position;
            _lastCenterPosition = centerPos;
            
            // Store each body part's relative position to the hip
            foreach (var bodypart in character.refs.ragdoll.partList)
            {
                if (bodypart?.Rig != null)
                {
                    _relativePositions[bodypart.Rig] = bodypart.Rig.position - centerPos;
                }
            }
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

        private void HandleNoClipInput(Character character)
        {
            if (!CanProcessInput(character)) return;

            var input = character.input;
            if (input == null) return;

            // Calculate movement direction based on camera look direction
            var lookDirection = character.data.lookDirection;
            var lookRight = character.data.lookDirection_Right;

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

            // Use unscaled delta time for consistent movement regardless of framerate
            var movement = moveVector * currentSpeed * Time.unscaledDeltaTime;
            
            if (movement != Vector3.zero)
            {
                // Accumulate movement for smoother application
                _accumulatedMovement += movement;
            }
        }

        private void MoveCharacterMaintainShape(Character character, Vector3 movement)
        {
            // Calculate the new center position
            var newCenterPosition = _lastCenterPosition + movement;
            
            // Move the main character transform first
            character.transform.position = newCenterPosition;
            
            // Move each body part to maintain relative positions
            foreach (var bodypart in character.refs.ragdoll.partList)
            {
                if (bodypart?.Rig != null && _relativePositions.TryGetValue(bodypart.Rig, out var relativePos))
                {
                    // Set absolute position based on new center + relative offset
                    bodypart.Rig.position = newCenterPosition + relativePos;
                    
                    // Clear velocities less frequently to reduce overhead
                    if (Time.fixedTime % 0.1f < Time.fixedDeltaTime && !bodypart.Rig.isKinematic)
                    {
                        bodypart.Rig.linearVelocity = Vector3.zero;
                        bodypart.Rig.angularVelocity = Vector3.zero;
                    }
                }
            }
            
            // Update animation helper transforms less frequently
            if (Time.fixedTime % 0.05f < Time.fixedDeltaTime)
            {
                UpdateAnimationTransforms(character, newCenterPosition);
            }
            
            // Update our center position tracking
            _lastCenterPosition = newCenterPosition;
        }

        private void UpdateAnimationTransforms(Character character, Vector3 newCenterPosition)
        {
            try
            {
                // Update helper transforms that the camera system uses
                if (character.refs.animationHeadTransform != null)
                {
                    var headRig = character.refs.ragdoll.partDict[BodypartType.Head].Rig;
                    character.refs.animationHeadTransform.position = headRig.position;
                }
                
                if (character.refs.animationHipTransform != null)
                {
                    character.refs.animationHipTransform.position = newCenterPosition;
                }
                
                if (character.refs.animationPositionTransform != null)
                {
                    character.refs.animationPositionTransform.position = newCenterPosition;
                }
                
                if (character.refs.animationLookTransform != null)
                {
                    var headRig = character.refs.ragdoll.partDict[BodypartType.Head].Rig;
                    character.refs.animationLookTransform.position = headRig.position;
                    character.refs.animationLookTransform.rotation = headRig.rotation;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogDebug($"Could not update animation transforms: {ex.Message}");
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
                            // Restore original physics states
                            if (_originalStates.TryGetValue(rigidbody, out var originalState))
                            {
                                rigidbody.isKinematic = originalState.isKinematic;
                                rigidbody.useGravity = originalState.useGravity;
                                rigidbody.constraints = originalState.constraints;
                            }
                            else
                            {
                                // Fallback to reasonable defaults
                                rigidbody.isKinematic = false;
                                rigidbody.useGravity = true;
                                rigidbody.constraints = RigidbodyConstraints.None;
                            }
                        }
                        else
                        {
                            // Clear velocities before making kinematic (to avoid warnings)
                            if (!rigidbody.isKinematic)
                            {
                                rigidbody.linearVelocity = Vector3.zero;
                                rigidbody.angularVelocity = Vector3.zero;
                            }
                            
                            // Disable physics for NoClip
                            rigidbody.useGravity = false;
                            rigidbody.isKinematic = true;
                            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
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