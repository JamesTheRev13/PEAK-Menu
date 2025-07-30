namespace PEAK_Menu.Commands
{
    public class ClearCommand : BaseCommand
    {
        public override string Name => "clear";
        public override string Description => "Clears the console output";

        public override void Execute(string[] parameters)
        {
            // This will integrate with the UI system to clear console
            LogInfo("Console cleared");
        }
    }
}