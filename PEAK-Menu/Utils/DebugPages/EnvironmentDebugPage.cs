using UnityEngine;
using UnityEngine.UIElements;
using Zorro.Core.CLI;

namespace PEAK_Menu.Utils.DebugPages
{
    public class EnvironmentDebugPage : BaseCustomDebugPage
    {
        protected override void BuildContent()
        {
            BuildTimeControlSection();
            BuildEnvironmentInfoSection();
            BuildGameplayModifiersSection();
            BuildCharacterEnvironmentSection();
            BuildSystemInfoSection();
        }

        private void BuildTimeControlSection()
        {
            var section = CreateSection("Time Control");

            // Live reactive time scale slider
            CreateLiveSlider("Time Scale", 
                () => Time.timeScale,
                (value) => {
                    Time.timeScale = value;
                    AddToConsole($"Time scale: {value:F2}");
                }, 0f, 5f, section);

            section.Add(CreateButton("Reset Time Scale", () =>
            {
                Time.timeScale = 1f;
                AddToConsole("Time scale reset to normal");
            }));

            _scrollView.Add(section);
        }

        private void BuildEnvironmentInfoSection()
        {
            var section = CreateSection("Day/Night Cycle");

            // Live day/night cycle information
            section.Add(CreateLiveLabel("Day Progress: ", () => {
                if (DayNightManager.instance != null)
                {
                    return $"{DayNightManager.instance.isDay * 100:F1}%";
                }
                return "Manager not available";
            }));

            section.Add(CreateLiveLabel("Is Day: ", () => {
                if (DayNightManager.instance != null)
                {
                    return DayNightManager.instance.isDay > 0.5f ? "Yes" : "No";
                }
                return "Unknown";
            }));

            section.Add(CreateLiveLabel("Night Cold Active: ", () => {
                return Ascents.isNightCold ? "Yes" : "No";
            }));

            _scrollView.Add(section);
        }

        private void BuildGameplayModifiersSection()
        {
            var section = CreateSection("Gameplay Modifiers");

            // Live gameplay multipliers
            section.Add(CreateLiveLabel("Hunger Rate: ", () => {
                return $"{Ascents.hungerRateMultiplier:F2}x";
            }));

            section.Add(CreateLiveLabel("Fall Damage: ", () => {
                return $"{Ascents.fallDamageMultiplier:F2}x";
            }));

            section.Add(CreateLiveLabel("Climb Stamina: ", () => {
                return $"{Ascents.climbStaminaMultiplier:F2}x";
            }));

            _scrollView.Add(section);
        }

        private void BuildCharacterEnvironmentSection()
        {
            var section = CreateSection("Character Environment");

            // Live character environment data
            section.Add(CreateLiveLabel("In Fog: ", () => {
                var character = Character.localCharacter;
                return character?.data.isInFog.ToString() ?? "N/A";
            }));

            section.Add(CreateLiveLabel("Grounded For: ", () => {
                var character = Character.localCharacter;
                return character != null ? $"{character.data.groundedFor:F1}s" : "N/A";
            }));

            section.Add(CreateLiveLabel("Since Grounded: ", () => {
                var character = Character.localCharacter;
                return character != null ? $"{character.data.sinceGrounded:F1}s" : "N/A";
            }));

            section.Add(CreateLiveLabel("Fall Duration: ", () => {
                var character = Character.localCharacter;
                return character != null ? $"{character.data.fallSeconds:F1}s" : "N/A";
            }));

            section.Add(CreateLiveLabel("Is Falling: ", () => {
                var character = Character.localCharacter;
                return character != null ? (character.data.fallSeconds > 0.1f ? "Yes" : "No") : "N/A";
            }));

            _scrollView.Add(section);
        }

        private void BuildSystemInfoSection()
        {
            var section = CreateSection("System Information");

            // Live system performance data
            section.Add(CreateLiveLabel("Frame Rate: ", () => {
                return $"{1f / Time.deltaTime:F1} FPS";
            }));

            section.Add(CreateLiveLabel("Delta Time: ", () => {
                return $"{Time.deltaTime * 1000:F1}ms";
            }));

            section.Add(CreateLiveLabel("Fixed Delta Time: ", () => {
                return $"{Time.fixedDeltaTime * 1000:F1}ms";
            }));

            section.Add(CreateLiveLabel("Unity Version: ", () => {
                return Application.unityVersion;
            }));

            section.Add(CreateLiveLabel("Platform: ", () => {
                return Application.platform.ToString();
            }));

            // Manual refresh for comprehensive environment info
            section.Add(CreateButton("Log Full Environment Report", () =>
            {
                AddToConsole("=== Full Environment Report ===");
                
                // Day/Night cycle info
                if (DayNightManager.instance != null)
                {
                    AddToConsole($"Day Progress: {DayNightManager.instance.isDay * 100:F1}%");
                }
                else
                {
                    AddToConsole("Day/Night Manager: Not available");
                }

                // Weather conditions
                AddToConsole($"Night Cold Active: {Ascents.isNightCold}");
                AddToConsole($"Hunger Rate Multiplier: {Ascents.hungerRateMultiplier:F2}");
                AddToConsole($"Fall Damage Multiplier: {Ascents.fallDamageMultiplier:F2}");
                AddToConsole($"Climb Stamina Multiplier: {Ascents.climbStaminaMultiplier:F2}");

                var character = Character.localCharacter;
                if (character != null)
                {
                    AddToConsole($"In Fog: {character.data.isInFog}");
                    AddToConsole($"Grounded For: {character.data.groundedFor:F1}s");
                    AddToConsole($"Since Grounded: {character.data.sinceGrounded:F1}s");
                    AddToConsole($"Fall Seconds: {character.data.fallSeconds:F1}s");
                }

                // System info
                AddToConsole($"Frame Rate: {1f / Time.deltaTime:F1} FPS");
                AddToConsole($"Time Scale: {Time.timeScale:F2}");
                AddToConsole($"Unity Version: {Application.unityVersion}");
                AddToConsole($"Platform: {Application.platform}");
                AddToConsole("=== End Report ===");
            }));

            _scrollView.Add(section);
        }

        public override VisualElement FocusOnDefault()
        {
            return _scrollView;
        }
    }
}