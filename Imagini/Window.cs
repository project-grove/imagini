using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Imagini.Internal;
using static Imagini.Internal.ErrorHandler;
using static SDL2.SDL_render;
using static SDL2.SDL_video;

namespace Imagini
{
    /// <summary>
    /// An app window.
    /// </summary>
    public class Window : IDisposable
    {
        internal IntPtr Handle;
        internal IntPtr Renderer;
        internal uint ID;

        private static Dictionary<uint, Window> _windows =
            new Dictionary<uint, Window>();
        internal IReadOnlyDictionary<uint, Window> Windows => _windows;
        internal static Window Current
        {
            get {
                if (_windows.Count == 0) return null;
                if (_windows.Count == 1) return _windows.Values.First();
                return _windows.Values
                    .Where(window => window.IsVisible && window.IsFocused)
                    .FirstOrDefault();
            }   
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
                SDL_GetWindowSize(Handle, out int w, out int h);
                return new Size(w, h);
            }
            set
            {
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
                SDL_GetWindowMinimumSize(Handle, out int w, out int h);
                return new Size(w, h);
            }
            set
            {
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
                SDL_GetWindowMaximumSize(Handle, out int w, out int h);
                return new Size(w, h);
            }
            set
            {
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

        internal Window(WindowSettings settings)
        {
            Try(() =>
                SDL_CreateWindowAndRenderer(100, 100, settings.GetFlags(),
                    out Handle, out Renderer),
                "SDL_CreateWindowAndRenderer");
            ID = SDL_GetWindowID(Handle);
            if (ID == 0)
                throw new InternalException("Could not get the window ID");
            Title = settings.Title;
            Apply(settings);
            // Generally should not happen, but it's better to check anyways
            if (_windows.ContainsKey(ID))
                throw new InternalException("Window with the specified ID already exists");
            else
                _windows.Add(ID, this);
        }

        /// <summary>
        /// Applies the specified settings to this window.
        /// </summary>
        public void Apply(WindowSettings settings)
        {
            settings.Apply(Handle);
            Settings = settings.Clone() as WindowSettings;
            if (settings.WindowMode == WindowMode.Fullscreen)
                settings.FullscreenDisplayMode = this.Display.CurrentMode;
        }

        /// <summary>
        /// Shows this window.
        /// </summary>
        public void Show() => SDL_ShowWindow(Handle);
        /// <summary>
        /// Hides this window.
        /// </summary>
        public void Hide() => SDL_HideWindow(Handle);

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
        public void Minimize() => SDL_MinimizeWindow(Handle);

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
        public void Maximize() => SDL_MaximizeWindow(Handle);

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
        public void Restore() => SDL_RestoreWindow(Handle);

        /// <summary>
        /// Raises this window above other windows and sets the input focus.
        /// </summary>
        public void Raise() => SDL_RaiseWindow(Handle);

        /// <summary>
        /// Gets or sets the window title.
        /// </summary>
        public string Title
        {
            get => SDL_GetWindowTitle(Handle);
            set => SDL_SetWindowTitle(Handle, value);
        }

        /// <summary>
        /// Returns the display index of this window.
        /// </summary>
        public int DisplayIndex => Display.GetCurrentDisplayIndexForWindow(Handle);
        /// <summary>
        /// Returns the display of this window.
        /// </summary>
        public Display Display => Display.GetCurrentDisplayForWindow(Handle);

        private bool HasFlag(SDL_WindowFlags flag) =>
            (SDL_GetWindowFlags(Handle) & (uint)flag) == (uint)flag;

        static Window() => Lifecycle.TryInitialize();


        /// <summary>
        /// Returns true if this window is disposed (destroyed).
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Destroys and disposes the window.
        /// </summary>
        public void Dispose()
        {
            if (Disposed) return;
            SDL_DestroyWindow(Handle);
            _windows.Remove(ID);
            Handle = Renderer = IntPtr.Zero;
            Disposed = true;
        }
    }
}