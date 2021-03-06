using static SDL2.SDL;
using static SDL2.SDL_version;
using static Imagini.Logger;

namespace Imagini
{
    internal static class Lifecycle
    {
        private static bool s_initialized = false;
        internal static void TryInitialize()
        {
            if (!s_initialized)
            {
                SDL_Init(
                    SDL_INIT_VIDEO |
                    SDL_INIT_JOYSTICK |
                    SDL_INIT_GAMECONTROLLER |
                    SDL_INIT_EVENTS
                );
                s_initialized = true;
                PrintVersion();
            }
        }

        private static void PrintVersion()
        {
            SDL_GetVersion(out SDL_Version version);
            Log.Information("Using SDL version {major}.{minor}.{patch}",
                version.major, version.minor, version.patch);
        }
    }
}