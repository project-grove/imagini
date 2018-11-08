using System;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.RuntimeInformation;
using static Imagini.Logger;
using static SDL2.SDL_syswm;
using static SDL2.SDL_version;

using X11Window = System.UInt32;
using Pixmap = System.UInt32;
using XID = System.UInt32;
using NativeLibraryLoader;
using System.Diagnostics.CodeAnalysis;

namespace Imagini.Core.Native
{
    [ExcludeFromCodeCoverage]
    internal struct SysWM_X11
    {
        public byte major;
        public byte minor;
        public byte patch;
        public IntPtr display;
        public X11Window window;
    }

    [ExcludeFromCodeCoverage]
    internal struct XWMHints
    {
        public long flags;     /* marks which fields in this structure are defined */
        public unsafe fixed byte padding[64 - 4]; /* arbitrary number tbh */
    }

    [ExcludeFromCodeCoverage]
    internal static class X11
    {
        public unsafe static void TrySetUrgencyHint(Window window)
        {
            try
            {
                if (window.Subsystem != WindowSubsystem.X11) return;
                SDL_GetVersion(out SDL_Version version);
                var container = Marshal.AllocHGlobal(sizeof(SysWM_X11));
                var p = (byte*)container;
                *p = version.major;
                *(p + 1) = version.minor;
                *(p + 2) = version.patch;
                SDL_GetWindowWMInfo(window.Handle, container);
                var data = Marshal.PtrToStructure<SysWM_X11>(container);
                SetUrgencyHint(data.display, data.window);
                Marshal.FreeHGlobal(container);
            } catch (Exception ex) {
                Log.Warning("Could not set X11 hint: {ex}", ex);
            }
        }

        private unsafe static void SetUrgencyHint(IntPtr display, uint window)
        {
            var hints = X11Lib.XGetWMHints(display, window);
            long* data = (long*)hints;
            *(data) = *(data) & (1L << 8);
            X11Lib.XSetWMHints(display, window, hints);
            X11Lib.XSync(display, 0);
        }
    }

    [ExcludeFromCodeCoverage]
    internal static class X11Lib
    {
        private static NativeLibrary libX11;

        static X11Lib()
        {
            if (IsOSPlatform(OSPlatform.Windows)) return;
            string[] names = null;
            if (IsOSPlatform(OSPlatform.Linux))
            {
                names = new[] { "libX11.so" };
            }
            if (names != null)
            {
                try
                {
                    libX11 = new NativeLibrary(names);
                }
                catch (Exception ex)
                {
                    Log.Warning("Could not find libX11: {ex}", ex);
                }
            }
            if (libX11 == null) return;
            try
            {
                s_XGetWMHints_IntPtr_X11Window_t = __LoadFunction<XGetWMHints_IntPtr_X11Window_t>("XGetWMHints");
                s_XSetWMHints_IntPtr_X11Window_IntPtr_t = __LoadFunction<XSetWMHints_IntPtr_X11Window_IntPtr_t>("XSetWMHints");
                s_XSync_IntPtr_int_t = __LoadFunction<XSync_IntPtr_int_t>("XSync");
            }
            catch (Exception ex)
            {
                Log.Warning("Could not load libX11 functions: {ex}", ex);
                s_XGetWMHints_IntPtr_X11Window_t = (d, w) => IntPtr.Zero;
                s_XSetWMHints_IntPtr_X11Window_IntPtr_t = (d, w, f) => 0;
                s_XSync_IntPtr_int_t = (x, y) => { };
            }
        }

        private delegate IntPtr XGetWMHints_IntPtr_X11Window_t(IntPtr display, X11Window window);
        private static XGetWMHints_IntPtr_X11Window_t s_XGetWMHints_IntPtr_X11Window_t;
        public static IntPtr XGetWMHints(IntPtr display, X11Window window) => s_XGetWMHints_IntPtr_X11Window_t(display, window);

        private delegate int XSetWMHints_IntPtr_X11Window_IntPtr_t(IntPtr display, X11Window window, IntPtr hints);
        private static XSetWMHints_IntPtr_X11Window_IntPtr_t s_XSetWMHints_IntPtr_X11Window_IntPtr_t;
        public static int XSetWMHints(IntPtr display, X11Window window, IntPtr hints) => s_XSetWMHints_IntPtr_X11Window_IntPtr_t(display, window, hints);

        private delegate void XSync_IntPtr_int_t(IntPtr display, int discard);
        private static XSync_IntPtr_int_t s_XSync_IntPtr_int_t;
        public static void XSync(IntPtr display, int discard) => s_XSync_IntPtr_int_t(display, discard);

        internal static T __LoadFunction<T>(string name) => libX11.LoadFunction<T>(name);
    }
}