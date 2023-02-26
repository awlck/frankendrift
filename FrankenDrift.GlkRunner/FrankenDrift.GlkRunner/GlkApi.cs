using System.Runtime.InteropServices;
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
    public enum BlorbError : uint
    {
        None = 0,
        CompileTime = 1,
        Alloc = 2,
        Read = 3,
        NotAMap = 4,
        Format = 5,
        NotFound = 6
    }

    enum CharOutput : uint
    {
        CannotPrint = 0,
        ApproxPrint = 1,
        ExactPrint = 2
    }

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

    public enum EventType : uint
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

    public enum FileMode : uint
    {
        Write = 0x01,
        Read = 0x02,
        ReadWrite = 0x03,
        WriteAppend = 0x05
    }

    [Flags]
    public enum FileUsage : uint
    {
#pragma warning disable CA1069 // Enums values should not be duplicated
        Data = 0x00,
        SavedGame = 0x01,
        Transcript = 0x02,
        InputRecord = 0x03,
        TypeMask = 0x0f,
        TextMode = 0x100,
        BinaryMode = 0x000
#pragma warning restore CA1069 // Enums values should not be duplicated
    }

    enum ImageAlign : int
    {
        InlineUp = 1,
        InlineDown = 2,
        InlineCenter = 3,
        MarginLeft = 4,
        MarginRight = 5
    }

    enum Justification : uint
    {
        LeftFlush = 0,
        LeftRight = 1,
        Centered = 2,
        RightFlush = 3
    }

    public enum SeekMode : uint
    {
        Start = 0,
        Current = 1,
        End = 2
    }

    public enum Style : uint
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

    public enum StyleHint : uint
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

    [Flags]
    public enum WinMethod : uint
    {
#pragma warning disable CA1069 // Enums values should not be duplicated
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
#pragma warning restore CA1069 // Enums values should not be duplicated
    }

    public enum WinType : uint
    {
        AllTypes = 0,
        Pair = 1,
        Blank = 2,
        TextBuffer = 3,
        TextGrid = 4,
        Graphics = 5
    }

    enum ZColor : uint
    {
        Transparent = 0xfffffffc,
        Cursor = 0xfffffffd,
        Current = 0xfffffffe,
        Default = 0xffffffff
    }

    public struct Event
    {
        public EventType type;
        public IntPtr win_handle;
        public uint val1;
        public uint val2;
    }

    public interface IGlk
    {
#pragma warning disable IDE1006 // Naming Styles
        BlorbError giblorb_set_resource_map(IntPtr fileStream);
        void glk_cancel_hyperlink_event(IntPtr winId);
        void glk_cancel_line_event(IntPtr winId, ref Event ev);
        void glk_exit();
        IntPtr glk_fileref_create_by_name(FileUsage usage, FileMode fmode, uint rock);
        IntPtr glk_fileref_create_by_prompt(FileUsage usage, FileMode fmode, uint rock);
        IntPtr glk_fileref_create_temp(FileUsage usage, uint rock);
        void glk_fileref_destroy(IntPtr fref);
        uint glk_image_draw(IntPtr winid, uint imageId, int val1, int val2);
        uint glk_image_get_info(uint imageId, ref uint width, ref uint height);
        void glk_put_buffer([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] s, uint len);
        void glk_put_buffer_stream(IntPtr streamId, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] byte[] s, uint len);
        void glk_put_buffer_uni([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] uint[] s, uint len);
        void glk_request_char_event(IntPtr winId);
        void glk_request_hyperlink_event(IntPtr winId);
        unsafe void glk_request_line_event(IntPtr win, byte* buf, uint maxlen, uint initlen);
        unsafe void glk_request_line_event_uni(IntPtr win, uint* buf, uint maxlen, uint initlen);
        IntPtr glk_schannel_create(uint rock);
        void glk_schannel_destroy(IntPtr chan);
        void glk_schannel_pause(IntPtr chan);
        uint glk_schannel_play(IntPtr chan, uint sndId);
        uint glk_schannel_play_ext(IntPtr chan, uint sndId, uint repeats, uint notify);
        void glk_schannel_set_volume(IntPtr chan, uint vol);
        void glk_schannel_stop(IntPtr chan);
        void glk_schannel_unpause(IntPtr chan);
        void glk_select(ref Event ev);
        void glk_set_hyperlink(uint linkval);
        void glk_set_style(Style s);
        void glk_set_window(IntPtr winId);
        IntPtr glk_stream_open_file(IntPtr fileref, FileMode fmode, uint rock);
        IntPtr glk_stream_open_memory(IntPtr buf, uint buflen, FileMode mode, uint rock);
        void glk_stream_set_position(IntPtr stream, int pos, SeekMode seekMode);
        void glk_stylehint_set(WinType wintype, Style styl, StyleHint hint, int val);
        uint glk_style_measure(IntPtr winid, Style styl, StyleHint hint, ref uint result);
        void glk_tick();
        void glk_window_clear(IntPtr winId);
        void glk_window_close(IntPtr winId, IntPtr streamResult);
        void glk_window_flow_break(IntPtr winId);
        void glk_window_get_size(IntPtr winId, out uint width, out uint height);
        IntPtr glk_window_get_stream(IntPtr winId);
        void glk_window_move_cursor(IntPtr winId, uint xpos, uint ypos);
        IntPtr glk_window_open(IntPtr split, WinMethod method, uint size, WinType wintype, uint rock);
        void garglk_set_zcolors(uint fg, uint bg);
        IntPtr glkunix_fileref_get_name(IntPtr fileref);

        // And some extra functions we want that could have different implementations
        void SetGameName(string game);
#pragma warning restore IDE1006 // Naming Styles
    }

    public class GlkUtil
    {
        private IGlk GlkApi;

        public GlkUtil(IGlk glk)
        {
            GlkApi = glk;
        }

        internal void OutputString(string msg)
        {
            var runes = msg.EnumerateRunes().Select(r => (uint)r.Value).ToArray();
            GlkApi.glk_put_buffer_uni(runes, (uint)runes.Length);
        }

        internal void OutputStringLatin1(string msg)
        {
            var encoder = Encoding.GetEncoding(Encoding.Latin1.CodePage, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);
            var bytes = encoder.GetBytes(msg);
            GlkApi.glk_put_buffer(bytes, (uint)bytes.Length);
        }
    }
}