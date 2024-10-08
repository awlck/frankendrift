using FrankenDrift.Glue;
using FrankenDrift.Glue.Infragistics.Win.UltraWinToolbars;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Application = Eto.Forms.Application;
using System.Text;

namespace FrankenDrift.Runner
{
    public partial class MainForm : Form, Glue.UIGlue, frmRunner
    {
        private AdriftOutput output;
        private AdriftInput input;
        private Label status;

        private Command loadGameCommand;
        private Command saveGameCommand;
        private Command restoreGameCommand;
        private Command restartGameCommand;
        private Command transcriptCommand;
        private Command replayCommand;
        private Command showMapCommand;
        private Command clearScreenCommand;

        public UltraToolbarsManager UTMMain => throw new NotImplementedException();

        public RichTextBox txtOutput => output;

        public RichTextBox txtInput => (Glue.RichTextBox)input;

        public bool Locked => false;

        private AdriftMap map;
        private readonly Dictionary<string, SecondaryWindow> _secondaryWindows = new();
        private GraphicsWindow _graphics = null;
        private readonly UITimer _timer;
        private FileStream _transcript = null;
        private bool _isReplaying = false;
        private bool _shouldReplayCancel = false;
        private int _commandRecallIdx = 0;
        private readonly string _myVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        private static bool GameIsOngoing => Adrift.SharedModule.Adventure is not null && Adrift.SharedModule.Adventure.eGameState == Adrift.clsAction.EndGameEnum.Running;

        internal GraphicsWindow Graphics { get
            {
                _graphics ??= new GraphicsWindow(this)
                {
                    Title = "Graphics - " + Title,
                    ShowActivated = false
                };
                _graphics.Show();
                return _graphics;
            } }

        public bool Quitting { get; private set; } = false;

        public MainForm()
        {
            InitializeComponent();
            map = new AdriftMap(this);
            _timer = new UITimer { Interval = 1.0d };

            loadGameCommand.Executed += LoadGameCommandOnExecuted;
            saveGameCommand.Executed += SaveGameCommandOnExecuted;
            restoreGameCommand.Executed += RestoreGameCommandOnExecuted;
            restartGameCommand.Executed += RestartGameCommandOnExecuted;
            transcriptCommand.Executed += TranscriptCommandOnExecuted;
            replayCommand.Executed += ReplayCommandOnExecuted;
            showMapCommand.Executed += ShowMapCommandOnExecuted;
            clearScreenCommand.Executed += ClearScreenCommandOnExecuted;
            _timer.Elapsed += TimerOnElapsed;
            KeyDown += MainFormOnKeyDown;
            Closing += MainFormOnClosing;
            // ensure the application quits after the main form closes, even on platforms where that wouldn't normally be the case,
            // or if there are auxiliary windows still open.
            Closed += MainFormOnClosed;
            // need to do this or gtk will keep recursively generating closing events as we try to quit.
            Application.Instance.Terminating += (s, e) => {
                Closing -= MainFormOnClosing;
                Closed -= MainFormOnClosed;
            };

            input.KeyDown += InputOnKeyDown;
            output.KeyDown += OutputOnKeyDown;

            if (!SettingsManager.Settings.SuppressMap)
                map.Show();

            Adrift.SharedModule.Glue = this;
            Adrift.SharedModule.fRunner = this;
            Adrift.SharedModule.UserSession = new Adrift.RunnerSession { Map = map };
            Glue.Application.SetFrontend(this);
            Adrift.SharedModule.UserSession.bShowShortLocations = !SettingsManager.Settings.SuppressLocationName;
            output.AppendHtml($"FrankenDrift {_myVersion}");
        }

