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
                SDL_Init(SDL_INIT_EVERYTHING);
                s_initialized = true;
                PrintVersion();
                InitSubsystems();
            }
        }

        private static void PrintVersion()
        {
            SDL_GetVersion(out SDL_Version version);
            Log.Information("Using SDL version {major}.{minor}.{patch}",
                version.major, version.minor, version.patch);
        }

        private static void InitSubsystems()
        {
            Display.UpdateDisplayInfo();
        }
    }
}