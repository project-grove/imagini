using System.Drawing;
using System.Runtime.InteropServices;

namespace Imagini.Drawing
{
    public interface IColor
    {
        PixelFormat Format { get; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ColorRGBA8888 : IColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public PixelFormat Format => PixelFormat.Format_RGBA8888;

        public ColorRGBA8888(Color color)
        {
            A = color.A;
            R = color.R;
            G = color.G;
            B = color.B;
        }

        private bool Equals(ColorRGBA8888 other)
        {
            return A == other.A && R == other.R && G == other.G && B == other.B;
        }

        /// <summary>
        /// Checks for object equality.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is ColorRGBA8888)
                return Equals((ColorRGBA8888)obj);
            return false;
        }

        /// <summary>
        /// Calculates the hash code.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked {
                return (int)((R << 24) | (G << 16) | (B << 8) | A);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ColorARGB8888 : IColor
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public ColorARGB8888(Color color)
        {
            A = color.A;
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public PixelFormat Format => PixelFormat.Format_ARGB8888;

        private bool Equals(ColorRGBA8888 other)
        {
            return A == other.A && R == other.R && G == other.G && B == other.B;
        }

        /// <summary>
        /// Checks for object equality.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is ColorRGBA8888)
                return Equals((ColorRGBA8888)obj);
            return false;
        }

        /// <summary>
        /// Calculates the hash code.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked {
                return (int)((A << 24) | (R << 16) | (G << 8) | B);
            }
        }
    }
}