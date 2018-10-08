using System;
using static SDL2.SDL_error;

namespace Imagini.Internal
{
    internal static class ErrorHandler
    {
        public static void Try(Func<int> func, string methodName)
        {
            int result = func();
            if (result >= 0) return;
            throw new InternalException($"{methodName}: {SDL_GetError()}");
        } 

        public static int TryGet(Func<int> func, string methodName)
        {
            int result = func();
            if (result >= 0) return result;
            throw new InternalException($"{methodName}: {SDL_GetError()}");
        }
    }
}