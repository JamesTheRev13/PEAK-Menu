namespace PEAK_Menu.Commands
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        void Execute(string[] parameters);
        bool CanExecute();
    }
}