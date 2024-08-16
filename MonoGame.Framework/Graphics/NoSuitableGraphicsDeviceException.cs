using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Thrown when no available graphics device fits the given device preferences.
    /// </summary>
    [DataContract]
    public sealed class NoSuitableGraphicsDeviceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoSuitableGraphicsDeviceException"/> class.
        /// </summary>
        public NoSuitableGraphicsDeviceException()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoSuitableGraphicsDeviceException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NoSuitableGraphicsDeviceException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoSuitableGraphicsDeviceException"/>
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">
        /// The exception that is the cause of the current exception,
        /// or a null reference if no inner exception is specified.
        /// </param>
        public NoSuitableGraphicsDeviceException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
