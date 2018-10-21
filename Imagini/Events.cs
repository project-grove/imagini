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
        public readonly JoystickEvents Joystick = new JoystickEvents();

        /// <summary>
        /// Returns the owner of this event queue, or null if this queue is global.
        /// </summary>
        public App Owner { get; private set; }

        internal Events() { }
        internal Events(App owner) => Owner = owner;

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
                    // the keyboard is on fire, we should probably take a break
                    break;
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
                // Joystick
                case SDL_EventType.SDL_JOYAXISMOTION:
                    Joystick.Fire(*((SDL_JoyAxisEvent*)&e));
                    break;
                case SDL_EventType.SDL_JOYBALLMOTION:
                    Joystick.Fire(*((SDL_JoyBallEvent*)&e));
                    break;
                case SDL_EventType.SDL_JOYHATMOTION:
                    Joystick.Fire(*((SDL_JoyHatEvent*)&e));
                    break;
                case SDL_EventType.SDL_JOYBUTTONUP:
                case SDL_EventType.SDL_JOYBUTTONDOWN:
                    Joystick.Fire(*((SDL_JoyButtonEvent*)&e));
                    break;
                case SDL_EventType.SDL_JOYDEVICEADDED:
                case SDL_EventType.SDL_JOYDEVICEREMOVED:
                    Joystick.Fire(*((SDL_JoyDeviceEvent*)&e));
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

        /// <summary>
        /// Contains joystick-related events.
        /// </summary>
        public class JoystickEvents
        {
            /// <summary>
            /// Fires when a joystick axis is moved.
            /// </summary>
            public event EventHandler<JoyAxisMotionEventArgs> AxisMoved;
            /// <summary>
            /// Fires when a joystick trackball is moved.
            /// </summary>
            public event EventHandler<JoyBallMotionEventArgs> BallMoved;
            /// <summary>
            /// Fires when a joystick hat is moved.
            /// </summary>
            public event EventHandler<JoyHatMotionEventArgs> HatMoved;
            /// <summary>
            /// Fires when a joystick button is pressed.
            /// </summary>
            public event EventHandler<JoyButtonEventArgs> ButtonPressed;
            /// <summary>
            /// Fires when a joystick button is released.
            /// </summary>
            public event EventHandler<JoyButtonEventArgs> ButtonReleased;

            /// <summary>
            /// Fires when a joystick is connected or disconnected.
            /// </summary>
            /// <remarks>Fires only on <see cref="Events.Global" />.</remarks>
            public event EventHandler<JoyDeviceStateEventArgs> StateChanged;

            internal void Fire(SDL_JoyAxisEvent e) =>
                AxisMoved?.Invoke(this, new JoyAxisMotionEventArgs(e));
            
            internal void Fire(SDL_JoyBallEvent e) =>
                BallMoved?.Invoke(this, new JoyBallMotionEventArgs(e));
            
            internal void Fire(SDL_JoyHatEvent e) =>
                HatMoved?.Invoke(this, new JoyHatMotionEventArgs(e));
            
            internal void Fire(SDL_JoyButtonEvent e)
            {
                if (e.type == (uint)SDL_EventType.SDL_JOYBUTTONDOWN)
                    ButtonPressed?.Invoke(this, new JoyButtonEventArgs(e));
                else
                    ButtonReleased?.Invoke(this, new JoyButtonEventArgs(e));
            }

            internal void Fire(SDL_JoyDeviceEvent e) =>
                StateChanged?.Invoke(this, new JoyDeviceStateEventArgs(e));
        }
    }

}