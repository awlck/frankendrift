using FrankenDrift.Glue;
using FrankenDrift.Glue.Infragistics.Win.UltraWinToolbars;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Application = Eto.Forms.Application;

namespace FrankenDrift.Runner
{
    public partial class MainForm : Form, Glue.UIGlue, frmRunner
    {
        private AdriftOutput output;
        private TextBox input;
        private Label status;

        private Command loadGameCommand;
        private Command saveGameCommand;
        private Command restoreGameCommand;
        private Command transcriptCommand;
        private Command replayCommand;

        public UltraToolbarsManager UTMMain => throw new NotImplementedException();

        public RichTextBox txtOutput => output;

        public RichTextBox txtInput => (Glue.RichTextBox) input;

        public bool Locked => false;

        private AdriftMap map;
        private Dictionary<string, SecondaryWindow> _secondaryWindows = new();
        private GraphicsWindow _graphics = null;
        private UITimer _timer;
        private bool _isTranscriptActive = false;
        
        internal GraphicsWindow Graphics { get
        {
            if (_graphics is null)
            {
                _graphics = new GraphicsWindow(this);
                _graphics.Title = "Graphics - " + Title;
                _graphics.ShowActivated = false;
            }
            _graphics.Show();
            return _graphics;
        }}

        public MainForm()
        {
            InitializeComponent();
            map = new AdriftMap();
            _timer = new UITimer { Interval = 1.0d };

            loadGameCommand.Executed += LoadGameCommandOnExecuted;
            saveGameCommand.Executed += SaveGameCommandOnExecuted;
            restoreGameCommand.Executed += RestoreGameCommandOnExecuted;
            transcriptCommand.Executed += TranscriptCommandOnExecuted;
            replayCommand.Executed += ReplayCommandOnExecuted;
            _timer.Elapsed += _timer_Elapsed;

            input.KeyDown += Input_KeyDown;

            Adrift.SharedModule.Glue = this;
            Adrift.SharedModule.fRunner = this;
            Adrift.SharedModule.UserSession = new Adrift.RunnerSession {Map = map};
            Glue.Application.SetFrontend(this);
            output.AppendHtml("FrankenDrift v0.1");
        }

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
                Rows = { new TableRow { ScaleHeight = true, Cells = { new TableCell(output) } }, input, status }
            };

            loadGameCommand = new Command { MenuText = "Open Game", Shortcut = Application.Instance.CommonModifier | Keys.O };
            saveGameCommand = new Command { MenuText = "Save", Enabled = false };
            restoreGameCommand = new Command { MenuText = "Restore", Enabled = false };
            transcriptCommand = new Command { MenuText = "Start Transcript", Enabled = false };
            replayCommand = new Command { MenuText = "Replay Commands", Enabled = false };

            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var aboutCommand = new Command { MenuText = "About..." };
            aboutCommand.Executed += (sender, e) => new AboutDialog
            {
                Copyright = "FrankenDrift (c) 2021-22 Adrian Welcker\nADRIFT Runner (c) 1996-2020 Campbell Wild",
                ProgramName = "FrankenDrift",
                ProgramDescription = "FrankenDrift: A \"Frankenstein's Monster\" consisting of the ADRIFT Runner Code " +
                                     "with a cross-platform UI layer (Eto.Forms) glued on top.",
                Version = "0.1"
            }.ShowDialog(this);

            var settingsCommand = new Command { MenuText = "&Preferences" };
            settingsCommand.Executed += (sender, args) => new SettingsDialog().ShowModal(this);

