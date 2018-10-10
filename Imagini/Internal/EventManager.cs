using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static SDL2.SDL_events;
using static SDL2.SDL_video;

namespace Imagini.Internal
{
    internal static class EventManager
    {
        static EventManager() => Lifecycle.TryInitialize();
        private static Dictionary<uint, EventQueue> _queues =
            new Dictionary<uint, EventQueue>();

        public static EventQueue CreateQueueFor(Window window)
        {
            var id = window.ID;
            if (_queues.ContainsKey(id)) return _queues[id];
            var queue = new EventQueue();
            _queues.Add(id, queue);
            return queue;
        }

        public static void DeleteQueueFor(Window window) =>
            _queues.Remove(window.ID);

        public unsafe static void Poll()
        {
            byte* data = stackalloc byte[56];
            var @event = *((SDL_Event*)&data);
            while (SDL_PollEvent(ref @event) != 0)
            {
                switch (@event.type)
                {
                    // uint32 type, uint32 timestamp, uint32 windowID
                    case (uint)SDL_EventType.SDL_WINDOWEVENT:
                    case (uint)SDL_EventType.SDL_KEYDOWN:
                    case (uint)SDL_EventType.SDL_KEYUP:
                    case (uint)SDL_EventType.SDL_TEXTEDITING:
                    case (uint)SDL_EventType.SDL_TEXTINPUT:
                    case (uint)SDL_EventType.SDL_MOUSEMOTION:
                    case (uint)SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    case (uint)SDL_EventType.SDL_MOUSEBUTTONUP:
                    case (uint)SDL_EventType.SDL_MOUSEWHEEL:
                    case (uint)SDL_EventType.SDL_USEREVENT:
                        var windowID = (uint)(data + 8);
                        PushTo(windowID, @event);
                        break;
                    case (uint)SDL_EventType.SDL_JOYAXISMOTION:
                    case (uint)SDL_EventType.SDL_JOYBALLMOTION:
                    case (uint)SDL_EventType.SDL_JOYHATMOTION:
                    case (uint)SDL_EventType.SDL_JOYBUTTONDOWN:
                    case (uint)SDL_EventType.SDL_JOYBUTTONUP:
                    case (uint)SDL_EventType.SDL_CONTROLLERAXISMOTION:
                    case (uint)SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
                    case (uint)SDL_EventType.SDL_CONTROLLERBUTTONUP:
                    case (uint)SDL_EventType.SDL_FINGERDOWN:
                    case (uint)SDL_EventType.SDL_FINGERUP:
                    case (uint)SDL_EventType.SDL_FINGERMOTION:
                    case (uint)SDL_EventType.SDL_DOLLARGESTURE:
                    case (uint)SDL_EventType.SDL_DOLLARRECORD:
                    case (uint)SDL_EventType.SDL_MULTIGESTURE:
                        PushToCurrent(@event);
                        break;
                    case (uint)SDL_EventType.SDL_DROPFILE:
                        windowID = (uint)(data + 8 + IntPtr.Size);
                        PushTo(windowID, @event);
                        break;
                    default:
                        PushToAll(@event);
                        break;
                }
            }
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
        private static void PushToAll(SDL_Event @event)
        {
            foreach(var queue in _queues.Values)
                queue.Enqueue(@event);
        }

        internal class EventQueue
        {
            private Queue<SDL_Event> _events = new Queue<SDL_Event>(32);
            public IReadOnlyCollection<SDL_Event> Events => _events;

            internal void Enqueue(SDL_Event @event) => 
                _events.Enqueue((SDL_Event)(object)@event);

            public void ProcessNext()
            {
                var e = _events.Dequeue();
                // TODO
            }

            public void ProcessAll()
            {
                while (_events.Count > 0)
                    ProcessNext();
            }
        }
    }
}