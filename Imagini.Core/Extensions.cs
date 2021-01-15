using static SDL2.SDL_keyboard;
using static SDL2.SDL_keycode;
using static SDL2.SDL_scancode;

namespace Imagini
{
    public static class Extensions
    {
        public static Keycode ToKeycode(this Scancode scancode)
        {
            return (Keycode)SDL_GetKeyFromScancode((SDL_Scancode)scancode);
        }

        public static Scancode ToScancode(this Keycode keycode)
        {
            return (Scancode)SDL_GetScancodeFromKey((SDL_Keycode)keycode);
        }
    }
}