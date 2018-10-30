using System;

namespace Imagini
{
    /// <summary>
    /// Internal library exception used by Imagini.
    /// </summary>
    public class ImaginiException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ImaginiException() : base() { }
        /// <summary>
        /// Constructor which accepts a message.
        /// </summary>
        public ImaginiException(string message) : base(message) { }
        /// <summary>
        /// Constructor which accepts a message and wraps an existing exception.
        /// </summary>
        public ImaginiException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}