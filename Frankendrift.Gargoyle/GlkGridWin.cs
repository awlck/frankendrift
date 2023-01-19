using FrankenDrift.Gargoyle.Glk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrankenDrift.Gargoyle
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
            Garglk_Pinvoke.garglk_set_zcolors((uint)ZColor.Default, (uint)ZColor.Default);
            Garglk_Pinvoke.glk_window_clear(glkwin_handle);
        }

        internal void RewriteStatus(string status)
        {
            Garglk_Pinvoke.glk_set_window(glkwin_handle);
            Garglk_Pinvoke.glk_window_move_cursor(glkwin_handle, 0, 0);
            GarGlk.OutputString(status);
        }

        internal int Width
        {
            get
            {
                Garglk_Pinvoke.glk_window_get_size(glkwin_handle, out var width, out _);
                return (int)width;
            }
        }
    }
}