        void InitializeComponent()
        {
            Title = "FrankenDrift";
            MinimumSize = new Size(400, 400);
            Size = new Size(600, 750);
            Padding = 10;

            if (Application.Instance.Platform.IsGtk)
                output = (AdriftOutput)new OutputLateFormatting(this);
            else
                output = new AdriftOutput(this);
            input = new AdriftInput { PlaceholderText = ">" };
            status = new Label();

            Content = new TableLayout
            {
                Rows = { new TableRow { ScaleHeight = true, Cells = { new TableCell(output) } }, input, status }
            };

            loadGameCommand = new Command { MenuText = "Open Game", Shortcut = Application.Instance.CommonModifier | Keys.O };
            saveGameCommand = new Command { MenuText = "Save", Enabled = false, Shortcut = Application.Instance.CommonModifier | Keys.S };
            restoreGameCommand = new Command { MenuText = "Restore a Saved Game", Enabled = false, Shortcut = Application.Instance.CommonModifier | Keys.R };
            restartGameCommand = new Command { MenuText = "Restart Game", Enabled = false, Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.R };
            transcriptCommand = new Command { MenuText = "Start Transcript", Enabled = false, Shortcut = Application.Instance.CommonModifier | Keys.T };
            replayCommand = new Command { MenuText = "Replay Commands", Enabled = false, Shortcut = Application.Instance.CommonModifier | Application.Instance.AlternateModifier | Keys.R };
            showMapCommand = new Command { MenuText = "Open Map (experimental)", Enabled = true, Shortcut = Application.Instance.CommonModifier | Keys.M };
            clearScreenCommand = new Command { MenuText = "Clear Screen", Enabled = true, ToolTip = "Clears the output window, removing all text. Doing this occasionally can improve performance during long-lasting game sessions." };

            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var aboutCommand = new Command { MenuText = "About..." };
            aboutCommand.Executed += (sender, e) => new AboutDialog
            {
                Copyright = "FrankenDrift (c) 2021-24 Adrian Welcker\nADRIFT Runner (c) 1996-2020 Campbell Wild",
                ProgramName = "FrankenDrift",
                ProgramDescription = "FrankenDrift: A \"Frankenstein's Monster\" consisting of the ADRIFT Runner Code " +
                                     "with a cross-platform UI layer (Eto.Forms) glued on top.",
                License = Licenses.LicenseText,
                Version = _myVersion
            }.ShowDialog(this);

            var settingsCommand = new Command { MenuText = "&Preferences" };
            settingsCommand.Executed += (sender, args) => new SettingsDialog().ShowModal(this);

            // create menu
            Menu = new MenuBar
            {
                Items =
                {
                    new SubMenuItem { Text = "&File", Items = { loadGameCommand } },
                    new SubMenuItem { Text = "&Game", Items = { saveGameCommand, restoreGameCommand, restartGameCommand, transcriptCommand, replayCommand }},
                    new SubMenuItem { Text = "&View", Items = { showMapCommand, clearScreenCommand }}
                },
                ApplicationItems =
                {
					// application (OS X) or file menu (others)
                    settingsCommand
                },
                QuitItem = quitCommand,
                AboutItem = aboutCommand
            };
        }

        private void ClearScreenCommandOnExecuted(object sender, EventArgs e)
        {
            output.Clear();
        }

        private void ShowMapCommandOnExecuted(object sender, EventArgs e)
        {
            map.Show();
        }

        private void TimerOnElapsed(object sender, EventArgs e)
        {
            Adrift.SharedModule.UserSession?.TimeBasedStuff();
        }

        private void InputOnKeyDown(object sender, KeyEventArgs e)
        {
            if (output.IsWaiting)
            {
                e.Handled = true;
                output.FinishWaiting();
            }

            if (e.Key == Keys.Up)
            {
                e.Handled = true;
                _commandRecallIdx++;
                if (_commandRecallIdx > Adrift.SharedModule.UserSession.salCommands.Count - 1) return;
                input.Text = Adrift.SharedModule.UserSession.salCommands[^(_commandRecallIdx + 1)];
                return;
            }
            else if (e.Key == Keys.Down)
            {
                e.Handled = true;
                input.Text = Adrift.SharedModule.UserSession.salCommands[^(_commandRecallIdx)];
                _commandRecallIdx--;
                if (_commandRecallIdx < 0) _commandRecallIdx = 0;
                return;
            }

            if (e.Key != Keys.Enter) return;
            if (Adrift.SharedModule.Adventure is not null)
            {
                SubmitCommand(input.Text);
            }
#if DEBUG
            else if (input.Text.StartsWith("<>")) OutputHTML(input.Text[2..]);
#endif
            else OutputHTML("\n(Click File > Open Game to load a game!)");

            input.Text = "";
            e.Handled = true;
        }

