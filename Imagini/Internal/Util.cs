using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Imagini.Internal
{
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
           unsafe {
               return FromNullTerminated((byte*)data);
           }
       }
    }
}