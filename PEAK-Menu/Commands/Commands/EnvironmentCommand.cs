namespace PEAK_Menu.Commands
{
    public class EnvironmentCommand : BaseCommand
    {
        public override string Name => "environment";
        public override string Description => "Shows environment information";

        public override string DetailedHelp =>
@"=== ENVIRONMENT Command Help ===
Shows environment and world information

Usage: environment

Displays:
  - Day/night cycle progress
  - Weather conditions (night cold)
  - Game multipliers (hunger, fall damage, climb stamina)
  - Character environmental state (fog, grounding, falling)

Useful for monitoring current world conditions
and understanding gameplay modifiers.";

        public override void Execute(string[] parameters)
        {
            LogInfo("=== Environment ===");
            
            // Day/Night cycle info
            if (DayNightManager.instance != null)
            {
                LogInfo($"Day Progress: {DayNightManager.instance.isDay * 100:F1}%");
            }
            else
            {
                LogInfo("Day/Night Manager: Not available");
            }

            // Weather conditions
            LogInfo($"Night Cold Active: {Ascents.isNightCold}");
            LogInfo($"Hunger Rate Multiplier: {Ascents.hungerRateMultiplier:F2}");
            LogInfo($"Fall Damage Multiplier: {Ascents.fallDamageMultiplier:F2}");
            LogInfo($"Climb Stamina Multiplier: {Ascents.climbStaminaMultiplier:F2}");

            var character = Character.localCharacter;
            if (character != null)
            {
                LogInfo($"In Fog: {character.data.isInFog}");
                LogInfo($"Grounded For: {character.data.groundedFor:F1}s");
                LogInfo($"Since Grounded: {character.data.sinceGrounded:F1}s");
                LogInfo($"Fall Seconds: {character.data.fallSeconds:F1}s");
            }
        }
    }
}