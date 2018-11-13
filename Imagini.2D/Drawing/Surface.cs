using System;
using System.Runtime.InteropServices;

using static SDL2.SDL_blendmode;
using static SDL2.SDL_error;
using static SDL2.SDL_surface;
using static Imagini.ErrorHandler;
using System.Drawing;
using Imagini;
using static SDL2.SDL_rect;
using System.Diagnostics.CodeAnalysis;
using Imagini.Core.Internal;

namespace Imagini.Drawing
{
    /// <summary>
    /// Defines surface blend modes.
    /// </summary>
    /// <remarks>
    /// None:
    /// <code>
    /// dstRGBA = srcRGBA
    /// </code>
    /// AlphaBlend:
    /// <code>
    /// dstRGB = (srcRGB * srcA) + (dstRGB * (1 - srcA))
    /// dstA = srcA + (dstA * (1 - srcA))
    /// </code>
    /// Add:
    /// <code>
    /// dstRGB = (srcRGB * srcA) + dstRGB
    /// dstA = dstA
    /// </code>
    /// Modulate:
    /// <code>
    /// dstRGB = srcRGB * dstRGB
    /// dstA = dstA
    /// </code>
    /// </remarks>
    public enum SurfaceBlendMode
    {
        None = SDL_BlendMode.SDL_BLENDMODE_NONE,
        AlphaBlend = SDL_BlendMode.SDL_BLENDMODE_BLEND,
        Add = SDL_BlendMode.SDL_BLENDMODE_ADD,
        Modulate = SDL_BlendMode.SDL_BLENDMODE_MOD
    }

    /// <summary>
    /// Defines a surface which stores it's pixel data in
    /// the RAM.
    /// </summary>
    public sealed class Surface : Resource, IDisposable
    {
        internal readonly IntPtr Handle;
        private readonly IntPtr _pixels;
        internal IntPtr PixelsHandle => _pixels;
        private readonly bool _shouldFreePixels;

        /// <summary>
        /// Returns the surface's pixel format.
        /// </summary>
        /// <returns></returns>
        public PixelFormatInfo PixelInfo { get; private set; }
        /// <summary>
        /// Returns the surface width in pixels.
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Returns the surface height in pixels.
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Returns the pixel count of the surface (width * height).
        /// </summary>
        public int PixelCount => Width * Height;
        /// <summary>
        /// Returns the surface stride (aka pitch, or length of a pixel row) in bytes.
        /// </summary>
        /// <returns></returns>
        public int Stride { get; private set; }
        /// <summary>
        /// Returns the size of the pixel data array in bytes (stride * height).
        /// </summary>
        public int SizeInBytes => Stride * Height;
        /// <summary>
        /// Indicates if this surface should be locked in order to access
        /// the pixel data.
        /// </summary>
        /// <returns></returns>    
        public bool MustBeLocked { get; private set; }
        /// <summary>
        /// Indicates if this surface is currently locked or not.
        /// </summary>
        public bool Locked { get; private set; }
        /// <summary>
        /// Indicates if this surface has RLE acceleration enabled.
        /// </summary>
        public bool RLEEnabled { get; private set; }

        /// <summary>
        /// Gets or sets the clipping rectangle.
        /// </summary>
        public Rectangle? ClipRect
        {
            get
            {
                unsafe
                {
                    var rect = new SDL_Rect();
                    var p = &rect;
                    SDL_GetClipRect(Handle, &rect);
                    if (rect.x == 0 && rect.y == 0 && rect.w == Width && rect.h == Height)
                        return null;
                    return rect.ToRectangle();
                }
            }
            set
            {
                unsafe
                {
                    var rect = value.HasValue ? value.Value.ToSDL() :
                        new SDL_Rect() { x = 0, y = 0, w = Width, h = Height };
                    var p = &rect;
                    Try(() => SDL_SetClipRect(Handle, p), "SDL_SetClipRect");
                }
            }
        }

        /// <summary>
        /// Gets or sets the additional color value multiplied into blit operations.
        /// Only R, G and B channels are used.
        /// </summary>
        /// <remarks>
        /// When this surface is blitted, during the blit operation each source color channel is modulated by the appropriate color value according to the following formula:
        /// srcC = srcC * (color / 255)
        /// </remarks>
        public Color ColorMod
        {
            get
            {
                byte r = 0; byte g = 0; byte b = 0;
                Try(() =>
                    SDL_GetSurfaceColorMod(Handle, ref r, ref g, ref b),
                    "SDL_GetSurfaceColorMod");
                return Color.FromArgb(r, g, b);
            }
            set
            {
                Try(() =>
                    SDL_SetSurfaceColorMod(Handle, value.R, value.G, value.B),
                    "SDL_SetSurfaceColorMod");
            }
        }

