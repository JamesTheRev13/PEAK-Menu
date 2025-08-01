using System.Reflection;
using UnityEngine;

namespace PEAK_Menu.Utils
{
    public class PlayerManager
    {
        private bool _noFallDamageEnabled = false;
        private bool _noWeightEnabled = false;
        private bool _afflictionImmunityEnabled = false;
        private bool _speedModEnabled = false;
        // TODO: Jump modifications are not fully implemented yet
        private bool _jumpModEnabled = false;
        private bool _climbModEnabled = false;

        // Reflection fields for game modification
        private static FieldInfo _movementModifierField;
        private static FieldInfo _jumpGravityField;
        private static FieldInfo _fallDamageTimeField;
        // TODO: Infinite stamina is already handled in AdminCommand - might need to refactor for consistency or connect the two
        private static PropertyInfo _infiniteStaminaProperty;
        private static PropertyInfo _statusLockProperty;

        static PlayerManager()
        {
            try
            {
                var movementType = typeof(CharacterMovement);
                _movementModifierField = movementType.GetField("movementModifier", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                _jumpGravityField = movementType.GetField("jumpGravity", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                _fallDamageTimeField = movementType.GetField("fallDamageTime", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                var characterType = typeof(Character);
                _infiniteStaminaProperty = characterType.GetProperty("infiniteStam", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                _statusLockProperty = characterType.GetProperty("statusesLocked", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogError($"Failed to initialize PlayerManager reflection: {ex.Message}");
            }
        }

        public void SetNoFallDamage(bool enabled)
        {
            _noFallDamageEnabled = enabled;
            var character = Character.localCharacter;
            if (character?.refs?.movement != null && _fallDamageTimeField != null)
            {
                _fallDamageTimeField.SetValue(character.refs.movement, enabled ? 999f : 1.5f);
                Plugin.Log?.LogInfo($"No fall damage: {(enabled ? "enabled" : "disabled")}");
            }
        }

        public void SetNoWeight(bool enabled)
        {
            _noWeightEnabled = enabled;
            // TODO: This needs to be implemented via Harmony patches
            Plugin.Log?.LogInfo($"No weight: {(enabled ? "enabled" : "disabled")}");
        }

        public void SetAfflictionImmunity(bool enabled)
        {
            _afflictionImmunityEnabled = enabled;
            var character = Character.localCharacter;
            if (character != null && _statusLockProperty != null)
            {
                _statusLockProperty.SetValue(character, enabled);
                Plugin.Log?.LogInfo($"Affliction immunity: {(enabled ? "enabled" : "disabled")}");
            }
        }

        public void SetMovementSpeedMultiplier(float multiplier)
        {
            var character = Character.localCharacter;
            if (character?.refs?.movement != null && _movementModifierField != null)
            {
                _movementModifierField.SetValue(character.refs.movement, multiplier);
                Plugin.Log?.LogInfo($"Movement speed set to: {multiplier:F2}x");
            }
        }

        public void SetJumpHeightMultiplier(float multiplier)
        {
            var character = Character.localCharacter;
            if (character?.refs?.movement != null && _jumpGravityField != null)
            {
                _jumpGravityField.SetValue(character.refs.movement, multiplier);
                Plugin.Log?.LogInfo($"Jump height set to: {multiplier:F2}x");
            }
        }

        public void SetClimbSpeedMultiplier(float multiplier)
        {
            // TODO: Needs specific implementation based on climbing system
            Plugin.Log?.LogInfo($"Climb speed set to: {multiplier:F2}x");
        }

        public void KillPlayer(Character target)
        {
            if (target == null) return;

            try
            {
                if (target.photonView != null)
                {
                    // Use the character's center position for item spawn
                    Vector3 itemSpawnPoint = target.Center + Vector3.up * 0.2f + Vector3.forward * 0.1f;
                    target.photonView.RPC("RPCA_Die", Photon.Pun.RpcTarget.All, itemSpawnPoint);
                    Plugin.Log?.LogInfo($"Killed player: {target.characterName}");
                }
                else
                {
                    // Fallback - set health to 0 if PhotonView not available
                    target.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Injury, 1.0f);
                    Plugin.Log?.LogInfo($"Killed player (fallback): {target.characterName}");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogError($"Failed to kill player {target.characterName}: {ex.Message}");
            }
        }

        public void BringPlayer(Character target, Vector3 position)
        {
            if (target == null) return;

            try
            {
                if (target.photonView != null)
                {
                    target.photonView.RPC("WarpPlayerRPC", Photon.Pun.RpcTarget.All, position, true);
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogError($"Failed to bring player {target.characterName}: {ex.Message}");
            }
        }

        // Property getters for UI
        public bool NoFallDamageEnabled => _noFallDamageEnabled;
        public bool NoWeightEnabled => _noWeightEnabled;
        public bool AfflictionImmunityEnabled => _afflictionImmunityEnabled;
        public bool SpeedModEnabled => _speedModEnabled;
        public bool JumpModEnabled => _jumpModEnabled;
        public bool ClimbModEnabled => _climbModEnabled;
    }
}