namespace PEAK_Menu.Commands
{
    public class ClearCommand : BaseCommand
    {
        public override string Name => "clear";
        public override string Description => "Clears the console output";

        public override void Execute(string[] parameters)
        {
            // Clear the console through the menu manager
            var menuManager = Plugin.Instance?._menuManager;
            if (menuManager != null)
            {
                // We'll need to add a ClearConsole method to MenuManager
                menuManager.ClearConsole();
                LogInfo("Console cleared");
            }
            else
            {
                LogError("Could not access menu manager to clear console");
            }
        }
    }
}