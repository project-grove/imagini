using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Imagini.ErrorHandler;
using static SDL2.SDL_error;
using static SDL2.SDL_hints;
using static SDL2.SDL_render;
using static SDL2.SDL_syswm;
using static SDL2.SDL_version;
using static SDL2.SDL_video;

namespace Imagini
{
    /// <summary>
    /// An app window.
    /// </summary>
    public sealed class Window : Resource
    {
        /// <summary>
        /// Gets the handle of this window.
        /// </summary>
        public IntPtr Handle { get; private set; }
        internal uint ID;


        private static Dictionary<uint, Window> _windows =
            new Dictionary<uint, Window>();
        /// <summary>
        /// Returns all existing app windows.
        /// </summary>
        public static IReadOnlyCollection<Window> Windows => _windows.Values;

        internal static Window Current
        {
            get
            {
                if (_overridedCurrent != null && !_overridedCurrent.IsDisposed) 
                    return _overridedCurrent;
                if (_windows.Count == 0) return null;
                return _windows.Values
                    .Where(window => !window.IsDisposed && window.IsVisible && window.IsFocused)
                    .FirstOrDefault();
            }
        }

        private static Window _overridedCurrent = null;
        /// <summary>
        /// Sets the specified window as the current one no matter
        /// if it's focused and visible or not. If a null is passed,
        /// then the overriding is disabled. If the specified window is
        /// disposed, then the overriding is disabled.
        /// </summary>
        public static void OverrideCurrentWith(Window window) => _overridedCurrent = window;

        internal static Window GetByID(uint id)
        {
            if (_windows.ContainsKey(id))
                return _windows[id];
            return null;
        }

        /// <summary>
        /// Gets or sets the size of the window's client area. Affects only
        /// non-fullscreen modes.
        /// </summary>
        /// <remarks>
        /// The window size in screen coordinates may differ from the size in
        /// pixels, if the window was created with <see cref="WindowSettings.AllowHighDpi" />.
        /// </remarks>
        public Size Size
        {
            get
            {
                CheckIfNotDisposed();
                SDL_GetWindowSize(Handle, out int w, out int h);
                return new Size(w, h);
            }
            set
            {
                CheckIfNotDisposed();
                SDL_SetWindowSize(Handle, value.Width, value.Height);
            }
        }

        /// <summary>
        /// Gets the size of the window's client area in pixels.
        /// </summary>
        /// <seealso cref="Size" />
        public Size SizeInPixels
        {
            get
            {
                CheckIfNotDisposed();
                SDL_GL_GetDrawableSize(Handle, out int w, out int h);
                return new Size(w, h);
            }
        }

        /// <summary>
        /// Gets or sets the window minimum size.
        /// </summary>
        public Size MinimumSize
        {
            get
            {
                CheckIfNotDisposed();
                SDL_GetWindowMinimumSize(Handle, out int w, out int h);
                return new Size(w, h);
            }
            set
            {
                CheckIfNotDisposed();
                SDL_SetWindowMinimumSize(Handle, value.Width, value.Height);
            }
        }

        /// <summary>
        /// Gets or sets the window minimum size.
        /// </summary>
        public Size MaximumSize
        {
            get
            {
                CheckIfNotDisposed();
                SDL_GetWindowMaximumSize(Handle, out int w, out int h);
                return new Size(w, h);
            }
            set
            {
                CheckIfNotDisposed();
                SDL_SetWindowMaximumSize(Handle, value.Width, value.Height);
            }
        }

        /// <summary>
        /// Returns the settings for this window.
        /// </summary>
        public WindowSettings Settings { get; private set; }

        /// <summary>
        /// Returns the <see cref="WindowMode" /> for this window.
        /// </summary>
        public WindowMode Mode => Settings.WindowMode;

        public Window(WindowSettings settings, uint additionalFlags = 0)
        {
            Handle = SDL_CreateWindow(settings.Title,
                SDL_WINDOWPOS_CENTERED_DISPLAY(settings.DisplayIndex),
                SDL_WINDOWPOS_CENTERED_DISPLAY(settings.DisplayIndex),
                settings.WindowWidth, settings.WindowHeight, settings.GetFlags() | additionalFlags);
            CheckWindowHandle();
            Title = settings.Title;
            Apply(settings);
            Register();
            Raise();
        }

        public Window(IntPtr handle)
        {
            Handle = handle;
            CheckWindowHandle();
            Register();
        }

        private void CheckWindowHandle()
        {
            if (Handle == IntPtr.Zero)
                throw new ImaginiException($"Could not create window: {SDL_GetError()}");
            ID = SDL_GetWindowID(Handle);
            if (ID == 0)
                throw new ImaginiException($"Could not get the window ID: {SDL_GetError()}");
        }

        private void Register()
        {
            // Generally should not happen, but it's better to check anyways
            if (_windows.ContainsKey(ID))
                throw new ImaginiException("Window with the specified ID already exists");
            else
                _windows.Add(ID, this);
            GetSubsystem();
        }

