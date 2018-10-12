using System;
using System.Linq;
using SDL2;
using static SDL2.SDL_events;
using static SDL2.SDL_keyboard;
using static SDL2.SDL_keycode;
using static SDL2.SDL_scancode;

namespace Imagini
{
    /// <summary>
    /// Describes event args shared by all available event types.
    /// </summary>
    public abstract class CommonEventArgs : EventArgs
    {
        public long Timestamp { get; protected set; }
        internal CommonEventArgs() => Timestamp = App.TotalTime;
        internal unsafe CommonEventArgs(SDL_CommonEvent e) => Timestamp = e.timestamp;
        internal abstract SDL_Event AsEvent();
    }

    /* Window events */
    /// <summary>
    /// Describes various window state modifications.
    /// </summary>
    /// <seealso cref="WindowStateChangeEventArgs" />
    /// <seealso cref="Events.WindowEvents" />
    public enum WindowStateChange
    {
        None,
        Shown,
        Hidden,
        Exposed,
        Moved,
        Resized,
        SizeChanged,
        Minimized,
        Maximized,
        Restored,
        MouseEnter,
        MouseLeave,
        FocusGained,
        FocusLost,
        Closed
    }

    /// <summary>
    /// Describes window state change event data.
    /// </summary>
    /// <seealso cref="Events.WindowEvents" />
    public sealed class WindowStateChangeEventArgs : CommonEventArgs
    {
        /// <summary>
        /// The window for which the event was fired.
        /// </summary>
        public Window Window { get; private set; }
        /// <summary>
        /// Target window state.
        /// </summary>
        public WindowStateChange State { get; private set; }
        /// <summary>
        /// Contains X coordinate or horizontal size if the window was moved, resized or it's size have changed.
        /// </summary>
        public int? X { get; private set; }
        /// <summary>
        /// Contains Y coordinate or vertical size if the window was moved, resized or it's size have changed.
        /// </summary>
        public int? Y { get; private set; }

        /// <summary>
        /// Creates an event args object.
        /// </summary>
        public WindowStateChangeEventArgs(
            Window window, WindowStateChange state, int? x = null, int? y = null)
            =>
            (this.Window, this.State, this.X, this.Y) =
            (window, state, x, y);

        internal WindowStateChangeEventArgs(SDL_WindowEvent e)
            : base(e)
        {
            State = (WindowStateChange)e.@event;
            Window = Window.GetByID(e.windowID);
            switch (State)
            {
                case WindowStateChange.Moved:
                case WindowStateChange.Resized:
                case WindowStateChange.SizeChanged:
                    X = e.data1; Y = e.data2; break;
            }
        }

        internal override SDL_Event AsEvent() =>
            new SDL_WindowEvent()
            {
                type = (uint)SDL_EventType.SDL_WINDOWEVENT,
                windowID = Window.ID,
                data1 = X ?? 0,
                data2 = Y ?? 0
            };
    }

    /* Keyboard events */
    /// <summary>
    /// Keyboard scancode.
    /// </summary>
    public enum Scancode // renamed SDL_Scancode
    {

        UNKNOWN = 0,
        A = 4,
        B = 5,
        C = 6,
        D = 7,
        E = 8,
        F = 9,
        G = 10,
        H = 11,
        I = 12,
        J = 13,
        K = 14,
        L = 15,
        M = 16,
        N = 17,
        O = 18,
        P = 19,
        Q = 20,
        R = 21,
        S = 22,
        T = 23,
        U = 24,
        V = 25,
        W = 26,
        X = 27,
        Y = 28,
        Z = 29,
        NUMBER_1 = 30,
        NUMBER_2 = 31,
        NUMBER_3 = 32,
        NUMBER_4 = 33,
        NUMBER_5 = 34,
        NUMBER_6 = 35,
        NUMBER_7 = 36,
        NUMBER_8 = 37,
        NUMBER_9 = 38,
        NUMBER_0 = 39,

