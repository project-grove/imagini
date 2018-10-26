using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Imagini;
using Xunit;

namespace Tests
{
    public class EventsTest : IDisposable
    {
        readonly Window window;
        readonly EventManager.EventQueue eventQueue;
        readonly Events events = new Events();

        public EventsTest()
        {
            window = new Window(new WindowSettings()
            {
                WindowWidth = 100,
                WindowHeight = 100
            });
            eventQueue = EventManager.CreateQueueFor(window);
        }

        [Fact]
        public void ShouldNotChangeEventData()
        {
            window.Raise();
            // Drop all existing events before pushing ours
            EventManager.Poll();
            eventQueue.ProcessAll(events);

            var expected = new List<CommonEventArgs>()
            {
                new WindowStateChangeEventArgs(WindowStateChange.None),
                new KeyboardEventArgs(new KeyboardKey() { Keycode = Keycode.AC_HOME }, true),
                new TextEditingEventArgs("平", 0, 1),
                new TextInputEventArgs("Never gonna give you up"),
                new MouseButtonEventArgs(MouseButton.X2, 10000, 10000, true),
                new MouseWheelEventArgs(10, 10),
                new JoyAxisMotionEventArgs(1337, 0, 10000),
                new JoyBallMotionEventArgs(1337, 0, 10, 10),
                new JoyHatMotionEventArgs(1337, 0, HatPosition.Up),
                new JoyButtonEventArgs(1337, 0, true),
                new JoyDeviceStateEventArgs(1337, true),
                new ControllerAxisEventArgs(1337, ControllerAxis.LeftX, 10000),
                new ControllerButtonEventArgs(1337, ControllerButton.X, true),
                new ControllerDeviceStateEventArgs(1337, true),
                new TouchFingerEventArgs(TouchEventType.FingerDown, 1337, 0, 
                    10.0f, 10.0f, 5.0f, 5.0f, 1.0f)
            };
            var actual = new List<CommonEventArgs>();
            events.Window.StateChanged += (s, args) => actual.Add(args);
            events.Input.OnTextEdit += (s, args) => actual.Add(args);
            events.Input.OnTextInput += (s, args) => actual.Add(args);
            events.Keyboard.KeyPressed += (s, args) => actual.Add(args);
            events.Mouse.MouseButtonPressed += (s, args) => actual.Add(args);
            events.Mouse.MouseWheelScrolled += (s, args) => actual.Add(args);
            events.Joystick.AxisMoved += (s, args) => actual.Add(args);
            events.Joystick.BallMoved += (s, args) => actual.Add(args);
            events.Joystick.HatMoved += (s, args) => actual.Add(args);
            events.Joystick.ButtonPressed += (s, args) => actual.Add(args);
            Events.Global.Joystick.StateChanged += (s, args) => actual.Add(args);
            events.Controller.AxisMoved += (s, args) => actual.Add(args);
            events.Controller.ButtonPressed += (s, args) => actual.Add(args);
            Events.Global.Controller.StateChanged += (s, args) => actual.Add(args);
            events.Touch.FingerPressed += (s, args) => actual.Add(args);

            // Push all our custom events to global queue and process them
            foreach(var e in expected)
                EventManager.Push(e);
            EventManager.Poll();
            eventQueue.ProcessAll(events);

            // Sort the events by type alphabetically to aid assertion
            actual = actual.OrderBy(e => e.GetType().ToString()).ToList();
            expected = expected.OrderBy(e => e.GetType().ToString()).ToList();

            actual.Should().BeEquivalentTo(expected, options => options
                .IncludingAllRuntimeProperties()
                .Excluding(m => m.SelectedMemberPath.EndsWith(".Timestamp")));
        }

        public void Dispose()
        {
            EventManager.DeleteQueueFor(window);
            window.Dispose();
        }
    }
}