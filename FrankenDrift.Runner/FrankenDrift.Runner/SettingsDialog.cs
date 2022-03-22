using System;
using Eto.Forms;
using System.Configuration;

namespace FrankenDrift.Runner
{
    public class SettingsDialog : Dialog
    {
        private CheckBox _graphics;
        private CheckBox _devColors;

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
                Checked = SettingsManager.Instance.Settings.EnableGraphics
            };
            _devColors = new CheckBox {
                Text = "Enable author-chosen colors",
                Checked = SettingsManager.Instance.Settings.EnableDevColors,
                ToolTip = "Whether or not FrankenDrift should honor the developer's choice of color. (A restart is needed for this setting to take full effect.)"
            };

            Title = "Settings - FrankenDrift";
            _okButton = new Button {Text = "OK"};
            _okButton.Click += OkButtonOnClick;
            _cancelButton = new Button {Text = "Cancel"};
            _cancelButton.Click += (sender, args) => Close(); 
            Content = new StackLayout(new StackLayoutItem(_graphics),
                new StackLayoutItem(_devColors),
                new StackLayoutItem(_okButton),
                new StackLayoutItem(_cancelButton));
            DefaultButton = _okButton;
            AbortButton = _cancelButton;
        }

        private void OkButtonOnClick(object? sender, EventArgs e)
        {
            SettingsManager.Instance.Settings.EnableGraphics = _graphics.Checked ?? false;
            SettingsManager.Instance.Settings.EnableDevColors = _devColors.Checked ?? false;
            SettingsManager.Instance.Save();
            Close();
        }
    }
}