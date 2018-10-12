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
        private Window _window;
        private EventManager.EventQueue _eventQueue;
        public Events Events { get; private set; }

        /// <summary>
        /// Returns total number of milliseconds since when the library was initialized.
        /// </summary>
        public static long TotalTime
        {
            get => SDL_GetTicks();
        }

        public App(WindowSettings windowSettings = null)
        {
            if (windowSettings == null)
                windowSettings = new WindowSettings();
            _window = new Window(windowSettings);
            _eventQueue = EventManager.CreateQueueFor(_window);
            Events = new Events();
        }

        protected virtual void ProcessEvents()
        {
            EventManager.Poll();
            _eventQueue.ProcessAll(Events);
        }

        public void Dispose()
        {
            EventManager.DeleteQueueFor(_window);
            _window.Dispose();
        }

        // TODO
    }
}