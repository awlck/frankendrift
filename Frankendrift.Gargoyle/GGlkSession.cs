using FrankenDrift.Gargoyle.Glk;
using FrankenDrift.Glue;
using FrankenDrift.Glue.Infragistics.Win.UltraWinToolbars;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;

namespace FrankenDrift.Gargoyle
{
    internal class MainSession : Glue.UIGlue, frmRunner
    {
        internal static MainSession? Instance = null;
        private GlkHtmlWin _output;
        private bool disposedValue;

        public UltraToolbarsManager UTMMain => throw new NotImplementedException();
        public RichTextBox txtOutput => _output;
        public RichTextBox txtInput => _output;  // huh?
        public bool Locked => false;
        public void Close() => Garglk_Pinvoke.glk_exit();

        internal MainSession(string gameFile)
        {
            if (Instance is not null)
                throw new ApplicationException("Dual MainSessions?");
            Instance = this;

            // If playing a blorb file, open it with the Glk library as well
            if (gameFile.EndsWith(".blorb"))
            {
                var blorb = File.ReadAllBytes(gameFile);
                // Blorb files produced by ADRIFT have the wrong file length in the header
                // for some reason, so passing it to Glk will not work unless we do this terribleness:
                var length = blorb.Length - 8;
                var lengthBytes = BitConverter.GetBytes(length);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(lengthBytes);
                for (int i = 0; i < 4; i++)
                    blorb[i + 4] = lengthBytes[i];
                var tmpFileRef = Garglk_Pinvoke.glk_fileref_create_temp(FileUsage.Data | FileUsage.BinaryMode, 0);
                var tmpFileStream = Garglk_Pinvoke.glk_stream_open_file(tmpFileRef, Glk.FileMode.ReadWrite, 0);
                Garglk_Pinvoke.glk_put_buffer_stream(tmpFileStream, blorb, (uint)blorb.Length);
                Garglk_Pinvoke.glk_fileref_destroy(tmpFileRef);
                Garglk_Pinvoke.glk_stream_set_position(tmpFileStream, 0, SeekMode.Start);
                Garglk_Pinvoke.giblorb_set_resource_map(tmpFileStream);
            }

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
            var fileref = Garglk_Pinvoke.glk_fileref_create_by_prompt(FileUsage.SavedGame | FileUsage.BinaryMode, Glk.FileMode.Read, 0);
            if (fileref == IntPtr.Zero) return "";
            var result = Garglk_Pinvoke.garglk_fileref_get_name(fileref);
            Garglk_Pinvoke.glk_fileref_destroy(fileref);
            return result;
        }

        public QueryResult QuerySaveBeforeQuit()
        {
            throw new NotImplementedException();
        }

        public string QuerySavePath()
        {
            var fileref = Garglk_Pinvoke.glk_fileref_create_by_prompt(FileUsage.SavedGame | FileUsage.BinaryMode, Glk.FileMode.Write, 0);
            if (fileref == IntPtr.Zero) return "";
            var result = Garglk_Pinvoke.garglk_fileref_get_name(fileref);
            Garglk_Pinvoke.glk_fileref_destroy(fileref);
            return result;
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
            Garglk_Pinvoke.garglk_set_story_name(name);
        }

        public void ShowCoverArt(byte[] img)
        {
            // we don't need the image data that the interpreter has provided us,
            // we just need to ask Glk to display image no. 1
            _output.DrawImageImmediately(1);
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
    // see: https://en.wikipedia.org/wiki/Flanimals#:~:text=Glonk%3A%20A%20green%20reptilian%20humanoid,known%20that%20it%20eats%20pizza.
    class GlonkMap : Glue.Map
    {
        public void RecalculateNode(object node) { }

        public void SelectNode(string key) { }
    }
}