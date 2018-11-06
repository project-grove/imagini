using System.Collections.Generic;

using static SDL2.SDL_error;
using static SDL2.SDL_pixels;
using static SDL2.SDL_render;

namespace Imagini.Drawing
{
    public sealed class RendererInfo
    {
        internal int Index { get; private set; } 

        private static List<RendererInfo> s_renderers = 
            new List<RendererInfo>();

        public string Name { get; private set; }
        public IReadOnlyCollection<PixelFormat> PixelFormats { get; private set; }

        /// <summary>
        /// Indicates whether this renderer is hardware accelerated.
        /// </summary>
        public bool IsHardwareAccelerated { get; private set; }

        /// <summary>
        /// Indicates whether this is a software renderer.
        /// </summary>
        public bool IsSoftware => !IsHardwareAccelerated;

        /// <summary>
        /// Indicates whether this renderer supports rendering to texture.
        /// </summary>
        public bool SupportsRenderingToTexture { get; private set; }

        /// <summary>
        /// Indicates whether this renderer supports refresh rate synchronization.
        /// </summary>
        public bool SupportsVSync { get; private set; }

        /// <summary>
        /// Returns the maximum supported texture width.
        /// </summary>
        public int MaxTextureWidth { get; private set; }
        /// <summary>
        /// Returns the maximum supported texture height.
        /// </summary>
        public int MaxTextureHeight { get; private set; }

        /// <summary>
        /// Returns the all available renderers.
        /// </summary>
        public static IReadOnlyCollection<RendererInfo> All => s_renderers;

        private RendererInfo(int index, SDL_RendererInfo info)
        {
            Index = index;
            Name = Util.FromNullTerminated(info.name);
            MaxTextureWidth = info.max_texture_width;
            MaxTextureHeight = info.max_texture_height;
            var flags = info.flags;
            IsHardwareAccelerated = flags.HasFlag(SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            SupportsRenderingToTexture = flags.HasFlag(SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE);
            SupportsVSync = flags.HasFlag(SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            var formats = new List<PixelFormat>((int)info.num_texture_formats);
            for (int i = 0; i < info.num_texture_formats; i++)
            {
                unsafe {
                    var format = *(info.texture_formats + i);
                    formats.Add((PixelFormat)format);
                }
            }
            PixelFormats = formats;
        }

        static RendererInfo()
        {
            Lifecycle.TryInitialize();
            int numRenderers = SDL_GetNumRenderDrivers();
            if (numRenderers < 0)
                throw new ImaginiException($"Could not obtain renderer info: {SDL_GetError()}");
            for (int i = 0; i < numRenderers; i++)
            {
                SDL_GetRenderDriverInfo(i, out SDL_RendererInfo info);
                s_renderers.Add(new RendererInfo(i, info));
            }
        }
    }
}