using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Thrown when no graphics device can be found that fits the desired graphics preferences.
    /// </summary>
    [DataContract]
    public sealed class NoSuitableGraphicsDeviceException : Exception
    {
        /// <summary>
        /// Creates a new instance of this exception.
        /// </summary>
        public NoSuitableGraphicsDeviceException()
            : base()
        {
        }

        /// <summary>
        /// Creates a new instance of this exception.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        public NoSuitableGraphicsDeviceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of this exception.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="inner">The exception that caused this exception. This exception was
        /// raised from the catch statement that handled the inner exception.</param>
        public NoSuitableGraphicsDeviceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
