using UnityEngine;
using System.Collections.Generic;
using PEAK_Menu.Config;
using PEAK_Menu.Menu.UI.Sections;

namespace PEAK_Menu.Menu.UI.Tabs
{
    public class PlayerTab : BaseTab
    {
        private static float _selfStatusValue = UIConstants.STATUS_VALUE_DEFAULT;
        private readonly PlayerInfoSection _playerInfoSection;
        private readonly HealthManagementSection _healthSection;
        private readonly AdminFeaturesSection _adminSection;
        private readonly AppearanceSection _appearanceSection;
        private readonly PlayerModificationsSection _modificationsSection;

        public PlayerTab(MenuManager menuManager, List<string> consoleOutput) 
            : base(menuManager, consoleOutput)
        {
            _playerInfoSection = new PlayerInfoSection();
            _healthSection = new HealthManagementSection();
            _adminSection = new AdminFeaturesSection(this);
            _appearanceSection = new AppearanceSection(menuManager);
            _modificationsSection = new PlayerModificationsSection(menuManager);
        }

        public override void Draw()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(UIConstants.TAB_HEIGHT));
            
            var character = Character.localCharacter;
            if (character == null)
            {
                GUILayout.Label("No character found");
                GUILayout.EndScrollView();
                return;
            }

            _playerInfoSection.Draw(character);
            _healthSection.Draw(character, ref _selfStatusValue, AddToConsole);
            _adminSection.Draw(character, AddToConsole);
            _appearanceSection.Draw(character, AddToConsole);
            _modificationsSection.Draw(AddToConsole);

            GUILayout.Space(UIConstants.LARGE_SPACING);
            GUILayout.EndScrollView();
        }

        // Expose the toggle button methods for sections
        public new bool DrawToggleButton(string featureName, bool isEnabled, float width = 0, int buttonId = -1)
            => base.DrawToggleButton(featureName, isEnabled, width, buttonId);

        public new bool DrawToggleButtonWithStatus(string featureName, bool isEnabled, 
            float buttonWidth = UIConstants.BUTTON_TOGGLE_WIDTH, float statusWidth = UIConstants.STATUS_LABEL_WIDTH, int buttonId = -1)
            => base.DrawToggleButtonWithStatus(featureName, isEnabled, buttonWidth, statusWidth, buttonId);
    }
}