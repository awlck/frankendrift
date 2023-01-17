using FrankenDrift.Adrift;
using FrankenDrift.Gargoyle.Glk;
using FrankenDrift.Glue;
using FrankenDrift.Glue.Infragistics.Win.UltraWinToolbars;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Text;

namespace FrankenDrift.Gargoyle
{
    internal class MainSession : Glue.UIGlue, frmRunner
    {
        internal static MainSession? Instance = null;

        private GlkHtmlWin _output;

        public UltraToolbarsManager UTMMain => throw new NotImplementedException();

        public RichTextBox txtOutput => _output;

        public RichTextBox txtInput => _output;  // huh?

        public bool Locked => false;

        public void Close() => Glk.Garglk_Pinvoke.glk_exit();

        internal MainSession(string gameFile)
        {
            if (Instance is not null)
                throw new ApplicationException("Dual MainSessions?");
            Instance = this;
            //_output = new GlkHtmlWin();
            Adrift.SharedModule.Glue = this;
            Adrift.SharedModule.fRunner = this;
            Glue.Application.SetFrontend(this);
            Adrift.SharedModule.UserSession = new Adrift.RunnerSession { Map = new GlonkMap(), bShowShortLocations = true };
            Adrift.SharedModule.UserSession.OpenAdventure(gameFile);
        }

        internal void Run()
        {
            while (true)
            {
                var cmd = _output.GetLineInput();
                SubmitCommand(cmd);
            }
        }

        internal void ProcessEvent(Event ev)
        {
            switch (ev.type)
            {
                case EventType.LineInput:
                    SubmitCommand();
                    break;
                default:
                    break;
            }
        }

        public bool AskYesNoQuestion(string question, string title = null)
        {
            throw new NotImplementedException();
        }

        public void ComplainAboutVersionMismatch(string advVer, string terpVer)
        {
            throw new NotImplementedException();
        }

        public void DoEvents()
        {
            Garglk_Pinvoke.glk_tick();
        }

        public void EnableButtons() { }

        public void ErrMsg(string message, Exception ex = null)
        {
            _output.AppendHTML($"<b>ADRIFT Fatal Error: {message}</b><br>");
        }

        public string GetAppDataPath()
        {
            throw new NotImplementedException();
        }

        public string GetClaimedAdriftVersion()
        {
            return "5.0000364";
        }

        public string GetExecutableLocation()
        {
            return Path.GetDirectoryName(GetExecutablePath());
        }

        public string GetExecutablePath()
        {
            return Environment.ProcessPath;
        }

        public void InitInput()
        { }

        public bool IsTranscriptActive() => false;

        public void MakeNote(string msg)
        {
            if (_output is not null)
                _output.AppendHTML($"ADRIFT Note: {msg}");
        }

        public void OutputHTML(string source) => _output.AppendHTML(source);

        public string QueryRestorePath()
        {
            throw new NotImplementedException();
        }

        public QueryResult QuerySaveBeforeQuit()
        {
            throw new NotImplementedException();
        }

        public string QuerySavePath()
        {
            throw new NotImplementedException();
        }

        public void ReloadMacros() { }

        public void SaveLayout() { }

        public void ScrollToEnd() { }

        public void SetBackgroundColour()
        {
            var adventure = Adrift.SharedModule.Adventure;
            if (!adventure.DeveloperDefaultBackgroundColour.IsEmpty)
            {
                int colorToBe = adventure.DeveloperDefaultBackgroundColour.ToArgb() & 0x00FFFFFF;
                foreach (Style s in (Style[]) Enum.GetValues(typeof(Style)))
                    Garglk_Pinvoke.glk_stylehint_set(WinType.AllTypes, s, StyleHint.BackColor, colorToBe);
            }
            if (!adventure.DeveloperDefaultOutputColour.IsEmpty && adventure.DeveloperDefaultOutputColour != adventure.DeveloperDefaultBackgroundColour)
            {
                int colorToBe = adventure.DeveloperDefaultOutputColour.ToArgb() & 0x00FFFFFF;
                foreach (Style s in (Style[]) Enum.GetValues(typeof(Style)))
                {
                    if (s == Style.Input) continue;
                    Garglk_Pinvoke.glk_stylehint_set(WinType.AllTypes, s, StyleHint.TextColor, colorToBe);
                }
            }
            if (!adventure.DeveloperDefaultInputColour.IsEmpty && adventure.DeveloperDefaultInputColour != adventure.DeveloperDefaultBackgroundColour)
            {
                int colorToBe = adventure.DeveloperDefaultInputColour.ToArgb() & 0x00FFFFFF;
                Garglk_Pinvoke.glk_stylehint_set(WinType.AllTypes, Style.Input, StyleHint.TextColor, colorToBe);
            }

            // Repurpose blockquote style for centered text (seems the most appropriate out of the bunch)
            Garglk_Pinvoke.glk_stylehint_set(WinType.AllTypes, Style.BlockQuote, StyleHint.Justification, (int)Justification.Centered);

            // It is perhaps bad form / unexpected to do this here, but we can't
            // open any windows until after the style hints have been adjusted.
            _output = new GlkHtmlWin();
        }

        public void SetGameName(string name)
        {
            Glk.Garglk_Pinvoke.garglk_set_story_name(name);
        }

        public void ShowCoverArt(byte[] img)
        {
            throw new NotImplementedException();
        }

        public void ShowInfo(string info, string title = null)
        {
            throw new NotImplementedException();
        }

        public void SubmitCommand()
        {
            // not sure what should go here
        }

        public void SubmitCommand(string cmd)
        {
            if (cmd == "!dumpstyles")
            {
                _output.DumpCurrentStyleInfo();
                return;
            }
            Adrift.SharedModule.UserSession.Process(cmd);
        }

        public void UpdateStatusBar(string desc, string score, string user)
        {
            // TODO
        }
    }

    // Glonk: does absolutely nothing and dies.
    class GlonkMap : Glue.Map
    {
        public void RecalculateNode(object node) { }

        public void SelectNode(string key) { }
    }
}