        /// <summary>
        /// Gets or sets an additional alpha value used in blit operations.
        /// </summary>
        /// <remarks>
        /// When this surface is blitted, during the blit operation the source alpha value is modulated by this alpha value according to the following formula:
        /// srcA = srcA * (alpha / 255)
        /// </remarks>
        public byte AlphaMod
        {
            get
            {
                byte a = 0;
                Try(() => SDL_GetSurfaceAlphaMod(Handle, ref a),
                    "SDL_GetSurfaceAlphaMod");
                return a;
            }
            set
            {
                Try(() => SDL_SetSurfaceAlphaMod(Handle, value),
                    "SDL_SetSurfaceAlphaMod");
            }
        }

        /// <summary>
        /// Gets or sets the blend mode used for blit operations.
        /// </summary>
        public SurfaceBlendMode BlendMode
        {
            get
            {
                var blendMode = SDL_BlendMode.SDL_BLENDMODE_NONE;
                Try(() => SDL_GetSurfaceBlendMode(Handle, ref blendMode),
                    "SDL_GetSurfaceBlendMode");
                return (SurfaceBlendMode)blendMode;
            }
            set
            {
                var blendMode = (SDL_BlendMode)value;
                Try(() => SDL_SetSurfaceBlendMode(Handle, blendMode),
                    "SDL_SetSurfaceBlendMode");
            }
        }

        /// <summary>
        /// Gets or sets the color key (transparent pixel).
        /// </summary>
        public Color? ColorKey
        {
            get
            {
                var key = 0u;
                var result = SDL_GetColorKey(Handle, ref key);
                if (result == 0) return ColorExtensions.FromUint(key, PixelInfo);
                if (result == -1) return null;
                throw new ImaginiException($"Could not obtain color key: {SDL_GetError()}");
            }
            set
            {
                var val = value?.AsUint(PixelInfo) ?? 0u;
                Try(() => SDL_SetColorKey(Handle, value != null ? 1 : 0, val),
                    "SDL_SetColorKey");
            }
        }

        internal Surface(IntPtr handle)
        {
            Handle = handle;
            var data = Marshal.PtrToStructure<SDL_Surface>(handle);
            PixelInfo = new PixelFormatInfo(data.format);
            Width = data.w;
            Height = data.h;
            Stride = data.pitch;
            MustBeLocked = SDL_MUSTLOCK(data);
            _pixels = data.pixels;
            _shouldFreePixels = false;
            Locked = data.locked > 0;
        }

        internal Surface(IntPtr handle, IntPtr pixels)
            : this(handle) =>
            (this._pixels, this._shouldFreePixels) = (pixels, true);

        /// <summary>
        /// Locks the surface 
        /// </summary>
        public void Lock()
        {
            if (Locked) return;
            Try(() => SDL_LockSurface(Handle),
                "SDL_LockSurface");
            Locked = true;
        }

        /// <summary>
        /// Unlocks the surface.
        /// </summary>
        public void Unlock()
        {
            if (!Locked) return;
            SDL_UnlockSurface(Handle);
            Locked = false;
        }

        /// <summary>
        /// Enabled or disables the RLE acceleration.
        /// </summary>
        public void SetRLEAcceleration(bool enable)
        {
            Try(() => SDL_SetSurfaceRLE(Handle, enable ? 1 : 0), "SDL_SetSurfaceRLE");
            RLEEnabled = enable;
            var data = Marshal.PtrToStructure<SDL_Surface>(Handle);
            MustBeLocked = SDL_MUSTLOCK(data);
        }

