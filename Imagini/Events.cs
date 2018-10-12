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
        public readonly MouseEvents Mouse = new MouseEvents();

        internal Events() { }

        internal unsafe void Process(SDL_Event e)
        {
            // TODO
            switch ((SDL_EventType)e.type)
            {
                // Window
                case SDL_EventType.SDL_WINDOWEVENT:
                    Window.Fire(*((SDL_WindowEvent*)&e));
                    break;
                // Keyboard
                case SDL_EventType.SDL_KEYDOWN:
                case SDL_EventType.SDL_KEYUP:
                    Keyboard.Fire(*((SDL_KeyboardEvent*)&e));
                    break; // the keyboard is on fire!
                // Mouse
                case SDL_EventType.SDL_MOUSEMOTION:
                    Mouse.Fire(*((SDL_MouseMotionEvent*)&e));
                    break;
                case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL_EventType.SDL_MOUSEBUTTONUP:
                    Mouse.Fire(*((SDL_MouseButtonEvent*)&e));
                    break;
                case SDL_EventType.SDL_MOUSEWHEEL:
                    Mouse.Fire(*((SDL_MouseWheelEvent*)&e));
                    break;
            }
        }

        /// <summary>
        /// Contains window-related events.
        /// </summary>
        public class WindowEvents
        {
            /// <summary>
            /// Fired when a window changes state.
            /// </summary>
            public event EventHandler<WindowStateChangeEventArgs> StateChanged;
            internal WindowEvents() { }
            internal void Fire(SDL_WindowEvent e) =>
                StateChanged?.Invoke(this, new WindowStateChangeEventArgs(e));
        }

        /// <summary>
        /// Contains keyboard-related events.
        /// </summary>
        public class KeyboardEvents
        {
            /// <summary>
            /// Fired when a keyboard key is pressed.
            /// </summary>
            public event EventHandler<KeyboardEventArgs> KeyPressed;
            /// <summary>
            /// Fired when a keyboard key is released.
            /// </summary>
            public event EventHandler<KeyboardEventArgs> KeyReleased;

            internal KeyboardEvents() { }
            internal void Fire(SDL_KeyboardEvent e)
            {
                if (e.type == (uint)SDL_EventType.SDL_KEYDOWN)
                    KeyPressed?.Invoke(this, new KeyboardEventArgs(e));
                else KeyReleased?.Invoke(this, new KeyboardEventArgs(e));
            }
        }

        /// <summary>
        /// Contains mouse-related events.
        /// </summary>
        public class MouseEvents
        {
            /// <summary>
            /// Fires when a mouse is moved.
            /// </summary>
            public event EventHandler<MouseMoveEventArgs> MouseMoved;
            /// <summary>
            /// Fires when a mouse button is pressed.
            /// </summary>
            public event EventHandler<MouseButtonEventArgs> MouseButtonPressed;
            /// <summary>
            /// Fires when a mouse button is released.
            /// </summary>
            public event EventHandler<MouseButtonEventArgs> MouseButtonReleased;

            public event EventHandler<MouseWheelEventArgs> MouseWheelScrolled;

            internal void Fire(SDL_MouseMotionEvent e) =>
                MouseMoved?.Invoke(this, new MouseMoveEventArgs(e));
            
            internal void Fire(SDL_MouseButtonEvent e)
            {
                if (e.type == (uint)SDL_EventType.SDL_MOUSEBUTTONDOWN)
                    MouseButtonPressed?.Invoke(this, new MouseButtonEventArgs(e));
                else
                    MouseButtonReleased?.Invoke(this, new MouseButtonEventArgs(e));
            }

            internal void Fire(SDL_MouseWheelEvent e) =>
                MouseWheelScrolled?.Invoke(this, new MouseWheelEventArgs(e));
        }
    }

}