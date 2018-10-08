using static SDL2.SDL;

namespace Imagini.Internal
{
    internal static class Lifecycle 
    {
        private static bool s_initialized = false;
        internal static void TryInitialize()
        {
            if (!s_initialized)
            {
                SDL_Init(SDL_INIT_EVERYTHING);
                s_initialized = true;
                InitSubsystems();
            }
        }

        private static void InitSubsystems()
        {
            Display.UpdateDisplayInfo();
        }

        internal static void Quit()
        {
            if (s_initialized)
            {
                SDL_Quit();
                s_initialized = false;
            }
        }
    }
}