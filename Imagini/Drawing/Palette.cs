using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using static SDL2.SDL_pixels;
using static Imagini.Internal.ErrorHandler;

namespace Imagini.Drawing
{
    /// <summary>
    /// Contains palette information.
    /// </summary>
    public sealed class Palette : Resource, IDisposable
    {
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
                if (count != _colors.Length)
                    throw new ArgumentOutOfRangeException("The number of colors should be the same");
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

        internal Palette(IntPtr handle) : base(nameof(Palette))
        {
            Handle = handle;
            var palette = Marshal.PtrToStructure<SDL_Palette>(Handle);
            var p = palette.colors;
            var colors = new Color[palette.ncolors];
            for (int i = 0; i < palette.ncolors; i++)
            {
                unsafe
                {
                    var ptr = (IntPtr)(p + i);
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
            : base(nameof(Palette))
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
        public void Dispose()
        {
            if (IsDisposed) return;
            SDL_FreePalette(Handle);
        }
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
    }
}