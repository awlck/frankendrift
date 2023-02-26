using FrankenDrift.GlkRunner.Glk;
using System.Runtime.InteropServices.JavaScript;

namespace FrankenDrift.GlkRunner.AsyncGlk
{
    internal static partial class AsyncGlk_imports
    {
        [JSImport("get_val", "main.js")]
        internal static partial int get_val(int val);
        [JSImport("glk_cancel_hyperlink_event", "main.js")]
        internal static partial void glk_cancel_hyperlink_event(IntPtr winId);
        [JSImport("glk_cancel_line_event", "main.js")]
        internal static partial void glk_cancel_line_event(IntPtr winId);
        [JSImport("glk_exit", "main.js")]
        internal static partial void glk_exit();
        [JSImport("glk_fileref_create_by_name", "main.js")]
        internal static partial IntPtr glk_fileref_create_by_name(int usage, int fmode, int rock);
        [JSImport("glk_fileref_create_by_prompt", "main.js")]
        internal static partial Task<IntPtr> glk_fileref_create_by_prompt(int usage, int fmode, int rock);
        [JSImport("glk_fileref_create_temp", "main.js")]
        internal static partial IntPtr glk_fileref_create_temp(int usage, int rock);
        [JSImport("glk_fileref_destroy", "main.js")]
        internal static partial void glk_fileref_destroy(IntPtr fref);
        [JSImport("glk_image_draw", "main.js")]
        internal static partial int glk_image_draw(IntPtr winid, int imageId, int val1, int val2);
        [JSImport("glk_image_get_info", "main.js")]
        internal static partial int glk_image_get_info(int imageId);
        [JSImport("glk_put_buffer", "main.js")]
        internal static partial void glk_put_buffer([JSMarshalAs<JSType.MemoryView>] Span<Byte> s);
        [JSImport("glk_put_buffer_stream", "main.js")]
        internal static partial void glk_put_buffer_stream(IntPtr streamId, [JSMarshalAs<JSType.MemoryView>] Span<Byte> s);
        [JSImport("glk_put_buffer_uni", "main.js")]
        internal static partial void glk_put_buffer_uni([JSMarshalAs<JSType.MemoryView>] Span<Int32> s);
        [JSImport("glk_request_char_event", "main.js")]
        internal static partial void glk_request_char_event(IntPtr winId);
        [JSImport("glk_request_hyperlink_event", "main.js")]
        internal static partial void glk_request_hyperlink_event(IntPtr winId);
        [JSImport("glk_request_line_event", "main.js")]
        internal static partial void glk_request_line_event(IntPtr win, [JSMarshalAs<JSType.MemoryView>] ArraySegment<Byte> buf, int initlen);
        [JSImport("glk_request_line_event_uni", "main.js")]
        internal static partial void glk_request_line_event_uni(IntPtr win, [JSMarshalAs<JSType.MemoryView>] ArraySegment<Int32> buf, int initlen);
        [JSImport("glk_select", "main.js")]
        internal static partial Task glk_select();
        [JSImport("glk_set_hyperlink", "main.js")]
        internal static partial void glk_set_hyperlink(int linkval);
        [JSImport("glk_set_style", "main.js")]
        internal static partial void glk_set_style(int s);
        [JSImport("glk_set_window", "main.js")]
        internal static partial void glk_set_window(IntPtr winId);
        [JSImport("glk_stream_open_file", "main.js")]
        internal static partial IntPtr glk_stream_open_file(IntPtr fileref, int fmode, int rock);
        [JSImport("glk_stream_open_memory", "main.js")]
        internal static partial IntPtr glk_stream_open_memory([JSMarshalAs<JSType.MemoryView>] ArraySegment<Byte> buf, int mode, int rock);
        [JSImport("glk_stream_set_position", "main.js")]
        internal static partial void glk_stream_set_position(IntPtr stream, int pos, int seekMode);
        [JSImport("glk_stylehint_set", "main.js")]
        internal static partial void glk_stylehint_set(int wintype, int style, int hint, int val);
        [JSImport("glk_tick", "main.js")]
        internal static partial void glk_tick();
        [JSImport("glk_window_clear", "main.js")]
        internal static partial void glk_window_clear(IntPtr winId);
        [JSImport("glk_window_close", "main.js")]
        internal static partial void glk_window_close(IntPtr winId, IntPtr streamResult);
        [JSImport("glk_window_flow_break", "main.js")]
        internal static partial void glk_window_flow_break(IntPtr winId);
        [JSImport("glk_window_get_size", "main.js")]
        internal static partial void glk_window_get_size(IntPtr winId);
        [JSImport("glk_window_get_stream", "main.js")]
        internal static partial IntPtr glk_window_get_stream(IntPtr winId);
        [JSImport("glk_window_move_cursor", "main.js")]
        internal static partial void glk_window_move_cursor(IntPtr winId, int xpos, int ypos);
        [JSImport("glk_window_open", "main.js")]
        internal static partial IntPtr glk_window_open(IntPtr split, int method, int size, int wintype, int rock);
        [JSImport("garglk_set_zcolors", "main.js")]
        internal static partial void garglk_set_zcolors(int fg, int bg);
        [JSImport("glkunix_fileref_get_name", "main.js")]
        internal static partial IntPtr glkunix_fileref_get_name(IntPtr fileref);
    }

