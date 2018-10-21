using System;
using Imagini.Internal;
using static SDL2.SDL_timer;

/// <summary>
/// The core namespace.
/// </summary>
namespace Imagini
{
    /// <summary>
    /// Main class which instantiates a window and an event loop.
    /// </summary>
    public abstract class App : IDisposable
    {
        public Window Window { get; private set; }
        private EventManager.EventQueue _eventQueue;
        public Events Events { get; private set; }

        /// <summary>
        /// Returns total number of milliseconds since when the library was initialized.
        /// </summary>
        public static long TotalTime
        {
            get => SDL_GetTicks();
        }

        /// <summary>
        /// Creates a new app and initializes it.
        /// </summary>
        public App(WindowSettings windowSettings = null)
        {
            if (windowSettings == null)
                windowSettings = new WindowSettings();
            Window = new Window(windowSettings);
            _eventQueue = EventManager.CreateQueueFor(Window);
            Events = new Events(this);
            Initialize();
        }

        /// <summary>
        /// Called after object constructor. Use it for the initialization logic.
        /// </summary>
        public abstract void Initialize();

        protected virtual void ProcessEvents()
        {
            EventManager.Poll();
            _eventQueue.ProcessAll(Events);
        }

        public void Dispose()
        {
            EventManager.DeleteQueueFor(Window);
            Window.Dispose();
        }

        // TODO
    }
}