﻿using FrankenDrift.Gargoyle.Glk;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FrankenDrift.Gargoyle
{
    /// <summary>
    /// Exception indicating concurrent Glk input requests
    /// </summary>
    internal class ConcurrentEventException : Exception
    {
        public ConcurrentEventException(string msg) : base(msg) { }
    }

    // Making the best out of the limited set of Glk styles...
    enum TextStyle : uint
    {
        Normal    = 0b00000,
        Bold      = 0b00001,
        Italic    = 0b00010,
        Monospace = 0b00100,
        Centered  = 0b01000
    }
    struct FontInfo
    {
        internal TextStyle Ts;
        internal uint TextColor;
        internal string TagName;
    }

    internal class GlkHtmlWin : Glue.RichTextBox, IDisposable
    {
        internal static GlkHtmlWin? MainWin = null;
        internal static uint NumberOfWindows = 0;

        private IntPtr glkwin_handle;

        public int TextLength => -1;

        public string Text { get => ""; set { } }
        public string SelectedText { get => ""; set { } }
        public int SelectionStart { get => -1; set { } }
        public int SelectionLength { get => -1; set { } }
        public bool IsDisposed => glkwin_handle == IntPtr.Zero;
        internal bool IsWaiting = false;
        private string _pendingText = "";

        static readonly string[] Monospaces = {
            "Andale Mono", "Cascadia Code", "Century Schoolbook Monospace", "Consolas", "Courier", "Courier New",
            "Liberation Mono", "Ubuntu Mono", "DejaVu Sans Mono",
            "Droid Sans Mono", "Lucida Console", "Menlo", "OCR-A", "OCR-A extended", "Overpass Mono", "Oxygen Mono",
            "Roboto Mono", "Source Code Pro", "Everson Mono", "Fira Mono", "Fixed", "Fixedsys", "FreeMono", "Go Mono",
            "HyperFont", "IBM MDA", "IBM Plex Mono", "Inconsolata", "Iosevka", "Letter Gothic", "Monaco", "Monofur",
            "Monospace", "Monospace (Unicode)", "Nimbus Mono L", "Noto Mono", "NK57 Monospace", "OCR-B", "PragmataPro",
            "Prestige Elite", "ProFont", "PT Mono", "Spleen", "Terminus", "Tex Gyre Cursor", "American Typewriter",
            "TADS-monospace"
        };

        internal IntPtr Stream => Garglk_Pinvoke.glk_window_get_stream(glkwin_handle);

        internal GlkHtmlWin()
        {
            if (MainWin is null)
            {
                MainWin = this;
                NumberOfWindows += 1;
                glkwin_handle = Garglk_Pinvoke.glk_window_open(IntPtr.Zero, 0, 0, WinType.TextBuffer, NumberOfWindows);
            }
            else
            {
                glkwin_handle = _doWindowOpen(MainWin, WinMethod.Right | WinMethod.Proportional, 30);
            }
        }

        GlkHtmlWin(GlkHtmlWin splitFrom, WinMethod splitMethod, uint splitSize)
        {
            glkwin_handle = _doWindowOpen(splitFrom, splitMethod, splitSize);
        }

        private IntPtr _doWindowOpen(GlkHtmlWin splitFrom, WinMethod splitMethod, uint splitSize)
        {
            var result = Garglk_Pinvoke.glk_window_open(splitFrom.glkwin_handle, splitMethod, splitSize, WinType.TextBuffer, NumberOfWindows);
            if (result == IntPtr.Zero)
                throw new GlkError("Failed to open window.");
            return result;
        }

        public void Clear()
        {
            Garglk_Pinvoke.garglk_set_zcolors((uint)ZColor.Default, (uint)ZColor.Default);
            Garglk_Pinvoke.glk_window_clear(glkwin_handle);
        }

        internal unsafe string GetLineInput()
        {
            if (IsWaiting)
                throw new ConcurrentEventException("Too many input events requested");
            IsWaiting = true;
            const uint capacity = 256;
            byte[] cmdToBe = new byte[capacity];
            var count = 0;
            fixed (byte* buf = cmdToBe)
            {
                Garglk_Pinvoke.glk_request_line_event(glkwin_handle, buf, capacity-1, 0);
                while (true)
                {
                    Event ev = new() { type = EventType.None };
                    Garglk_Pinvoke.glk_select(ref ev);
                    if (ev.type == EventType.LineInput && ev.win_handle == glkwin_handle)
                    {
                        count = (int) ev.val1;
                        break;
                    }
                    else MainSession.Instance!.ProcessEvent(ev);
                }
            }
            IsWaiting = false;
            var dec = Encoding.GetEncoding(Encoding.Latin1.CodePage, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);
            return dec.GetString(cmdToBe, 0, count);
        }

        internal uint GetCharInput()
        {
            if (IsWaiting)
                throw new ConcurrentEventException("Too many input events requested");
            IsWaiting = true;
            uint result;
            Garglk_Pinvoke.glk_request_char_event(glkwin_handle);
            while (true)
            {
                Event ev = new() { type = EventType.None };
                Garglk_Pinvoke.glk_select(ref ev);
                if (ev.type == EventType.CharInput && ev.win_handle == glkwin_handle)
                {
                    result = ev.val1;
                    break;
                }
                else MainSession.Instance!.ProcessEvent(ev);
            }
            IsWaiting = false;
            return result;
        }

        // Janky-ass HTML parser, 2nd edition.
        public void AppendHTML(string src)
        {
            // don't echo back the command, the Glk library already takes care of that
            if (src.StartsWith("<c><font face=\"Wingdings\" size=14>") && src.EndsWith("</c>\r\n"))
                return;
            // strip redundant carriage-return characters
            src = src.Replace("\r\n", "\n");
            if (IsWaiting)
            {
                _pendingText += src;
                return;
            }
            Garglk_Pinvoke.glk_set_window(glkwin_handle);
            Garglk_Pinvoke.garglk_set_zcolors((uint)ZColor.Default, (uint)ZColor.Default);

            var consumed = 0;
            var inToken = false;
            var current = new StringBuilder();
            var currentToken = "";
            var previousToken = "";
            var skip = 0;
            var styleHistory = new Stack<FontInfo>();
            styleHistory.Push(new FontInfo { Ts = TextStyle.Normal, TextColor = (uint)ZColor.Default, TagName = "<base>" });

            foreach (var c in src)
            {
                consumed++;
                if (skip-- > 0) continue;

                if (c == '<' && !inToken)
                {
                    inToken = true;
                    previousToken = currentToken;
                    currentToken = "";
                }
                else if (c != '>' && inToken)
                {
                    currentToken += c;
                }
                else if (c == '>' && inToken)
                {
                    var currentTextStyle = styleHistory.Peek();
                    inToken = false;
                    if (currentToken == "del" && current.Length > 0)
                    {
                        // As long as we haven't committed to displaying anything,
                        // we can remove the last character.
                        current.Remove(current.Length - 1, 1);
                        continue;

                        // TODO: ask for a garglk extension akin to garglk_unput_string
                        // that only needs the number of characters to delete.
                    }
                    OutputStyled(current.ToString(), currentTextStyle);
                    current.Clear();
                    switch (currentToken)
                    {
                        case "br":
                            GarGlk.OutputString("\n");
                            break;
                        case "c":  // Keep the 'input' style reserved for actual input, only mimic it with color where possible.
                            styleHistory.Push(new FontInfo { Ts = currentTextStyle.Ts, TextColor = GetInputTextColor(), TagName = "c" });
                            break;
                        case "b":
                            styleHistory.Push(new FontInfo { Ts = currentTextStyle.Ts |= TextStyle.Bold, TextColor = currentTextStyle.TextColor, TagName = "b" });
                            break;
                        case "i":
                            styleHistory.Push(new FontInfo { Ts = currentTextStyle.Ts |= TextStyle.Italic, TextColor = currentTextStyle.TextColor, TagName = "i" });
                            break;
                        case "center":
                            styleHistory.Push(new FontInfo { Ts = currentTextStyle.Ts |= TextStyle.Centered, TextColor = currentTextStyle.TextColor, TagName = "center" });
                            break;
                        case "tt":
                            styleHistory.Push(new FontInfo { Ts = currentTextStyle.Ts |= TextStyle.Monospace, TextColor = currentTextStyle.TextColor, TagName = "tt" });
                            break;
                        case "/c" when currentTextStyle.TagName == "c":
                        case "/b" when currentTextStyle.TagName == "b":
                        case "/i" when currentTextStyle.TagName == "i":
                        case "/center" when currentTextStyle.TagName == "center":
                        case "/tt" when currentTextStyle.TagName == "tt":
                        case "/font" when currentTextStyle.TagName == "font":
                            styleHistory.Pop();
                            break;
                        case "cls":
                            styleHistory.Clear();
                            styleHistory.Push(new FontInfo { Ts = TextStyle.Normal, TextColor = (uint)ZColor.Default, TagName = "<base>" });
                            Clear();
                            break;
                        case "waitkey":
                            GetCharInput();
                            break;
                    }
                    if (currentToken.StartsWith("font"))
                    {
                        var color = currentTextStyle.TextColor;
                        var tokenLower = currentToken.ToLower();
                        var re = new Regex("color ?= ?\"?#?([0-9A-Fa-f]{6})\"?");
                        var col = re.Match(tokenLower);
                        if (col.Success)
                        {
                            if (uint.TryParse(col.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out var newColor))
                                color = newColor;
                        }
                        else if (tokenLower.Contains("black"))
                            color = 0x000000;
                        else if (tokenLower.Contains("blue"))
                            color = 0x0000ff;
                        else if (tokenLower.Contains("gray"))
                            color = 0x808080;
                        else if (tokenLower.Contains("darkgreen"))
                            color = 0x006400;
                        else if (tokenLower.Contains("green"))
                            color = 0x008000;
                        else if (tokenLower.Contains("lime"))
                            color = 0x00FF00;
                        else if (tokenLower.Contains("magenta"))
                            color = 0xFF00FF;
                        else if (tokenLower.Contains("maroon"))
                            color = 0x800000;
                        else if (tokenLower.Contains("navy"))
                            color = 0x000080;
                        else if (tokenLower.Contains("olive"))
                            color = 0x808000;
                        else if (tokenLower.Contains("orange"))
                            color = 0xffa500;
                        else if (tokenLower.Contains("pink"))
                            color = 0xffc0cb;
                        else if (tokenLower.Contains("purple"))
                            color = 0x800080;
                        else if (tokenLower.Contains("red"))
                            color = 0xff0000;
                        else if (tokenLower.Contains("silver"))
                            color = 0xc0c0c0;
                        else if (tokenLower.Contains("teal"))
                            color = 0x008080;
                        else if (tokenLower.Contains("white"))
                            color = 0xffffff;
                        else if (tokenLower.Contains("yellow"))
                            color = 0xffff00;
                        else if (tokenLower.Contains("cyan"))
                            color = 0x00ffff;
                        else if (tokenLower.Contains("darkolive"))
                            color = 0x556b2f;
                        else if (tokenLower.Contains("tan"))
                            color = 0xd2b48c;

                        var currstyle = currentTextStyle.Ts;
                        re = new Regex("face ?= ?\"(.*?)\"");
                        var face = re.Match(tokenLower);
                        if (face.Success)
                        {
                            var f = face.Groups[1].Value;
                            if (Monospaces.Any(msf => msf.ToLowerInvariant() == f))
                                currstyle |= TextStyle.Monospace;
                        }

                        styleHistory.Push(new FontInfo { Ts = currstyle, TextColor = color, TagName = "font" });
                    }
                    else if (currentToken.StartsWith("img"))
                    {
                        var imgPath = new Regex("src ?= ?\"(.+)\"").Match(currentToken);
                        if (imgPath.Success && Adrift.SharedModule.Adventure.BlorbMappings is { Count: > 0 }
                                && Adrift.SharedModule.Adventure.BlorbMappings.ContainsKey(imgPath.Groups[1].Value))
                        {
                            var res = Adrift.SharedModule.Adventure.BlorbMappings[imgPath.Groups[1].Value];
                            DrawImageImmediately((uint)res);
                        }
                    }
                }
                else current.Append(c);
            }

            OutputStyled(current.ToString(), styleHistory.Peek());

            if (!IsWaiting && !string.IsNullOrEmpty(_pendingText))
            {
                var tmpText = _pendingText;
                _pendingText = "";
                AppendHTML(tmpText);
            }
        }

        internal bool DrawImageImmediately(uint imgId)
        {
            var result = Garglk_Pinvoke.glk_image_draw(glkwin_handle, imgId, (int)ImageAlign.MarginLeft, 0);
            if (result != 0)
            {
                Garglk_Pinvoke.glk_window_flow_break(glkwin_handle);
                return true;
            }
            return false;
        }

        private void OutputStyled(string txt, FontInfo fi)
        {
            Garglk_Pinvoke.garglk_set_zcolors(fi.TextColor, (uint)ZColor.Default);
            if ((fi.Ts & TextStyle.Monospace) != 0)
            {
                Garglk_Pinvoke.glk_set_style(Style.Preformatted);
            }
            else if ((fi.Ts & TextStyle.Centered) != 0)
            {
                Garglk_Pinvoke.glk_set_style(Style.BlockQuote);
            }
            else if ((fi.Ts & (TextStyle.Italic | TextStyle.Bold)) != 0)
            {
                Garglk_Pinvoke.glk_set_style(Style.Alert);
            }
            else if ((fi.Ts & TextStyle.Italic) != 0)
            {
                Garglk_Pinvoke.glk_set_style(Style.Emphasized);
            }
            else if ((fi.Ts & TextStyle.Bold) != 0)
            {
                Garglk_Pinvoke.glk_set_style(Style.Subheader);
            }
            else
            {
                Garglk_Pinvoke.glk_set_style(Style.Normal);
            }
            GarGlk.OutputString(txt);
        }

        private uint GetInputTextColor()
        {
            uint result = 0;
            var success = Garglk_Pinvoke.glk_style_measure(glkwin_handle, Style.Input, StyleHint.TextColor, ref result);
            if (success == 0)
                return (uint)ZColor.Default;
            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (glkwin_handle != IntPtr.Zero)
            {
                if (disposing)
                {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                Garglk_Pinvoke.glk_window_close(glkwin_handle, IntPtr.Zero);
                glkwin_handle = IntPtr.Zero;
            }
        }

        ~GlkHtmlWin()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        internal void DumpCurrentStyleInfo()
        {
            foreach (Style s in (Style[])Enum.GetValues(typeof(Style)))
            {
                uint result = 0;
                AppendHTML($"Style status for style: {s}\n");
                AppendHTML("  Indentation: ");
                var success = Garglk_Pinvoke.glk_style_measure(glkwin_handle, s, StyleHint.Indentation, ref result);
                if (success == 1) AppendHTML($"{result}\n");
                else AppendHTML("n/a\n");
                AppendHTML("  Paragraph Indentation: ");
                success = Garglk_Pinvoke.glk_style_measure(glkwin_handle, s, StyleHint.ParaIndentation, ref result);
                if (success == 1) AppendHTML($"{result}\n");
                else AppendHTML("n/a\n");
                AppendHTML("  Justification: ");
                success = Garglk_Pinvoke.glk_style_measure(glkwin_handle, s, StyleHint.Justification, ref result);
                if (success == 1) AppendHTML($"{(Justification)result}\n");
                else AppendHTML("n/a\n");
                AppendHTML("  Font Size: ");
                success = Garglk_Pinvoke.glk_style_measure(glkwin_handle, s, StyleHint.Size, ref result);
                if (success == 1) AppendHTML($"{result}\n");
                else AppendHTML("n/a\n");
                AppendHTML("  Font Weight: ");
                success = Garglk_Pinvoke.glk_style_measure(glkwin_handle, s, StyleHint.Weight, ref result);
                if (success == 1) AppendHTML($"{(int)result}\n");
                else AppendHTML("n/a\n");
                AppendHTML("  Italics: ");
                success = Garglk_Pinvoke.glk_style_measure(glkwin_handle, s, StyleHint.Oblique, ref result);
                if (success == 1) AppendHTML(result == 1 ? "yes\n" : "no\n");
                else AppendHTML("n/a\n");
                AppendHTML("  Font Type: ");
                success = Garglk_Pinvoke.glk_style_measure(glkwin_handle, s, StyleHint.Proportional, ref result);
                if (success == 1) AppendHTML(result == 1 ? "proportional\n" : "fixed-width\n");
                else AppendHTML("n/a\n");
                AppendHTML("  Text color: ");
                success = Garglk_Pinvoke.glk_style_measure(glkwin_handle, s, StyleHint.TextColor, ref result);
                if (success == 1) AppendHTML($"0x{result:X}\n");
                else AppendHTML("n/a\n");
            }
        }
    }
}
