namespace PEAK_Menu.Commands
{
    public class ClearCommand : BaseCommand
    {
        public override string Name => "clear";
        public override string Description => "Clears the console output";
        public override string DetailedHelp =>
@"=== CLEAR Command Help ===
Clears the console output

Usage: clear

Removes all text from the console window";

        public override void Execute(string[] parameters)
        {
            // Clear the console through the menu manager
            var menuManager = Plugin.Instance?._menuManager;
            if (menuManager != null)
            {
                menuManager.ClearConsole();
                // Don't use LogInfo here as it would immediately add to the cleared console
                Plugin.Log.LogInfo("[clear] Console cleared");
                // Add a simple message after clearing
                menuManager.AddToConsole("Console cleared");
            }
            else
            {
                LogError("Could not access menu manager to clear console");
            }
        }
    }
}