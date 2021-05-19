using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MonoMac.CoreImage;

namespace Adravalon.Runner
{
    public class AdriftOutput : RichTextArea, Glue.RichTextBox
    {
        public AdriftOutput() : base()
        {
            BackgroundColor = _defaultBackground;
            SelectionForeground = _defaultColor;
            _defaultFont = SelectionFont;
            
            _fonts.Push(new Tuple<Font, Color>(_defaultFont, _defaultColor));
        }
        
        public void Clear()
        {
            Text = "";
        }

        public int TextLength => Text.Length;
        public int SelectionStart { get => Selection.Start; set => Selection = Selection.WithStart(value); }
        public int SelectionLength { get => Selection.Length(); set => Selection = Selection.WithLength(value); }

        private string _pendingText;
        internal bool IsWaiting { get; private set; } = false;

        // Whenever ADRIFT's timer runs out, the engine outputs whatever it was trying to output. Like this, we only
        // get duplicate text rather than crashes...
        private readonly Mutex _outputMutex = new Mutex();
        
        private readonly Color _defaultColor = Colors.Cyan;
        private readonly Color _defaultBackground = Colors.Black;
        private readonly Color _defaultInput = Colors.Red;
        private readonly Font _defaultFont;
        private Stack<Tuple<Font, Color>> _fonts = new();

        private float CalculateTextSize(int requestedSize)
        {
            return requestedSize - 12 + _defaultFont.Size;
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
            var consumed = 0;
            var inToken = false;
            var current = new StringBuilder();
            var currentToken = "";
            foreach (var c in src)
            {
                consumed++;
                if (c == '<' && !inToken)
                {
                    inToken = true;
                    currentToken = "";
                    _outputMutex.WaitOne();
                    Append(current.ToString(), true);
                    _outputMutex.ReleaseMutex();
                    current.Clear();
                }
                else if (c != '>' && inToken)
                {
                    currentToken += c;
                }
                else if (c == '>' && inToken)
                {
                    inToken = false;
                    switch (currentToken)
                    {
                        case "br":
                            _outputMutex.WaitOne();
                            Append("\n");
                            _outputMutex.ReleaseMutex();
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
                            _fonts.Push(new Tuple<Font, Color>(SelectionFont, SelectionForeground));
                            SelectionForeground = _defaultInput;
                            break;
                        /* case "/c":
                            SelectionForeground = _defaultColor;
                            break; */
                        case "waitkey":
                            _pendingText = src[consumed..];
                            IsWaiting = true;
                            _outputMutex.WaitOne();
                            Append("\n(Press any key to continue)");
                            _outputMutex.ReleaseMutex();
                            return;
                        case "/c":
                        case "/font":
                            // SelectionForeground = _savedColor;
                            // SelectionFont = _defaultFont;
                            var restore = _fonts.Peek();
                            SelectionFont = restore.Item1;
                            SelectionForeground = restore.Item2;
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
                    if (!currentToken.StartsWith("font")) continue;
                    // _savedColor = SelectionForeground;
                    _fonts.Push(new Tuple<Font, Color>(SelectionFont, SelectionForeground));
                    var re = new Regex("color ?= ?\"?(#?[0-9A-Fa-f]{6})\"?");
                    var col = re.Match(currentToken);
                    if (col.Success)
                    {
                        var matchString = col.Groups[1].ToString();
                        var colorString = matchString.StartsWith('#') ? matchString : ("#" + matchString);
                        if (Color.TryParse(colorString, out var newColor))
                            SelectionForeground = newColor;
                        //SelectionForeground = ParseHexColor(col.Groups[0].Value);
                    }
                    else if (currentToken.Contains("black"))
                        SelectionForeground = Colors.Black;
                    else if (currentToken.Contains("blue"))
                        SelectionForeground = Colors.Blue;
                    else if (currentToken.Contains("gray"))
                        SelectionForeground = Colors.Gray;
                    else if (currentToken.Contains("green"))
                        SelectionForeground = Colors.Green;
                    else if (currentToken.Contains("lime"))
                        SelectionForeground = Colors.Lime;
                    else if (currentToken.Contains("magenta"))
                        SelectionForeground = Colors.Magenta;
                    else if (currentToken.Contains("maroon"))
                        SelectionForeground = Colors.Maroon;
                    else if (currentToken.Contains("navy"))
                        SelectionForeground = Colors.Navy;
                    else if (currentToken.Contains("olive"))
                        SelectionForeground = Colors.Olive;
                    else if (currentToken.Contains("orange"))
                        SelectionForeground = Colors.Orange;
                    else if (currentToken.Contains("pink"))
                        SelectionForeground = Colors.Pink;
                    else if (currentToken.Contains("purple"))
                        SelectionForeground = Colors.Purple;
                    else if (currentToken.Contains("red"))
                        SelectionForeground = Colors.Red;
                    else if (currentToken.Contains("silver"))
                        SelectionForeground = Colors.Silver;
                    else if (currentToken.Contains("teal"))
                        SelectionForeground = Colors.Teal;
                    else if (currentToken.Contains("white"))
                        SelectionForeground = Colors.White;
                    else if (currentToken.Contains("yellow"))
                        SelectionForeground = Colors.Yellow;
                    else if (currentToken.Contains("cyan"))
                        SelectionForeground = Colors.Cyan;
                    re = new Regex("face ?= ?\"(.*?)\"");
                    var face = re.Match(currentToken);
                    if (face.Success)
                    {
                        try
                        {
                            SelectionFont = SelectionFont.WithFontFace(face.Groups[1].Value);
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            System.Diagnostics.Debug.Print(ex.Message);
                        }
                    }

                    re = new Regex("size ?= ?\"?([+-]?\\d+)\"?");
                    var size = re.Match(currentToken);
                    if (size.Success)
                    {
                        var sizeString = size.Groups[1].Value;
                        if (sizeString.StartsWith('+'))
                        {
                            if (int.TryParse(sizeString[1..], out var sizeDelta))
                                SelectionFont = SelectionFont.WithSize(SelectionFont.Size + sizeDelta);
                        }
                        else if (sizeString.StartsWith('-'))
                        {
                            if (int.TryParse(sizeString[1..], out var sizeDelta))
                                SelectionFont = SelectionFont.WithSize(SelectionFont.Size - sizeDelta);
                        }
                        else
                        {
                            if (int.TryParse(sizeString, out var targetSize))
                                SelectionFont = SelectionFont.WithSize(CalculateTextSize(targetSize));
                        }
                    }
                }
                else current.Append(c);
            }
            _outputMutex.WaitOne();
            Append(current.ToString(), true);
            _outputMutex.ReleaseMutex();
        }

        internal void FinishWaiting()
        {
            IsWaiting = false;
            AppendHtml(_pendingText);
            _pendingText = "";
        }
    }
}
