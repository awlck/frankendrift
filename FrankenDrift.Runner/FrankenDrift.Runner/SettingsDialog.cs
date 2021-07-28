using System;
using Eto.Forms;
using System.Configuration;

namespace FrankenDrift.Runner
{
    public class SettingsDialog : Dialog
    {
        private CheckBox _graphics;
        private Button _okButton;
        private Button _cancelButton;
        
        public SettingsDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Title = "Settings - FrankenDrift";
            _graphics = new CheckBox {Text = "Enable graphics", Checked = SettingsManager.Instance.Settings.EnableGraphics};
            _okButton = new Button {Text = "OK"};
            _okButton.Click += OkButtonOnClick;
            _cancelButton = new Button {Text = "Cancel"};
            _cancelButton.Click += (sender, args) => Close(); 
            Content = new StackLayout(new StackLayoutItem(_graphics),
                new StackLayoutItem(_okButton),
                new StackLayoutItem(_cancelButton));
            DefaultButton = _okButton;
            AbortButton = _cancelButton;
        }

        private void OkButtonOnClick(object? sender, EventArgs e)
        {
            SettingsManager.Instance.Settings.EnableGraphics = _graphics.Checked ?? false;
            SettingsManager.Instance.Save();
            Close();
        }
    }
}