        /// <summary>
        /// Applies the specified settings to this window.
        /// </summary>
        public void Apply(WindowSettings settings)
        {
            CheckIfNotDisposed();
            settings.Apply(Handle);
            Settings = settings.Clone() as WindowSettings;
            if (settings.WindowMode == WindowMode.Fullscreen)
                settings.FullscreenDisplayMode = this.Display.CurrentMode;
            var h = SDL_GetHint(SDL_HINT_RENDER_VSYNC);
            Settings.VSync = SDL_GetHint(SDL_HINT_RENDER_VSYNC) == "1";
            Title = settings.Title;
            OnSettingsChanged?.Invoke(this, settings);
        }

        /// <summary>
        /// Fires after <see cref="Apply()">.
        /// </summary>
        public event EventHandler<WindowSettings> OnSettingsChanged;

        /// <summary>
        /// Shows this window.
        /// </summary>
        public void Show() => NotDisposed(() => SDL_ShowWindow(Handle));
        /// <summary>
        /// Hides this window.
        /// </summary>
        public void Hide() => NotDisposed(() => SDL_HideWindow(Handle));

        /// <summary>
        /// Indicates if the window is visible.
        /// </summary>
        public bool IsVisible
        {
            get => HasFlag(SDL_WindowFlags.SDL_WINDOW_SHOWN);
        }

        /// <summary>
        /// Returns true if this window has input focus.
        /// </summary>
        public bool IsFocused => HasFlag(SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) ||
            HasFlag(SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS) ||
            HasFlag(SDL_WindowFlags.SDL_WINDOW_INPUT_GRABBED);

        /// <summary>
        /// Minimizes this window.
        /// </summary>
        public void Minimize() => NotDisposed(() => SDL_MinimizeWindow(Handle));

        /// <summary>
        /// Indicates if the window is minimized.
        /// </summary>
        public bool IsMinimized
        {
            get => HasFlag(SDL_WindowFlags.SDL_WINDOW_MINIMIZED);
        }

        /// <summary>
        /// Maximizes this window.
        /// </summary>
        public void Maximize() => NotDisposed(() => SDL_MaximizeWindow(Handle));

        /// <summary>
        /// Indicates if the window is maximized.
        /// </summary>
        public bool IsMaximized
        {
            get => HasFlag(SDL_WindowFlags.SDL_WINDOW_MAXIMIZED);
        }

        /// <summary>
        /// Restores the window. Called when IsMinimized or IsMaximized is set
        /// to false.
        /// </summary>
        public void Restore() => NotDisposed(() => SDL_RestoreWindow(Handle));

        /// <summary>
        /// Raises this window above other windows and sets the input focus.
        /// </summary>
        public void Raise() => NotDisposed(() =>
        {
            SDL_RaiseWindow(Handle);
            SDL_SetWindowInputFocus(Handle); // X11 only
        });

        /// <summary>
        /// Gets or sets the window title.
        /// </summary>
        public string Title
        {
            get => NotDisposed(() => SDL_GetWindowTitle(Handle));
            set => NotDisposed(() => SDL_SetWindowTitle(Handle, value));
        }

        /// <summary>
        /// Returns the display index of this window.
        /// </summary>
        public int DisplayIndex => NotDisposed(() =>
            Display.GetCurrentDisplayIndexForWindow(Handle));
        /// <summary>
        /// Returns the display of this window.
        /// </summary>
        public Display Display => NotDisposed(() =>
            Display.GetCurrentDisplayForWindow(Handle));

        private uint _flags => SDL_GetWindowFlags(Handle);

        private bool HasFlag(SDL_WindowFlags flag) =>
            NotDisposed(() => (_flags & (uint)flag) == (uint)flag);

        static Window() => Lifecycle.TryInitialize();

        internal override void Destroy()
        {
            if (IsDisposed) return;
            base.Destroy();
            SDL_DestroyWindow(Handle);
            _windows.Remove(ID);
            Handle = IntPtr.Zero;
        }

        /// <summary>
        /// Returns the underlying window subsystem type.
        /// </summary>
        public WindowSubsystem Subsystem { get; private set; }

        private unsafe void GetSubsystem()
        {
            SDL_GetVersion(out SDL_Version version);
            var container = Marshal.AllocHGlobal(32);
            var p = (byte*)container;
            *p = version.major;
            *(p + 1) = version.minor;
            *(p + 2) = version.patch;
            SDL_GetWindowWMInfo(Handle, container);
            Subsystem = WindowSubsystem.Unknown;
            var min = Enum.GetValues(typeof(WindowSubsystem)).Cast<int>().Min();
            var max = Enum.GetValues(typeof(WindowSubsystem)).Cast<int>().Max();
            for (int i = 0; i < 4; i++)
            {
                // run through enum bytes because the num size is not fixed
                var value = *(p + 3 + i);
                if (value >= min && value <= max)
                {
                    Subsystem = (WindowSubsystem)value;
                    break;
                }
            }
            Marshal.FreeHGlobal(container);
        }
    }

    /// <summary>
    /// Defines the windowing system type.
    /// </summary>
    public enum WindowSubsystem
    {
        Unknown,
        Windows,
        X11,
        DirectFB,
        Cocoa,
        UIKit,
        Wayland,
        Mir,
        WinRT,
        Android,
        Vivante
    }
}