using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace FrankenDrift.Runner
{
    public partial class AdriftOutput : RichTextArea, Glue.RichTextBox
    {
        protected struct TextStyle
        {
            internal bool Bold;
            internal bool Italic;
            internal bool Underline;
        }

        public AdriftOutput(MainForm main) : base()
        {
            _main = main;
            ReadOnly = true;
            BackgroundColor = _defaultBackground;
            SelectionForeground = _defaultColor;
            if (!string.IsNullOrEmpty(SettingsManager.Settings.DefaultFontName))
                _defaultFont = SelectionFont.WithFontFace(SettingsManager.Settings.DefaultFontName);
            else
                _defaultFont = SelectionFont;
            if (SettingsManager.Settings.EnableDevFont)
                _defaultFont = _defaultFont.WithSize(SelectionFont.Size+SettingsManager.Settings.AlterFontSize);
            else
                _defaultFont = _defaultFont.WithSize(SettingsManager.Settings.UserFontSize);
            SelectionFont = _defaultFont;
            Append(" ");
            
            _fonts.Push(new Tuple<Font, Color>(_defaultFont, _defaultColor));
            // Improved font availability detection: insert some text with the Wingdings font, then observe
            // whether the selected font actually changes. (Which it doesn't on Mac, for some reason.)
            AppendHtml("<font face=\"Wingdings\">T");
            _wingdingsAvailable = SelectionFont.FamilyName == "Wingdings";
            Clear(true);
        }

        // Needs to be a separate overload rather than just introducing an optional
        // parameter due to the RichTextBox interface.
        public void Clear() => Clear(false);
        
        public void Clear(bool force)
        {
            Text = "";
            if (force)
            {
                // Discard all pending text and stop waiting for a key press. To be
                // used when a new game is about to begin.
                IsWaiting = false;
                _pendingText = "";
            }
            _fonts.Clear();
            _fonts.Push(new Tuple<Font, Color>(_defaultFont, _defaultColor));
            BackgroundColor = _defaultBackground;
            SelectionForeground = _defaultColor;
            SelectionFont = _defaultFont;
            _style = new()
            {
                Bold = false,
                Italic = false,
                Underline = false
            };
            _previousStyle = null;
        }
        
        public int SelectionStart { get => Selection.Start; set => Selection = Selection.WithStart(value); }
        public int SelectionLength { get => Selection.Length(); set => Selection = Selection.WithLength(value); }

        private string _pendingText;
        internal bool IsWaiting { get; private set; } = false;

        internal Color _defaultColor = Colors.Cyan;
        internal Color _defaultBackground = Colors.Black;
        internal Color _defaultInput = Colors.Red;
        internal Font _defaultFont;
        protected readonly Stack<Tuple<Font, Color>> _fonts = new();
        private readonly MainForm _main;
        private readonly bool _wingdingsAvailable;

        protected TextStyle _style;
        protected TextStyle? _previousStyle = null;
        private bool _fastForward;

        internal float CalculateTextSize(int requestedSize)
        {
            if (SettingsManager.Settings.EnableDevFont)
                return requestedSize + SettingsManager.Settings.AlterFontSize;
            // no idea why specifically, but this seems to give sensible results.
            return Math.Max(requestedSize - 12 + _defaultFont.Size, 1);
        }
        
        // We! Are the masters of the Jank-axy, we're the Lords of Space Jank-ee!
        // The destroyers of normality! Knights of Jankness arise!
        public void AppendHtml(string src)
        {
            if (IsWaiting)
            {
                _pendingText += src;
                return;
            }
            if (_main.Quitting) return;
            ReadOnly = false;
            if (!_wingdingsAvailable)
                src = src.Replace("<font face=\"Wingdings\" size=14>Ø</font>", "<font size=+1>></font>");
            var consumed = 0;
            var inToken = false;
            var current = new StringBuilder();
            var currentToken = "";
            var previousToken = "";
            var skip = 0;
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
                    inToken = false;
                    if (currentToken == "del" && current.Length > 0)
                    {
                        // As long as we haven't committed to displaying anything,
                        // use the more reliable method for deleting the last character.
                        current.Remove(current.Length - 1, 1);
                        continue;
                    }
                    AppendWithFont(current.ToString());
                    current.Clear();
                    switch (currentToken)
                    {
                        case "br":
                            Append("\n");
                            break;
                        case "b":
                            _style.Bold = true;
                            break;
                        case "/b":
                            _style.Bold = false;
                            break;
                        case "i":
                            _style.Italic = true;
                            break;
                        case "/i":
                            _style.Italic = false;
                            break;
                        case "u":
                            _style.Underline = true;
                            break;
                        case "/u":
                            _style.Underline = false;
                            break;
                        case "c":
                            _fonts.Push(new Tuple<Font, Color>(_fonts.Peek().Item1, _defaultInput));
                            break;
                        case "waitkey" when !_fastForward:
                            _pendingText = src[consumed..];
                            IsWaiting = true;
                            if (SettingsManager.Settings.EnablePressAnyKey)
                                AppendWithFont("\n(Press any key to continue)");
                            ScrollToEnd();
                            return;
                        case "/c":
                        case "/font":
                            if (_fonts.Count > 1)
                                _fonts.Pop();
                            break;
                        case "del":
                            // If the `del` case wasn't handled above, remove the last character from the
                            // already-displayed text. This crashes on macOS for unknown reasons, so it's
                            // disabled on that platform.
                            if (!Application.Instance.Platform.IsMac)
                                Buffer.Delete(new Range<int>(Text.Length - 1,1));
                            break;
                        case "cls":
                            Clear();
                            break;
                        case "":
                            continue;
                    }

                    // Yay, special cases!
                    if (currentToken.StartsWith("window"))  // alternate windows.
                    {
                        skip = SendToAnotherWindow(src[consumed..], currentToken);
                        continue;
                    }
                    if (currentToken.StartsWith("img"))  // graphics.
                    {
                        var imgPath = ImgSrcRegex().Match(currentToken);
                        if (imgPath.Success && SettingsManager.Settings.EnableGraphics)
                            _main.Graphics.DisplayImage(imgPath.Groups[1].Value);
                        continue;
                    }
                    
                    // fonts.
                    if (!currentToken.StartsWith("font")) continue;
                    var (font, color) = _fonts.Peek();
                    if (current.Length == 0 && previousToken.StartsWith("font"))
                        _fonts.Pop();
                    var tokenLower = currentToken.ToLower();
                    var col = FontColorRegex().Match(currentToken);
                    if (col.Success)
                    {
                        var colorString =  ("#" + col.Groups[1].Value);
                        if (Color.TryParse(colorString, out var newColor))
                            color = newColor;
                    }
                    else if (tokenLower.Contains("black"))
                        color = Colors.Black;
                    else if (tokenLower.Contains("blue"))
                        color = Colors.Blue;
                    else if (tokenLower.Contains("gray"))
                        color = Colors.Gray;
                    else if (tokenLower.Contains("darkgreen"))
                        color = Colors.DarkGreen;
                    else if (tokenLower.Contains("green"))
                        color = Colors.Green;
                    else if (tokenLower.Contains("lime"))
                        color = Colors.Lime;
                    else if (tokenLower.Contains("magenta"))
                        color = Colors.Magenta;
                    else if (tokenLower.Contains("maroon"))
                        color = Colors.Maroon;
                    else if (tokenLower.Contains("navy"))
                        color = Colors.Navy;
                    else if (tokenLower.Contains("olive"))
                        color = Colors.Olive;
                    else if (tokenLower.Contains("orange"))
                        color = Colors.Orange;
                    else if (tokenLower.Contains("pink"))
                        color = Colors.Pink;
                    else if (tokenLower.Contains("purple"))
                        color = Colors.Purple;
                    else if (tokenLower.Contains("red"))
                        color = Colors.Red;
                    else if (tokenLower.Contains("silver"))
                        color = Colors.Silver;
                    else if (tokenLower.Contains("teal"))
                        color = Colors.Teal;
                    else if (tokenLower.Contains("white"))
                        color = Colors.White;
                    else if (tokenLower.Contains("yellow"))
                        color = Colors.Yellow;
                    else if (tokenLower.Contains("cyan"))
                        color = Colors.Cyan;
                    else if (tokenLower.Contains("darkolive"))
                        color = Colors.DarkOliveGreen;
                    else if (tokenLower.Contains("olive"))
                        color = Colors.Olive;
                    else if (tokenLower.Contains("tan"))
                        color = Colors.Tan;

                    if (font is null)
                    {
                        if (_defaultFont is not null)
                            font = _defaultFont;
                        else
                        {
                            font = new Font(
                                Fonts.AvailableFontFamilies.FirstOrDefault(f =>
                                    f.Name is "Helvetica" or "Arial" or "Liberation Sans"),
                                SettingsManager.Settings.UserFontSize);
                        }
                    }
                    var face = FontFaceRegex().Match(currentToken);
                    if (face.Success)
                    {
                        var f = face.Groups[1].Value;
                        if (!SettingsManager.Settings.BanComicSans || !f.StartsWith("Comic Sans"))
                            font = font.WithFontFace(f);
                    }

                    var size = FontSizeRegex().Match(currentToken);
                    if (size.Success)
                    {
                        var sizeString = size.Groups[1].Value;
                        if (sizeString.StartsWith('+'))
                        {
                            if (int.TryParse(sizeString[1..], out var sizeDelta))
                                font = font.WithSize(SelectionFont.Size + sizeDelta);
                        }
                        else if (sizeString.StartsWith('-'))
                        {
                            if (int.TryParse(sizeString[1..], out var sizeDelta))
                                font = font.WithSize(SelectionFont.Size - sizeDelta);
                        }
                        else
                        {
                            if (int.TryParse(sizeString, out var targetSize))
                                font = font.WithSize(CalculateTextSize(targetSize));
                        }
                    }
                    _fonts.Push(new Tuple<Font, Color>(font, color));
                }
                else current.Append(c);
            }
            AppendWithFont(current.ToString());
            ReadOnly = true;
            ScrollToEnd();
        }

        public virtual void AppendWithFont(string src, bool scroll = false)
        {
            var (font, color) = _fonts.Peek();
            SelectionFont = font;
            SelectionForeground = color;
            if (_style.Bold != _previousStyle?.Bold)
                SelectionBold = _style.Bold;
            if (_style.Underline != _previousStyle?.Underline)
                SelectionUnderline = _style.Underline;
            if (_style.Italic != _previousStyle?.Italic)
                SelectionItalic = _style.Italic;
            var text = src.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&perc;", "%").Replace("&quot;", "\"");
            Append(text, scroll);
            _previousStyle = _style;
        }

        internal void FinishWaiting()
        {
            IsWaiting = false;
            var theText = _pendingText;
            _pendingText = "";
            AppendHtml(theText);
        }

        // Immediately dump all pending text, ignoring any `waitkey` tags within it.
        // Intended for use when the user chooses to restore a game while sitting on
        // some sort of splash screen or intro where we don't necessarily want to
        // clear out the screen but we also don't want to require the player to click
        // through whatever is happening first.
        internal void FastForwardText()
        {
            _fastForward = true;
            FinishWaiting();
            _fastForward = false;
        }

        private int SendToAnotherWindow(string output, string tag)
        {
            var consumed = 0;
            var inToken = false;
            var current = new StringBuilder();
            var currentToken = "";
            var nestingDepth = 1;
            foreach (var c in output)
            {
                consumed++;
                if (c == '<' && !inToken)
                {
                    inToken = true;
                    currentToken = "";
                }
                else if (c != '>' && inToken)
                {
                    currentToken += c;
                }
                else if (c == '>' && inToken)
                {
                    inToken = false;
                    if (currentToken == "/window" && --nestingDepth == 0)
                    {
                        _main.GetSecondaryWindow(tag[7..]).AppendHtml(current.ToString());
                        return consumed;
                    }
                    else
                    {
                        if (currentToken.StartsWith("window"))
                            nestingDepth++;
                        current.Append('<').Append(currentToken).Append('>');
                    }
                }
                else current.Append(c);
            }
            _main.GetSecondaryWindow(tag[7..]).AppendHtml(current.ToString());
            return consumed;
        }

        [GeneratedRegex("size ?= ?\"?([+-]?\\d+)\"?")]
        private static partial Regex FontSizeRegex();

        [GeneratedRegex("face ?= ?\"(.*?)\"")]
        private static partial Regex FontFaceRegex();

        [GeneratedRegex("color ?= ?\"?#?([0-9A-Fa-f]{6})\"?")]
        private static partial Regex FontColorRegex();

        [GeneratedRegex("src ?= ?\"(.+)\"")]
        private static partial Regex ImgSrcRegex();
    }

    class OutputLateFormatting(MainForm main) : AdriftOutput(main)
    {
        public override void AppendWithFont(string src, bool scroll = false)
        {
            var text = src.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&perc;", "%").Replace("&quot;", "\"");
            if (string.IsNullOrEmpty(text)) return;
            var begin = TextLength;
            Append(text, scroll);
            var end = TextLength;
            Selection = new Range<int>(begin, end);
            var (font, color) = _fonts.Peek();
            SelectionFont = font;
            SelectionForeground = color;
            if (_style.Bold)
                SelectionBold = true;
            if (_style.Underline)
                SelectionUnderline = true;
            if (_style.Italic)
                SelectionItalic = true;
            Selection = new Range<int>(end + 1, end + 1);
        }
    }
}
