using System;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

using static SDL2.SDL_rect;

namespace Imagini
{
    [ExcludeFromCodeCoverage]
    internal static class Util
    {
        public unsafe static string FromNullTerminated(byte* data)
        {
            var length = 0;
            var i = 0;
            while (*(data + i) != 0)
            {
                length++; i++;
            }
            return Encoding.UTF8.GetString(data, length);
        }

        public static string FromNullTerminated(IntPtr data)
        {
            unsafe
            {
                return FromNullTerminated((byte*)data);
            }
        }

        public static unsafe void CopyTo<T>(T[] target, IntPtr from, int count)
            where T : struct
        {
            var sizeInBytes = SizeOf<T>() * count;
            var dstHandle = GCHandle.Alloc(target, GCHandleType.Pinned);
            var dst = dstHandle.AddrOfPinnedObject();
            unsafe {
                Buffer.MemoryCopy((void*)from, (void*)dst, sizeInBytes, sizeInBytes);
            }
            dstHandle.Free();
        }

        public static unsafe int SizeOf<T>() where T : struct
        {
            Type type = typeof(T);

            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return sizeof(bool);
                case TypeCode.Char:
                    return sizeof(char);
                case TypeCode.SByte:
                    return sizeof(sbyte);
                case TypeCode.Byte:
                    return sizeof(byte);
                case TypeCode.Int16:
                    return sizeof(short);
                case TypeCode.UInt16:
                    return sizeof(ushort);
                case TypeCode.Int32:
                    return sizeof(int);
                case TypeCode.UInt32:
                    return sizeof(uint);
                case TypeCode.Int64:
                    return sizeof(long);
                case TypeCode.UInt64:
                    return sizeof(ulong);
                case TypeCode.Single:
                    return sizeof(float);
                case TypeCode.Double:
                    return sizeof(double);
                case TypeCode.Decimal:
                    return sizeof(decimal);
                case TypeCode.DateTime:
                    return sizeof(DateTime);
                default:
                    T[] tArray = new T[2];
                    GCHandle tArrayPinned = GCHandle.Alloc(tArray, GCHandleType.Pinned);
                    try
                    {
                        TypedReference tRef0 = __makeref(tArray[0]);
                        TypedReference tRef1 = __makeref(tArray[1]);
                        IntPtr ptrToT0 = *((IntPtr*)&tRef0);
                        IntPtr ptrToT1 = *((IntPtr*)&tRef1);

                        return (int)(((byte*)ptrToT1) - ((byte*)ptrToT0));
                    }
                    finally
                    {
                        tArrayPinned.Free();
                    }
            }
        }
    }

    [ExcludeFromCodeCoverage]
    internal static class Extensions
    {
        public static SDL_Rect ToSDL(this Rectangle src) =>
            new SDL_Rect() {
                x = src.X,
                y = src.Y,
                w = src.Width,
                h = src.Height
            };
    }
}