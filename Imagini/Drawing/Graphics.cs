using System;
using Imagini.Internal;
using static SDL2.SDL_render;

namespace Imagini.Drawing
{
    public sealed class Graphics 
    {
        internal IntPtr Handle;
        internal Graphics(Window owner) => Handle = owner.Handle;

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