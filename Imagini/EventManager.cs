using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Imagini.Internal;
using static SDL2.SDL_error;
using static SDL2.SDL_events;
using static SDL2.SDL_video;

namespace Imagini
{
    /// <summary>
    /// Represents an event manager.
    /// </summary>
    public static class EventManager
    {
        private const uint GLOBAL_QUEUE_ID = 0;

        static EventManager() => Lifecycle.TryInitialize();
        private static Dictionary<uint, EventQueue> _queues =
            new Dictionary<uint, EventQueue>() {
                { GLOBAL_QUEUE_ID, new EventQueue() }
            };

        internal static EventQueue CreateQueueFor(Window window)
        {
            var id = window.ID;
            if (_queues.ContainsKey(id)) return _queues[id];
            var queue = new EventQueue();
            _queues.Add(id, queue);
            return queue;
        }

        internal static void DeleteQueueFor(Window window) =>
            _queues.Remove(window.ID);
        
        internal static EventQueue GlobalQueue => _queues[GLOBAL_QUEUE_ID];

        /// <summary>
        /// Gathers all available events and distributes them to the
        /// corresponding queues.
        /// </summary>
        /// <param name="suppressGlobalProcessing">
        /// If true, global event queue will not be processed after calling this method.
        /// </param>
        public unsafe static void Poll(bool suppressGlobalProcessing = false)
        {
            while (SDL_PollEvent(out SDL_Event @event) != 0)
            {
                byte* data = (byte*)&@event;
                switch ((SDL_EventType)@event.type)
                {
                    // uint32 type, uint32 timestamp, uint32 windowID
                    case SDL_EventType.SDL_WINDOWEVENT:
                    case SDL_EventType.SDL_KEYDOWN:
                    case SDL_EventType.SDL_KEYUP:
                    case SDL_EventType.SDL_TEXTEDITING:
                    case SDL_EventType.SDL_TEXTINPUT:
                    case SDL_EventType.SDL_MOUSEMOTION:
                    case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    case SDL_EventType.SDL_MOUSEBUTTONUP:
                    case SDL_EventType.SDL_MOUSEWHEEL:
                    case SDL_EventType.SDL_USEREVENT:
                        var windowID = (uint)*(data + 8);
                        PushTo(windowID, @event);
                        break;
                    case SDL_EventType.SDL_JOYAXISMOTION:
                    case SDL_EventType.SDL_JOYBALLMOTION:
                    case SDL_EventType.SDL_JOYHATMOTION:
                    case SDL_EventType.SDL_JOYBUTTONDOWN:
                    case SDL_EventType.SDL_JOYBUTTONUP:
                    case SDL_EventType.SDL_CONTROLLERAXISMOTION:
                    case SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
                    case SDL_EventType.SDL_CONTROLLERBUTTONUP:
                    case SDL_EventType.SDL_FINGERDOWN:
                    case SDL_EventType.SDL_FINGERUP:
                    case SDL_EventType.SDL_FINGERMOTION:
                    case SDL_EventType.SDL_DOLLARGESTURE:
                    case SDL_EventType.SDL_DOLLARRECORD:
                    case SDL_EventType.SDL_MULTIGESTURE:
                        PushToCurrent(@event);
                        break;
                    case SDL_EventType.SDL_DROPFILE:
                        windowID = (uint)*(data + 8 + IntPtr.Size);
                        PushTo(windowID, @event);
                        break;
                    default:
                        PushToGlobal(@event);
                        break;
                }
            }
            if (!suppressGlobalProcessing)
                GlobalQueue.ProcessAll(Events.Global);
        }

        /// <summary>
        /// Pushes an event onto the poll queue.
        /// </summary>
        public static void Push(CommonEventArgs e)
        {
            var _e = e.AsEvent();
            if (SDL_PushEvent(ref _e) < 0)
                throw new InternalException($"Unable to push event: {SDL_GetError()}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PushTo(uint windowId, SDL_Event @event)
        {
            if (_queues.ContainsKey(windowId))
                _queues[windowId].Enqueue(@event);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PushToCurrent(SDL_Event @event)
        {
            var current = Window.Current;
            if (current == null) return;
            PushTo(current.ID, @event);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PushToGlobal(SDL_Event @event) =>
            _queues[GLOBAL_QUEUE_ID].Enqueue(@event);

        internal class EventQueue
        {
            private Queue<SDL_Event> _events = new Queue<SDL_Event>(32);
            public IReadOnlyCollection<SDL_Event> Events => _events;

            internal void Enqueue(SDL_Event @event) => _events.Enqueue(@event);

            public void ProcessNext(Events events) => events.Process(_events.Dequeue());

            public void ProcessAll(Events events)
            {
                while (_events.Count > 0)
                    ProcessNext(events);
            }
        }
    }
}