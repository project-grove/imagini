using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static Imagini.ErrorHandler;
using static SDL2.SDL_error;
using static SDL2.SDL_hints;
using static SDL2.SDL_render;
using static SDL2.SDL_video;

namespace Imagini
{
    /// <summary>
    /// An app window.
    /// </summary>
    public sealed class Window : Resource
    {
        public IntPtr Handle { get; private set; }
        internal uint ID;


        private static Dictionary<uint, Window> _windows =
            new Dictionary<uint, Window>();
        internal IReadOnlyDictionary<uint, Window> Windows => _windows;
        internal static Window Current
        {
            get
            {
                if (_windows.Count == 0) return null;
                if (_windows.Count == 1) return _windows.Values.First();
                return _windows.Values
                    .Where(window => !window.IsDisposed && window.IsVisible && window.IsFocused)
                    .FirstOrDefault();
            }
        }

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
        /// Use 
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

        internal Window(WindowSettings settings) : base(nameof(Window))
        {
            Handle = SDL_CreateWindow(settings.Title,
                SDL_WINDOWPOS_CENTERED_DISPLAY(settings.DisplayIndex),
                SDL_WINDOWPOS_CENTERED_DISPLAY(settings.DisplayIndex),
                settings.WindowWidth, settings.WindowHeight, settings.GetFlags());
            if (Handle == IntPtr.Zero)
                throw new ImaginiException($"Could not create window: {SDL_GetError()}");
            ID = SDL_GetWindowID(Handle);
            if (ID == 0)
                throw new ImaginiException($"Could not get the window ID: {SDL_GetError()}");
            Title = settings.Title;
            Apply(settings);
            // Generally should not happen, but it's better to check anyways
            if (_windows.ContainsKey(ID))
                throw new ImaginiException("Window with the specified ID already exists");
            else
                _windows.Add(ID, this);
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
        }

        /// <summary>
        /// Shows this window.
        /// </summary>
        public void Show() => NotDisposed(() => SDL_ShowWindow(Handle));
        /// <summary>
        /// Hides this window.
        /// </summary>
        public void Hide() => NotDisposed(() => SDL_HideWindow(Handle));

        /// <summary>
        /// Gets or sets the visibility of this window.
        /// </summary>
        public bool IsVisible
        {
            get => HasFlag(SDL_WindowFlags.SDL_WINDOW_SHOWN);
            set
            {
                if (value) Show(); else Hide();
            }
        }

        /// <summary>
        /// Returns true if this window has input focus.
        /// </summary>
        public bool IsFocused => HasFlag(SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) ||
            HasFlag(SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS);

        /// <summary>
        /// Minimizes this window.
        /// </summary>
        public void Minimize() => NotDisposed(() => SDL_MinimizeWindow(Handle));

        /// <summary>
        /// Gets or sets if the window should be minimized.
        /// </summary>
        public bool IsMinimized
        {
            get => HasFlag(SDL_WindowFlags.SDL_WINDOW_MINIMIZED);
            set
            {
                if (value) Minimize(); else Restore();
            }
        }

        /// <summary>
        /// Maximizes this window.
        /// </summary>
        public void Maximize() => NotDisposed(() => SDL_MaximizeWindow(Handle));

        /// <summary>
        /// Gets or sets if the window should me maximized.
        /// </summary>
        public bool IsMaximized
        {
            get => HasFlag(SDL_WindowFlags.SDL_WINDOW_MAXIMIZED);
            set
            {
                if (value) Maximize(); else Restore();
            }
        }

        /// <summary>
        /// Restores the window. Called when IsMinimized or IsMaximized is set
        /// to false.
        /// </summary>
        public void Restore() => NotDisposed(() => SDL_RestoreWindow(Handle));

        /// <summary>
        /// Raises this window above other windows and sets the input focus.
        /// </summary>
        public void Raise() => NotDisposed(() => SDL_RaiseWindow(Handle));

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

        private bool HasFlag(SDL_WindowFlags flag) =>
            NotDisposed(() => (SDL_GetWindowFlags(Handle) & (uint)flag) == (uint)flag);

        static Window() => Lifecycle.TryInitialize();

        internal override void Destroy()
        {
            if (IsDisposed) return;
            SDL_DestroyWindow(Handle);
            _windows.Remove(ID);
            Handle = IntPtr.Zero;
        }
    }
}