        RETURN = 40,
        ESCAPE = 41,
        BACKSPACE = 42,
        TAB = 43,
        SPACE = 44,
        MINUS = 45,
        EQUALS = 46,
        LEFTBRACKET = 47,
        RIGHTBRACKET = 48,
        BACKSLASH = 49,
        NONUSHASH = 50,
        SEMICOLON = 51,
        APOSTROPHE = 52,
        GRAVE = 53,
        COMMA = 54,
        PERIOD = 55,
        SLASH = 56,
        CAPSLOCK = 57,
        F1 = 58,
        F2 = 59,
        F3 = 60,
        F4 = 61,
        F5 = 62,
        F6 = 63,
        F7 = 64,
        F8 = 65,
        F9 = 66,
        F10 = 67,
        F11 = 68,
        F12 = 69,
        PRINTSCREEN = 70,
        SCROLLLOCK = 71,
        PAUSE = 72,
        INSERT = 73,
        HOME = 74,
        PAGEUP = 75,
        DELETE = 76,
        END = 77,
        PAGEDOWN = 78,
        RIGHT = 79,
        LEFT = 80,
        DOWN = 81,
        UP = 82,
        NUMLOCKCLEAR = 83,
        KP_DIVIDE = 84,
        KP_MULTIPLY = 85,
        KP_MINUS = 86,
        KP_PLUS = 87,
        KP_ENTER = 88,
        KP_1 = 89,
        KP_2 = 90,
        KP_3 = 91,
        KP_4 = 92,
        KP_5 = 93,
        KP_6 = 94,
        KP_7 = 95,
        KP_8 = 96,
        KP_9 = 97,
        KP_0 = 98,
        KP_PERIOD = 99,
        NONUSBACKSLASH = 100,
        APPLICATION = 101,
        POWER = 102,
        KP_EQUALS = 103,
        F13 = 104,
        F14 = 105,
        F15 = 106,
        F16 = 107,
        F17 = 108,
        F18 = 109,
        F19 = 110,
        F20 = 111,
        F21 = 112,
        F22 = 113,
        F23 = 114,
        F24 = 115,
        EXECUTE = 116,
        HELP = 117,
        MENU = 118,
        SELECT = 119,
        STOP = 120,
        AGAIN = 121,
        UNDO = 122,
        CUT = 123,
        COPY = 124,
        PASTE = 125,
        FIND = 126,
        MUTE = 127,
        VOLUMEUP = 128,
        VOLUMEDOWN = 129,
        KP_COMMA = 133,
        KP_EQUALSAS400 = 134,
        INTERNATIONAL1 = 135,
        INTERNATIONAL2 = 136,
        INTERNATIONAL3 = 137,
        INTERNATIONAL4 = 138,
        INTERNATIONAL5 = 139,
        INTERNATIONAL6 = 140,
        INTERNATIONAL7 = 141,
        INTERNATIONAL8 = 142,
        INTERNATIONAL9 = 143,
        LANG1 = 144,
        LANG2 = 145,
        LANG3 = 146,
        LANG4 = 147,
        LANG5 = 148,
        LANG6 = 149,
        LANG7 = 150,
        LANG8 = 151,
        LANG9 = 152,
        ALTERASE = 153,
        SYSREQ = 154,
        CANCEL = 155,
        CLEAR = 156,
        PRIOR = 157,
        RETURN2 = 158,
        SEPARATOR = 159,
        OUT = 160,
        OPER = 161,
        CLEARAGAIN = 162,
        CRSEL = 163,
        EXSEL = 164,
        KP_00 = 176,
        KP_000 = 177,
        THOUSANDSSEPARATOR = 178,
        DECIMALSEPARATOR = 179,
        CURRENCYUNIT = 180,
        CURRENCYSUBUNIT = 181,
        KP_LEFTPAREN = 182,
        KP_RIGHTPAREN = 183,
        KP_LEFTBRACE = 184,
        KP_RIGHTBRACE = 185,
        KP_TAB = 186,
        KP_BACKSPACE = 187,
        KP_A = 188,
        KP_B = 189,
        KP_C = 190,
        KP_D = 191,
        KP_E = 192,
        KP_F = 193,
        KP_XOR = 194,
        KP_POWER = 195,
        KP_PERCENT = 196,
        KP_LESS = 197,
        KP_GREATER = 198,
        KP_AMPERSAND = 199,
        KP_DBLAMPERSAND = 200,
        KP_VERTICALBAR = 201,
        KP_DBLVERTICALBAR = 202,
        KP_COLON = 203,
        KP_HASH = 204,
        KP_SPACE = 205,
        KP_AT = 206,
        KP_EXCLAM = 207,
        KP_MEMSTORE = 208,
        KP_MEMRECALL = 209,
        KP_MEMCLEAR = 210,
        KP_MEMADD = 211,
        KP_MEMSUBTRACT = 212,
        KP_MEMMULTIPLY = 213,
        KP_MEMDIVIDE = 214,
        KP_PLUSMINUS = 215,
        KP_CLEAR = 216,
        KP_CLEARENTRY = 217,
        KP_BINARY = 218,
        KP_OCTAL = 219,
        KP_DECIMAL = 220,
        KP_HEXADECIMAL = 221,
        LCTRL = 224,
        LSHIFT = 225,
        LALT = 226,
        LGUI = 227,
        RCTRL = 228,
        RSHIFT = 229,
        RALT = 230,
        RGUI = 231,
        MODE = 257,
        AUDIONEXT = 258,
        AUDIOPREV = 259,
        AUDIOSTOP = 260,
        AUDIOPLAY = 261,
        AUDIOMUTE = 262,
        MEDIASELECT = 263,
        WWW = 264,
        MAIL = 265,
        CALCULATOR = 266,
        COMPUTER = 267,
        AC_SEARCH = 268,
        AC_HOME = 269,
        AC_BACK = 270,
        AC_FORWARD = 271,
        AC_STOP = 272,
        AC_REFRESH = 273,
        AC_BOOKMARKS = 274,
        BRIGHTNESSDOWN = 275,
        BRIGHTNESSUP = 276,
        DISPLAYSWITCH = 277,
        KBDILLUMTOGGLE = 278,
        KBDILLUMDOWN = 279,
        KBDILLUMUP = 280,
        EJECT = 281,
        SLEEP = 282,
        APP1 = 283,
        APP2 = 284,
    }

