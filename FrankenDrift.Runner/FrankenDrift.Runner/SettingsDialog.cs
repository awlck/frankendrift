using System;
using Eto.Forms;
using System.Configuration;

namespace FrankenDrift.Runner
{
    public class SettingsDialog : Dialog
    {
        private CheckBox _graphics;
        private CheckBox _devColors;
        private CheckBox _devFont;
        private CheckBox _anyKeyPrompt;

        private Button _okButton;
        private Button _cancelButton;
        
        public SettingsDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            _graphics = new CheckBox {
                Text = "Enable graphics",
                Checked = SettingsManager.Settings.EnableGraphics
            };
            _devColors = new CheckBox {
                Text = "Enable author-chosen colors",
                Checked = SettingsManager.Settings.EnableDevColors,
                ToolTip = "Whether or not FrankenDrift should honor the developer's choice of color. (A restart is needed for this setting to take full effect.)"
            };
            _devFont = new CheckBox
            {
                Text = "Enable author-chosen default font",
                Checked = SettingsManager.Settings.EnableDevFont,
                ToolTip = "Whether or not FrankenDrift should honor the developer's choice of default color. (A restart is needed for this setting to take full effect.)"
            };
            _anyKeyPrompt = new CheckBox {
                Text = "Enable \"Press any key\" prompts",
                Checked = SettingsManager.Settings.EnablePressAnyKey,
                ToolTip = "Whether to show \"(Press any key to continue)\" when the game waits for a key press."
            };

            Title = "Settings - FrankenDrift";
            _okButton = new Button {Text = "OK"};
            _okButton.Click += OkButtonOnClick;
            _cancelButton = new Button {Text = "Cancel"};
            _cancelButton.Click += (sender, args) => Close(); 
            Content = new StackLayout(
                new StackLayoutItem(_graphics),
                new StackLayoutItem(_devColors),
                new StackLayoutItem(_devFont),
                new StackLayoutItem(_anyKeyPrompt),
                new StackLayoutItem(_okButton),
                new StackLayoutItem(_cancelButton));
            DefaultButton = _okButton;
            AbortButton = _cancelButton;
        }

        private void OkButtonOnClick(object? sender, EventArgs e)
        {
            SettingsManager.Settings.EnableGraphics = _graphics.Checked ?? false;
            SettingsManager.Settings.EnableDevColors = _devColors.Checked ?? false;
            SettingsManager.Settings.EnableDevFont = _devFont.Checked ?? false;
            SettingsManager.Settings.EnablePressAnyKey = _anyKeyPrompt.Checked ?? false;
            SettingsManager.Instance.Save();
            Close();
        }
    }
}