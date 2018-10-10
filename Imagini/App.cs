using System;
using Imagini.Internal;
/// <summary>
/// The core namespace.
/// </summary>
namespace Imagini
{
    /// <summary>
    /// Main class which instantiates a window and event loop.
    /// </summary>
    public abstract class App : IDisposable
    {
        public event EventHandler Activated;
        public event EventHandler Deactivated;
        public event EventHandler Disposed;
        public event EventHandler Exiting;

        private Window _window;
        private EventManager.EventQueue _eventQueue;

        public App(WindowSettings windowSettings = null)
        {
            if (windowSettings == null)
                windowSettings = new WindowSettings();
            _window = new Window(windowSettings);
            _eventQueue = EventManager.CreateQueueFor(_window);
        }

        public void Dispose()
        {
            EventManager.DeleteQueueFor(_window);
            _window.Dispose();
        }
    }
}