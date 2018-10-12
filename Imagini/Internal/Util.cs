using System.Text;

namespace Imagini.Internal
{
    internal static class Util
    {
       public unsafe static string FromNullTerminated(char* data)
       {
           var length = 0;
           var i = 0;
           while (*(data + i) != 0)
           {
               length++; i++;
           }
           return Encoding.UTF8.GetString((byte*)data, length);
       }
    }
}