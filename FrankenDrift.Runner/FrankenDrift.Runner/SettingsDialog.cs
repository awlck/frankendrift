using System;
using System.Linq;
using Eto.Forms;

namespace FrankenDrift.Runner
{
    public class SettingsDialog : Dialog
    {
        private CheckBox _graphics;
        private CheckBox _devColors;
        private CheckBox _devFont;
        private CheckBox _banComicSans;
        private CheckBox _anyKeyPrompt;
        private Label _fontPickerLabel;
        private ComboBox _fontPicker;

        private Button _okButton;
        private Button _cancelButton;
        
        public SettingsDialog()
        {
            _graphics = new CheckBox {
                Text = "Enable graphics",
                Checked = SettingsManager.Settings.EnableGraphics
            };

            _devColors = new CheckBox {
                Text = "Enable author-chosen default colors",
                Checked = SettingsManager.Settings.EnableDevColors,
                ToolTip = "Whether or not FrankenDrift should honor the developer's choice of default colors.\n(A restart is needed for this setting to take full effect.)"
            };

            _devFont = new CheckBox
            {
                Text = "Enable author-chosen default font",
                Checked = SettingsManager.Settings.EnableDevFont,
                ToolTip = "Whether or not FrankenDrift should honor the developer's choice of default font.\n(A restart is needed for this setting to take full effect.)"
            };
            _devFont.CheckedChanged += DevFontOnCheckedChanged;

            _fontPickerLabel = new Label { Text = SettingsManager.Settings.EnableDevFont ? "If that font is unavailable, use:" : "Use this font instead:" };

            _fontPicker = new ComboBox();
            _fontPicker.Items.AddRange(Eto.Drawing.Fonts.AvailableFontFamilies.Select(f => new ListItem { Text = f.Name }));
            _fontPicker.Text = SettingsManager.Settings.DefaultFontName ?? "";
            _fontPicker.ToolTip = "(The application needs to be restarted for this setting to take effect)";

            _banComicSans = new CheckBox
            {
                Text = "Ban Comic Sans",
                Checked = SettingsManager.Settings.BanComicSans,
                ToolTip = "If this is checked, any and all requests to use the Comic Sans font will be ignored."
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
            Content = new TableLayout {
                Rows = {
                    _graphics,
                    _devColors,
                    _devFont,
                    new TableRow { Cells = { _fontPickerLabel, _fontPicker } },
                    _banComicSans,
                    _anyKeyPrompt,
                    new TableRow { Cells = { _okButton, _cancelButton } }
                }
            };
            DefaultButton = _okButton;
            AbortButton = _cancelButton;
        }

        private void DevFontOnCheckedChanged(object sender, EventArgs e)
        {
            _fontPickerLabel.Text = _devFont.Checked ?? false ? "If that font is unavailable, use:" : "Use this font instead:";
        }

        private void OkButtonOnClick(object? sender, EventArgs e)
        {
            SettingsManager.Settings.EnableGraphics = _graphics.Checked ?? false;
            SettingsManager.Settings.EnableDevColors = _devColors.Checked ?? false;
            SettingsManager.Settings.EnableDevFont = _devFont.Checked ?? false;
            SettingsManager.Settings.DefaultFontName = _fontPicker.Text;
            SettingsManager.Settings.BanComicSans = _banComicSans.Checked ?? false;
            SettingsManager.Settings.EnablePressAnyKey = _anyKeyPrompt.Checked ?? false;
            SettingsManager.Instance.Save();
            Close();
        }
    }
}