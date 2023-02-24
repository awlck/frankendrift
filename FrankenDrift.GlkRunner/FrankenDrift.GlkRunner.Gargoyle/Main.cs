using FrankenDrift.GlkRunner.Glk;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FrankenDrift.GlkRunner.Gargoyle
{
    static class Garglk_Pinvoke
    {
        // Universal Glk functions.
        [DllImport("libgarglk")]
        internal static extern BlorbError giblorb_set_resource_map(IntPtr fileStream);
        [DllImport("libgarglk")]
        internal static extern void glk_cancel_hyperlink_event(IntPtr winId);
        [DllImport("libgarglk")]
        internal static extern void glk_cancel_line_event(IntPtr winId, ref Event ev);
        [DllImport("libgarglk")]
        internal static extern void glk_exit();
        [DllImport("libgarglk")]
        internal static extern IntPtr glk_fileref_create_by_name(FileUsage usage, Glk.FileMode fmode, uint rock);
        [DllImport("libgarglk")]
        internal static extern IntPtr glk_fileref_create_by_prompt(FileUsage usage, Glk.FileMode fmode, uint rock);
        [DllImport("libgarglk")]
        internal static extern IntPtr glk_fileref_create_temp(FileUsage usage, uint rock);
        [DllImport("libgarglk")]
        internal static extern void glk_fileref_destroy(IntPtr fref);
        [DllImport("libgarglk")]
        internal static extern uint glk_image_draw(IntPtr winid, uint imageId, int val1, int val2);
        [DllImport("libgarglk")]
        internal static extern uint glk_image_get_info(uint imageId, ref uint width, ref uint height);
        [DllImport("libgarglk")]
        internal static extern void glk_put_buffer([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] s, uint len);
        [DllImport("libgarglk")]
        internal static extern void glk_put_buffer_stream(IntPtr streamId, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] s, uint len);
        [DllImport("libgarglk")]
        internal static extern void glk_put_buffer_uni([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] uint[] s, uint len);
        [DllImport("libgarglk")]
        internal static extern void glk_request_char_event(IntPtr winId);
        [DllImport("libgarglk")]
        internal static extern void glk_request_hyperlink_event(IntPtr winId);
        [DllImport("libgarglk")]
        internal static extern unsafe void glk_request_line_event(IntPtr win, byte* buf, uint maxlen, uint initlen);
        [DllImport("libgarglk")]
        internal static extern unsafe void glk_request_line_event_uni(IntPtr win, uint* buf, uint maxlen, uint initlen);
        [DllImport("libgarglk")]
        internal static extern IntPtr glk_schannel_create(uint rock);
        [DllImport("libgarglk")]
        internal static extern void glk_schannel_destroy(IntPtr chan);
        [DllImport("libgarglk")]
        internal static extern void glk_schannel_pause(IntPtr chan);
        [DllImport("libgarglk")]
        internal static extern uint glk_schannel_play(IntPtr chan, uint sndId);
        [DllImport("libgarglk")]
        internal static extern uint glk_schannel_play_ext(IntPtr chan, uint sndId, uint repeats, uint notify);
        [DllImport("libgarglk")]
        internal static extern void glk_schannel_set_volume(IntPtr chan, uint vol);
        [DllImport("libgarglk")]
        internal static extern void glk_schannel_stop(IntPtr chan);
        [DllImport("libgarglk")]
        internal static extern void glk_schannel_unpause(IntPtr chan);
        [DllImport("libgarglk")]
        internal static extern void glk_select(ref Event ev);
        [DllImport("libgarglk")]
        internal static extern void glk_set_hyperlink(uint linkval);
        [DllImport("libgarglk")]
        internal static extern void glk_set_style(Style s);
        [DllImport("libgarglk")]
        internal static extern void glk_set_window(IntPtr winId);
        [DllImport("libgarglk")]
        internal static extern IntPtr glk_stream_open_file(IntPtr fileref, Glk.FileMode fmode, uint rock);
        [DllImport("libgarglk")]
        internal static extern IntPtr glk_stream_open_memory(IntPtr buf, uint buflen, Glk.FileMode mode, uint rock);
        [DllImport("libgarglk")]
        internal static extern void glk_stream_set_position(IntPtr stream, int pos, SeekMode seekMode);
        [DllImport("libgarglk")]
        internal static extern void glk_stylehint_set(WinType wintype, Style styl, StyleHint hint, int val);
        [DllImport("libgarglk")]
        internal static extern uint glk_style_measure(IntPtr winid, Style styl, StyleHint hint, ref uint result);
        [DllImport("libgarglk")]
        internal static extern void glk_tick();
        [DllImport("libgarglk")]
        internal static extern void glk_window_clear(IntPtr winId);
        [DllImport("libgarglk")]
        internal static extern void glk_window_close(IntPtr winId, IntPtr streamResult);
        [DllImport("libgarglk")]
        internal static extern void glk_window_flow_break(IntPtr winId);
        [DllImport("libgarglk")]
        internal static extern void glk_window_get_size(IntPtr winId, out uint width, out uint height);
        [DllImport("libgarglk")]
        internal static extern IntPtr glk_window_get_stream(IntPtr winId);
        [DllImport("libgarglk")]
        internal static extern void glk_window_move_cursor(IntPtr winId, uint xpos, uint ypos);
        [DllImport("libgarglk")]
        internal static extern IntPtr glk_window_open(IntPtr split, WinMethod method, uint size, WinType wintype, uint rock);
        [DllImport("libgarglk")]
        internal static extern void garglk_set_zcolors(uint fg, uint bg);
        [DllImport("libgarglk")]
        internal static extern IntPtr glkunix_fileref_get_name(IntPtr fileref);

        // Garglk initialization functions.
        [DllImport("libgarglk")]
        internal static extern void garglk_set_program_name([MarshalAs(UnmanagedType.LPStr)] string name);
        [DllImport("libgarglk")]
        internal static extern void garglk_set_story_name([MarshalAs(UnmanagedType.LPStr)] string name);
        [DllImport("libgarglk")]
        internal static extern void gli_startup(int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] argv);
    }

    class GarGlk: IGlk
    {
        public BlorbError giblorb_set_resource_map(IntPtr fileStream) => Garglk_Pinvoke.giblorb_set_resource_map(fileStream);
        public void glk_cancel_hyperlink_event(IntPtr winId) => Garglk_Pinvoke.glk_cancel_hyperlink_event(winId);
        public void glk_cancel_line_event(IntPtr winId, ref Event ev) => Garglk_Pinvoke.glk_cancel_line_event(winId, ref ev);
        public void glk_exit() => Garglk_Pinvoke.glk_exit();
        public IntPtr glk_fileref_create_by_name(FileUsage usage, Glk.FileMode fmode, uint rock) => Garglk_Pinvoke.glk_fileref_create_by_name(usage, fmode, rock);
        public IntPtr glk_fileref_create_by_prompt(FileUsage usage, Glk.FileMode fmode, uint rock) => Garglk_Pinvoke.glk_fileref_create_by_prompt(usage, fmode, rock);
        public IntPtr glk_fileref_create_temp(FileUsage usage, uint rock) => Garglk_Pinvoke.glk_fileref_create_temp(usage, rock);
        public void glk_fileref_destroy(IntPtr fref) => Garglk_Pinvoke.glk_fileref_destroy(fref);
        public uint glk_image_draw(IntPtr winid, uint imageId, int val1, int val2) => Garglk_Pinvoke.glk_image_draw(winid, imageId, val1, val2);
        public uint glk_image_get_info(uint imageId, ref uint width, ref uint height) => Garglk_Pinvoke.glk_image_get_info(imageId, ref width, ref height);
        public void glk_put_buffer([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] s, uint len) => Garglk_Pinvoke.glk_put_buffer(s, len);
        public void glk_put_buffer_stream(IntPtr streamId, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] s, uint len) => Garglk_Pinvoke.glk_put_buffer_stream(streamId, s, len);
        public void glk_put_buffer_uni([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] uint[] s, uint len) => Garglk_Pinvoke.glk_put_buffer_uni(s, len);
        public void glk_request_char_event(IntPtr winId) => Garglk_Pinvoke.glk_request_char_event(winId);
        public void glk_request_hyperlink_event(IntPtr winId) => Garglk_Pinvoke.glk_request_hyperlink_event(winId);
        public unsafe void glk_request_line_event(IntPtr win, byte* buf, uint maxlen, uint initlen) => Garglk_Pinvoke.glk_request_line_event(win, buf, maxlen, initlen);
        public unsafe void glk_request_line_event_uni(IntPtr win, uint* buf, uint maxlen, uint initlen) => Garglk_Pinvoke.glk_request_line_event_uni(win, buf, maxlen, initlen);
        public IntPtr glk_schannel_create(uint rock) => Garglk_Pinvoke.glk_schannel_create(rock);
        public void glk_schannel_destroy(IntPtr chan) => Garglk_Pinvoke.glk_schannel_destroy(chan);
        public void glk_schannel_pause(IntPtr chan) => Garglk_Pinvoke.glk_schannel_pause(chan);
        public uint glk_schannel_play(IntPtr chan, uint sndId) => Garglk_Pinvoke.glk_schannel_play(chan, sndId);
        public uint glk_schannel_play_ext(IntPtr chan, uint sndId, uint repeats, uint notify) => Garglk_Pinvoke.glk_schannel_play_ext(chan, sndId, repeats, notify);
        public void glk_schannel_set_volume(IntPtr chan, uint vol) => Garglk_Pinvoke.glk_schannel_set_volume(chan, vol);
        public void glk_schannel_stop(IntPtr chan) => Garglk_Pinvoke.glk_schannel_stop(chan);
        public void glk_schannel_unpause(IntPtr chan) => Garglk_Pinvoke.glk_schannel_unpause(chan);
        public void glk_select(ref Event ev) => Garglk_Pinvoke.glk_select(ref ev);
        public void glk_set_hyperlink(uint linkval) => Garglk_Pinvoke.glk_set_hyperlink(linkval);
        public void glk_set_style(Style s) => Garglk_Pinvoke.glk_set_style(s);
        public void glk_set_window(IntPtr winId) => Garglk_Pinvoke.glk_set_window(winId);
        public IntPtr glk_stream_open_file(IntPtr fileref, Glk.FileMode fmode, uint rock) => Garglk_Pinvoke.glk_stream_open_file(fileref, fmode, rock);
        public IntPtr glk_stream_open_memory(IntPtr buf, uint buflen, Glk.FileMode mode, uint rock) => Garglk_Pinvoke.glk_stream_open_memory(buf, buflen, mode, rock);
        public void glk_stream_set_position(IntPtr stream, int pos, SeekMode seekMode) => Garglk_Pinvoke.glk_stream_set_position(stream, pos, seekMode);
        public void glk_stylehint_set(WinType wintype, Style styl, StyleHint hint, int val) => Garglk_Pinvoke.glk_stylehint_set(wintype, styl, hint, val);
        public uint glk_style_measure(IntPtr winid, Style styl, StyleHint hint, ref uint result) => Garglk_Pinvoke.glk_style_measure(winid, styl, hint, ref result);
        public void glk_tick() => Garglk_Pinvoke.glk_tick();
        public void glk_window_clear(IntPtr winId) => Garglk_Pinvoke.glk_window_clear(winId);
        public void glk_window_close(IntPtr winId, IntPtr streamResult) => Garglk_Pinvoke.glk_window_close(winId, streamResult);
        public void glk_window_flow_break(IntPtr winId) => Garglk_Pinvoke.glk_window_flow_break(winId);
        public void glk_window_get_size(IntPtr winId, out uint width, out uint height) => Garglk_Pinvoke.glk_window_get_size(winId, out width, out height);
        public IntPtr glk_window_get_stream(IntPtr winId) => Garglk_Pinvoke.glk_window_get_stream(winId);
        public void glk_window_move_cursor(IntPtr winId, uint xpos, uint ypos) => Garglk_Pinvoke.glk_window_move_cursor(winId, xpos, ypos);
        public IntPtr glk_window_open(IntPtr split, WinMethod method, uint size, WinType wintype, uint rock) => Garglk_Pinvoke.glk_window_open(split, method, size, wintype, rock);
        public void garglk_set_zcolors(uint fg, uint bg) => Garglk_Pinvoke.garglk_set_zcolors(fg, bg);
        public IntPtr glkunix_fileref_get_name(IntPtr fileref) => Garglk_Pinvoke.glkunix_fileref_get_name(fileref);

        public void SetGameName(string game) => Garglk_Pinvoke.garglk_set_story_name(game);
    }

    public class GarGlkRunner
    {
        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: No file selected!");
                return 1;
            }

            GarGlk GlkApi = new GarGlk();
            Startup(args);

            var sess = new MainSession(args[^1], GlkApi);
            sess.Run();

            return 0;
        }

        private static void Startup(string[] args)
        {
            string[] argv = new string[args.Length + 1];
            argv[0] = Assembly.GetEntryAssembly().Location;
            for (int i = 1; i <= args.Length; i++)
            {
                argv[i] = args[i - 1];
            }
            Garglk_Pinvoke.gli_startup(argv.Length, argv);
            Garglk_Pinvoke.garglk_set_program_name("FrankenDrift for Gargoyle");
        }
    }
}