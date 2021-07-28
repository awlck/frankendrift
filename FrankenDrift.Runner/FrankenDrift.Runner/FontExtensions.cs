using Eto.Drawing;
using System;
using System.Linq;

namespace FrankenDrift.Runner
{
    public static class FontExtensions
    {
        static readonly string[] Monospaces = { "Courier", "Courier New", "Consolas", "Andale Mono", "Liberation Mono", "Ubuntu Mono", "DejaVu Sans Mono", "Droid Sans Mono", "Lucida Console", "Menlo", "OCR-A" };

        public static Font WithFontFace(this Font font, string face)
        {
            try
            {
                return new(face, font.Size, font.FontStyle);
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                if (Monospaces.Any(f => f == face))
                {
                    foreach (string f in Monospaces)
                        try
                        {
                            return new(f, font.Size, font.FontStyle);
                        }
                        catch (ArgumentException)
                        {
                            continue;
                        }
                }
                return font;
            }
        }

        public static Font WithSize(this Font font, float size)
        {
            return new(font.Typeface, size, font.FontDecoration);
        }
    }
}