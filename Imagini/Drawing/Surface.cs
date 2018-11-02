using static SDL2.SDL_surface;

namespace Imagini.Drawing
{
    /// <summary>
    /// Defines a surface which stores it's pixel data in
    /// the RAM.
    /// </summary>
    public sealed class Surface : Resource
    {
        internal SDL_Surface Handle;
        /// <summary>
        /// Returns the renderer which owns this surface.
        /// </summary>
        public Graphics Owner { get; private set; }

        internal Surface(SDL_Surface handle, Graphics owner)
            : base(nameof(Surface)) =>
            (this.Handle, this.Owner) = (handle, owner);
        
        // TODO

        internal override void Destroy()
        {
            if (IsDisposed) return;
            SDL_FreeSurface(ref Handle);
        }
    }
}