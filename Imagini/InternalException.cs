using System;

namespace Imagini
{
    /// <summary>
    /// Internal library exception used by Imagini.
    /// </summary>
    public class InternalException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public InternalException() : base() { }
        /// <summary>
        /// Constructor which accepts a message.
        /// </summary>
        public InternalException(string message) : base(message) { }
        /// <summary>
        /// Constructor which accepts a message and wraps an existing exception.
        /// </summary>
        public InternalException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}