using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Imagini.Core.Internal
{
    [ExcludeFromCodeCoverage]
    internal sealed class PinnedCopy<T>
        where T : struct
    {
        public IntPtr Handle { get; private set; }
        public int SizeInBytes { get; private set; }

        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                Marshal.StructureToPtr<T>(value, Handle, false);
                _value = value;
            }
        }

        public PinnedCopy(T value)
        {
            SizeInBytes = Marshal.SizeOf<T>();
            Handle = Marshal.AllocHGlobal(SizeInBytes);
        }
    }
}