            // create menu
            Menu = new MenuBar
            {
                Items =
                {
					// File submenu
					new SubMenuItem { Text = "&File", Items = { loadGameCommand } },
                    new SubMenuItem { Text = "&Game", Items = { saveGameCommand, restoreGameCommand, transcriptCommand, replayCommand }}
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

        private void _timer_Elapsed(object sender, EventArgs e)
        {
            if (Adrift.SharedModule.UserSession != null)
                Adrift.SharedModule.UserSession.TimeBasedStuff();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (output.IsWaiting)
            {
                e.Handled = true;
                output.FinishWaiting();
            }

            if (e.Key != Keys.Enter) return;
            if (Adrift.SharedModule.Adventure is not null)
            {
                SubmitCommand(input.Text);
            }
#if DEBUG
            else if (input.Text.StartsWith("<>")) OutputHTML(input.Text[2..]);
#endif
            else OutputHTML("(Click File > Open Game to load a game!)");

            input.Text = "";
            e.Handled = true;
        }
        
        public void SubmitCommand(string cmd)
        {
            OutputHTML("<br>");
            if (cmd.Length > 0)
            {
                var cmds = Adrift.SharedModule.UserSession.salCommands;
                cmds.Add("");
                cmds[^2] = input.Text;
                Adrift.SharedModule.Adventure.Turns++;
            }

            Adrift.SharedModule.UserSession.Process(cmd);
        }

        internal string QueryLoadPath()
        {
            var ofd = new OpenFileDialog { MultiSelect = false };
            ofd.Filters.Add(new FileFilter { Name = "ADRIFT Game File", Extensions = new[] { ".taf", ".blorb" } });
            var result = ofd.ShowDialog(this);
            return result == DialogResult.Ok ? ofd.FileName : "";
        }

        private void RestoreGameCommandOnExecuted(object sender, EventArgs e)
        {
            if (Adrift.SharedModule.Adventure is not null)
            {
                Adrift.SharedModule.UserSession.Process("restore");
            }
        }

        private void SaveGameCommandOnExecuted(object sender, EventArgs e)
        {
            if (Adrift.SharedModule.Adventure is not null)
            {
                Adrift.SharedModule.UserSession.Process("save");
            }
        }

        private void LoadGameCommandOnExecuted(object sender, EventArgs e)
        {
            var toLoad = QueryLoadPath();
            if (!string.IsNullOrWhiteSpace(toLoad))
            {
                Adrift.SharedModule.UserSession.OpenAdventure(toLoad);
                if (Adrift.SharedModule.Adventure is null) return;
                saveGameCommand.Enabled = true;
                restoreGameCommand.Enabled = true;
                transcriptCommand.Enabled = true;
                replayCommand.Enabled = true;
                _timer.Start();
            }
        }
        
        private void TranscriptCommandOnExecuted(object? sender, EventArgs e)
        {
            if (_isTranscriptActive)
            {
                Adrift.SharedModule.UserSession.sTranscriptFile = "";
                transcriptCommand.MenuText = "Start Transcript";
                _isTranscriptActive = false;
            }
            else
            {
                var sfd = new SaveFileDialog();
                sfd.Filters.Add(new FileFilter { Name = "Text file", Extensions = new[] { ".txt" } });
                var result = sfd.ShowDialog(this);
                if (result != DialogResult.Ok) return;
                Adrift.SharedModule.UserSession.sTranscriptFile = sfd.FileName;
                transcriptCommand.MenuText = "Stop Transcript";
                _isTranscriptActive = true;
            }
        }

        private void ReplayCommandOnExecuted(object? sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filters.Add(new FileFilter { Name = "Command files", Extensions = new[] { ".txt", ".cmd" } });
            var result = ofd.ShowDialog(this);
            if (result != DialogResult.Ok) return;
            var lines = File.ReadLines(ofd.FileName);
            foreach (var line in lines)
            {
                if (output.IsWaiting) output.FinishWaiting();
                if (line.StartsWith("<KEY>")) continue;
                SubmitCommand(line);
            }
        }

        internal AdriftOutput GetSecondaryWindow(string name)
        {
            if (_secondaryWindows.ContainsKey(name)) return _secondaryWindows[name].Output;
            var win = new SecondaryWindow(this);
            win.ShowActivated = false;
            win.Title = name + " - " + this.Title;
            win.Show();
            _secondaryWindows[name] = win;
            return win.Output;
        }

        public void ErrMsg(string message, Exception ex = null)
        {
            var msg = $"ADRIFT ERROR: {message}";
            if (ex is not null) {
                msg += $"\n{ex!.GetType()}: {ex!.Message}\n{ex.StackTrace}";
            }
            MessageBox.Show(msg, MessageBoxType.Error);
        }

        public void MakeNote(string msg)
        {
            output.Text += $"\n*** ADRIFT Note: {msg}\n";
        }

        public void EnableButtons()
        {
            throw new NotImplementedException();
        }

        public void SetGameName(string name)
        {
            Title = $"{name} - FrankenDrift";
        }

        public bool IsTranscriptActive() => _isTranscriptActive;

        public void ScrollToEnd()
        {
            // output.ScrollToBottom();
        }

        public bool AskYesNoQuestion(string question, string title = null)
        {
            var result = MessageBox.Show(question, title ?? "FrankenDrift", MessageBoxType.Question);
            return result == DialogResult.Yes;
        }

        public void ShowInfo(string info, string title = null)
        {
            MessageBox.Show(info, title ?? "FrankenDrift", MessageBoxType.Information);
        }

        public void ComplainAboutVersionMismatch(string advVer, string terpVer)
        {
            MessageBox.Show(
                $"The underlying Runner (v{terpVer}) can't run this game, which was written with ADRIFT version {advVer}.",
                "FrankenDrift", MessageBoxType.Error);
        }

        public string QuerySavePath()
        {
            var sfd = new SaveFileDialog();
            sfd.Filters.Add(new FileFilter { Name = "ADRIFT Save File", Extensions = new[] { ".tas" } });
            var result = sfd.ShowDialog(this);
            return result == DialogResult.Ok ? sfd.FileName : "";
        }

        public string QueryRestorePath()
        {
            var ofd = new OpenFileDialog { MultiSelect = false };
            ofd.Filters.Add(new FileFilter { Name = "ADRIFT Save File", Extensions = new[] { ".tas" } });
            var result = ofd.ShowDialog(this);
            return result == DialogResult.Ok ? ofd.FileName : "";
        }

        public QueryResult QuerySaveBeforeQuit()
        {
            var result = MessageBox.Show("Would you like to save before quitting?", MessageBoxButtons.YesNoCancel,
                MessageBoxType.Question);
            switch (result)
            {
                case DialogResult.Yes:
                    return QueryResult.YES;
                case DialogResult.No:
                    return QueryResult.NO;
                case DialogResult.Cancel:
                    return QueryResult.CANCEL;
            }
            return QueryResult.CANCEL;
        }

        public void OutputHTML(string source) => output.AppendHtml(source);

        public void InitInput()
        {
            // Nothing to do
        }

        public void ShowCoverArt(System.Drawing.Image img)
        {
            throw new NotImplementedException();
        }

        public void DoEvents()
        {
            Application.Instance.RunIteration();
        }

        public string GetAppDataPath()
        {
            throw new NotImplementedException();
        }

        public string GetExecutableLocation()
        {
            return System.IO.Path.GetDirectoryName(GetExecutablePath());
        }

        public string GetExecutablePath()
        {
            return typeof(Adrift.SharedModule).Assembly.Location;
        }

        public string GetClaimedAdriftVersion()
        {
            return "5.0000364";
        }

        public void ReloadMacros()
        {
            throw new NotImplementedException();
        }

        public void SaveLayout()
        {
            // pass for now
        }

        public void SetBackgroundColour()
        {
            output.BackgroundColor = output._defaultBackground;
        }

        public void UpdateStatusBar(string desc, string score, string user)
        {
            status.Text = desc;
            if (!string.IsNullOrWhiteSpace(score)) status.Text += $" -- {score}";
            if (!string.IsNullOrWhiteSpace(user)) status.Text += $" -- {Adrift.SharedModule.ReplaceALRs(user)}";
        }

        public void SubmitCommand()
        {
            SubmitCommand(input.Text);
            input.Text = "";
        }
        
        internal void ReportSecondaryClosing(SecondaryWindow secondaryWindow)
        {
            if (!_secondaryWindows.ContainsValue(secondaryWindow)) return;
            foreach(var item in _secondaryWindows.Where(kvp => kvp.Value == secondaryWindow).ToList())
                _secondaryWindows.Remove(item.Key);
        }

        internal void ReportGraphicsClosing(GraphicsWindow graphics)
        {
            if (_graphics != graphics) throw new ArgumentException("Reporting closure of a graphics window that isn't ours", nameof(graphics));
            _graphics = null;
        }
    }
}