    /// <summary>
    /// Keyboard virtual key.
    /// </summary>
    public enum Keycode
    {
        UNKNOWN = 0,
        BACKSPACE = 8,
        TAB = 9,
        RETURN = 13,
        ESCAPE = 27,
        SPACE = 32,
        EXCLAIM = 33,
        QUOTEDBL = 34,
        HASH = 35,
        DOLLAR = 36,
        PERCENT = 37,
        AMPERSAND = 38,
        QUOTE = 39,
        LEFTPAREN = 40,
        RIGHTPAREN = 41,
        ASTERISK = 42,
        PLUS = 43,
        COMMA = 44,
        MINUS = 45,
        PERIOD = 46,
        SLASH = 47,
        NUMBER_0 = 48,
        NUMBER_1 = 49,
        NUMBER_2 = 50,
        NUMBER_3 = 51,
        NUMBER_4 = 52,
        NUMBER_5 = 53,
        NUMBER_6 = 54,
        NUMBER_7 = 55,
        NUMBER_8 = 56,
        NUMBER_9 = 57,
        COLON = 58,
        SEMICOLON = 59,
        LESS = 60,
        EQUALS = 61,
        GREATER = 62,
        QUESTION = 63,
        AT = 64,
        LEFTBRACKET = 91,
        BACKSLASH = 92,
        RIGHTBRACKET = 93,
        CARET = 94,
        UNDERSCORE = 95,
        BACKQUOTE = 96,
        a = 97,
        b = 98,
        c = 99,
        d = 100,
        e = 101,
        f = 102,
        g = 103,
        h = 104,
        i = 105,
        j = 106,
        k = 107,
        l = 108,
        m = 109,
        n = 110,
        o = 111,
        p = 112,
        q = 113,
        r = 114,
        s = 115,
        t = 116,
        u = 117,
        v = 118,
        w = 119,
        x = 120,
        y = 121,
        z = 122,
        DELETE = 127,
        CAPSLOCK = 1073741881,
        F1 = 1073741882,
        F2 = 1073741883,
        F3 = 1073741884,
        F4 = 1073741885,
        F5 = 1073741886,
        F6 = 1073741887,
        F7 = 1073741888,
        F8 = 1073741889,
        F9 = 1073741890,
        F10 = 1073741891,
        F11 = 1073741892,
        F12 = 1073741893,
        PRINTSCREEN = 1073741894,
        SCROLLLOCK = 1073741895,
        PAUSE = 1073741896,
        INSERT = 1073741897,
        HOME = 1073741898,
        PAGEUP = 1073741899,
        END = 1073741901,
        PAGEDOWN = 1073741902,
        RIGHT = 1073741903,
        LEFT = 1073741904,
        DOWN = 1073741905,
        UP = 1073741906,
        NUMLOCKCLEAR = 1073741907,
        KP_DIVIDE = 1073741908,
        KP_MULTIPLY = 1073741909,
        KP_MINUS = 1073741910,
        KP_PLUS = 1073741911,
        KP_ENTER = 1073741912,
        KP_1 = 1073741913,
        KP_2 = 1073741914,
        KP_3 = 1073741915,
        KP_4 = 1073741916,
        KP_5 = 1073741917,
        KP_6 = 1073741918,
        KP_7 = 1073741919,
        KP_8 = 1073741920,
        KP_9 = 1073741921,
        KP_0 = 1073741922,
        KP_PERIOD = 1073741923,
        APPLICATION = 1073741925,
        POWER = 1073741926,
        KP_EQUALS = 1073741927,
        F13 = 1073741928,
        F14 = 1073741929,
        F15 = 1073741930,
        F16 = 1073741931,
        F17 = 1073741932,
        F18 = 1073741933,
        F19 = 1073741934,
        F20 = 1073741935,
        F21 = 1073741936,
        F22 = 1073741937,
        F23 = 1073741938,
        F24 = 1073741939,
        EXECUTE = 1073741940,
        HELP = 1073741941,
        MENU = 1073741942,
        SELECT = 1073741943,
        STOP = 1073741944,
        AGAIN = 1073741945,
        UNDO = 1073741946,
        CUT = 1073741947,
        COPY = 1073741948,
        PASTE = 1073741949,
        FIND = 1073741950,
        MUTE = 1073741951,
        VOLUMEUP = 1073741952,
        VOLUMEDOWN = 1073741953,
        KP_COMMA = 1073741957,
        KP_EQUALSAS400 = 1073741958,
        ALTERASE = 1073741977,
        SYSREQ = 1073741978,
        CANCEL = 1073741979,
        CLEAR = 1073741980,
        PRIOR = 1073741981,
        RETURN2 = 1073741982,
        SEPARATOR = 1073741983,
        OUT = 1073741984,
        OPER = 1073741985,
        CLEARAGAIN = 1073741986,
        CRSEL = 1073741987,
        EXSEL = 1073741988,
        KP_00 = 1073742000,
        KP_000 = 1073742001,
        THOUSANDSSEPARATOR = 1073742002,
        DECIMALSEPARATOR = 1073742003,
        CURRENCYUNIT = 1073742004,
        CURRENCYSUBUNIT = 1073742005,
        KP_LEFTPAREN = 1073742006,
        KP_RIGHTPAREN = 1073742007,
        KP_LEFTBRACE = 1073742008,
        KP_RIGHTBRACE = 1073742009,
        KP_TAB = 1073742010,
        KP_BACKSPACE = 1073742011,
        KP_A = 1073742012,
        KP_B = 1073742013,
        KP_C = 1073742014,
        KP_D = 1073742015,
        KP_E = 1073742016,
        KP_F = 1073742017,
        KP_XOR = 1073742018,
        KP_POWER = 1073742019,
        KP_PERCENT = 1073742020,
        KP_LESS = 1073742021,
        KP_GREATER = 1073742022,
        KP_AMPERSAND = 1073742023,
        KP_DBLAMPERSAND = 1073742024,
        KP_VERTICALBAR = 1073742025,
        KP_DBLVERTICALBAR = 1073742026,
        KP_COLON = 1073742027,
        KP_HASH = 1073742028,
        KP_SPACE = 1073742029,
        KP_AT = 1073742030,
        KP_EXCLAM = 1073742031,
        KP_MEMSTORE = 1073742032,
        KP_MEMRECALL = 1073742033,
        KP_MEMCLEAR = 1073742034,
        KP_MEMADD = 1073742035,
        KP_MEMSUBTRACT = 1073742036,
        KP_MEMMULTIPLY = 1073742037,
        KP_MEMDIVIDE = 1073742038,
        KP_PLUSMINUS = 1073742039,
        KP_CLEAR = 1073742040,
        KP_CLEARENTRY = 1073742041,
        KP_BINARY = 1073742042,
        KP_OCTAL = 1073742043,
        KP_DECIMAL = 1073742044,
        KP_HEXADECIMAL = 1073742045,
        LCTRL = 1073742048,
        LSHIFT = 1073742049,
        LALT = 1073742050,
        LGUI = 1073742051,
        RCTRL = 1073742052,
        RSHIFT = 1073742053,
        RALT = 1073742054,
        RGUI = 1073742055,
        MODE = 1073742081,
        AUDIONEXT = 1073742082,
        AUDIOPREV = 1073742083,
        AUDIOSTOP = 1073742084,
        AUDIOPLAY = 1073742085,
        AUDIOMUTE = 1073742086,
        MEDIASELECT = 1073742087,
        WWW = 1073742088,
        MAIL = 1073742089,
        CALCULATOR = 1073742090,
        COMPUTER = 1073742091,
        AC_SEARCH = 1073742092,
        AC_HOME = 1073742093,
        AC_BACK = 1073742094,
        AC_FORWARD = 1073742095,
        AC_STOP = 1073742096,
        AC_REFRESH = 1073742097,
        AC_BOOKMARKS = 1073742098,
        BRIGHTNESSDOWN = 1073742099,
        BRIGHTNESSUP = 1073742100,
        DISPLAYSWITCH = 1073742101,
        KBDILLUMTOGGLE = 1073742102,
        KBDILLUMDOWN = 1073742103,
        KBDILLUMUP = 1073742104,
        EJECT = 1073742105,
        SLEEP = 1073742106
    }

