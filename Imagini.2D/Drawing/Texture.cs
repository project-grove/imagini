using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using static Imagini.ErrorHandler;
using static SDL2.SDL_blendmode;
using static SDL2.SDL_render;
using static SDL2.SDL_surface;

namespace Imagini.Drawing
{
    /// <summary>
    /// Defines texture access patterns.
    /// Static - changes rarely, not lockable.
    /// Streaming - changes frequently, lockable.
    /// Target - can be used as render target.
    /// </summary>
    public enum TextureAccess
    {
        Static = SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC,
        Streaming = SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
        Target = SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET
    }

    /// <summary>
    /// Specifies texture scaling quality.
    /// </summary>
    public enum TextureScalingQuality
    {
        Nearest,
        Linear,
        Anisotropic
    }

    [ExcludeFromCodeCoverage]
    internal static class TextureScalingQualityExtensions
    {
        public static string AsHint(this TextureScalingQuality quality)
        {
            switch (quality)
            {
                case TextureScalingQuality.Nearest: return "nearest";
                case TextureScalingQuality.Linear: return "linear";
                case TextureScalingQuality.Anisotropic: return "best";
                default: return "";
            }
        }
    }

    /// <summary>
    /// Defines a texture which stores it's data on GPU.
    /// </summary>
    public sealed class Texture : Resource, IDisposable
    {
        internal IntPtr Handle;
        /// <summary>
        /// Returns the renderer which owns this texture.
        /// </summary>
        public Graphics Owner { get; private set; }
        /// <summary>
        /// Gets the width of this texture in pixels.
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Gets the height of this texture in pixels.
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Gets the pixel format of this texture.
        /// </summary>
        /// <returns></returns>
        public PixelFormat Format { get; private set; }
        /// <summary>
        /// Gets the <see cref="TextureAccess" /> for this texture.
        /// </summary>
        public TextureAccess Access { get; private set; }

        /// <summary>
        /// Indicates if this texture is locked.
        /// </summary>
        public bool Locked { get; private set; }

        /// <summary>
        /// Gets or sets the additional color value multiplied into render operations.
        /// </summary>
        public Color ColorMod
        {
            get
            {
                byte r = 0; byte g = 0; byte b = 0;
                Try(() =>
                    SDL_GetTextureColorMod(Handle, ref r, ref g, ref b),
                    "SDL_GetTextureColorMod");
                return Color.FromArgb(r, g, b);
            }
            set
            {
                Try(() =>
                    SDL_SetTextureColorMod(Handle, value.R, value.G, value.B),
                    "SDL_SetTextureColorMod");
            }
        }

        /// <summary>
        /// Gets or sets an additional alpha value used in render operations.
        /// </summary>
        public byte AlphaMod
        {
            get
            {
                byte a = 0;
                Try(() => SDL_GetTextureAlphaMod(Handle, ref a),
                    "SDL_GetTextureAlphaMod");
                return a;
            }
            set
            {
                Try(() => SDL_SetTextureAlphaMod(Handle, value),
                    "SDL_SetTextureAlphaMod");
            }
        }

        /// <summary>
        /// Gets or sets the blend mode used for render operations.
        /// </summary>
        public BlendMode BlendMode
        {
            get
            {
                var blendMode = SDL_BlendMode.SDL_BLENDMODE_NONE;
                Try(() => SDL_GetTextureBlendMode(Handle, ref blendMode),
                    "SDL_GetTextureBlendMode");
                return (BlendMode)blendMode;
            }
            set
            {
                var blendMode = (SDL_BlendMode)value;
                Try(() => SDL_SetTextureBlendMode(Handle, blendMode),
                    "SDL_SetTextureBlendMode");
            }
        }

        internal Texture(IntPtr handle, Graphics owner)
        {
            (this.Handle, this.Owner) = (handle, owner);
            owner.Register(this);

            var format = 0u;
            var access = 0;
            var w = 0;
            var h = 0;
            Try(() =>
                SDL_QueryTexture(Handle, ref format, ref access, ref w, ref h),
                "SDL_QueryTexture");
            Format = (PixelFormat)format;
            Access = (TextureAccess)access;
            Width = w;
            Height = h;
        }

