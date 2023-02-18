using FrankenDrift.GlkRunner.Glk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrankenDrift.GlkRunner
{
    internal class GlkGridWin
    {
        private IntPtr glkwin_handle;

        internal GlkGridWin(IntPtr handle)
        {
            glkwin_handle = handle;
        }

        internal void Clear()
        {
            Glk_Pinvoke.garglk_set_zcolors((uint)ZColor.Default, (uint)ZColor.Default);
            Glk_Pinvoke.glk_window_clear(glkwin_handle);
        }

        internal void RewriteStatus(string status)
        {
            Glk_Pinvoke.glk_set_window(glkwin_handle);
            Glk_Pinvoke.glk_window_move_cursor(glkwin_handle, 0, 0);
            GlkUtil.OutputString(status);
        }

        internal int Width
        {
            get
            {
                Glk_Pinvoke.glk_window_get_size(glkwin_handle, out var width, out _);
                return (int)width;
            }
        }
    }
}
