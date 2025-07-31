namespace PEAK_Menu.Commands
{
    public class StatusCommand : BaseCommand
    {
        public override string Name => "status";
        public override string Description => "Shows detailed player status information";
        
        public override string DetailedHelp =>
@"=== STATUS Command Help ===
Shows detailed player status information

Usage: status

Displays:
  - Health percentage
  - Stamina percentage
  - Hunger level
  - Temperature (Cold/Hot)
  - Poison level
  - Position coordinates
  - Movement state (grounded, climbing)
  - Life state (passed out, dead)";

        public override void Execute(string[] parameters)
        {
            var character = Character.localCharacter;
            if (character == null)
            {
                LogError("No local character found");
                return;
            }

            var afflictions = character.refs.afflictions;
            var data = character.data;

            LogInfo("=== Player Status ===");
            LogInfo($"Health: {(1f - afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury)) * 100:F1}%");
            LogInfo($"Stamina: {character.GetTotalStamina() * 100:F1}%");
            LogInfo($"Hunger: {afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hunger) * 100:F1}%");
            LogInfo($"Cold: {afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Cold) * 100:F1}%");
            LogInfo($"Hot: {afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hot) * 100:F1}%");
            LogInfo($"Poison: {afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Poison) * 100:F1}%");
            LogInfo($"Position: {character.Center}");
            LogInfo($"Grounded: {data.isGrounded}");
            LogInfo($"Climbing: {data.isClimbingAnything}");
            LogInfo($"Passed Out: {data.passedOut}");
            LogInfo($"Dead: {data.dead}");
        }
    }
}