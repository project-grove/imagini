using System;
using static SDL2.SDL_error;
using static SDL2.SDL_pixels;
using static Imagini.ErrorHandler;
using System.Runtime.InteropServices;

namespace Imagini.Drawing
{
    /// <summary>
    /// Contains pixel format information.
    /// </summary>
    public sealed class PixelFormatInfo : Resource, IDisposable
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
        /// <summary>
        /// Returns bytes per pixel.
        /// </summary>
        public int BytesPerPixel { get; private set; }

        /// <summary>
        /// A bit mask representing the location of the red component of the pixel.
        /// </summary>
        public int MaskR { get; private set; }
        /// <summary>
        /// A bit mask representing the location of the green component of the pixel.
        /// </summary>
        public int MaskG { get; private set; }
        /// <summary>
        /// A bit mask representing the location of the blue component of the pixel.
        /// </summary>
        public int MaskB { get; private set; }
        /// <summary>
        /// A bit mask representing the location of the alpha component of the
        /// pixel or 0 if the pixel format doesn't have any alpha information.
        /// </summary>
        public int MaskA { get; private set; }

        private Palette _palette;
        /// <summary>
        /// Gets or sets the palette used by this pixel format. Returns null
        /// if the palette is not present.
        /// </summary>
        public Palette Palette
        {
            get
            {
                return _palette;
            }
            set
            {
                if (!Format.IsIndexed())
                    throw new ImaginiException("Only indexed formats can have palettes");
                Try(() =>
                    SDL_SetPixelFormatPalette(Handle, value.Handle),
                    "SDL_SetPixelFormatPalette");
                if (_palette != null)
                    Unregister(_palette);
                _palette = value;
                Register(_palette);
            }
        }

        /// <summary>
        /// Creates a new PixelFormatInfo with the specified pixel format.
        /// </summary>
        public PixelFormatInfo(PixelFormat format)
        {
            Handle = SDL_AllocFormat((uint)format);
            if (Handle == IntPtr.Zero)
                throw new ImaginiException($"Could not create a pixel format: {SDL_GetError()}");
            FromSDL(Marshal.PtrToStructure<SDL_PixelFormat>(Handle));
        }

        internal PixelFormatInfo(IntPtr handle)
        {
            Handle = handle;
            FromSDL(Marshal.PtrToStructure<SDL_PixelFormat>(Handle));
        }

        private void FromSDL(SDL_PixelFormat fmt)
        {
            if (fmt.palette != IntPtr.Zero)
                _palette = new Palette(fmt.palette);
            Format = (PixelFormat)fmt.format;
            BitsPerPixel = Format.GetBitsPerPixel();
            BytesPerPixel = Format.GetBytesPerPixel();
            unchecked
            {
                MaskR = (int)fmt.Rmask;
                MaskG = (int)fmt.Gmask;
                MaskB = (int)fmt.Bmask;
                MaskA = (int)fmt.Amask;
            }
        }

        internal override void Destroy()
        {
            if (IsDisposed) return;
            base.Destroy();
            SDL_FreeFormat(Handle);
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose() => Destroy();

        static PixelFormatInfo() => Lifecycle.TryInitialize();
    }
}