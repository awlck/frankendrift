using Eto.Drawing;

namespace Adravalon.Runner
{
    public static class FontExtensions
    {
        public static Font WithFontFace(this Font font, string face)
        {
            return new(face, font.Size, font.FontStyle);
        }

        public static Font WithSize(this Font font, float size)
        {
            return new(font.Typeface, size, font.FontDecoration);
        }
    }
}