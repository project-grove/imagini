using System;

using static SDL2.SDL_pixels;

namespace Imagini.Drawing
{
    [Flags]
    /// <summary>
    /// Describes a pixel format. Entries starting with Format_ define full
    /// format specifications, other values are their flags.
    /// </summary>
    public enum PixelFormat : uint
    {
        Unknown = 0,
        Format_INDEX1LSB = 0x11100100,
        Format_INDEX1MSB = 0x11200100,
        Format_INDEX4LSB = 0x12100400,
        Format_INDEX4MSB = 0x12200400,
        Format_INDEX8 = 0x13000801,
        Format_RGB332 = 0x14110801,
        Format_RGB444 = 0x15120c02,
        Format_RGB555 = 0x15130f02,
        Format_BGR555 = 0x15530f02,
        Format_ARGB4444 = 0x15321002,
        Format_RGBA4444 = 0x15421002,
        Format_ABGR4444 = 0x15721002,
        Format_BGRA4444 = 0x15821002,
        Format_ARGB1555 = 0x15331002,
        Format_RGBA5551 = 0x15441002,
        Format_ABGR1555 = 0x15731002,
        Format_BGRA5551 = 0x15841002,
        Format_RGB565 = 0x15151002,
        Format_BGR565 = 0x15551002,
        Format_RGB24 = 0x17101803,
        Format_BGR24 = 0x17401803,
        Format_RGB888 = 0x16161804,
        Format_RGBX8888 = 0x16261804,
        Format_BGR888 = 0x16561804,
        Format_BGRX8888 = 0x16661804,
        Format_ARGB8888 = 0x16362004,
        Format_RGBA8888 = 0x16462004,
        Format_ABGR8888 = 0x16762004,
        Format_BGRA8888 = 0x16862004,
        Format_ARGB2101010 = 0x16372004,
        Format_YV12 = 0x32315659,
        Format_IYUV = 0x56555949,
        Format_YUY2 = 0x32595559,
        Format_UYVY = 0x59565955,
        Format_YVYU = 0x55595659,

        Type_Index1 = (1 << 24),
        Type_Index4,
        Type_Index8,
        Type_Packed8,
        Type_Packed16,
        Type_Packed32,
        Type_ArrayU8,
        Type_ArrayU16,
        Type_ArrayU32,
        Type_ArrayF16,
        Type_ArrayF32,

        /* Bitmap component order */
        Order_Bitmap4321 = (1 << 20),
        Order_Bitmap1234,

        /* Packed component order */
        Order_PackedXRGB = (1 << 20),
        Order_PackedRGBX,
        Order_PackedARGB,
        Order_PackedRGBA,
        Order_PackedXBGR,
        Order_PackedBGRX,
        Order_PackedABGR,
        Order_PackedBGRA,
        /* Array component order */
        Order_ArrayRGB = (1 << 20),
        Order_ArrayRGBA,
        Order_ArrayARGB,
        Order_ArrayBGR,
        Order_ArrayBGRA,
        Order_ArrayABGR,

        /* Packed layout */
        Layout_Packed332 = (1 << 16),
        Layout_Packed4444,
        Layout_Packed1555,
        Layout_Packed5551,
        Layout_Packed565,
        Layout_Packed8888,
        Layout_Packed2101010,
        Layout_Packed1010102
    }

    /// <summary>
    /// Defines pixel type.
    /// </summary>
    public enum PixelType
    {
        Unknown = 0,
        Index1 = (1 << 24),
        Index4,
        Index8,
        Packed8,
        Packed16,
        Packed32,
        ArrayU8,
        ArrayU16,
        ArrayU32,
        ArrayF16,
        ArrayF32
    }

    /// <summary>
    /// Describes pixel component type.
    /// </summary>
    public enum PixelComponentType
    {
        Bitmap,
        Packed,
        Array
    }

    /// <summary>
    /// Describes bitmap component order.
    /// </summary>
    public enum PixelBitmapOrder
    {
        MSB = (1 << 20),
        LSB,
    }

    /// <summary>
    /// Describes packed component order.
    /// </summary>
    public enum PixelPackedOrder
    {
        XRGB = (1 << 20),
        RGBX,
        ARGB,
        RGBA,
        XBGR,
        BGRX,
        ABGR,
        BGRA,
    }

    /// <summary>
    /// Describes array component order.
    /// </summary>
    public enum PixelArrayOrder
    {
        RGB = (1 << 20),
        RGBA,
        ARGB,
        BGR,
        BGRA,
        ABGR,
    }

    /// <summary>
    /// Describes packed pixel layout.
    /// </summary>
    public enum PixelLayout
    {
        Default = 0,
        Packed332 = (1 << 16),
        Packed4444,
        Packed1555,
        Packed5551,
        Packed565,
        Packed8888,
        Packed2101010,
        Packed1010102
    }

    public static class PixelFormatExtensions
    {
        private static int GetByte(this PixelFormat format, int offset) =>
            (int)((((uint)format) >> offset) & 0x0F);
        
        /// <summary>
        /// Returns the pixel type of the specified format.
        /// </summary>
        public static PixelType GetPixelType(this PixelFormat format) =>
            (PixelType)format.GetByte(24);
        
        /// <summary>
        /// Returns the component type.
        /// </summary>
        public static PixelComponentType GetComponentType(this PixelFormat format)
        {
            switch (format.GetPixelType())
            {
                case PixelType.ArrayF16:
                case PixelType.ArrayF32:
                case PixelType.ArrayU16:
                case PixelType.ArrayU32:
                case PixelType.ArrayU8:
                    return PixelComponentType.Array;
                case PixelType.Packed8:
                case PixelType.Packed16:
                case PixelType.Packed32:
                    return PixelComponentType.Packed;
                default:
                    return PixelComponentType.Bitmap;
            }
        }
        
        /// <summary>
        /// Returns the pixel layout of the specified format.
        /// </summary>
        public static PixelLayout GetLayout(this PixelFormat format) =>
            (PixelLayout)format.GetByte(16);

        /// <summary>
        /// Returns true if the specified format is an 4CC format.
        /// </summary>
        public static bool IsFourCC(this PixelFormat format) =>
            SDL_ISPIXELFORMAT_FOURCC((uint)format);

        /// <summary>
        /// Returns true if the specified format is an indexed format.
        /// </summary>
        public static bool IsIndexed(this PixelFormat format) =>
            SDL_ISPIXELFORMAT_INDEXED((uint)format);
        
        /// <summary>
        /// Returns true if the specified format has alpha channel.
        /// </summary>
        public static bool HasAlpha(this PixelFormat format) =>
            SDL_ISPIXELFORMAT_ALPHA((uint)format);
        
        /// <summary>
        /// Returns the number of bytes per pixel for the specified format.
        /// </summary>
        public static int GetBytesPerPixel(this PixelFormat format) =>
            SDL_BYTESPERPIXEL((uint)format);
        
        /// <summary>
        /// Returns the number of bits per pixel for the specified format.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static int GetBitsPerPixel(this PixelFormat format) =>
            SDL_BITSPERPIXEL((uint)format);
    }
}