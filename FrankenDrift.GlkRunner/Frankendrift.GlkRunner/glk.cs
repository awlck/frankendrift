using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;

namespace FrankenDrift.GlkRunner
{
    public class GlkError : Exception
    {
        public GlkError(string what) : base(what) { }
    }
}

namespace FrankenDrift.GlkRunner.Glk
{
    enum Gestalt : uint
    {
        Version = 0,
        CharInput = 1,
        LineInput = 2,
        CharOutput = 3,
        MouseInput = 4,
        Timer = 5,
        Graphics = 6,
        DrawImage = 7,
        Sound = 9,
        SoundVolume = 10,
        Hyperlinks = 11,
        HyperlinkInput = 12,
        SoundMusic = 13,
        GraphicsTransparency = 14,
        Unicode = 15,
        UnicodeNorm = 16,
        LineInputEcho = 17,
        LineTerminators = 18,
        LineTerminatorKey = 19,
        DateTime = 20,
        Sound2 = 21,
        ResourceStream = 22,
        GraphicsCharInput = 23
    }

    enum CharOutput : uint
    {
        CannotPrint = 0,
        ApproxPrint = 1,
        ExactPrint = 2
    }

    enum EventType : uint
    {
        None = 0,
        Timer = 1,
        CharInput = 2,
        LineInput = 3,
        MouseInput = 4,
        Arrange = 5,
        Redraw = 6,
        SoundNotify = 7,
        Hyperlink = 8,
        VolumeNotify = 9
    }

    enum Style : uint
    {
        Normal = 0,
        Emphasized = 1,
        Preformatted = 2,
        Header = 3,
        Subheader = 4,
        Alert = 5,
        Note = 6,
        BlockQuote = 7,
        Input = 8,
        User1 = 9,
        User2 = 10
    }

    enum WinType : uint
    {
        AllTypes = 0,
        Pair = 1,
        Blank = 2,
        TextBuffer = 3,
        TextGrid = 4,
        Graphics = 5
    }

    [Flags]
    enum WinMethod : uint
    {
        Left = 0x00,
        Right = 0x01,
        Above = 0x02,
        Below = 0x03,
        DirMask = 0x0f,
        Fixed = 0x10,
        Proportional = 0x20,
        DivisionMask = 0xf0,
        Border = 0x000,
        NoBorder = 0x100,
        BorderMask = 0x100
    }

    [Flags]
    enum FileUsage : uint
    {
        Data = 0x00,
        SavedGame = 0x01,
        Transcript = 0x02,
        InputRecord = 0x03,
        TypeMask = 0x0f,
        TextMode = 0x100,
        BinaryMode = 0x000
    }

    enum FileMode : uint
    {
        Write = 0x01,
        Read = 0x02,
        ReadWrite = 0x03,
        WriteAppend = 0x05
    }

    enum SeekMode : uint
    {
        Start = 0,
        Current = 1,
        End = 2
    }

    enum StyleHint : uint
    {
        Indentation = 0,
        ParaIndentation = 1,
        Justification = 2,
        Size = 3,
        Weight = 4,
        Oblique = 5,
        Proportional = 6,
        TextColor = 7,
        BackColor = 8,
        ReverseColor = 9
    }

    enum Justification : uint
    {
        LeftFlush = 0,
        LeftRight = 1,
        Centered = 2,
        RightFlush = 3
    }

    enum ZColor : uint
    {
        Transparent = 0xfffffffc,
        Cursor = 0xfffffffd,
        Current = 0xfffffffe,
        Default = 0xffffffff
    }

    enum ImageAlign : int
    {
        InlineUp = 1,
        InlineDown = 2,
        InlineCenter = 3,
        MarginLeft = 4,
        MarginRight = 5
    }

    enum BlorbError : uint
    {
        None = 0,
        CompileTime = 1,
        Alloc = 2,
        Read = 3,
        NotAMap = 4,
        Format = 5,
        NotFound = 6
    }

    internal static class Glk_Pinvoke
    {
        internal const string DEFAULT_DLL_NAME = "@@GLK@UNSET@@";

