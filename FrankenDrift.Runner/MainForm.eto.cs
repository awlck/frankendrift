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

            output = new AdriftOutput(); //{ BackgroundColor = Color.FromRgb(0), SelectionForeground = Color.FromRgb((255<<8) + 255) /* BrowserContextMenuEnabled = true */ };
            input = new AdriftInput();
            status = new Label();

            Content = new TableLayout
            {
                Rows = { new TableRow { ScaleHeight = true, Cells = { new TableCell(output) } }, input, status}
            };

            // create a few commands that can be used for the menu and toolbar
            var clickMe = new Command { MenuText = "Click Me!", ToolBarText = "Click Me!" };
            clickMe.Executed += (sender, e) => MessageBox.Show(this, "I was clicked!");

            loadGameCommand = new Command { MenuText = "Open Game" };
            saveGameCommand = new Command { MenuText = "Save", Enabled = false };
            restoreGameCommand = new Command { MenuText = "Restore", Enabled = false };

            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var aboutCommand = new Command { MenuText = "About..." };
            aboutCommand.Executed += (sender, e) => new AboutDialog().ShowDialog(this);

            // create menu
            Menu = new MenuBar
            {
                Items =
                {
					// File submenu
					new SubMenuItem { Text = "&File", Items = { loadGameCommand, saveGameCommand, restoreGameCommand } },
					// new SubMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
					// new SubMenuItem { Text = "&View", Items = { /* commands/items */ } },
				},
                ApplicationItems =
                {
					// application (OS X) or file menu (others)
					new ButtonMenuItem { Text = "&Preferences..." },
                },
                QuitItem = quitCommand,
                AboutItem = aboutCommand
            };

            // create toolbar			
            // ToolBar = new ToolBar { Items = { loadGameCommand, saveGameCommand, restoreGameCommand } };
        }
    }
}
