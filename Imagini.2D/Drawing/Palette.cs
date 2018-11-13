using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using static SDL2.SDL_pixels;
using static Imagini.ErrorHandler;

namespace Imagini.Drawing
{
    /// <summary>
    /// Contains palette information.
    /// </summary>
    public sealed class Palette : Resource, IDisposable
    {
        /// <summary>
        /// Defines maximum color count in a palette.
        /// </summary>
        public const int MaximumColors = 256;
        internal IntPtr Handle;
        private Color[] _colors;
        public Color[] Colors
        {
            get
            {
                CheckIfNotDisposed();
                return _colors;
            }
            set
            {
                var count = value.Length;
                if (count < 1 || count > MaximumColors)
                    throw new ArgumentOutOfRangeException("Invalid color count - maximum of 256 is supported");
                var sdlColors = new SDL_Color[count];
                for (int i = 0; i < count; i++)
                    sdlColors[i] = value[i].ToSDLColor();
                unsafe
                {
                    var colorPtr = (SDL_Color*)Marshal.AllocHGlobal(sizeof(SDL_Color) * sdlColors.Length);
                    for (int i = 0; i < count; i++)
                        *(colorPtr + i) = value[i].ToSDLColor();
                    Try(() => 
                        SDL_SetPaletteColors(Handle, (IntPtr)colorPtr, 0, count),
                        "SDL_SetPaletteColors");
                }
                _colors = value;
            }
        }

        internal Palette(IntPtr handle)
        {
            Handle = handle;
            var palette = Marshal.PtrToStructure<SDL_Palette>(Handle);
            var p = palette.colors;
            var colors = new Color[palette.ncolors];
            for (int i = 0; i < palette.ncolors; i++)
            {
                unsafe
                {
                    var ptr = (IntPtr)(p + i * 4);
                    var color = Marshal.PtrToStructure<SDL_Color>(ptr);
                    colors[i] = color.FromSDLColor();
                }
            }
            Colors = colors;
        }

        /// <summary>
        /// Creates a new palette from the specified colors.
        /// </summary>
        public Palette(params Color[] colors)
        {
            Handle = SDL_AllocPalette(colors.Length);
            Colors = colors;
        }

        /// <summary>
        /// Creates a new palette from the specified colors.
        /// </summary>
        public Palette(IEnumerable<Color> colors) : this(colors.ToArray()) { }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose() => Destroy();

        internal override void Destroy()
        {
            if (IsDisposed) return;
            base.Destroy();
            SDL_FreePalette(Handle);
        }

        static Palette() => Lifecycle.TryInitialize();
    }

    internal static class ColorExtensions
    {
        public static Color FromSDLColor(this SDL_Color color) =>
            Color.FromArgb(color.a, color.r, color.g, color.b);

        public static SDL_Color ToSDLColor(this Color color) =>
            new SDL_Color()
            {
                a = color.A,
                r = color.R,
                g = color.G,
                b = color.B
            };

        public static uint AsUint(this Color color, PixelFormatInfo format) =>
            SDL_MapRGBA(format.Handle, color.R, color.G, color.B, color.A);

        public static Color FromUint(uint value, PixelFormatInfo format)
        {
            byte r = 0; byte g = 0; byte b = 0; byte a = 0;
            SDL_GetRGBA(value, format.Handle, ref r, ref g, ref b, ref a);
            return Color.FromArgb(a, r, g, b);
        }
    }
}