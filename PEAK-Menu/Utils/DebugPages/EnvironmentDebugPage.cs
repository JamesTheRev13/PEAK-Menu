using UnityEngine;
using UnityEngine.UIElements;
using Zorro.Core.CLI;

namespace PEAK_Menu.Utils.DebugPages
{
    public class EnvironmentDebugPage : BaseCustomDebugPage
    {
        public EnvironmentDebugPage()
        {
        }

        protected override void BuildContent()
        {
            BuildTimeSection();
            BuildEnvironmentInfoSection();
        }

        private void BuildTimeSection()
        {
            var section = CreateSection("Time Control");

            CreateSlider("Time Scale", Time.timeScale, 0f, 5f, (value) =>
            {
                Time.timeScale = value;
                AddToConsole($"Time scale: {value:F2}");
            });

            section.Add(CreateButton("Reset Time Scale", () =>
            {
                Time.timeScale = 1f;
                AddToConsole("Time scale reset to normal");
            }));

            _scrollView.Add(section);
        }

        private void BuildEnvironmentInfoSection()
        {
            var section = CreateSection("Environment Information");

            section.Add(CreateButton("Refresh Environment Info", () =>
            {
                var character = Character.localCharacter;
                if (character != null)
                {
                    AddToConsole("=== Environment Status ===");
                    AddToConsole($"Time Scale: {Time.timeScale:F2}");
                    AddToConsole($"Player Position: {character.Center}");
                    AddToConsole($"Frame Rate: {1f / Time.deltaTime:F1} FPS");
                }
            }));

            _scrollView.Add(section);
        }

        public override VisualElement FocusOnDefault()
        {
            return _scrollView;
        }
    }
}