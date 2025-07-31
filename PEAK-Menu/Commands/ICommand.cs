namespace PEAK_Menu.Commands
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        string DetailedHelp { get; } // New property for detailed help
        void Execute(string[] parameters);
        bool CanExecute();
    }
}