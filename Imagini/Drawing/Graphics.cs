using System;
using System.Linq;
using Imagini.Internal;
using static SDL2.SDL_error;
using static SDL2.SDL_render;

namespace Imagini.Drawing
{
    public sealed class Graphics
    {
        internal IntPtr Handle;
        public RendererInfo Renderer { get; private set; }
        internal Graphics(Window owner, RendererInfo rendererInfo)
        {
            var fallbackRenderer = 
                RendererInfo.All.FirstOrDefault(r => r.IsHardwareAccelerated) ??
                RendererInfo.All.First();
            
            Renderer = rendererInfo ?? fallbackRenderer;
            Handle = SDL_CreateRenderer(owner.Handle, Renderer.Index, 0);
            if (Handle == IntPtr.Zero)
                throw new ImaginiException($"Could not initialize renderer: {SDL_GetError()}");
        }

        /// <summary>
        /// Indicates if this object is disposed or not.
        /// </summary>
        public bool Disposed { get; private set; }
        internal void Dispose()
        {
            if (Disposed) return;
            SDL_DestroyRenderer(Handle);
            Disposed = true;
        }
    }
}