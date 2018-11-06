using System;
using static SDL2.SDL_error;

namespace Imagini
{
    internal static class ErrorHandler
    {
        public static void Try(Func<int> func, string methodName)
        {
            int result = func();
            if (result >= 0) return;
            throw new ImaginiException($"{methodName}: {SDL_GetError()}");
        }

        public static void Check(Func<int> func, string methodName)
        {
            int result = func();
            if (result == 1) return;
            throw new ImaginiException($"{methodName}: {SDL_GetError()}");
        }

        public static int TryGet(Func<int> func, string methodName)
        {
            int result = func();
            if (result >= 0) return result;
            throw new ImaginiException($"{methodName}: {SDL_GetError()}");
        }
    }
}