        /// <summary>
        /// Creates a new surface with the specified format.
        /// </summary>
        /// <param name="width">Surface width</param>
        /// <param name="height">Surface height</param>
        /// <param name="format">Surface format</param>
        public static Surface Create(int width, int height, PixelFormat format = PixelFormat.Format_ARGB8888)
        {
            var fmt = new PixelFormatInfo(format);
            var result = Create(width, height, fmt.BitsPerPixel,
                fmt.MaskR, fmt.MaskG, fmt.MaskB, fmt.MaskA);
            fmt.Dispose();
            return result;
        }

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
        public static Surface Create(int width, int height, int depth,
            int Rmask = 0, int Gmask = 0, int Bmask = 0, int Amask = 0)
        {
            unchecked
            {
                var handle = SDL_CreateRGBSurface(0, width, height, depth,
                    (uint)Rmask, (uint)Gmask, (uint)Bmask, (uint)Amask);
                if (handle == IntPtr.Zero)
                    throw new ImaginiException($"Could not create surface: {SDL_GetError()}");
                return new Surface(handle);
            }
        }

        /// <summary>
        /// Creates a surface with the specified format from the specified pixel data.
        /// </summary>
        /// <param name="data">The pixel data to create surface from</param>
        /// <param name="width">Surface width</param>
        /// <param name="height">Surface height</param>
        /// <param name="format">Surface format</param>
        public static Surface CreateFrom(byte[] data, int width, int height, PixelFormat format)
        {
            var fmt = new PixelFormatInfo(format);
            var result = CreateFrom(data, width, height,
                fmt.BytesPerPixel * width, fmt.BitsPerPixel,
                fmt.MaskR, fmt.MaskG, fmt.MaskB, fmt.MaskA);
            fmt.Dispose();
            return result;
        }

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

        /// <summary>
        /// Copies the surface into a new one that is optimized for blitting to
        /// a surface of the specified pixel format.
        /// </summary>
        public Surface OptimizeFor(PixelFormat format)
        {
            var fmt = new PixelFormatInfo(format);
            return OptimizeFor(fmt);
        }

        /// <summary>
        /// Copies the surface into a new one that is optimized for blitting to
        /// a surface of the specified pixel format.
        /// </summary>
        public Surface OptimizeFor(PixelFormatInfo format)
        {
            var handle = SDL_ConvertSurface(Handle, format.Handle, 0);
            if (handle == IntPtr.Zero)
                throw new ImaginiException($"Could not create surface: {SDL_GetError()}");
            return new Surface(handle);
        }

        /// <summary>
        /// Copies the surface into a new one that has the specified pixel format.
        /// </summary>
        public Surface ConvertTo(PixelFormat format)
        {
            var handle = SDL_ConvertSurfaceFormat(Handle, (uint)format, 0);
            if (handle == IntPtr.Zero)
                throw new ImaginiException($"Could not create surface: {SDL_GetError()}");
            return new Surface(handle);
        }

        [ExcludeFromCodeCoverage]
        /// <summary>
        /// Copies the surface into a new one that has the specified pixel format.
        /// </summary>
        public Surface ConvertTo(PixelFormatInfo format) => ConvertTo(format.Format);

        /// <summary>
        /// Reads the pixel data to the specified pixel buffer, making automatic
        /// conversion if necessary.
        /// </summary>
        public void GetPixelData<T>(ref T[] target)
            where T : struct, IColor
        {
            if (MustBeLocked && !Locked)
                throw new ImaginiException("Surface must be locked before accessing");
            var p = _pixels;
            var shouldFree = false;
            var targetFormat = target[0].Format;
            var targetStride = targetFormat.GetBytesPerPixel() * Width;
            var sizeInBytes = Util.SizeOf<T>() * (Width * Height);
            if (PixelInfo.Format != targetFormat)
            {
                var p2 = Marshal.AllocHGlobal(sizeInBytes);
                Try(() => SDL_ConvertPixels(Width, Height,
                    (uint)PixelInfo.Format, p, Stride,
                    (uint)targetFormat, p2, targetStride),
                    "SDL_ConvertPixels");
                p = p2;
                shouldFree = true;
            }
            Util.CopyTo(target, from: p, count: Width * Height);
            if (shouldFree)
                Marshal.FreeHGlobal(p);
        }

        /// <summary>
        /// Copies the pixel data in the specified byte array.
        /// </summary>
        public void GetPixelData(ref byte[] target)
        {
            if (MustBeLocked && !Locked)
                throw new ImaginiException("Surface must be locked before accessing");
            Marshal.Copy(_pixels, target, 0, Stride * Height);
        }

