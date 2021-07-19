using Eto.Drawing;
using Eto.Forms;
using System;

namespace Adravalon.Runner
{
    partial class MainForm : Form
    {
        private AdriftOutput output;
        private TextBox input;
        private Label status;

        private Command loadGameCommand;
        private Command saveGameCommand;
        private Command restoreGameCommand;

        void InitializeComponent()
        {
            Title = "FrankenDrift";
            MinimumSize = new Size(400, 400);
            Padding = 10;

            output = new AdriftOutput(this);
            input = new AdriftInput { PlaceholderText = ">" };
            status = new Label();

            Content = new TableLayout
            {
                Rows = { new TableRow { ScaleHeight = true, Cells = { new TableCell(output) } }, input, status}
            };

            // create a few commands that can be used for the menu and toolbar
            var clickMe = new Command { MenuText = "Click Me!", ToolBarText = "Click Me!" };
            clickMe.Executed += (sender, e) => MessageBox.Show(this, "I was clicked!");

            loadGameCommand = new Command { MenuText = "Open Game", Shortcut = Application.Instance.CommonModifier | Keys.O };
            saveGameCommand = new Command { MenuText = "Save", Enabled = false };
            restoreGameCommand = new Command { MenuText = "Restore", Enabled = false };

            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var aboutCommand = new Command { MenuText = "About..." };
            aboutCommand.Executed += (sender, e) => new AboutDialog
            {
                Copyright = "FrankenDrift (c) 2021 Adrian Welcker\nADRIFT Runner (c) 1996-2020 Campbell Wild",
                ProgramName = "FrankenDrift",
                ProgramDescription = "FrankenDrift: A \"Frankenstein's Monster\" consisting of the ADRIFT Runner Code " +
                                     "with a cross-platform UI layer (Eto.Forms) glued on top.",
                Version = "0.1-alpha4"
            }.ShowDialog(this);

            var settingsCommand = new Command {MenuText = "&Preferences"};
            settingsCommand.Executed += (sender, args) => new SettingsDialog().ShowModal(this);

            // create menu
            Menu = new MenuBar
            {
                Items =
                {
					// File submenu
					new SubMenuItem { Text = "&File", Items = { loadGameCommand, saveGameCommand, restoreGameCommand } },
				},
                ApplicationItems =
                {
					// application (OS X) or file menu (others)
					// new ButtonMenuItem { Text = "&Preferences..." },
                    settingsCommand
                },
                QuitItem = quitCommand,
                AboutItem = aboutCommand
            };

            // create toolbar			
            // ToolBar = new ToolBar { Items = { loadGameCommand, saveGameCommand, restoreGameCommand } };
        }
    }
}
