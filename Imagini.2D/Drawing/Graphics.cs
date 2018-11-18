using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using static Imagini.ErrorHandler;
using static SDL2.SDL_error;
using static SDL2.SDL_hints;
using static SDL2.SDL_rect;
using static SDL2.SDL_render;

namespace Imagini.Drawing
{
    public sealed class Graphics : Resource
    {
        internal IntPtr Handle;
        public RendererInfo Renderer { get; private set; }
        /// <summary>
        /// Returns the output size of this renderer in pixels.
        /// </summary>
        public Size OutputSize
        {
            get
            {
                var w = 0; var h = 0;
                Try(() => SDL_GetRendererOutputSize(Handle, out w, out h),
                    "SDL_GetRendererOutputSize");
                return new Size(w, h);
            }
        }

        /// <summary>
        /// Gets the output pixel count.
        /// </summary>
        public int PixelCount
        {
            get
            {
                var size = OutputSize;
                return size.Width * size.Height;
            }
        }

        internal Graphics(Window owner, RendererInfo rendererInfo)
        {
            var fallbackRenderer =
                RendererInfo.All.FirstOrDefault(r =>
                    r.IsHardwareAccelerated && r.SupportsRenderingToTexture) ??
                RendererInfo.All.FirstOrDefault(r =>
                    r.IsHardwareAccelerated) ??
                RendererInfo.All.First();

            Renderer = rendererInfo ?? fallbackRenderer;
            Handle = SDL_CreateRenderer(owner.Handle, Renderer.Index, 0);
            if (Handle == IntPtr.Zero)
                throw new ImaginiException($"Could not initialize renderer: {SDL_GetError()}");
        }

        /// <summary>
        /// Creates a texture with the specified dimensions, format and access
        /// type.
        /// </summary>
        /// <param name="w">Width in pixels</param>
        /// <param name="h">Height in pixels</param>
        /// <param name="quality">Filtering quality when texture is scaled</param>
        /// <param name="format">Pixel format</param>
        /// <param name="access">Texture access type</param>
        public Texture CreateTexture(int w, int h,
            TextureScalingQuality quality = TextureScalingQuality.Nearest,
            PixelFormat format = PixelFormat.Format_ARGB8888,
            TextureAccess access = TextureAccess.Static)
        {
            Try(() =>
                SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, quality.AsHint()),
                "SDL_SetHint");
            var texture = SDL_CreateTexture(Handle, (uint)format, (int)access, w, h);
            if (texture == IntPtr.Zero)
                throw new ImaginiException($"Could not create texture: {SDL_GetError()}");
            return new Texture(texture, this);
        }