        /// <summary>
        /// Copies the pixel data from the specified pixel array, making a
        /// conversion if necessary.
        /// </summary>
        public void SetPixelData<T>(ref T[] source)
            where T : struct, IColor
        {
            if (MustBeLocked && !Locked)
                throw new ImaginiException("Surface must be locked before accessing");
            var shouldFree = false;
            var sourceFormat = source[0].Format;
            var sourceStride = sourceFormat.GetBytesPerPixel() * Width;
            var result = new T[Width * Height];
            var srcHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
            var p = srcHandle.AddrOfPinnedObject();
            try
            {
                if (PixelInfo.Format != sourceFormat)
                {
                    var p2 = Marshal.AllocHGlobal(SizeInBytes);
                    shouldFree = true;
                    Try(() => SDL_ConvertPixels(Width, Height,
                        (uint)sourceFormat, p, sourceStride,
                        (uint)PixelInfo.Format, p2, Stride),
                        "SDL_ConvertPixels");
                    p = p2;
                }
                unsafe
                {
                    Buffer.MemoryCopy((void*)p, (void*)_pixels, SizeInBytes, SizeInBytes);
                }
            }
            finally
            {
                srcHandle.Free();
                if (shouldFree)
                    Marshal.FreeHGlobal(p);
            }
        }

        /// <summary>
        /// Copies the pixel data from the specified byte array to the surface.
        /// </summary>
        /// <param name="source"></param>    
        public void SetPixelData(ref byte[] source)
        {
            if (MustBeLocked && !Locked)
                throw new ImaginiException("Surface must be locked before accessing");
            Marshal.Copy(source, 0, _pixels, Stride * Height);
        }

        /// <summary>
        /// Performs a fast fill of a rectangle with the specified color. No
        /// alpha blending is performed if alpha channel data is present.
        /// </summary>
        /// <param name="rectangle">Rectangle to fill, or NULL to fill entire surface</param>
        /// <param name="color">Color to fill with</param>
        public void Fill(Color color, Rectangle? rectangle = null)
        {
            SDL_Rect? rect = rectangle?.ToSDL();
            var rectHandle = GCHandle.Alloc(rect, GCHandleType.Pinned);
            try
            {
                unsafe
                {
                    Try(() => SDL_FillRect(Handle,
                       (SDL_Rect*)rectHandle.AddrOfPinnedObject(), color.AsUint(PixelInfo)),
                       "SDL_FillRect");
                }
            }
            finally
            {
                rectHandle.Free();
            }
        }

        /// <summary>
        /// Performs a fast fill of rectangles with the specified color. No
        /// alpha blending is performed if alpha channel data is present.
        /// </summary>
        /// <param name="rectangle">Rectangles to fill</param>
        /// <param name="color">Color to fill with</param>
        public void Fill(Color color, params Rectangle[] rectangles)
        {
            var rects = new SDL_Rect[rectangles.Length];
            for (int i = 0; i < rects.Length; i++)
                rects[i] = rectangles[i].ToSDL();
            var rectsHandle = GCHandle.Alloc(rects, GCHandleType.Pinned);
            try
            {
                unsafe
                {
                    Try(() => SDL_FillRects(Handle,
                        rectsHandle.AddrOfPinnedObject(),
                        rects.Length, color.AsUint(PixelInfo)),
                        "SDL_FillRects");
                }
            }
            finally
            {
                rectsHandle.Free();
            }
        }

        /// <summary>
        /// Performs a scaled surface copy to a destination surface.
        /// </summary>
        /// <param name="srcRect">Rectangle to be copied, or null to copy entire surface</param>
        /// <param name="destination">Blit target</param>
        /// <param name="dstRect">Rectangle that is copied into, or null to copy into the entire surface</param>
        public void BlitTo(Surface destination, Rectangle? srcRect = null, Rectangle? dstRect = null)
        {
            SDL_Rect? src = srcRect?.ToSDL();
            SDL_Rect? dst = dstRect?.ToSDL();
            var srcHandle = GCHandle.Alloc(src, GCHandleType.Pinned);
            var dstHandle = GCHandle.Alloc(dst, GCHandleType.Pinned);
            try
            {
                unsafe
                {
                    Try(() =>
                        SDL_BlitScaled(Handle,
                            (SDL_Rect*)srcHandle.AddrOfPinnedObject(),
                            destination.Handle,
                            (SDL_Rect*)dstHandle.AddrOfPinnedObject()),
                        "SDL_BlitScaled");
                }
            }
            finally
            {
                srcHandle.Free();
                dstHandle.Free();
            }
        }

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