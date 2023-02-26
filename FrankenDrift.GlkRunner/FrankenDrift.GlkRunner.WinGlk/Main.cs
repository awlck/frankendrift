using FrankenDrift.GlkRunner.Glk;
using System.Runtime.InteropServices;

namespace FrankenDrift.GlkRunner.WinGlk
{
    // WinGlk startup functions.
    internal static class Winglk_Pinvoke
    {
        // Universal Glk functions.
        [DllImport("Glk")]
        internal static extern BlorbError giblorb_set_resource_map(IntPtr fileStream);
        [DllImport("Glk")]
        internal static extern void glk_cancel_hyperlink_event(WindowHandle winId);
        [DllImport("Glk")]
        internal static extern void glk_cancel_line_event(WindowHandle winId, ref Event ev);
        [DllImport("Glk")]
        internal static extern void glk_exit();
        [DllImport("Glk")]
        internal static extern FileRefHandle glk_fileref_create_by_name(FileUsage usage, [MarshalAs(UnmanagedType.LPStr)] string name, Glk.FileMode fmode, uint rock);
        [DllImport("Glk")]
        internal static extern FileRefHandle glk_fileref_create_by_prompt(FileUsage usage, Glk.FileMode fmode, uint rock);
        [DllImport("Glk")]
        internal static extern FileRefHandle glk_fileref_create_temp(FileUsage usage, uint rock);
        [DllImport("Glk")]
        internal static extern void glk_fileref_destroy(FileRefHandle fref);
        [DllImport("Glk")]
        internal static extern uint glk_image_draw(WindowHandle winid, uint imageId, int val1, int val2);
        [DllImport("Glk")]
        internal static extern uint glk_image_get_info(uint imageId, ref uint width, ref uint height);
        [DllImport("Glk")]
        internal static extern void glk_put_buffer([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] s, uint len);
        [DllImport("Glk")]
        internal static extern void glk_put_buffer_stream(IntPtr streamId, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] s, uint len);
        [DllImport("Glk")]
        internal static extern void glk_put_buffer_uni([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] uint[] s, uint len);
        [DllImport("Glk")]
        internal static extern void glk_request_char_event(WindowHandle winId);
        [DllImport("Glk")]
        internal static extern void glk_request_hyperlink_event(WindowHandle winId);
        [DllImport("Glk")]
        internal static extern unsafe void glk_request_line_event(WindowHandle win, byte* buf, uint maxlen, uint initlen);
        [DllImport("Glk")]
        internal static extern unsafe void glk_request_line_event_uni(WindowHandle win, uint* buf, uint maxlen, uint initlen);
        [DllImport("Glk")]
        internal static extern IntPtr glk_schannel_create(uint rock);
        [DllImport("Glk")]
        internal static extern void glk_schannel_destroy(IntPtr chan);
        [DllImport("Glk")]
        internal static extern void glk_schannel_pause(IntPtr chan);
        [DllImport("Glk")]
        internal static extern uint glk_schannel_play(IntPtr chan, uint sndId);
        [DllImport("Glk")]
        internal static extern uint glk_schannel_play_ext(IntPtr chan, uint sndId, uint repeats, uint notify);
        [DllImport("Glk")]
        internal static extern void glk_schannel_set_volume(IntPtr chan, uint vol);
        [DllImport("Glk")]
        internal static extern void glk_schannel_stop(IntPtr chan);
        [DllImport("Glk")]
        internal static extern void glk_schannel_unpause(IntPtr chan);
        [DllImport("Glk")]
        internal static extern void glk_select(ref Event ev);
        [DllImport("Glk")]
        internal static extern void glk_set_hyperlink(uint linkval);
        [DllImport("Glk")]
        internal static extern void glk_set_style(Style s);
        [DllImport("Glk")]
        internal static extern void glk_set_window(WindowHandle winId);
        [DllImport("Glk")]
        internal static extern IntPtr glk_stream_open_file(FileRefHandle fileref, Glk.FileMode fmode, uint rock);
        [DllImport("Glk")]
        internal static extern IntPtr glk_stream_open_memory(IntPtr buf, uint buflen, Glk.FileMode mode, uint rock);
        [DllImport("Glk")]
        internal static extern void glk_stream_set_position(IntPtr stream, int pos, SeekMode seekMode);
        [DllImport("Glk")]
        internal static extern void glk_stylehint_set(WinType wintype, Style styl, StyleHint hint, int val);
        [DllImport("Glk")]
        internal static extern uint glk_style_measure(WindowHandle winid, Style styl, StyleHint hint, ref uint result);
        [DllImport("Glk")]
        internal static extern void glk_tick();
        [DllImport("Glk")]
        internal static extern void glk_window_clear(WindowHandle winId);
        [DllImport("Glk")]
        internal static extern void glk_window_close(WindowHandle winId, IntPtr streamResult);
        [DllImport("Glk")]
        internal static extern void glk_window_flow_break(WindowHandle winId);
        [DllImport("Glk")]
        internal static extern void glk_window_get_size(WindowHandle winId, out uint width, out uint height);
        [DllImport("Glk")]
        internal static extern IntPtr glk_window_get_stream(WindowHandle winId);
        [DllImport("Glk")]
        internal static extern void glk_window_move_cursor(WindowHandle winId, uint xpos, uint ypos);
        [DllImport("Glk")]
        internal static extern WindowHandle glk_window_open(WindowHandle split, WinMethod method, uint size, WinType wintype, uint rock);
        [DllImport("Glk")]
        internal static extern void garglk_set_zcolors(uint fg, uint bg);
        [DllImport("Glk")]
        internal static extern IntPtr glkunix_fileref_get_name(FileRefHandle fileref);

