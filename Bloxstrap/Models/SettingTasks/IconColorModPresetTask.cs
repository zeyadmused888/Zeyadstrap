using Bloxstrap.Models.SettingTasks.Base;

namespace Bloxstrap.Models.SettingTasks
{
    public class IconColorModPresetTask : BaseTask
    {
        private bool _originalEnabled;
        private string _originalColor;
        private bool _newEnabled;
        private string _newColor;

        public bool Enabled
        {
            get => _newEnabled;
            set
            {
                _newEnabled = value;
                UpdatePendingState();
            }
        }

        public string Color
        {
            get => _newColor;
            set
            {
                _newColor = value;
                UpdatePendingState();
            }
        }

        public override bool Changed => _newEnabled != _originalEnabled || !String.Equals(_newColor, _originalColor, StringComparison.OrdinalIgnoreCase);

        public IconColorModPresetTask() : base("ModPreset", "IconColor")
        {
            _originalEnabled = App.Settings.Prop.IconColorEnabled;
            _originalColor = App.Settings.Prop.IconColor;
            _newEnabled = _originalEnabled;
            _newColor = _originalColor;
        }

        public override void Execute()
        {
            App.Settings.Prop.IconColorEnabled = _newEnabled;
            App.Settings.Prop.IconColor = BuilderIconColorizer.IsValidHexColor(_newColor) ? _newColor.ToUpperInvariant() : "#FFFFFF";

            BuilderIconColorizer.RemoveModFiles();

            if (_newEnabled)
            {
                string versionDirectory = Path.Combine(Paths.Versions, App.PlayerState.Prop.VersionGuid);

                if (Directory.Exists(versionDirectory))
                    BuilderIconColorizer.GenerateModFiles(versionDirectory, App.Settings.Prop.IconColor);
            }

            _originalEnabled = _newEnabled;
            _originalColor = App.Settings.Prop.IconColor;
        }

        private void UpdatePendingState()
        {
            if (Changed)
                App.PendingSettingTasks[Name] = this;
            else
                App.PendingSettingTasks.Remove(Name);
        }
    }
}