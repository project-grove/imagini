using System;
using System.Runtime.InteropServices;
using Imagini.Internal;
using static SDL2.SDL_error;
using static SDL2.SDL_surface;

namespace Imagini.Drawing
{
    /// <summary>
    /// Defines a surface which stores it's pixel data in
    /// the RAM.
    /// </summary>
    public sealed class Surface : Resource, IDisposable
    {
        internal readonly IntPtr Handle;
        private readonly IntPtr _pixels;
        private readonly bool _shouldFreePixels;

        internal Surface(IntPtr handle)
            : base(nameof(Surface))
        {
            Handle = handle;
            var data = Marshal.PtrToStructure<SDL_Surface>(handle);
            // TODO Read data
            _pixels = data.pixels;
            _shouldFreePixels = false;
        }

        internal Surface(IntPtr handle, IntPtr pixels)
            : this(handle) =>
            (this._pixels, this._shouldFreePixels) = (pixels, true);

        /// <summary>
        /// Creates a new surface.
        /// </summary>
        /// <param name="width">Surface width</param>
        /// <param name="height">Surface height</param>
        /// <param name="depth">Surface depth in bits (defaults to 32)</param>
        /// <remarks>
        /// The mask parameters are the bitmasks used to extract that
        /// color from a pixel. For instance, Rmask being FF000000 means
        /// the red data is stored in the most significant byte. Uzing zeros for
        /// the RGB masks sets a default value, based on the depth. However,
        /// using zero for the Amask results in an Amask of 0.
        /// </remarks>
        public static Surface Create(int width, int height, int depth = 32,
            int Rmask = 0, int Gmask = 0, int Bmask = 0, int Amask = 0x000000FF)
        {
            var handle = SDL_CreateRGBSurface(0, width, height, depth,
                (uint)Rmask, (uint)Gmask, (uint)Bmask, (uint)Amask);
            if (handle == IntPtr.Zero)
                throw new ImaginiException($"Could not create surface: {SDL_GetError()}");
            return new Surface(handle);
        }

        /// <summary>
        /// Creates a new surface from existing data. Data is copied.
        /// </summary>
        /// <param name="data">The pixel data to create surface from</param>
        /// <param name="width">Surface width</param>
        /// <param name="height">Surface height</param>
        /// <param name="depth">Surface depth in bits (defaults to 32)</param>
        /// <remarks>
        /// The mask parameters are the bitmasks used to extract that
        /// color from a pixel. For instance, Rmask being FF000000 means
        /// the red data is stored in the most significant byte. Uzing zeros for
        /// the RGB masks sets a default value, based on the depth. However,
        /// using zero for the Amask results in an Amask of 0.
        /// </remarks>
        public static Surface CreateFrom(byte[] data, int width, int height, int depth = 32,
            int Rmask = 0, int Gmask = 0, int Bmask = 0, int Amask = 0x000000FF) =>
            CreateFrom(data, width, height, width * depth / 4, depth, Rmask, Gmask, Bmask, Amask);

        /// <summary>
        /// Creates a new surface from existing data. Data is copied.
        /// </summary>
        /// <param name="data">The pixel data to create surface from</param>
        /// <param name="width">Surface width</param>
        /// <param name="height">Surface height</param>
        /// <param name="stride">Length of pixel row in bytes. RGBA = 4 * width</params>
        /// <param name="depth">Surface depth in bits (defaults to 32)</param>
        /// <remarks>
        /// The mask parameters are the bitmasks used to extract that
        /// color from a pixel. For instance, Rmask being FF000000 means
        /// the red data is stored in the most significant byte. Uzing zeros for
        /// the RGB masks sets a default value, based on the depth. However,
        /// using zero for the Amask results in an Amask of 0.
        /// </remarks>
        public static Surface CreateFrom(byte[] data, int width, int height, int stride, int depth,
            int Rmask, int Gmask, int Bmask, int Amask)
        {
            var allocated = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, allocated, data.Length);
            var handle = SDL_CreateRGBSurfaceFrom(allocated, width, height, depth, stride,
                (uint)Rmask, (uint)Gmask, (uint)Bmask, (uint)Amask);
            if (handle == IntPtr.Zero)
                throw new ImaginiException($"Could not create surface: {SDL_GetError()}");
            return new Surface(handle, allocated);
        }

        // TODO
        internal override void Destroy()
        {
            if (IsDisposed) return;
            SDL_FreeSurface(Handle);
            if (_pixels != IntPtr.Zero && _shouldFreePixels)
                Marshal.FreeHGlobal(_pixels);
        }

        /// <summary>
        /// Disposes the surface.
        /// </summary>
        public void Dispose() => Destroy();


        static Surface() => Lifecycle.TryInitialize();
    }
}