        // Universal Glk functions.
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern IntPtr glk_window_open(IntPtr split, WinMethod method, uint size, WinType wintype, uint rock);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern IntPtr glk_window_get_stream(IntPtr winId);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_window_close(IntPtr winId, IntPtr streamResult);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_window_clear(IntPtr winId);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_set_window(IntPtr winId);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_window_move_cursor(IntPtr winId, uint xpos, uint ypos);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_window_get_size(IntPtr winId, out uint width, out uint height);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_put_buffer([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] s, uint len);

        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_put_buffer_uni([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] uint[] s, uint len);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_put_buffer_stream(IntPtr streamId, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] s, uint len);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_select(ref Event ev);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_tick();
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_exit();
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_request_char_event(IntPtr winId);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_request_hyperlink_event(IntPtr winId);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_cancel_hyperlink_event(IntPtr winId);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern unsafe void glk_request_line_event(IntPtr win, byte* buf, uint maxlen, uint initlen);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern unsafe void glk_request_line_event_uni(IntPtr win, uint* buf, uint maxlen, uint initlen);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_cancel_line_event(IntPtr winId, ref Event ev);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_set_hyperlink(uint linkval);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_stylehint_set(WinType wintype, Style styl, StyleHint hint, int val);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_set_style(Style s);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern uint glk_style_measure(IntPtr winid, Style styl, StyleHint hint, ref uint result);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern uint glk_image_draw(IntPtr winid, uint imageId, int val1, int val2);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern uint glk_image_get_info(uint imageId, ref uint width, ref uint height);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_window_flow_break(IntPtr winId);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern IntPtr glk_schannel_create(uint rock);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_schannel_destroy(IntPtr chan);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern uint glk_schannel_play(IntPtr chan, uint sndId);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern uint glk_schannel_play_ext(IntPtr chan, uint sndId, uint repeats, uint notify);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_schannel_stop(IntPtr chan);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_schannel_set_volume(IntPtr chan, uint vol);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_schannel_pause(IntPtr chan);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_schannel_unpause(IntPtr chan);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern IntPtr glk_fileref_create_temp(FileUsage usage, uint rock);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern IntPtr glk_fileref_create_by_prompt(FileUsage usage, FileMode fmode, uint rock);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern IntPtr glk_fileref_create_by_name(FileUsage usage, FileMode fmode, uint rock);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_fileref_destroy(IntPtr fref);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern IntPtr glkunix_fileref_get_name(IntPtr fileref);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern IntPtr glk_stream_open_file(IntPtr fileref, FileMode fmode, uint rock);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern IntPtr glk_stream_open_memory(IntPtr buf, uint buflen, FileMode mode, uint rock);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void glk_stream_set_position(IntPtr stream, int pos, SeekMode seekMode);
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern BlorbError giblorb_set_resource_map(IntPtr fileStream);
        // mostly-universal-by-now extension
        [DllImport(DEFAULT_DLL_NAME)]
        internal static extern void garglk_set_zcolors(uint fg, uint bg);


        // Windows Glk specific functions.
        [DllImport("Glk")]
        internal static extern void InitGlk(uint version);
        [DllImport("Glk")]
        internal static extern void winglk_app_set_name([MarshalAs(UnmanagedType.LPStr)] string name);

        // Garglk specific runtime functions
        [DllImport("libgarglk")]
        internal static extern void garglk_set_story_name([MarshalAs(UnmanagedType.LPStr)] string name);
    }

    internal struct Event
    {
        internal EventType type;
        internal IntPtr win_handle;
        internal uint val1;
        internal uint val2;
    }

    public static class GlkUtil
    {
        private static string selectedGlkLib = "";
        public static void SelectGlkLib(string dllName)
        {
            NativeLibrary.SetDllImportResolver(typeof(Glk_Pinvoke).Assembly, (name, asm, search) => {
                if (name == Glk_Pinvoke.DEFAULT_DLL_NAME)
                {
                    return NativeLibrary.Load(dllName, asm, search);
                }
                return IntPtr.Zero;
            });
            selectedGlkLib = dllName;
        }

        internal static void OutputStringLatin1(string msg)
        {
            var encoder = Encoding.GetEncoding(Encoding.Latin1.CodePage, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);
            var bytes = encoder.GetBytes(msg);
            Glk_Pinvoke.glk_put_buffer(bytes, (uint) bytes.Length);
        }

        internal static void OutputString(string msg)
        {
            var runes = msg.EnumerateRunes().Select(r => (uint)r.Value).ToArray();
            Glk_Pinvoke.glk_put_buffer_uni(runes, (uint)runes.Length);
        }

        internal static string FilerefGetName(IntPtr fileref)
        {
            var fn = Glk_Pinvoke.glkunix_fileref_get_name(fileref);
            return Marshal.PtrToStringAnsi(fn);
        }

        internal static void SetGameName(string game)
        {
            switch (selectedGlkLib)
            {
                case "garglk":
                case "libgarglk":
                    Glk_Pinvoke.garglk_set_story_name(game); break;
                default:
                    return;
            }
        }
    }
}