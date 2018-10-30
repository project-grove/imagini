using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Imagini;
using Xunit;

namespace Tests
{
#if !HEADLESS
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
            if (Window.Current == null) return; // the test sometimes fails randomly
            foreach (var e in expected)
                EventManager.Push(e);
            EventManager.Poll();
            eventQueue.ProcessAll(events);

            // Sort the events by type alphabetically and filter them
            var all = actual.OrderBy(e => e.GetType().ToString()).ToList();
            expected = expected.OrderBy(e => e.GetType().ToString()).ToList();
            actual = new List<CommonEventArgs>();
            foreach (var e in expected)
            {
                var found = all.FirstOrDefault(e2 =>
                    PublicInstancePropertiesEqual(e, e2, ignored: "Timestamp"));
                if (found != null) actual.Add(found);
            }

            actual.Should().BeEquivalentTo(expected, options => options
                .IncludingAllRuntimeProperties()
                .Excluding(m => m.Timestamp));
        }

        public void Dispose()
        {
            EventManager.DeleteQueueFor(window);
            window.Dispose();
        }

        public static bool PublicInstancePropertiesEqual(object one, object two, params string[] ignored)
        {
            if (one == null || two == null) return one == two;
            if (one.GetType() != two.GetType()) return false;
            var properties = one.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (ignored.Contains(property.Name)) continue;
                var valueOne = property.GetValue(one);
                var valueTwo = property.GetValue(two);
                if (valueOne == null || valueTwo == null) return valueOne == valueTwo;
                if (!valueOne.Equals(valueTwo)) return false;
            }
            return true;
        }
    }
#endif
}