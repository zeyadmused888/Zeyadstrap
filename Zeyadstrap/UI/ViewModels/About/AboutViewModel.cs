using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Zeyadstrap.UI.ViewModels.About
{
    public class AboutViewModel : NotifyPropertyChangedViewModel
    {
        public string Version => string.Format(Strings.Menu_About_Version, App.Version);

        public BuildMetadataAttribute BuildMetadata => App.BuildMetadata;

        public string BuildTimestamp => BuildMetadata.Timestamp.ToFriendlyString();
        public string BuildCommitHashUrl => $"https://github.com/{App.ProjectRepository}/commit/{BuildMetadata.CommitHash}";


        public string RuntimeVersion => $".NET {Environment.Version}";

        public string InstallLocation => Paths.Base;

        public string ProcessLocation => Paths.Process;

        public string DiscordApplicationConfigured => String.IsNullOrWhiteSpace(App.DiscordApplicationId) ? "No" : "Yes";

        public string GameRichPresenceEnabled => App.Settings.Prop.UseDiscordRichPresence ? "Enabled" : "Disabled";

        public string LauncherRichPresenceEnabled => App.Settings.Prop.ShowZeyadstrapRichPresence ? "Enabled" : "Disabled";

        public ICommand OpenLogsFolderCommand => new RelayCommand(OpenLogsFolder);

        private static void OpenLogsFolder()
        {
            Directory.CreateDirectory(Paths.Logs);
            Process.Start("explorer.exe", Paths.Logs);
        }
        public Visibility BuildInformationVisibility => App.IsProductionBuild ? Visibility.Collapsed : Visibility.Visible;
        public Visibility BuildCommitVisibility => App.IsActionBuild ? Visibility.Visible : Visibility.Collapsed;
    }
}
