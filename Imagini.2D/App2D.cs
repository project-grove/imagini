using System;
using Imagini.Drawing;
using static SDL2.SDL_error;
using static SDL2.SDL_render;
using static SDL2.SDL_video;

namespace Imagini
{
    /// <summary>
    /// Main class which instantiates a window, an event loop and a 
    /// 2D accelerated renderer.
    /// </summary>
    public abstract class App2D : AppBase
    {
        /// <summary>
        /// Provides access to hardware accelerated drawing functions for the app's window.
        /// If software renderer is used, returns null.
        /// </summary>
        public Graphics Graphics { get; private set; }

        /// <summary>
        /// Returns the window surface used for software rendering.
        /// If a hardware renderer is used, returns null.
        /// </summary>
        public Surface Surface { get; private set; }

        /// <summary>
        /// Gets or sets if this app uses hardware acceleration.
        /// </summary>
        /// <returns></returns>
        public bool IsHardwareAccelerated { get; private set; }

        /// <summary>
        /// Creates a new app with the specified window settings.
        /// </summary>
        /// <remarks>
        /// If you have your own constructor, make sure to call this
        /// one because it initializes the window and the event queue.
        /// </remarks>
        public App2D(WindowSettings settings = null, RendererInfo driver = null) : base(settings)
        {
            IsHardwareAccelerated = driver?.IsHardwareAccelerated ?? true;
            if (IsHardwareAccelerated)
                Graphics = new Graphics(Window, driver);
            else
            {
                var handle = SDL_GetWindowSurface(Window.Handle);
                if (handle == IntPtr.Zero)
                    throw new ImaginiException($"Could not obtain window surface: {SDL_GetError()}");
                Surface = new Surface(handle);
            } 
        }

        protected override void AfterDraw(TimeSpan frameTime)
        {
            if (IsHardwareAccelerated)
                SDL_RenderPresent(Graphics.Handle);
            else
                SDL_UpdateWindowSurface(Window.Handle);
        }

        protected override void OnDispose()
        {
            Graphics?.Destroy();
            Surface?.Dispose();
        }
    }

}