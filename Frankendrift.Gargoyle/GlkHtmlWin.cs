using FrankenDrift.Gargoyle.Glk;
using FrankenDrift.Glue;
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

        public int TextLength => throw new NotImplementedException();

        public string Text { get => ""; set { } }
        public string SelectedText { get => ""; set { } }
        public int SelectionStart { get => -1; set { } }
        public int SelectionLength { get => -1; set { } }
        public bool IsDisposed => glkwin_handle == IntPtr.Zero;

        internal IntPtr Stream => Glk.Garglk_Pinvoke.glk_window_get_stream(glkwin_handle);

        internal GlkHtmlWin()
        {
            if (MainWin is null)
            {
                MainWin = this;
                NumberOfWindows += 1;
                glkwin_handle = Glk.Garglk_Pinvoke.glk_window_open(System.IntPtr.Zero, 0, 0, Glk.WinType.TextBuffer, NumberOfWindows);
            }
            else
            {
                glkwin_handle = _doWindowOpen(MainWin, Glk.WinMethod.Right | Glk.WinMethod.Proportional, 30);
            }
        }

        GlkHtmlWin(GlkHtmlWin splitFrom, Glk.WinMethod splitMethod, uint splitSize)
        {
            glkwin_handle = _doWindowOpen(splitFrom, splitMethod, splitSize);
        }

        private IntPtr _doWindowOpen(GlkHtmlWin splitFrom, Glk.WinMethod splitMethod, uint splitSize)
        {
            var result = Glk.Garglk_Pinvoke.glk_window_open(splitFrom.glkwin_handle, splitMethod, splitSize, Glk.WinType.TextBuffer, NumberOfWindows);
            if (result == IntPtr.Zero)
                throw new GlkError("Failed to open window.");
            return result;
        }

        public void Clear()
        {
            Glk.Garglk_Pinvoke.glk_window_clear(glkwin_handle);
        }

        public void RequestInput(ref StringBuilder target)
        {
            Glk.Garglk_Pinvoke.glk_request_line_event(glkwin_handle, target, (uint) target.Capacity, 0);
        }

        public void AppendHTML(string source)
        {
            Glk.Garglk_Pinvoke.glk_set_window(glkwin_handle);
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
                Glk.Garglk_Pinvoke.glk_window_close(glkwin_handle);
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