        [DllImport("Glk")]
        internal static extern int InitGlk(uint version);
        [DllImport("Glk")]
        internal static extern void winglk_app_set_name([MarshalAs(UnmanagedType.LPStr)] string name);
        [DllImport("Glk")]
        internal static extern void winglk_window_set_title([MarshalAs(UnmanagedType.LPStr)] string name);
    }
    class WindowsGlk : IGlk
    {
        public BlorbError giblorb_set_resource_map(IntPtr fileStream) => Winglk_Pinvoke.giblorb_set_resource_map(fileStream);
        public void glk_cancel_hyperlink_event(WindowHandle winId) => Winglk_Pinvoke.glk_cancel_hyperlink_event(winId);
        public void glk_cancel_line_event(WindowHandle winId, ref Event ev) => Winglk_Pinvoke.glk_cancel_line_event(winId, ref ev);
        public void glk_exit() => Winglk_Pinvoke.glk_exit();
        public FileRefHandle glk_fileref_create_by_name(FileUsage usage, string name, Glk.FileMode fmode, uint rock) => Winglk_Pinvoke.glk_fileref_create_by_name(usage, name, fmode, rock);
        public FileRefHandle glk_fileref_create_by_prompt(FileUsage usage, Glk.FileMode fmode, uint rock) => Winglk_Pinvoke.glk_fileref_create_by_prompt(usage, fmode, rock);
        public FileRefHandle glk_fileref_create_temp(FileUsage usage, uint rock) => Winglk_Pinvoke.glk_fileref_create_temp(usage, rock);
        public void glk_fileref_destroy(FileRefHandle fref) => Winglk_Pinvoke.glk_fileref_destroy(fref);
        public uint glk_image_draw(WindowHandle winid, uint imageId, int val1, int val2) => Winglk_Pinvoke.glk_image_draw(winid, imageId, val1, val2);
        public uint glk_image_get_info(uint imageId, ref uint width, ref uint height) => Winglk_Pinvoke.glk_image_get_info(imageId, ref width, ref height);
        public void glk_put_buffer(byte[] s, uint len) => Winglk_Pinvoke.glk_put_buffer(s, len);
        public void glk_put_buffer_stream(IntPtr streamId, byte[] s, uint len) => Winglk_Pinvoke.glk_put_buffer_stream(streamId, s, len);
        public void glk_put_buffer_uni(uint[] s, uint len) => Winglk_Pinvoke.glk_put_buffer_uni(s, len);
        public void glk_request_char_event(WindowHandle winId) => Winglk_Pinvoke.glk_request_char_event(winId);
        public void glk_request_hyperlink_event(WindowHandle winId) => Winglk_Pinvoke.glk_request_hyperlink_event(winId);
        public unsafe void glk_request_line_event(WindowHandle win, byte* buf, uint maxlen, uint initlen) => Winglk_Pinvoke.glk_request_line_event(win, buf, maxlen, initlen);
        public unsafe void glk_request_line_event_uni(WindowHandle win, uint* buf, uint maxlen, uint initlen) => Winglk_Pinvoke.glk_request_line_event_uni(win, buf, maxlen, initlen);
        public IntPtr glk_schannel_create(uint rock) => Winglk_Pinvoke.glk_schannel_create(rock);
        public void glk_schannel_destroy(IntPtr chan) => Winglk_Pinvoke.glk_schannel_destroy(chan);
        public void glk_schannel_pause(IntPtr chan) => Winglk_Pinvoke.glk_schannel_pause(chan);
        public uint glk_schannel_play(IntPtr chan, uint sndId) => Winglk_Pinvoke.glk_schannel_play(chan, sndId);
        public uint glk_schannel_play_ext(IntPtr chan, uint sndId, uint repeats, uint notify) => Winglk_Pinvoke.glk_schannel_play_ext(chan, sndId, repeats, notify);
        public void glk_schannel_set_volume(IntPtr chan, uint vol) => Winglk_Pinvoke.glk_schannel_set_volume(chan, vol);
        public void glk_schannel_stop(IntPtr chan) => Winglk_Pinvoke.glk_schannel_stop(chan);
        public void glk_schannel_unpause(IntPtr chan) => Winglk_Pinvoke.glk_schannel_unpause(chan);
        public void glk_select(ref Event ev) => Winglk_Pinvoke.glk_select(ref ev);
        public void glk_set_hyperlink(uint linkval) => Winglk_Pinvoke.glk_set_hyperlink(linkval);
        public void glk_set_style(Style s) => Winglk_Pinvoke.glk_set_style(s);
        public void glk_set_window(WindowHandle winId) => Winglk_Pinvoke.glk_set_window(winId);
        public IntPtr glk_stream_open_file(FileRefHandle fileref, Glk.FileMode fmode, uint rock) => Winglk_Pinvoke.glk_stream_open_file(fileref, fmode, rock);
        public IntPtr glk_stream_open_memory(IntPtr buf, uint buflen, Glk.FileMode mode, uint rock) => Winglk_Pinvoke.glk_stream_open_memory(buf, buflen, mode, rock);
        public void glk_stream_set_position(IntPtr stream, int pos, SeekMode seekMode) => Winglk_Pinvoke.glk_stream_set_position(stream, pos, seekMode);
        public void glk_stylehint_set(WinType wintype, Style styl, StyleHint hint, int val) => Winglk_Pinvoke.glk_stylehint_set(wintype, styl, hint, val);
        public uint glk_style_measure(WindowHandle winid, Style styl, StyleHint hint, ref uint result) => Winglk_Pinvoke.glk_style_measure(winid, styl, hint, ref result);
        public void glk_tick() => Winglk_Pinvoke.glk_tick();
        public void glk_window_clear(WindowHandle winId) => Winglk_Pinvoke.glk_window_clear(winId);
        public void glk_window_close(WindowHandle winId, IntPtr streamResult) => Winglk_Pinvoke.glk_window_close(winId, streamResult);
        public void glk_window_flow_break(WindowHandle winId) => Winglk_Pinvoke.glk_window_flow_break(winId);
        public void glk_window_get_size(WindowHandle winId, out uint width, out uint height) => Winglk_Pinvoke.glk_window_get_size(winId, out width, out height);
        public IntPtr glk_window_get_stream(WindowHandle winId) => Winglk_Pinvoke.glk_window_get_stream(winId);
        public void glk_window_move_cursor(WindowHandle winId, uint xpos, uint ypos) => Winglk_Pinvoke.glk_window_move_cursor(winId, xpos, ypos);
        public WindowHandle glk_window_open(WindowHandle split, WinMethod method, uint size, WinType wintype, uint rock) => Winglk_Pinvoke.glk_window_open(split, method, size, wintype, rock);
        public void garglk_set_zcolors(uint fg, uint bg) => Winglk_Pinvoke.garglk_set_zcolors(fg, bg);
        public string? glkunix_fileref_get_name(FileRefHandle fileref) => Marshal.PtrToStringAnsi(Winglk_Pinvoke.glkunix_fileref_get_name(fileref));

        public void SetGameName(string game) => Winglk_Pinvoke.winglk_window_set_title(game);
    }

    public class WinGlkRunner
    {
        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: No file selected!");
                return 1;
            }

            if (Winglk_Pinvoke.InitGlk(0x00000704) == 0) { return 2; }
            Winglk_Pinvoke.winglk_app_set_name("Windows Glk FrankenDrift");

            WindowsGlk GlkApi = new WindowsGlk();
            var sess = new MainSession(args[^1], GlkApi);
            sess.Run();

            return 0;
        }
    }
}