        /// <summary>
        /// Creates a static texture from an existing surface.
        /// </summary>
        /// <param name="surface">Surface to create texture from</param>
        /// <param name="quality">Filtering quality when texture is scaled</param>
        /// <remarks>
        /// The surface is not modified or disposed by this function.
        /// <see cref="TextureAccess"> is static.
        /// The pixel format of the created texture may be different from the
        /// pixel format of the surface.
        /// </remarks>
        public Texture CreateTextureFromSurface(Surface surface,
            TextureScalingQuality quality = TextureScalingQuality.Nearest)
        {
            Try(() =>
                SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, quality.AsHint()),
                "SDL_SetHint");
            var texture = SDL_CreateTextureFromSurface(Handle, surface.Handle);
            if (texture == IntPtr.Zero)
                throw new ImaginiException($"Could not create texture: {SDL_GetError()}");
            return new Texture(texture, this);
        }

        private Texture _renderTarget = null;
        /// <summary>
        /// Returns the currently active render target or null if it's not set.
        /// </summary>
        public Texture GetRenderTarget() => _renderTarget;
        /// <summary>
        /// Sets the active render target. If null is specified, sets the
        /// default render target.
        /// </summary>
        public void SetRenderTarget(Texture texture)
        {
            if (texture != null && texture.Access != TextureAccess.Target)
                throw new ImaginiException("Texture should have Target access");
            Try(() => SDL_SetRenderTarget(Handle, texture?.Handle ?? IntPtr.Zero),
                "SDL_SetRenderTarget");
            _renderTarget = texture;
        }

        /// <summary>
        /// Reads the pixel data from current render target to the specified pixel data buffer.
        /// </summary>
        /// <param name="pixelData"></param>
        /// <param name="rect">Rectangle to read data from, or null to read entire texture</param>
        /// <remarks>This function is pretty slow and shouldn't be used often.</remarks>
        public void ReadPixels(ref ColorRGB888[] pixelData, Rectangle? rectangle = null)
        {
            if (pixelData.Length < GetPixelBufferSize(rectangle))
                throw new ArgumentOutOfRangeException("Pixel array is too small");
            SDL_Rect? rect = rectangle?.ToSDL();
            var rectHandle = GCHandle.Alloc(rect, GCHandleType.Pinned);
            var pixelHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
            var width = rect?.w ?? OutputSize.Width;
            try
            {
                unsafe
                {
                    var pitch = width * 4;
                    Try(() => SDL_RenderReadPixels(Handle,
                        rectHandle.AddrOfPinnedObject(),
                        (uint)PixelFormat.Format_RGB888,
                        pixelHandle.AddrOfPinnedObject(),
                        pitch),
                        "SDL_RenderReadPixels");
                }
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
        public int GetPixelBufferSize(Rectangle? rectangle = null)
        {
            var size = OutputSize;
            return Texture.InternalGetPixelBufferSize(size.Width, size.Height, rectangle);
        }

        /// <summary>
        /// Sets the current drawing color.
        /// </summary>
        public void SetDrawingColor(Color color) =>
            Try(() =>
                SDL_SetRenderDrawColor(Handle, color.R, color.G, color.B, color.A),
                "SDL_SetRenderDrawColor");

        /// <summary>
        /// Gets the current drawing color.
        /// </summary>
        public Color GetDrawingColor()
        {
            byte r = 0; byte g = 0; byte b = 0; byte a = 0;
            Try(() =>
                SDL_GetRenderDrawColor(Handle, ref r, ref g, ref b, ref a),
                "SDL_GetRenderDrawColor");
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Clears the current render target with the drawing color.
        /// </summary>
        public void Clear() =>
            Try(() => SDL_RenderClear(Handle), "SDL_RenderClear");

        /// <summary>
        /// Clears the current render target with the specified color.
        /// </summary>
        public void Clear(Color color)
        {
            var oldColor = GetDrawingColor();
            SetDrawingColor(color);
            Clear();
            SetDrawingColor(oldColor);
        }

        /// <summary>
        /// Copies a portion of the texture to the current rendering target.
        /// </summary>
        /// <param name="texture">Texture to copy</param>
        /// <param name="srcRect">Source rectangle (null for copying whole texture)</param>
        /// <param name="dstRect">Destination rectangle (null to fill the entire render target)</param>
        public void Draw(Texture texture, Rectangle? srcRect = null,
            Rectangle? dstRect = null)
        {
            var srcR = srcRect?.ToSDL();
            var dstR = dstRect?.ToSDL();
            var src = GCHandle.Alloc(srcR, GCHandleType.Pinned);
            var dst = GCHandle.Alloc(dstR, GCHandleType.Pinned);
            try
            {
                unsafe
                {
                    Try(() => SDL_RenderCopy(Handle, texture.Handle,
                        (SDL_Rect*)src.AddrOfPinnedObject(),
                        (SDL_Rect*)dst.AddrOfPinnedObject()),
                        "SDL_RenderCopy");
                }
            }
            finally
            {
                src.Free();
                dst.Free();
            }
        }

        internal override void Destroy()
        {
            if (IsDisposed) return;
            base.Destroy();
            SDL_DestroyRenderer(Handle);
        }

        static Graphics() => Lifecycle.TryInitialize();
    }
}