using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Imagini.ErrorHandler;
using static SDL2.SDL_blendmode;
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

        [ExcludeFromCodeCoverage]
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
        /// <see cref="TextureAccess" /> is static.
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
            var rectHandle = Pin(rect);
            var pixelHandle = Pin(pixelData);
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
        /// Sets the blend mode.
        /// </summary>
        public void SetBlendMode(BlendMode mode) =>
            Try(() => SDL_SetRenderDrawBlendMode(Handle, (SDL_BlendMode)mode),
                "SDL_SetRenderDrawBlendMode");

        /// <summary>
        /// Gets the blend mode.
        /// </summary>
        public BlendMode GetBlendMode()
        {
            SDL_BlendMode val = SDL_BlendMode.SDL_BLENDMODE_NONE;
            Try(() => SDL_GetRenderDrawBlendMode(Handle, ref val),
                "SDL_GetRenderDrawBlendMode");
            return (BlendMode)val;
        }

        /// <summary>
        /// Sets the clipping rectangle.
        /// </summary>
        /// <param name="rect">Clipping rectangle. If null, disables clipping</param>
        public void SetClipRectangle(Rectangle? rect)
        {
            var rectHandle = Pin(rect?.ToSDL());
            try
            {
                unsafe
                {
                    Try(() => SDL_RenderSetClipRect(Handle,
                        (SDL_Rect*)rectHandle.AddrOfPinnedObject()),
                        "SDL_RenderSetClipRect");
                }
            }
            finally
            {
                rectHandle.Free();
            }
        }

        /// <summary>
        /// Gets the clip rectangle, or null if clipping is disabled.
        /// </summary>
        public Rectangle? GetClipRectangle()
        {
            unsafe
            {
                var p = stackalloc int[4];
                SDL_RenderGetClipRect(Handle, (SDL_Rect*)p);
                var value = new Rectangle(
                    *p,
                    *(p + 1),
                    *(p + 2),
                    *(p + 3)
                );
                if (value.Size == Size.Empty)
                    return null;
                return value;
            }
        }

        /// <summary>
        /// Sets the viewport.
        /// </summary>
        /// <param name="rect">Viewport rectangle. If null, whole target is used</param>
        public void SetViewport(Rectangle? rect)
        {
            var rectHandle = Pin(rect?.ToSDL());
            try
            {
                unsafe
                {
                    Try(() => SDL_RenderSetViewport(Handle,
                        (SDL_Rect*)rectHandle.AddrOfPinnedObject()),
                        "SDL_RenderSetViewport");
                }
            }
            finally
            {
                rectHandle.Free();
            }
        }

        /// <summary>
        /// Gets the current viewport bounds.
        /// </summary>
        public Rectangle GetViewport()
        {
            unsafe
            {
                var p = stackalloc int[4];
                SDL_RenderGetViewport(Handle, (SDL_Rect*)p);
                var value = new Rectangle(
                    *p,
                    *(p + 1),
                    *(p + 2),
                    *(p + 3)
                );
                return value;
            }
        }

        /// <summary>
        /// Sets the drawing scale factor for the current target.
        /// </summary>
        public void SetScale(float scaleX, float scaleY) =>
            Try(() => SDL_RenderSetScale(Handle, scaleX, scaleY),
                "SDL_RenderSetScale");

        /// <summary>
        /// Sets the drawing scale factor for the current target.
        /// </summary>
        public void SetScale(SizeF scale) =>
            SetScale(scale.Width, scale.Height);

        /// <summary>
        /// Gets the drawing scale factor for the current target.
        /// </summary>
        public void GetScale(out float scaleX, out float scaleY)
        {
            var sX = 0.0f;
            var sY = 0.0f;
            SDL_RenderGetScale(Handle, ref sX, ref sY);
            scaleX = sX; scaleY = sY;
        }

        /// <summary>
        /// Gets the drawing scale factor for the current target.
        /// </summary>
        public SizeF GetScale()
        {
            GetScale(out float scaleX, out float scaleY);
            return new SizeF(scaleX, scaleY);
        }

        /// <summary>
        /// Sets a device independent resolution for rendering.
        /// </summary>
        public void SetLogicalSize(int width, int height) =>
            Try(() => SDL_RenderSetLogicalSize(Handle, width, height),
                "SDL_RenderSetLogicalSize");

        /// <summary>
        /// Sets a device independent resolution for rendering.
        /// </summary>
        public void SetLogicalSize(Size size) =>
            SetLogicalSize(size.Width, size.Height);

        /// <summary>
        /// Gets a device independent resolution for rendering. If it was never
        /// set, returns zeros.
        /// </summary>
        public void GetLogicalSize(out int width, out int height)
        {
            var w = 0; var h = 0;
            SDL_RenderGetLogicalSize(Handle, ref w, ref h);
            width = w; height = h;
        }

        /// <summary>
        /// Gets a device independent resolution for rendering. If it was never
        /// set, returns zeros.
        /// </summary>
        public Size GetLogicalSize()
        {
            GetLogicalSize(out int w, out int h);
            return new Size(w, h);
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
            var src = Pin(srcR);
            var dst = Pin(dstR);
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

        /// <summary>
        /// Copies a portion of the texture to the current rendering target.
        /// </summary>
        /// <param name="texture">Texture to copy</param>
        /// <param name="srcRect">Source rectangle (null for copying whole texture)</param>
        /// <param name="dstRect">Destination rectangle (null to fill the entire render target)</param>
        /// <param name="angle">
        /// Angle in degrees that indicates the rotation that will be applied
        /// to dstRect, rotating it in a clockwise direction
        /// </param>
        /// <param name="center">
        /// Point around which dstRect will be rotated (if null, rotation will be
        /// done around dstRect's center)
        /// </param>
        /// <param name="flip">
        /// Flipping actions that should be performed on the texture
        /// </param>
        public void Draw(Texture texture, Rectangle? srcRect,
            Rectangle? dstRect, double angle = 0.0,
            Point? center = null, TextureFlip flip = TextureFlip.None)
        {
            var srcR = Pin(srcRect?.ToSDL());
            var dstR = Pin(dstRect?.ToSDL());
            var cntr = Pin(center?.ToSDL());
            try
            {
                unsafe
                {
                    Try(() => SDL_RenderCopyEx(Handle, texture.Handle,
                        (SDL_Rect*)srcR.AddrOfPinnedObject(),
                        (SDL_Rect*)dstR.AddrOfPinnedObject(),
                        angle,
                        (SDL_Point*)cntr.AddrOfPinnedObject(),
                        (SDL_RendererFlip)flip),
                        "SDL_RenderCopyEx");
                }
            }
            finally
            {
                srcR.Free();
                dstR.Free();
                cntr.Free();
            }
        }

        /// <summary>
        /// Draws a line between two points with the current color.
        /// </summary>
        public void DrawLine(int x1, int y1, int x2, int y2) =>
            Try(() =>
                SDL_RenderDrawLine(Handle, x1, y1, x2, y2),
                "SDL_RenderDrawLine");

        /// <summary>
        /// Draws a line between two points with the current color.
        /// </summary>
        public void DrawLine(Point from, Point to) =>
            DrawLine(from.X, from.Y, to.X, to.Y);

        /// <summary>
        /// Draws a series of connected lines between the specified points with
        /// the current color.
        /// </summary>
        public void DrawLines(params Point[] points)
        {
            unsafe
            {
                var p = Pin(points);
                try
                {
                    Try(() => SDL_RenderDrawLines(Handle,
                        (SDL_Point*)p.AddrOfPinnedObject(), points.Length),
                        "SDL_RenderDrawLines");
                }
                finally
                {
                    p.Free();
                }
            }
        }

        /// <summary>
        /// Draws a point at the specified coordinates using current color.
        /// </summary>
        public void DrawPoint(int x, int y) =>
            Try(() => SDL_RenderDrawPoint(Handle, x, y),
                "SDL_RenderDrawPoint");

        /// <summary>
        /// Draws a point at the specified coordinates using current color.
        /// </summary>
        public void DrawPoint(Point point) =>
            DrawPoint(point.X, point.Y);

        /// <summary>
        /// Draws points at the specified coordinates using current color.
        /// </summary>
        public void DrawPoints(params Point[] points)
        {
            unsafe
            {
                var p = Pin(points);
                try
                {
                    Try(() => SDL_RenderDrawPoints(Handle,
                        (SDL_Point*)p.AddrOfPinnedObject(), points.Length),
                        "SDL_RenderDrawPoints");
                }
                finally
                {
                    p.Free();
                }
            }
        }

        /// <summary>
        /// Draws a rectangle outline with the current color.
        /// </summary>
        /// <param name="rectangle">Rectangle to draw outline for. If null,
        /// outlines a whole target.</param>
        public void DrawRect(Rectangle? rectangle)
        {
            unsafe
            {
                var p = Pin(rectangle);
                try
                {
                    Try(() => 
                        SDL_RenderDrawRect(Handle, (SDL_Rect*)p.AddrOfPinnedObject()),
                        "SDL_RenderDrawRect");
                }
                finally
                {
                    p.Free();
                }
            }
        }

        /// <summary>
        /// Draws a rectangle outline with the current color.
        /// </summary>
        /// <param name="rectangles">Rectangles to draw outline for</param>
        public void DrawRects(params Rectangle[] rectangles)
        {
            unsafe
            {
                var p = Pin(rectangles);
                try
                {
                    Try(() => 
                        SDL_RenderDrawRects(Handle, 
                            (SDL_Rect*)p.AddrOfPinnedObject(), rectangles.Length),
                        "SDL_RenderDrawRects");
                }
                finally
                {
                    p.Free();
                }
            }
        }

        /// <summary>
        /// Fills a rectangle outline with the current color.
        /// </summary>
        /// <param name="rectangle">Rectangle to fill. If null,
        /// fills a whole target.</param>
        public void FillRect(Rectangle? rectangle)
        {
            unsafe
            {
                var p = Pin(rectangle);
                try
                {
                    Try(() => 
                        SDL_RenderFillRect(Handle, (SDL_Rect*)p.AddrOfPinnedObject()),
                        "SDL_RenderFillRect");
                }
                finally
                {
                    p.Free();
                }
            }
        }

        /// <summary>
        /// Fills rectangles with the current color.
        /// </summary>
        /// <param name="rectangles">Rectangles to fill</param>
        public void FillRects(params Rectangle[] rectangles)
        {
            unsafe
            {
                var p = Pin(rectangles);
                try
                {
                    Try(() => 
                        SDL_RenderFillRects(Handle, 
                            (SDL_Rect*)p.AddrOfPinnedObject(), rectangles.Length),
                        "SDL_RenderFillRects");
                }
                finally
                {
                    p.Free();
                }
            }
        }

        internal override void Destroy()
        {
            if (IsDisposed) return;
            base.Destroy();
            SDL_DestroyRenderer(Handle);
        }

        static Graphics() => Lifecycle.TryInitialize();

        // TODO Explore possibility of using stackalloc instead of pinning

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static GCHandle Pin(object obj) => GCHandle.Alloc(obj, GCHandleType.Pinned);
    }
}