        /// <summary>
        /// Locks a portion or the whole texture for write-only access and
        /// returns a pointer to it.
        /// </summary>
        /// <param name="rect">Rectangle to be locked, or null to lock entire texture</param>
        /// <param name="length">Length of the byte array</param>
        /// <param name="stride">Stride of the pixel data (bytes per row)</param>
        public IntPtr Lock(out int length, out int stride, Rectangle? rect = null)
        {
            if (Locked)
                throw new ImaginiException("This texture is already locked");
            if (Access != TextureAccess.Streaming)
                throw new ImaginiException("Only texture with streaming access can be locked");

            var r = rect?.ToSDL();
            var rectHandle = GCHandle.Alloc(r, GCHandleType.Pinned);
            var pitch = 0;
            var result = IntPtr.Zero;
            try
            {
                Try(() => SDL_LockTexture(Handle, rectHandle.AddrOfPinnedObject(),
                    out result, ref pitch),
                    "SDL_LockTexture");
                stride = pitch;
                length = GetPixelBufferSizeInBytes(rect);
                Locked = true;
            }
            finally
            {
                rectHandle.Free();
            }
            return result;
        }

        /// <summary>
        /// Use this function to update the given texture rectangle with new
        /// pixel data.
        /// </summary>
        /// <param name="rect">Area to update, or null to update entire texture</param>
        /// <param name="pixelData">Pixel data array</param>
        /// <remarks>
        /// The pixel data must be in the pixel format of the texture.
        /// This is a fairly slow function, intended for use with static textures that do not change often.
        /// If the texture is intended to be updated often, it is preferred to
        /// create the texture as streaming and use the locking functions.
        /// </remarks>
        public void SetPixels(byte[] pixelData, Rectangle? rect = null)
        {
            if (GetPixelBufferSizeInBytes(rect) > pixelData.Length)
                throw new ArgumentOutOfRangeException("Pixel array is too small");
            var r = rect?.ToSDL();
            var rectHandle = GCHandle.Alloc(r, GCHandleType.Pinned);
            var pixelHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
            try
            {
                Try(() => SDL_UpdateTexture(Handle,
                    rectHandle.AddrOfPinnedObject(),
                    pixelHandle.AddrOfPinnedObject(),
                    Width * Format.GetBytesPerPixel()),
                    "SDL_UpdateTexture");
            }
            finally
            {
                pixelHandle.Free();
                rectHandle.Free();
            }
        }

        /// <summary>
        /// Calculates the buffer size needed for pixel writing and reading
        /// operations.
        /// </summary>
        /// <param name="rectangle">Rectangle to read the data from, or null to read entire texture</param>
        /// <seealso cref="Lock" />
        public int GetPixelBufferSizeInBytes(Rectangle? rectangle = null) =>
            InternalGetPixelBufferSizeInBytes(Width, Height, Format, rectangle);

        internal static int InternalGetPixelBufferSizeInBytes(int width, int height,
            PixelFormat format, Rectangle? rectangle)
        {
            var bpp = format.GetBytesPerPixel();
            if (!rectangle.HasValue) return width * height * bpp;
            var rect = rectangle.Value;
            var length = width * rect.Height * bpp;
            // substract pixels before top left rectangle corner
            length -= rect.X * bpp;
            // substract pixels after bottom right rectangle corner
            length -= (width - rect.Width - rect.X) * bpp;
            return length;
        }

        /// <summary>
        /// Unlocks the texture.
        /// </summary>
        public void Unlock()
        {
            if (!Locked) return;
            SDL_UnlockTexture(Handle);
            Locked = false;
        }

        internal override void Destroy()
        {
            if (IsDisposed) return;
            SDL_DestroyTexture(Handle);
            base.Destroy();
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            Destroy();
        }

        static Texture() => Lifecycle.TryInitialize();
    }
}