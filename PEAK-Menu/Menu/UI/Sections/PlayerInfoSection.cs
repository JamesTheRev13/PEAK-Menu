using UnityEngine;

namespace PEAK_Menu.Menu.UI
{
    public class PlayerInfoSection
    {
        public void Draw(Character character)
        {
            GUILayout.Label("=== Player Information ===");
            GUILayout.Label($"Name: {character.characterName}");
            GUILayout.Label($"Position: {character.Center}");
            GUILayout.Label($"Health: {(1f - character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury)) * 100:F1}%");
            GUILayout.Label($"Stamina: {character.GetTotalStamina() * 100:F1}%");
            GUILayout.Label($"Hunger: {character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hunger) * 100:F1}%");
            GUILayout.Label($"Cold: {character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Cold) * 100:F1}%");
            GUILayout.Label($"Hot: {character.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hot) * 100:F1}%");
            GUILayout.Label($"Grounded: {character.data.isGrounded}");
            GUILayout.Label($"Climbing: {character.data.isClimbingAnything}");
            GUILayout.Space(10);
        }
    }
}