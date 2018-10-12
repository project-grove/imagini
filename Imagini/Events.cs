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

        internal Events() { }

        internal void Process(SDL_Event e)
        {
            // TODO
        }
    }
}