namespace PEAK_Menu.Commands
{
    public abstract class BaseCommand : ICommand
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string DetailedHelp { get; } // Each command provides its own help

        public virtual bool CanExecute()
        {
            return true;
        }

        public abstract void Execute(string[] parameters);

        protected void LogInfo(string message)
        {
            Plugin.Log.LogInfo($"[{Name}] {message}");
            Plugin.Instance?._menuManager?.AddToConsole($"[INFO] {message}");
        }

        protected void LogError(string message)
        {
            Plugin.Log.LogError($"[{Name}] {message}");
            Plugin.Instance?._menuManager?.AddToConsole($"[ERROR] {message}");
        }

        protected void LogWarning(string message)
        {
            Plugin.Log.LogWarning($"[{Name}] {message}");
            Plugin.Instance?._menuManager?.AddToConsole($"[WARNING] {message}");
        }
    }
}