    [Flags]
    /// <summary>
    /// Keyboard key modifiers.
    /// </summary>
    public enum KeyModifiers
    {
        NONE = 0x0000,
        LSHIFT = 0x0001,
        RSHIFT = 0x0002,
        LCTRL = 0x0040,
        RCTRL = 0x0080,
        LALT = 0x0100,
        RALT = 0x0200,
        LGUI = 0x0400,
        RGUI = 0x0800,
        NUM = 0x1000,
        CAPS = 0x2000,
        MODE = 0x4000,
        RESERVED = 0x8000
    }

    /// <summary>
    /// Represents a keyboard key.
    /// </summary>
    public struct KeyboardKey
    {
        public Scancode Scancode;
        public Keycode Keycode;
        public KeyModifiers Modifiers;

        internal KeyboardKey(SDL_KeyboardEvent e)
        {
            Scancode = (Scancode)(int)e.keysym.scancode;
            Keycode = (Keycode)(int)e.keysym.sym;
            Modifiers = (KeyModifiers)(int)e.keysym.mod;
        }

        internal SDL_Keysym AsKeysym()
        {
            return new SDL_Keysym()
            {
                scancode = (SDL_Scancode)(int)Scancode,
                sym = (SDL_Keycode)(int)Keycode,
                mod = (ushort)Modifiers
            };
        }
    }