        private void MainFormOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Escape && _isReplaying)
            {
                _shouldReplayCancel = true;
            }
        }

        private void MainFormOnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Quitting = true;
            if (GameIsOngoing)
            {
                var result = QuerySaveBeforeQuit();
                switch (result)
                {
                    case QueryResult.YES:
                        SaveGameCommandOnExecuted(null, null);
                        break;
                    case QueryResult.NO:
                        break;
                    case QueryResult.CANCEL:
                        e.Cancel = true;
                        return;
                }
            }
            if (_transcript is not null)
                _transcript.Dispose();
            Closing -= MainFormOnClosing;
        }

        private void MainFormOnClosed(object sender, EventArgs e)
        {
            Application.Instance.Quit();
        }

        private void OutputOnKeyDown(object sender, KeyEventArgs e)
        {
            // Allow keyboard shortcuts, e.g. for copying selected text
            if (e.Control || e.Alt || e.Application)
                return;
            // ...but redirect everything else to the input bar.
            input.Focus();
            if (e.IsChar) input.Text += e.KeyChar.ToString();
            e.Handled = true;
        }

        public void SubmitCommand(string cmd)
        {
            _transcript?.Flush();  // ensure the transcript is written to disk each turn.
            OutputHTML("<br>");
            if (cmd.Length > 0)
            {
                var cmds = Adrift.SharedModule.UserSession.salCommands;
                cmds.Add("");
                cmds[^2] = cmd.Trim();
                Adrift.SharedModule.Adventure.Turns++;
                _commandRecallIdx = 0;
            }

            Adrift.SharedModule.UserSession.Process(cmd.Trim());
            // just to be extra sure, redraw the map each time a command
            // is done processing.
            map.Invalidate();
        }

        internal string QueryLoadPath()
        {
            var ofd = new OpenFileDialog { MultiSelect = false };
            ofd.Filters.Add(new FileFilter { Name = "ADRIFT Game File", Extensions = [".taf", ".blorb"] });
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
                EnableButtons();
                _timer.Start();
                input.Focus();
            }
        }

        private void RestartGameCommandOnExecuted(object sender, EventArgs e)
        {
            var result = MessageBox.Show(this,
                "Are you sure you want to restart the game? Any unsaved progress will be lost!",
                "Restart Game? - FrankenDrift", MessageBoxButtons.YesNo, MessageBoxType.Warning);
            if (result == DialogResult.Yes)
            {
                output.Clear(true);
                // Call relevant function directly rather than submitting a "restart" command to
                // ensure that the restart occurs even if the game would normally block it
                Adrift.SharedModule.UserSession.OpenAdventure(Adrift.SharedModule.Adventure.FullPath);
            }
        }

        private void TranscriptCommandOnExecuted(object? sender, EventArgs e)
        {
            if (_transcript is not null)
            {
                Menu.Items.GetSubmenu("&Game").Items[3].Text = "Start Transcript";
                _transcript.Dispose();
                _transcript = null;
                OutputHTML("<i>Transcript stopped.</i><br><br>");
            }
            else
            {
                var sfd = new SaveFileDialog();
                sfd.Filters.Add(new FileFilter { Name = "Text file", Extensions = [".txt"] });
                var result = sfd.ShowDialog(this);
                if (result != DialogResult.Ok) return;
                Menu.Items.GetSubmenu("&Game").Items[3].Text = "Stop Transcript";
                _transcript = new(sfd.FileName, System.IO.FileMode.Create, FileAccess.Write, FileShare.Read);
                OutputHTML("<i>Transcript starting.</i><br><br>");
            }
        }

        private void ReplayCommandOnExecuted(object? sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filters.Add(new FileFilter { Name = "Command files", Extensions = [".txt", ".cmd"] });
            var result = ofd.ShowDialog(this);
            if (result != DialogResult.Ok) return;
            var lines = File.ReadLines(ofd.FileName);
            _isReplaying = true;
            _shouldReplayCancel = false;
            OutputHTML("<i>Replaying commands.</i><br><br>");
            foreach (var line in lines)
            {
                if (output.IsWaiting) output.FinishWaiting();
                if (line.StartsWith("<KEY>")) continue;
                SubmitCommand(line);
                DoEvents();
                if (_shouldReplayCancel) break;
            }
            _shouldReplayCancel = false;
            _isReplaying = false;
        }

        internal AdriftOutput GetSecondaryWindow(string name)
        {
            if (_secondaryWindows.TryGetValue(name, out SecondaryWindow value)) return value.Output;
            var win = new SecondaryWindow(this)
            {
                ShowActivated = false,
                Title = name + " - " + this.Title
            };
            win.Output._defaultBackground = output._defaultBackground;
            win.Output._defaultColor = output._defaultColor;
            win.Output._defaultInput = output._defaultInput;
            win.Output._defaultFont = output._defaultFont;
            win.Output.Clear(true);
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
            saveGameCommand.Enabled = true;
            restoreGameCommand.Enabled = true;
            restartGameCommand.Enabled = true;
            transcriptCommand.Enabled = true;
            replayCommand.Enabled = true;
        }

        public void SetGameName(string name)
        {
            Title = $"{name} - FrankenDrift";
        }

        public bool IsTranscriptActive() => _transcript is not null;

        public void WriteTranscript(string text)
        {
            _transcript?.Write(Encoding.UTF8.GetBytes(text));
        }

        public void ScrollToEnd()
        {
            output.ScrollToEnd();
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

        public string QuerySavePath()
        {
            var sfd = new SaveFileDialog();
            sfd.Filters.Add(new FileFilter { Name = "ADRIFT Save File", Extensions = [".tas"] });
            var result = sfd.ShowDialog(this);
            return result == DialogResult.Ok ? sfd.FileName : "";
        }

        public string QueryRestorePath()
        {
            var ofd = new OpenFileDialog { MultiSelect = false };
            ofd.Filters.Add(new FileFilter { Name = "ADRIFT Save File", Extensions = [".tas"] });
            var result = ofd.ShowDialog(this);
            if (result != DialogResult.Ok) return "";
            // Fast-forward output only if the player did indeed commit to restoring a saved game.
            output.FastForwardText();
            return ofd.FileName;
        }

        public QueryResult QuerySaveBeforeQuit()
        {
            var result = MessageBox.Show("Would you like to save before quitting?", "Quit? -- FrankenDrift",
                MessageBoxButtons.YesNoCancel, MessageBoxType.Question);
            return result switch
            {
                DialogResult.Yes => QueryResult.YES,
                DialogResult.No => QueryResult.NO,
                _ => QueryResult.CANCEL,
            };
        }

        public void OutputHTML(string source) {
            if (_transcript is not null)
                WriteTranscript(Util.StripCarats(source).Replace("�", ">"));
            output.AppendHtml(source);
        }

        public void InitInput() => input.Focus();

        public void ShowCoverArt(byte[] img)
        {
            if (SettingsManager.Settings.EnableGraphics)
            {
                var theImage = new Bitmap(img);
                Graphics.DisplayImage(theImage);
            }
        }

        public void DoEvents() => Application.Instance.RunIteration();

        public string GetAppDataPath() => throw new NotImplementedException();

        public string GetExecutableLocation() => Path.GetDirectoryName(GetExecutablePath());

        public string GetExecutablePath() => Environment.ProcessPath;

        public string GetClaimedAdriftVersion() => "5.0000364";

        public void ReloadMacros() => throw new NotImplementedException();

        public void SaveLayout()
        {
            // pass for now
        }

        // This sets way more than just the background color, but it's a convenient hook called before the game outputs
        // any text for the first time.
        public void SetBackgroundColour()
        {
            var adventure = Adrift.SharedModule.Adventure;
            if (SettingsManager.Settings.EnableDevColors)
            {
                if (adventure.DeveloperDefaultBackgroundColour != 0)
                    output._defaultBackground = Color.FromArgb(adventure.DeveloperDefaultBackgroundColour);
                if (adventure.DeveloperDefaultOutputColour != 0 && adventure.DeveloperDefaultOutputColour != adventure.DeveloperDefaultBackgroundColour)
                    output._defaultColor = Color.FromArgb(adventure.DeveloperDefaultOutputColour);
                if (adventure.DeveloperDefaultInputColour != 0 && adventure.DeveloperDefaultInputColour != adventure.DeveloperDefaultBackgroundColour)
                    output._defaultInput = Color.FromArgb(adventure.DeveloperDefaultInputColour);
            }
            if (SettingsManager.Settings.EnableDevFont && !string.IsNullOrWhiteSpace(adventure.DefaultFontName)
)
            {
                if ((!SettingsManager.Settings.BanComicSans || !adventure.DefaultFontName.StartsWith("Comic Sans")))
                {
                    output._defaultFont = output.SelectionFont.WithFontFace(adventure.DefaultFontName);
                }
                output._defaultFont = output._defaultFont.WithSize(output.CalculateTextSize(adventure.DefaultFontSize));
            }
            // A new game is about to start, so clear the output window "with force", that is, discarding
            // any pending text that would have been printed after a key press.
            output.Clear(true);
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

        internal void DisableMap()
        {
            showMapCommand.Enabled = false;
        }
    }
}
