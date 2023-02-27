﻿using FrankenDrift.GlkRunner.Glk;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FrankenDrift.GlkRunner.Gargoyle
{
    static class Garglk_Pinvoke
    {
        // Universal Glk functions.
        [DllImport("libgarglk")]
        internal static extern BlorbError giblorb_set_resource_map(StreamHandle fileStream);
        [DllImport("libgarglk")]
        internal static extern void glk_cancel_hyperlink_event(WindowHandle winId);
        [DllImport("libgarglk")]
        internal static extern void glk_cancel_line_event(WindowHandle winId, ref Event ev);
        [DllImport("libgarglk")]
        internal static extern void glk_exit();
        [DllImport("libgarglk")]
        internal static extern FileRefHandle glk_fileref_create_by_prompt(FileUsage usage, Glk.FileMode fmode, uint rock);
        [DllImport("libgarglk")]
        internal static extern FileRefHandle glk_fileref_create_temp(FileUsage usage, uint rock);
        [DllImport("libgarglk")]
        internal static extern void glk_fileref_destroy(FileRefHandle fref);
        [DllImport("libgarglk")]
        internal static extern uint glk_image_draw(WindowHandle winid, uint imageId, int val1, int val2);
        [DllImport("libgarglk")]
        internal static extern uint glk_image_get_info(uint imageId, ref uint width, ref uint height);
        [DllImport("libgarglk")]
        internal static extern void glk_put_buffer([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] s, uint len);
        [DllImport("libgarglk")]
        internal static extern void glk_put_buffer_stream(StreamHandle streamId, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] s, uint len);
        [DllImport("libgarglk")]
        internal static extern void glk_put_buffer_uni([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] uint[] s, uint len);
        [DllImport("libgarglk")]
        internal static extern void glk_request_char_event(WindowHandle winId);
        [DllImport("libgarglk")]
        internal static extern void glk_request_hyperlink_event(WindowHandle winId);
        [DllImport("libgarglk")]
        internal static extern void glk_request_line_event_uni(WindowHandle win, Memory<UInt32> buf, uint maxlen, uint initlen);
        [DllImport("libgarglk")]
        internal static extern SoundChannel glk_schannel_create(uint rock);
        [DllImport("libgarglk")]
        internal static extern void glk_schannel_pause(SoundChannel chan);
        [DllImport("libgarglk")]
        internal static extern uint glk_schannel_play_ext(SoundChannel chan, uint sndId, uint repeats, uint notify);
        [DllImport("libgarglk")]
        internal static extern void glk_schannel_stop(SoundChannel chan);
        [DllImport("libgarglk")]
        internal static extern void glk_schannel_unpause(SoundChannel chan);
        [DllImport("libgarglk")]
        internal static extern void glk_select(ref Event ev);
        [DllImport("libgarglk")]
        internal static extern void glk_set_hyperlink(uint linkval);
        [DllImport("libgarglk")]
        internal static extern void glk_set_style(Style s);
        [DllImport("libgarglk")]
        internal static extern void glk_set_window(WindowHandle winId);
        [DllImport("libgarglk")]
        internal static extern StreamHandle glk_stream_open_file(FileRefHandle fileref, Glk.FileMode fmode, uint rock);
        [DllImport("libgarglk")]
        internal static extern void glk_stream_set_position(StreamHandle stream, int pos, SeekMode seekMode);
        [DllImport("libgarglk")]
        internal static extern void glk_stylehint_set(WinType wintype, Style styl, StyleHint hint, int val);
        [DllImport("libgarglk")]
        internal static extern uint glk_style_measure(WindowHandle winid, Style styl, StyleHint hint, ref uint result);
        [DllImport("libgarglk")]
        internal static extern void glk_tick();
        [DllImport("libgarglk")]
        internal static extern void glk_window_clear(WindowHandle winId);
        [DllImport("libgarglk")]
        internal static extern void glk_window_close(WindowHandle winId, ref StreamResult streamResult);
        [DllImport("libgarglk")]
        internal static extern void glk_window_flow_break(WindowHandle winId);
        [DllImport("libgarglk")]
        internal static extern void glk_window_get_size(WindowHandle winId, out uint width, out uint height);
        [DllImport("libgarglk")]
        internal static extern StreamHandle glk_window_get_stream(WindowHandle winId);
        [DllImport("libgarglk")]
        internal static extern void glk_window_move_cursor(WindowHandle winId, uint xpos, uint ypos);
        [DllImport("libgarglk")]
        internal static extern WindowHandle glk_window_open(WindowHandle split, WinMethod method, uint size, WinType wintype, uint rock);
        [DllImport("libgarglk")]
        internal static extern void garglk_set_zcolors(uint fg, uint bg);
        [DllImport("libgarglk")]
        internal static extern IntPtr glkunix_fileref_get_name(FileRefHandle fileref);

        // Garglk initialization functions.
        [DllImport("libgarglk")]
        internal static extern void garglk_set_program_name([MarshalAs(UnmanagedType.LPStr)] string name);
        [DllImport("libgarglk")]
        internal static extern void garglk_set_story_name([MarshalAs(UnmanagedType.LPStr)] string name);
        [DllImport("libgarglk")]
        internal static extern void gli_startup(int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] argv);
        [DllImport("libgarglk")]
        internal static extern void garglk_startup(int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] argv);
        [DllImport("libgarglk", EntryPoint = "_Z11gli_startupiPPc")]
        internal static extern void gli_startup_m_(int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] argv);
    }

    class GarGlk: IGlk
    {
        public BlorbError giblorb_set_resource_map(StreamHandle fileStream) => Garglk_Pinvoke.giblorb_set_resource_map(fileStream);
        public void glk_cancel_hyperlink_event(WindowHandle winId) => Garglk_Pinvoke.glk_cancel_hyperlink_event(winId);
        public void glk_cancel_line_event(WindowHandle winId, ref Event ev) => Garglk_Pinvoke.glk_cancel_line_event(winId, ref ev);
        public void glk_exit() => Garglk_Pinvoke.glk_exit();
        public FileRefHandle glk_fileref_create_by_prompt(FileUsage usage, Glk.FileMode fmode, uint rock) => Garglk_Pinvoke.glk_fileref_create_by_prompt(usage, fmode, rock);
        public FileRefHandle glk_fileref_create_temp(FileUsage usage, uint rock) => Garglk_Pinvoke.glk_fileref_create_temp(usage, rock);
        public void glk_fileref_destroy(FileRefHandle fref) => Garglk_Pinvoke.glk_fileref_destroy(fref);
        public uint glk_image_draw(WindowHandle winid, uint imageId, int val1, int val2) => Garglk_Pinvoke.glk_image_draw(winid, imageId, val1, val2);
        public uint glk_image_get_info(uint imageId, ref uint width, ref uint height) => Garglk_Pinvoke.glk_image_get_info(imageId, ref width, ref height);
        public void glk_put_buffer(byte[] s, uint len) => Garglk_Pinvoke.glk_put_buffer(s, len);
        public void glk_put_buffer_stream(StreamHandle streamId, byte[] s, uint len) => Garglk_Pinvoke.glk_put_buffer_stream(streamId, s, len);
        public void glk_put_buffer_uni(uint[] s, uint len) => Garglk_Pinvoke.glk_put_buffer_uni(s, len);
        public void glk_request_char_event(WindowHandle winId) => Garglk_Pinvoke.glk_request_char_event(winId);
        public void glk_request_hyperlink_event(WindowHandle winId) => Garglk_Pinvoke.glk_request_hyperlink_event(winId);
        public void glk_request_line_event_uni(WindowHandle win, Memory<UInt32> buf, uint maxlen, uint initlen) => Garglk_Pinvoke.glk_request_line_event_uni(win, buf, maxlen, initlen);
        public SoundChannel glk_schannel_create(uint rock) => Garglk_Pinvoke.glk_schannel_create(rock);
        public void glk_schannel_pause(SoundChannel chan) => Garglk_Pinvoke.glk_schannel_pause(chan);
        public uint glk_schannel_play_ext(SoundChannel chan, uint sndId, uint repeats, uint notify) => Garglk_Pinvoke.glk_schannel_play_ext(chan, sndId, repeats, notify);
        public void glk_schannel_stop(SoundChannel chan) => Garglk_Pinvoke.glk_schannel_stop(chan);
        public void glk_schannel_unpause(SoundChannel chan) => Garglk_Pinvoke.glk_schannel_unpause(chan);
        public void glk_select(ref Event ev) => Garglk_Pinvoke.glk_select(ref ev);
        public void glk_set_hyperlink(uint linkval) => Garglk_Pinvoke.glk_set_hyperlink(linkval);
        public void glk_set_style(Style s) => Garglk_Pinvoke.glk_set_style(s);
        public void glk_set_window(WindowHandle winId) => Garglk_Pinvoke.glk_set_window(winId);
        public StreamHandle glk_stream_open_file(FileRefHandle fileref, Glk.FileMode fmode, uint rock) => Garglk_Pinvoke.glk_stream_open_file(fileref, fmode, rock);
        public void glk_stream_set_position(StreamHandle stream, int pos, SeekMode seekMode) => Garglk_Pinvoke.glk_stream_set_position(stream, pos, seekMode);
        public void glk_stylehint_set(WinType wintype, Style styl, StyleHint hint, int val) => Garglk_Pinvoke.glk_stylehint_set(wintype, styl, hint, val);
        public uint glk_style_measure(WindowHandle winid, Style styl, StyleHint hint, ref uint result) => Garglk_Pinvoke.glk_style_measure(winid, styl, hint, ref result);
        public void glk_tick() => Garglk_Pinvoke.glk_tick();
        public void glk_window_clear(WindowHandle winId) => Garglk_Pinvoke.glk_window_clear(winId);
        public void glk_window_close(WindowHandle winId, ref StreamResult streamResult) => Garglk_Pinvoke.glk_window_close(winId, ref streamResult);
        public void glk_window_flow_break(WindowHandle winId) => Garglk_Pinvoke.glk_window_flow_break(winId);
        public void glk_window_get_size(WindowHandle winId, out uint width, out uint height) => Garglk_Pinvoke.glk_window_get_size(winId, out width, out height);
        public StreamHandle glk_window_get_stream(WindowHandle winId) => Garglk_Pinvoke.glk_window_get_stream(winId);
        public void glk_window_move_cursor(WindowHandle winId, uint xpos, uint ypos) => Garglk_Pinvoke.glk_window_move_cursor(winId, xpos, ypos);
        public WindowHandle glk_window_open(WindowHandle split, WinMethod method, uint size, WinType wintype, uint rock) => Garglk_Pinvoke.glk_window_open(split, method, size, wintype, rock);
        public void garglk_set_zcolors(uint fg, uint bg) => Garglk_Pinvoke.garglk_set_zcolors(fg, bg);
        public string? glkunix_fileref_get_name(FileRefHandle fileref) => Marshal.PtrToStringAnsi(Garglk_Pinvoke.glkunix_fileref_get_name(fileref));

        public void SetGameName(string game) => Garglk_Pinvoke.garglk_set_story_name(game);

        internal void GarglkInit(string[] argv)
        {
            // Depending on the Garglk library version, the startup function could have
            // a number of names. We need to try them all.
            try  // First attempt.
            {
                Garglk_Pinvoke.garglk_startup(argv.Length, argv);
                return;
            } catch (EntryPointNotFoundException) { }  // do nothing

            try  // Second attempt.
            {
                Garglk_Pinvoke.gli_startup_m_(argv.Length, argv);
                return;
            } catch (EntryPointNotFoundException) { }  // do nothing

            // Last attempt.
            // This must succeed, or the Garglk library would go uninitialized,
            // so we let the program crash and burn if this one fails as well.
            Garglk_Pinvoke.gli_startup(argv.Length, argv);
        }
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
            Startup(args, GlkApi);

            var sess = new MainSession(args[^1], GlkApi);
            sess.Run();

            return 0;
        }

        private static void Startup(string[] args, GarGlk api)
        {
            string[] argv = new string[args.Length + 1];
            argv[0] = Assembly.GetEntryAssembly().Location;
            for (int i = 1; i <= args.Length; i++)
            {
                argv[i] = args[i - 1];
            }
            api.GarglkInit(argv);
            Garglk_Pinvoke.garglk_set_program_name("FrankenDrift for Gargoyle");
        }
    }
}