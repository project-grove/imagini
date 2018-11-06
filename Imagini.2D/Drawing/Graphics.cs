using System;
using System.Linq;

using static SDL2.SDL_error;
using static SDL2.SDL_render;

namespace Imagini.Drawing
{
    public sealed class Graphics : Resource
    {
        internal IntPtr Handle;
        public RendererInfo Renderer { get; private set; }
        internal Graphics(Window owner, RendererInfo rendererInfo)
            : base(nameof(Graphics))
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

        internal override void Destroy()
        {
            if (IsDisposed) return;
            base.Destroy();
            SDL_DestroyRenderer(Handle);
        }

        static Graphics() => Lifecycle.TryInitialize();
    }
}