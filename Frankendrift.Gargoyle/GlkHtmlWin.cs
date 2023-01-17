using FrankenDrift.Gargoyle.Glk;
using FrankenDrift.Glue;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrankenDrift.Gargoyle
{
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
            Glk.Garglk_Pinvoke.glk_window_clear(glkwin_handle);
        }

        public unsafe string GetLineInput()
        {
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
                    if (ev.type == EventType.LineInput)
                    {
                        count = (int) ev.val1;
                        break;
                    }
                    else MainSession.Instance.ProcessEvent(ev);
                }
            }
            var dec = Encoding.GetEncoding(Encoding.Latin1.CodePage, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);
            return dec.GetString(cmdToBe, 0, count);
        }

        public void AppendHTML(string source)
        {
            Garglk_Pinvoke.glk_set_window(glkwin_handle);
            GarGlk.OutputString(source);
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
                Garglk_Pinvoke.glk_window_close(glkwin_handle);
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
    }
}
