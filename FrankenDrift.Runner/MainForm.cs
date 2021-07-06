using Adravalon.Glue;
using Adravalon.Glue.Infragistics.Win.UltraWinToolbars;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using Application = Eto.Forms.Application;

namespace Adravalon.Runner
{
    public partial class MainForm : Form, Glue.UIGlue, frmRunner
    {
        public UltraToolbarsManager UTMMain => throw new NotImplementedException();

        public RichTextBox txtOutput => output;

        public RichTextBox txtInput => (Glue.RichTextBox) input;

        public bool Locked => false;

        private AdriftMap map;
        private Dictionary<string, SecondaryWindow> _secondaryWindows = new();
        private GraphicsWindow _graphics = null;
        
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

            loadGameCommand.Executed += LoadGameCommand_Executed;
            saveGameCommand.Executed += SaveGameCommand_Executed;
            restoreGameCommand.Executed += RestoreGameCommand_Executed;

            input.KeyDown += Input_KeyDown;

            Adrift.SharedModule.Glue = this;
            Adrift.SharedModule.fRunner = this;
            Adrift.SharedModule.UserSession = new Adrift.RunnerSession {Map = map};
            Glue.Application.SetFrontend(this);
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (output.IsWaiting)
            {
                e.Handled = true;
                output.FinishWaiting();
            }
            switch (e.Key)
            {
                case Keys.Enter:
                    if (Adrift.SharedModule.Adventure is not null)
                    {
                        OutputHTML("<br><br>");
                        Adrift.SharedModule.UserSession.Process(input.Text);
                    }
                    #if DEBUG
                    else if (input.Text.StartsWith("<>")) OutputHTML(input.Text[2..]);
                    #endif
                    else OutputHTML("(Click File > Open Game to load a game!)");
                    input.Text = "";
                    e.Handled = true;
                    break;
            }
        }

        internal string QueryLoadPath()
        {
            var ofd = new OpenFileDialog { MultiSelect = false };
            ofd.Filters.Add(new FileFilter { Name = "ADRIFT Game File", Extensions = new[] { ".taf", ".blorb" } });
            var result = ofd.ShowDialog(this);
            return result == DialogResult.Ok ? ofd.FileName : "";
        }

        private void RestoreGameCommand_Executed(object sender, EventArgs e)
        {
            if (Adrift.SharedModule.Adventure is not null)
            {
                Adrift.SharedModule.UserSession.Process("restore");
            }
        }

        private void SaveGameCommand_Executed(object sender, EventArgs e)
        {
            if (Adrift.SharedModule.Adventure is not null)
            {
                Adrift.SharedModule.UserSession.Process("save");
            }
        }

        private void LoadGameCommand_Executed(object sender, EventArgs e)
        {
            var toLoad = QueryLoadPath();
            if (!string.IsNullOrWhiteSpace(toLoad))
                Adrift.SharedModule.UserSession.OpenAdventure(toLoad);
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

        public bool IsTranscriptActive()
        {
            return false;
        }

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
            output.BackgroundColor = Eto.Platform.Detect.IsMac ? Colors.Black : Colors.LightGrey;
        }

        public void UpdateStatusBar(string desc, string score, string user)
        {
            status.Text = desc;
            if (!string.IsNullOrWhiteSpace(score)) status.Text += $" -- {score}";
            if (!string.IsNullOrWhiteSpace(user)) status.Text += $" -- {Adrift.SharedModule.ReplaceALRs(user)}";
        }

        public void SubmitCommand()
        {
            throw new NotImplementedException();
        }

        internal void ReportSecondaryClosing(SecondaryWindow secondaryWindow)
        {
            if (!_secondaryWindows.ContainsValue(secondaryWindow)) return;
            foreach(var item in _secondaryWindows.Where(kvp => kvp.Value == secondaryWindow).ToList())
                _secondaryWindows.Remove(item.Key);
        }
    }
}