    class AsyncGlk : IGlk
    {
        private void fill_event(ref Event ev)
        {
            ev.type = (EventType) AsyncGlk_imports.get_val(0);
            ev.win_handle = AsyncGlk_imports.get_val(1);
            ev.val1 = (uint) AsyncGlk_imports.get_val(2);
            ev.val2 = (uint) AsyncGlk_imports.get_val(3);
        }

        public BlorbError giblorb_set_resource_map(IntPtr fileStream) => BlorbError.None;
        public void glk_cancel_hyperlink_event(IntPtr winId) => AsyncGlk_imports.glk_cancel_hyperlink_event(winId);
        public void glk_cancel_line_event(IntPtr winId, ref Event ev)
        {
            AsyncGlk_imports.glk_cancel_line_event(winId);
            fill_event(ref ev);
        }
        public void glk_exit() => AsyncGlk_imports.glk_exit();
        public IntPtr glk_fileref_create_by_name(FileUsage usage, Glk.FileMode fmode, uint rock) => AsyncGlk_imports.glk_fileref_create_by_name((int) usage, (int) fmode, (int) rock);
        public IntPtr glk_fileref_create_by_prompt(FileUsage usage, Glk.FileMode fmode, uint rock)
        {
            Task<IntPtr> t = AsyncGlk_imports.glk_fileref_create_by_prompt((int) usage, (int) fmode, (int) rock);
            t.Wait(-1);
            return t.Result;
        }
        public IntPtr glk_fileref_create_temp(FileUsage usage, uint rock) => AsyncGlk_imports.glk_fileref_create_temp((int) usage, (int) rock);
        public void glk_fileref_destroy(IntPtr fref) => AsyncGlk_imports.glk_fileref_destroy(fref);
        public uint glk_image_draw(IntPtr winid, uint imageId, int val1, int val2) => (uint) AsyncGlk_imports.glk_image_draw(winid, (int) imageId, (int) val1, (int) val2);
        public uint glk_image_get_info(uint imageId, ref uint width, ref uint height)
        {
            uint res = (uint) AsyncGlk_imports.glk_image_get_info((int) imageId);
            width = (uint) AsyncGlk_imports.get_val(0);
            height = (uint) AsyncGlk_imports.get_val(1);
            return res;
        }
        public void glk_put_buffer(byte[] s, uint len) => AsyncGlk_imports.glk_put_buffer(new Span<Byte>(s, 0, (int) len));
        public void glk_put_buffer_stream(IntPtr streamId, byte[] s, uint len) => AsyncGlk_imports.glk_put_buffer_stream(streamId, new Span<Byte>(s, 0, (int) len));
        public void glk_put_buffer_uni(uint[] s, uint len) => AsyncGlk_imports.glk_put_buffer_uni(new Span<Int32>((int[])(object) s, 0, (int) len));
        public void glk_request_char_event(IntPtr winId) => AsyncGlk_imports.glk_request_char_event(winId);
        public void glk_request_hyperlink_event(IntPtr winId) => AsyncGlk_imports.glk_request_hyperlink_event(winId);
        public unsafe void glk_request_line_event(IntPtr win, byte* buf, uint maxlen, uint initlen) => AsyncGlk_imports.glk_request_line_event(win, buf, maxlen, (int) initlen);
        public unsafe void glk_request_line_event_uni(IntPtr win, uint* buf, uint maxlen, uint initlen) => AsyncGlk_imports.glk_request_line_event_uni(win, buf, maxlen, (int) initlen);
        public IntPtr glk_schannel_create(uint rock) => 0;
        public void glk_schannel_destroy(IntPtr chan) {}
        public void glk_schannel_pause(IntPtr chan) {}
        public uint glk_schannel_play(IntPtr chan, uint sndId) => 0;
        public uint glk_schannel_play_ext(IntPtr chan, uint sndId, uint repeats, uint notify) => 0;
        public void glk_schannel_set_volume(IntPtr chan, uint vol) {}
        public void glk_schannel_stop(IntPtr chan) {}
        public void glk_schannel_unpause(IntPtr chan) {}
        public void glk_select(ref Event ev)
        {
            Task t = AsyncGlk_imports.glk_select();
            t.Wait(-1);
            fill_event(ref ev);
        }
        public void glk_set_hyperlink(uint linkval) => AsyncGlk_imports.glk_set_hyperlink((int) linkval);
        public void glk_set_style(Style s) => AsyncGlk_imports.glk_set_style((int) s);
        public void glk_set_window(IntPtr winId) => AsyncGlk_imports.glk_set_window(winId);
        public IntPtr glk_stream_open_file(IntPtr fileref, Glk.FileMode fmode, uint rock) => AsyncGlk_imports.glk_stream_open_file(fileref, (int) fmode, (int) rock);
        public IntPtr glk_stream_open_memory(IntPtr buf, uint buflen, Glk.FileMode mode, uint rock) => 0;
        public void glk_stream_set_position(IntPtr stream, int pos, SeekMode seekMode) => AsyncGlk_imports.glk_stream_set_position(stream, (int) pos, (int) seekMode);
        public void glk_stylehint_set(WinType wintype, Style styl, StyleHint hint, int val) => AsyncGlk_imports.glk_stylehint_set((int) wintype, (int) styl, (int) hint, (int) val);
        public uint glk_style_measure(IntPtr winid, Style styl, StyleHint hint, ref uint result) => 0;
        public void glk_tick() => AsyncGlk_imports.glk_tick();
        public void glk_window_clear(IntPtr winId) => AsyncGlk_imports.glk_window_clear(winId);
        public void glk_window_close(IntPtr winId, IntPtr streamResult) => AsyncGlk_imports.glk_window_close(winId, streamResult);
        public void glk_window_flow_break(IntPtr winId) => AsyncGlk_imports.glk_window_flow_break(winId);
        public void glk_window_get_size(IntPtr winId, out uint width, out uint height)
        {
            AsyncGlk_imports.glk_window_get_size(winId);
            width = (uint) AsyncGlk_imports.get_val(0);
            height = (uint) AsyncGlk_imports.get_val(1);
        }
        public IntPtr glk_window_get_stream(IntPtr winId) => AsyncGlk_imports.glk_window_get_stream(winId);
        public void glk_window_move_cursor(IntPtr winId, uint xpos, uint ypos) => AsyncGlk_imports.glk_window_move_cursor(winId, (int) xpos, (int) ypos);
        public IntPtr glk_window_open(IntPtr split, WinMethod method, uint size, WinType wintype, uint rock) => AsyncGlk_imports.glk_window_open(split, (int) method, (int) size, (int) wintype, (int) rock);
        public void garglk_set_zcolors(uint fg, uint bg) => AsyncGlk_imports.garglk_set_zcolors((int) fg, (int) bg);
        public IntPtr glkunix_fileref_get_name(IntPtr fileref) => AsyncGlk_imports.glkunix_fileref_get_name(fileref);

        public void SetGameName(string game) {}
    }

    public class AsyncGlkRunner
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: No file selected!");
                return 1;
            }

            return 0;
        }
    }
}