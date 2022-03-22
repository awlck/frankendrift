using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace FrankenDrift.Runner
{
    public class AdriftOutput : RichTextArea, Glue.RichTextBox
    {
        public AdriftOutput(MainForm main) : base()
        {
            _main = main;
            ReadOnly = true;
            BackgroundColor = _defaultBackground;
            SelectionForeground = _defaultColor;
            _defaultFont = SelectionFont.WithSize(SelectionFont.Size+1);
            SelectionFont = _defaultFont;
            Append(" ");
            
            _fonts.Push(new Tuple<Font, Color>(_defaultFont, _defaultColor));
        }
        
        public void Clear()
        {
            Text = "";
            _fonts.Clear();
            _fonts.Push(new Tuple<Font, Color>(_defaultFont, _defaultColor));
            BackgroundColor = _defaultBackground;
            SelectionForeground = _defaultColor;
            SelectionFont = _defaultFont;
        }
        
        public int TextLength => Text.Length;
        public int SelectionStart { get => Selection.Start; set => Selection = Selection.WithStart(value); }
        public int SelectionLength { get => Selection.Length(); set => Selection = Selection.WithLength(value); }

        private string _pendingText;
        internal bool IsWaiting { get; private set; } = false;

        internal Color _defaultColor = Colors.Cyan;
        internal Color _defaultBackground = Colors.Black;
        internal Color _defaultInput = Colors.Red;
        private readonly Font _defaultFont;
        private readonly Stack<Tuple<Font, Color>> _fonts = new();
        private readonly MainForm _main;

        private float CalculateTextSize(int requestedSize)
        {
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
            ReadOnly = false;
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
                    if (currentToken == "del")
                    {
                        current.Remove(current.Length - 1, 1);
                        continue;
                    }
                    else
                    {
                        AppendWithFont(current.ToString(), true);
                        current.Clear();
                    }
                    switch (currentToken)
                    {
                        case "br":
                            Append("\n");
                            break;
                        case "b":
                            SelectionBold = true;
                            break;
                        case "/b":
                            SelectionBold = false;
                            break;
                        case "i":
                            SelectionItalic = true;
                            break;
                        case "/i":
                            SelectionItalic = false;
                            break;
                        case "u":
                            SelectionUnderline = true;
                            break;
                        case "/u":
                            SelectionUnderline = false;
                            break;
                        case "c":
                            _fonts.Push(new Tuple<Font, Color>(SelectionFont, _defaultInput));
                            break;
                        case "waitkey":
                            _pendingText = src[consumed..];
                            IsWaiting = true;
                            AppendWithFont("\n(Press any key to continue)");
                            return;
                        case "/c":
                        case "/font":
                            if (_fonts.Count > 1)
                                _fonts.Pop();
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
                        var imgPath = new Regex("src ?= ?\"(.+)\"").Match(currentToken);
                        if (imgPath.Success && SettingsManager.Instance.Settings.EnableGraphics)
                            _main.Graphics.DisplayImage(imgPath.Groups[1].Value);
                        continue;
                    }
                    
                    // fonts.
                    if (!currentToken.StartsWith("font")) continue;
                    var (font, color) = _fonts.Peek();
                    if (current.Length == 0 && previousToken.StartsWith("font"))
                        _fonts.Pop();
                    var tokenLower = currentToken.ToLower();
                    var re = new Regex("color ?= ?\"?#?([0-9A-Fa-f]{6})\"?");
                    var col = re.Match(currentToken);
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
                    
                    re = new Regex("face ?= ?\"(.*?)\"");
                    var face = re.Match(currentToken);
                    if (face.Success)
                    {
                        font = font.WithFontFace(face.Groups[1].Value);
                    }

                    re = new Regex("size ?= ?\"?([+-]?\\d+)\"?");
                    var size = re.Match(currentToken);
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
            AppendWithFont(current.ToString(), true);
            ReadOnly = true;
        }

        // For some reason formatting gets lost upon changing fonts unless we do this terribleness:
        private void AppendWithFont(string src, bool scroll = false)
        {
            var bold = SelectionBold;
            var underline = SelectionUnderline;
            var italics = SelectionItalic;
            var (font, color) = _fonts.Peek();
            SelectionForeground = color;
            SelectionFont = font;
            SelectionBold = bold;
            SelectionUnderline = underline;
            SelectionItalic = italics;
            Append(src, scroll);
            SelectionForeground = color;
            SelectionFont = font;
            SelectionBold = bold;
            SelectionUnderline = underline;
            SelectionItalic = italics;
        }

        internal void FinishWaiting()
        {
            IsWaiting = false;
            var theText = _pendingText;
            _pendingText = "";
            AppendHtml(theText);
        }

        // This can't cope with nested <window ...> tags. Too bad!
        private int SendToAnotherWindow(string output, string tag)
        {
            var consumed = 0;
            var inToken = false;
            var current = new StringBuilder();
            var currentToken = "";
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
                    if (currentToken == "/window")
                    {
                        _main.GetSecondaryWindow(tag[7..]).AppendHtml(current.ToString());
                        return consumed;
                    }
                    else current.Append('<').Append(currentToken).Append('>');
                }
                else current.Append(c);
            }
            _main.GetSecondaryWindow(tag[7..]).AppendHtml(current.ToString());
            return consumed;
        }
    }
}
