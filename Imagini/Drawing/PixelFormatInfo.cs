using System;
using static SDL2.SDL_pixels;
using static Imagini.Internal.ErrorHandler;

namespace Imagini.Drawing
{
    /// <summary>
    /// Contains pixel format information.
    /// </summary>
    public sealed class PixelFormatInfo
    {
        internal IntPtr Handle;

        /// <summary>
        /// Gets the pixel format.
        /// </summary>
        public PixelFormat Format { get; private set; }
        /// <summary>
        /// Returns bits per pixel.
        /// </summary>
        public int BitsPerPixel { get; private set; }
        
        private Palette _palette;
        /// <summary>
        /// Gets or sets the palette used by this pixel format. Returns null
        /// if the palette is not present.
        /// </summary>
        public Palette Palette
        {
            get {
                return _palette;
            }
            set {
                if (_palette == null)
                    throw new ImaginiException("No palette is present");
                Try(() => 
                    SDL_SetPixelFormatPalette(Handle, value.Handle),
                    "SDL_SetPixelFormatPalette");
            }
        }

        // TODO Additional properties and constructor
    }
}