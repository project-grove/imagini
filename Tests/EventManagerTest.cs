using System;
using Imagini;
using Xunit;
using static SDL2.SDL_events;
using static SDL2.SDL_keyboard;
using static SDL2.SDL_scancode;

namespace Tests
{
#if !HEADLESS
    [DisplayTestMethodName]
    public class EventManagerTest : IDisposable
    {
        private Window window;
        private EventManager.EventQueue queue;

        private const uint eventType = (uint)SDL_EventType.SDL_KEYDOWN;

        private CommonEventArgs CreateEvent(Window window) =>
            new KeyboardEventArgs(new KeyboardKey(), true, window);

        public EventManagerTest()
        {
            window = new Window(new WindowSettings()
            {
                IsVisible = false
            });
            queue = EventManager.CreateQueueFor(window);
        }

        [Fact]
        public void ShouldPushEvents()
        {
            
            var @event = CreateEvent(window);
            EventManager.Push(@event);
            EventManager.Poll();
            Assert.Contains(queue.Events, e => e.type == eventType);
        }

        [Fact]
        public void ShouldPushEventsToCorrespondingWindows()
        {
            
            var window2 = new Window(new WindowSettings());
            var queue2 = EventManager.CreateQueueFor(window2);    
            var @event = CreateEvent(window2);

            EventManager.Push(@event);
            EventManager.Poll();
            Assert.DoesNotContain(queue.Events, e => e.type == eventType);
            Assert.Contains(queue2.Events, e => e.type == eventType);

            EventManager.DeleteQueueFor(window2);
            window2.Destroy();
        }


        [Fact]
        public void ShouldPushGlobalEventsToGlobalQueue()
        {
            var @event = new JoyDeviceStateEventArgs(1337, true);
            EventManager.Push(@event);
            EventManager.Poll(suppressGlobalProcessing: true);
            Assert.Contains(EventManager.GlobalQueue.Events, 
                e => e.type == @event.AsEvent().type);
        }

        public void Dispose()
        {
            EventManager.DeleteQueueFor(window);
            window.Destroy();
        }
    }
#endif
}