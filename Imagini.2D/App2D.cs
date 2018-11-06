using System;
using Imagini.Drawing;
using static SDL2.SDL_render;

namespace Imagini
{
    /// <summary>
    /// Main class which instantiates a window, an event loop and a 
    /// 2D accelerated renderer.
    /// </summary>
    public abstract class App2D : AppBase
    {
        /// <summary>
        /// Provides access to drawing functions for the app's window.
        /// </summary>
        public Graphics Graphics { get; private set; }

        /// <summary>
        /// Creates a new app with the specified window settings.
        /// </summary>
        /// <remarks>
        /// If you have your own constructor, make sure to call this
        /// one because it initializes the window and the event queue.
        /// </remarks>
        public App2D(WindowSettings settings = null, RendererInfo driver = null) : base(settings)
        {
            Graphics = new Graphics(Window, driver);
        }

        protected override void AfterDraw(TimeSpan frameTime)
        {
            SDL_RenderPresent(Graphics.Handle);
        }

        protected override void OnDispose()
        {
            Graphics.Destroy();
        }
    }

}