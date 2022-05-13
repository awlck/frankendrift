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
        private CheckBox _suppressLocation;
        private Label _fontPickerLabel;
        private ComboBox _fontPicker;
        private Label _fontSizeLabel;
        private NumericStepper _fontSize;

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

            _suppressLocation = new CheckBox
            {
                Text = "Suppress display of location names",
                Checked = SettingsManager.Settings.SuppressLocationName,
                ToolTip = "Whether to display the names of locations as you enter them."
            };

            _fontSizeLabel = new Label {Text = SettingsManager.Settings.EnableDevFont ? "Alter default size by:" : "Use this font size:" };

            _fontSize = new NumericStepper {
                Value = SettingsManager.Settings.EnableDevFont ? SettingsManager.Settings.AlterFontSize : SettingsManager.Settings.UserFontSize,
                MinValue = SettingsManager.Settings.EnableDevFont ? -10 : 6,
                ToolTip = "(The application needs to be restarted for this setting to take effect)"
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
                    new TableRow { Cells = { _fontSizeLabel, _fontSize } },
                    _banComicSans,
                    _anyKeyPrompt,
                    _suppressLocation,
                    new TableRow { Cells = { _okButton, _cancelButton } }
                }
            };
            DefaultButton = _okButton;
            AbortButton = _cancelButton;
        }

        private void DevFontOnCheckedChanged(object sender, EventArgs e)
        {
            if (_devFont.Checked ?? false)
            {
                _fontPickerLabel.Text = "If that font is unavailable, use:";
                _fontSizeLabel.Text = "Alter default size by:";
                _fontSize.Value = SettingsManager.Settings.AlterFontSize;
                _fontSize.MinValue = -10;
            }
            else
            {
                _fontPickerLabel.Text = "Use this font instead:";
                _fontSizeLabel.Text = "Use this font size:";
                _fontSize.Value = SettingsManager.Settings.UserFontSize;
                _fontSize.MinValue = 6;
            }
        }

        private void OkButtonOnClick(object? sender, EventArgs e)
        {
            SettingsManager.Settings.EnableGraphics = _graphics.Checked ?? false;
            SettingsManager.Settings.EnableDevColors = _devColors.Checked ?? false;
            SettingsManager.Settings.EnableDevFont = _devFont.Checked ?? false;
            SettingsManager.Settings.DefaultFontName = _fontPicker.Text;
            if (SettingsManager.Settings.EnableDevFont)
                SettingsManager.Settings.AlterFontSize = (int) _fontSize.Value;
            else
                SettingsManager.Settings.UserFontSize = (int) _fontSize.Value;
            SettingsManager.Settings.BanComicSans = _banComicSans.Checked ?? false;
            SettingsManager.Settings.EnablePressAnyKey = _anyKeyPrompt.Checked ?? false;
            SettingsManager.Settings.SuppressLocationName = _suppressLocation.Checked ?? false;
            if (Adrift.SharedModule.UserSession is not null)
                Adrift.SharedModule.UserSession.bShowShortLocations = !SettingsManager.Settings.SuppressLocationName;
            SettingsManager.Instance.Save();
            Close();
        }
    }
}