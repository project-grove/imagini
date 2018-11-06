using System;
using static SDL2.SDL_render;
using static SDL2.SDL_surface;

namespace Imagini.Drawing
{
    /// <summary>
    /// Defines a texture which stores it's data on GPU.
    /// </summary>
    public sealed class Texture : Resource
    {
        internal IntPtr Handle;
        /// <summary>
        /// Returns the renderer which owns this texture.
        /// </summary>
        public Graphics Owner { get; private set; }

        internal Texture(IntPtr handle, Graphics owner) : base(nameof(Texture)) =>
            (this.Handle, this.Owner) = (handle, owner);

        internal override void Destroy()
        {
            if (IsDisposed) return;
            SDL_DestroyTexture(Handle);
        }

        static Texture() => Lifecycle.TryInitialize();
    }
}