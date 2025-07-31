namespace PEAK_Menu.Commands
{
    public class VersionCommand : BaseCommand
    {
        public override string Name => "version";
        public override string Description => "Shows plugin version information";

        public override string DetailedHelp =>
@"=== VERSION Command Help ===
Shows plugin version information

Usage: version

Displays:
  - Plugin name and version
  - Author information
  - Plugin GUID

Useful for checking which version of the plugin
is currently running and for troubleshooting.";

        public override void Execute(string[] parameters)
        {
            LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}");
            LogInfo($"Author: Bob Saget");
            LogInfo($"GUID: {MyPluginInfo.PLUGIN_GUID}");
        }
    }
}