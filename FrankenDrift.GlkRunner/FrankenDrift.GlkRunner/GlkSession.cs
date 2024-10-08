﻿using FrankenDrift.GlkRunner.Glk;
using FrankenDrift.Glue;
using FrankenDrift.Glue.Infragistics.Win.UltraWinToolbars;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace FrankenDrift.GlkRunner
{
    public class MainSession : Glue.UIGlue, frmRunner
    {
        internal static MainSession? Instance = null;
        private readonly IGlk GlkApi;
        private GlkHtmlWin? _output;
        private GlkGridWin? _status;
        private readonly bool _soundSupported;

        public UltraToolbarsManager UTMMain => throw new NotImplementedException();
        public RichTextBox txtOutput => _output;
        public RichTextBox txtInput => _output;  // huh?
        public bool Locked => false;
        public void Close() => GlkApi.glk_exit();

        private readonly Dictionary<int, SoundChannel> _sndChannels = new();
        private readonly Dictionary<int, string> _recentlyPlayedSounds = new();
        private bool _showCoverArt = false;

        public MainSession(string gameFile, IGlk glk)
        {
            if (Instance is not null)
                throw new ApplicationException("Dual MainSessions?");
            Instance = this;
            GlkApi = glk;

            var util = new GlkUtil(GlkApi);
            if (!util._unicodeAvailable)
            {
                _output = new(glk);
                _output.AppendHTML("Sorry, can't run with a non-unicode Glk library.\n<waitkey>\n");
                Environment.Exit(2);
            }

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
                var tmpFileRef = GlkApi.glk_fileref_create_temp(FileUsage.Data | FileUsage.BinaryMode, 0);
                var tmpFileStream = GlkApi.glk_stream_open_file(tmpFileRef, Glk.FileMode.ReadWrite, 0);
                GlkApi.glk_put_buffer_stream(tmpFileStream, blorb, (uint)blorb.Length);
                GlkApi.glk_fileref_destroy(tmpFileRef);
                GlkApi.glk_stream_set_position(tmpFileStream, 0, SeekMode.Start);
                GlkApi.giblorb_set_resource_map(tmpFileStream);
            }

            Adrift.SharedModule.Glue = this;
            Adrift.SharedModule.fRunner = this;
            Glue.Application.SetFrontend(this);
            Adrift.SharedModule.UserSession = new Adrift.RunnerSession { Map = new GlonkMap(), bShowShortLocations = true };
            _soundSupported = GlkApi.glk_gestalt(Gestalt.Sound2, 0) != 0;
            if (_soundSupported)
                for (int i = 1; i <= 8; i++)
                    _sndChannels[i] = GlkApi.glk_schannel_create((uint)i);
            Adrift.SharedModule.UserSession.OpenAdventure(gameFile);
            // The underlying Runner wants a tick once per second to trigger real-time-based events
            if (GlkApi.glk_gestalt(Gestalt.Timer, 0) != 0)
                GlkApi.glk_request_timer_events(1000);
        }

        public void Run()
        {
            // Adrift Authors have a habit of immediately clearing the screen on startup, so sneak in our welcome message before the first prompt.
            var myVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var backendVersion = Assembly.GetAssembly(typeof(Adrift.SharedModule))?.GetName()?.Version;
            _output.AppendHTML($"\n<i>(Glk FrankenDrift v{myVersion}({GetClaimedAdriftVersion()}/{backendVersion}) -- type !metahelp for interpreter commands.)</i>\n");
            while (true)
            {
                _output.AppendHTML("<c>&gt; </c>");
                var cmd = _output.GetLineInput();
                SubmitCommand(cmd.Trim());
            }
        }

        internal void ProcessEvent(Event ev)
        {
            switch (ev.type)
            {
                case EventType.LineInput:
                    SubmitCommand();
                    break;
                case EventType.Timer:
                    // For what little good it does us -- the output window will be tied up waiting for input,
                    // so any text that gets output won't be seen until the user has sent off the command they
                    // are currently editing. But this will still cause anything other than text output to happen.
                    Adrift.SharedModule.UserSession.TimeBasedStuff();
                    break;
                default:
                    break;
            }
        }

        public bool AskYesNoQuestion(string question, string title = null)
        {
            if (_output is null)
                _output = new(GlkApi);
            if (title is not null)
                _output.AppendHTML($"\n<font color=\"red\"><b>{title}</b></font>");
            _output.AppendHTML($"\n<font color=\"red\">{question}</font>");
            while (true)
            {
                _output.AppendHTML("\n[yes/no] > ");
                var result = _output.GetLineInput().ToLower();
                switch (result)
                {
                    case "y":
                    case "yes":
                        return true;
                    case "n":
                    case "no":
                        return false;
                }
            }
        }

        public void DoEvents()
        {
            GlkApi.glk_tick();
        }

        public void EnableButtons() { }

        public void ErrMsg(string message, Exception ex = null)
        {
            if (_output is null)
                _output = new(GlkApi);
            _output.AppendHTML($"<b>ADRIFT Fatal Error: {message}</b><br>");
            if (ex is not null)
                _output.AppendHTML(ex.ToString());
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

        public void MakeNote(string msg)
        {
            _output?.AppendHTML($"ADRIFT Note: {msg}");
        }

        public void OutputHTML(string source) => _output.AppendHTML(source);

        public string QueryRestorePath()
        {
            var fileref = GlkApi.glk_fileref_create_by_prompt(FileUsage.SavedGame | FileUsage.BinaryMode, Glk.FileMode.Read, 0);
            if (!fileref.IsValid) return "";
            var result = GlkApi.glkunix_fileref_get_name(fileref);
            GlkApi.glk_fileref_destroy(fileref);
            return result ?? "";
        }

        public QueryResult QuerySaveBeforeQuit()
        {
            if (_output is null)
                return QueryResult.NO;  // if there is no output window, no game was loaded, so there is nothing to save.
            _output.AppendHTML("\n<b><font color=\"red\">Would you like to save before quitting?</font></b>");
            while (true)
            {
                _output.AppendHTML("\n[yes/no/cancel] > ");
                var result = _output.GetLineInput().ToLower();
                switch (result)
                {
                    case "y":
                    case "yes":
                        return QueryResult.YES;
                    case "n":
                    case "no":
                        return QueryResult.NO;
                    case "c":
                    case "cancel":
                        return QueryResult.CANCEL;
                }
            }
        }

        public string QuerySavePath()
        {
            var fileref = GlkApi.glk_fileref_create_by_prompt(FileUsage.SavedGame | FileUsage.BinaryMode, Glk.FileMode.Write, 0);
            if (!fileref.IsValid) return "";
            var result = GlkApi.glkunix_fileref_get_name(fileref);
            GlkApi.glk_fileref_destroy(fileref);
            return result ?? "";
        }

        public void ReloadMacros() { }

        public void SaveLayout() { }

        public void ScrollToEnd() { }

        public void SetBackgroundColour()
        {
            var adventure = Adrift.SharedModule.Adventure;
            if (adventure.DeveloperDefaultBackgroundColour != 0)
            {
                int colorToBe = adventure.DeveloperDefaultBackgroundColour & 0x00FFFFFF;
                foreach (Style s in (Style[]) Enum.GetValues(typeof(Style)))
                    GlkApi.glk_stylehint_set(WinType.AllTypes, s, StyleHint.BackColor, colorToBe);
            }
            if (adventure.DeveloperDefaultOutputColour != 0 && adventure.DeveloperDefaultOutputColour != adventure.DeveloperDefaultBackgroundColour)
            {
                int colorToBe = adventure.DeveloperDefaultOutputColour & 0x00FFFFFF;
                foreach (Style s in (Style[]) Enum.GetValues(typeof(Style)))
                {
                    if (s == Style.Input) continue;
                    GlkApi.glk_stylehint_set(WinType.AllTypes, s, StyleHint.TextColor, colorToBe);
                }
            }
            if (adventure.DeveloperDefaultInputColour != 0 && adventure.DeveloperDefaultInputColour != adventure.DeveloperDefaultBackgroundColour)
            {
                int colorToBe = adventure.DeveloperDefaultInputColour & 0x00FFFFFF;
                GlkApi.glk_stylehint_set(WinType.AllTypes, Style.Input, StyleHint.TextColor, colorToBe);
            }

            // Repurpose blockquote style for centered text (seems the most appropriate out of the bunch)
            GlkApi.glk_stylehint_set(WinType.AllTypes, Style.BlockQuote, StyleHint.Justification, (int)Justification.Centered);

            // It is perhaps bad form / unexpected to do this here, but we can't
            // open any windows until after the style hints have been adjusted.
            // If a window was already opened to ask questions during the loading process, close it.
            if (_output is { IsDisposed: false })
            {
                _output.Dispose();
            }
            _output = new GlkHtmlWin(GlkApi);
            _status = _output.CreateStatusBar();
            // show the cover art if available.
            if (_showCoverArt)
                _output.DrawImageImmediately(1);
        }

        public void SetGameName(string name)
        {
            GlkApi.SetGameName(name);
        }

        public void ShowCoverArt(byte[] img)
        {
            // we don't need the image data that the interpreter has provided us,
            // we just need to ask Glk to display image no. 1
            // (but since the request to display cover art comes early in the blorb loading stage,
            //  before color information is made available, we can't actually open a window
            //  to display anything, so we defer this.)
            _showCoverArt = true;
        }

        public void ShowInfo(string info, string title = null)
        {
            MakeNote(info);
        }

        public void SubmitCommand()
        {
            // not sure what should go here
        }

        internal void SubmitCommand(string cmd)
        {
            cmd = cmd.Trim(' ');
            if (cmd.StartsWith('!'))
            {
                var metacmd = cmd.ToLower();
                switch (metacmd)
                {
                    case "!dumpstyles":
                        _output!.DumpCurrentStyleInfo();
                        return;
                    case "!transcript":
                    case "!script":
                    case "!transcripton":
                    case "!scripton":
                        TranscriptOn();
                        return;
                    case "!transcriptoff":
                    case "!scriptoff":
                        TranscriptOff();
                        return;
                    case "!help":
                    case "!metahelp":
                        ShowMetaHelp();
                        return;
                    case "!forcequit":
                        GlkApi.glk_exit();
                        return;
                }
            }
            Adrift.SharedModule.UserSession.Process(cmd);
            Adrift.SharedModule.Adventure.Turns += 1;
        }

        public void UpdateStatusBar(string desc, string score, string user)
        {
            if (_status is null) return;
            if (string.IsNullOrEmpty(user))
            {
                desc = Adrift.SharedModule.ReplaceALRs(desc);
                score = Adrift.SharedModule.ReplaceALRs(score);
                var winWidth = _status.Width;
                var spaces = winWidth - desc.Length - score.Length - 1;
                if (spaces < 2) spaces = 2;
                _status.RewriteStatus(desc + new string(' ', spaces) + score);
            }
            else
            {
                desc = Adrift.SharedModule.ReplaceALRs(desc);
                score = Adrift.SharedModule.ReplaceALRs(score);
                user = Adrift.SharedModule.ReplaceALRs(user);
                var winWidth = _status.Width;
                var spaces = (winWidth - desc.Length - score.Length - user.Length - 1) / 2;
                if (spaces < 2) spaces = 2;
                _status.RewriteStatus(desc + new string(' ', spaces) + score + new string(' ', spaces) + user);
            }
        }

        internal void PlaySound(string snd, int channel, bool loop)
        {
            if (!_soundSupported) return;
            if (_recentlyPlayedSounds.TryGetValue(channel, out string? value) && value == snd)
            {
                UnpauseSound(channel);
                return;
            }
            if (!(Adrift.SharedModule.Adventure.BlorbMappings is { Count: > 0 })
                    || !Adrift.SharedModule.Adventure.BlorbMappings.TryGetValue(snd, out int theSound))
                return;
            _recentlyPlayedSounds[channel] = snd;
            var success = GlkApi.glk_schannel_play_ext(_sndChannels[channel], (uint)theSound, loop ? 0xFFFFFFFF : 1, 0);
        }

        private void UnpauseSound(int channel)
        {
            if (!_soundSupported) return;
            GlkApi.glk_schannel_unpause(_sndChannels[channel]);
        }

        internal void PauseSound(int channel)
        {
            if (!_soundSupported) return;
            GlkApi.glk_schannel_pause(_sndChannels[channel]);
        }

        internal void StopSound(int channel)
        {
            if (!_soundSupported) return;
            GlkApi.glk_schannel_stop(_sndChannels[channel]);
            if (_recentlyPlayedSounds.ContainsKey(channel))
                _recentlyPlayedSounds.Remove(channel);
        }

        internal void TranscriptOn()
        {
            if (_output is null) return;
            if (_output is { IsEchoing: true })
            {
                OutputHTML("<i>Transcript is already on -- use <font face=\"Courier\">!scriptoff</font> to disable it.</i>\n");
                return;
            }
            var fileref = GlkApi.glk_fileref_create_by_prompt(FileUsage.Transcript | FileUsage.TextMode, Glk.FileMode.Write, 0);
            if (!fileref.IsValid)
            {
                _output.AppendHTML("<i>Transcript activation canceled.</i>\n");
                return;
            }
            try
            {
                var stream = GlkApi.glk_stream_open_file(fileref, Glk.FileMode.Write, 0);
                if (stream.IsValid)
                    _output.EchoStream = stream;
                else
                    _output.AppendHTML("<i>Transcript activation failed, sorry.</i>\n");
            }
            finally
            {
                GlkApi.glk_fileref_destroy(fileref);
            }
        }

        internal void TranscriptOff()
        {
            if (_output is null) return;
            if (_output is { IsEchoing: false })
            {
                OutputHTML("<i>Transcript is not running -- use <font face=\"Courier\">!scripton</font> to start it.</i>\n");
                return;
            }
            StreamResult result = new();
            GlkApi.glk_stream_close(_output.EchoStream, ref result);
            _output.AppendHTML("<i>Transcript stopped.</i>\n");
        }

        internal void ShowMetaHelp()
        {
            if (_output is null)
                return;  // not like anyone could input a meta-command without the output window existing
            OutputHTML("<i>Meta-Commands understood by FrankenDrift:\n<font face=\"Courier\">!scripton</font> -- start a transcript\n<font face=\"Courier\">!scriptoff</font> -- stop a running transcript\n<font face=\"Courier\">!dumpstyles</font> -- show the Glk style settings\n<font face=\"Courier\">!forcequit</font> -- immediately terminate the current game session. Unsaved progress will be lost.</i>\n");
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