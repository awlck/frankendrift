using Eto.Drawing;
using System;
using System.Linq;

namespace FrankenDrift.Runner
{
    public static class FontExtensions
    {
        static readonly string[] Monospaces = {
            "monospace", "Courier", "Courier New", "Consolas", "Andale Mono", "Liberation Mono", "Ubuntu Mono","DejaVu Sans Mono",
            "Droid Sans Mono", "Lucida Console", "Menlo", "OCR-A", "OCR-A extended", "Overpass Mono", "Oxygen Mono", "Roboto Mono",
            "Source Code Pro", "TADS-typewriter"
        };

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

    public static class ColorExtensions
    {
        // Adapted from: https://gist.github.com/paulcollett/5533b0f3b9be16a068f58f9e76ad6289

        internal static double RoughLuminanceRatio(this Color color)
        {
            var rgb = color.ToPremultipliedArgb();
            var r = Math.Pow(((double)((rgb & 0x00FF0000) >> 16)) / 255, 2.218);
            var g = Math.Pow(((double)((rgb & 0x0000FF00) >> 8)) / 255, 2.218);
            var b = Math.Pow(((double)(rgb & 0x000000FF)) / 255, 2.218);
            return r * 0.2126 + g * 0.7152 + b * 0.0722;
        }

        internal static double RoughLightness(this Color color)
        {
            return Math.Pow(color.RoughLuminanceRatio(), 0.33);
        }
        
        public static bool IsCloseTo(this Color color, Color otherColor)
        {
            var luminanceRatioA = color.RoughLuminanceRatio();
            var luminanceRatioB = otherColor.RoughLuminanceRatio();
            var contrast = ((Math.Max(luminanceRatioA, luminanceRatioB) + 0.05) /
                            (Math.Min(luminanceRatioA, luminanceRatioB) + 0.05));
            return contrast < 1.5;
        }
    }
}