    /// <summary>
    /// Describes keyboard event data.
    /// </summary>
    public sealed class KeyboardEventArgs : CommonEventArgs
    {
        /// <summary>
        /// Target window.
        /// </summary>
        public Window Window { get; private set; }
        /// <summary>
        /// Returns the pressed or released key.
        /// </summary>
        public KeyboardKey Key { get; private set; }

        /// <summary>
        /// Defines if the key is pressed or released.
        /// </summary>
        public bool IsPressed { get; private set; }

        /// <summary>
        /// Defines if this is a key repeat
        /// </summary>
        public bool IsRepeat { get; private set; }

        /// <summary>
        /// Creates a new event args object.
        /// </summary>
        public KeyboardEventArgs(KeyboardKey key, bool isPressed,
            Window window = null, bool isRepeat = false)
            =>
            (this.Window, this.Key, this.IsPressed, this.IsRepeat) =
            (window ?? Window.Current, key, isPressed, isRepeat);

        internal KeyboardEventArgs(SDL_KeyboardEvent e)
        {
            Window = Window.GetByID(e.windowID);
            Key = new KeyboardKey(e);
            IsPressed = e.state == 1;
            IsRepeat = e.repeat > 0;
        }

        internal override SDL_Event AsEvent() =>
            new SDL_KeyboardEvent()
            {
                type = (uint)(IsPressed ? SDL_EventType.SDL_KEYDOWN : SDL_EventType.SDL_KEYUP),
                timestamp = (uint)Timestamp,
                windowID = Window.ID,
                state = IsPressed ? (byte)1 : (byte)0,
                repeat = IsRepeat ? (byte)1 : (byte)0,
                keysym = Key.AsKeysym()
            };
    }
}