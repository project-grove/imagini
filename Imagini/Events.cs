using System;
using static SDL2.SDL_events;

namespace Imagini
{
    /// <summary>
    /// Allows subscribing to the app events.
    /// </summary>
    public class Events
    {
        private static Events _global = new Events();
        /// <summary>
        /// Returns the global event handler.
        /// </summary>
        public static Events Global => _global;

        public readonly WindowEvents Window = new WindowEvents();
        public readonly KeyboardEvents Keyboard = new KeyboardEvents();

        internal Events() { }

        internal unsafe void Process(SDL_Event e)
        {
            // TODO
            switch ((SDL_EventType)e.type)
            {
                case SDL_EventType.SDL_WINDOWEVENT:
                    Window.Fire(*((SDL_WindowEvent*)&e));
                    break;
                case SDL_EventType.SDL_KEYDOWN:
                case SDL_EventType.SDL_KEYUP:
                    Keyboard.Fire(*((SDL_KeyboardEvent*)&e));
                    break; // the keyboard is on fire!
            }
        }

        public class WindowEvents
        {
            public event EventHandler<WindowStateChangeEventArgs> StateChanged;
            internal WindowEvents() { }
            internal void Fire(SDL_WindowEvent e) =>
                StateChanged?.Invoke(this, new WindowStateChangeEventArgs(e));
        }

        public class KeyboardEvents
        {
            public event EventHandler<KeyboardEventArgs> KeyDown;
            public event EventHandler<KeyboardEventArgs> KeyUp;
            internal KeyboardEvents() { }
            internal void Fire(SDL_KeyboardEvent e)
            {
                if (e.type == (uint)SDL_EventType.SDL_KEYDOWN)
                    KeyDown?.Invoke(this, new KeyboardEventArgs(e));
                else KeyUp?.Invoke(this, new KeyboardEventArgs(e));
            }
        }
    }

}