using System;
using Imagini.Internal;
using static SDL2.SDL_error;
using static SDL2.SDL_render;

namespace Imagini.Drawing
{
    public sealed class Graphics 
    {
        internal IntPtr Handle;
        internal Graphics(Window owner)
        {
            Handle = SDL_CreateRenderer(owner.Handle, -1, 0);
            if (Handle == IntPtr.Zero)
                throw new InternalException($"Could not initialize renderer: {SDL_GetError()}");
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