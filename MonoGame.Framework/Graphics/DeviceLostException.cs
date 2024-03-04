using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The exception that is thrown when the device has been lost, but cannot be reset at this time.
    /// Therefore, rendering is not possible.
    /// </summary>
    [DataContract]
    public sealed class DeviceLostException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceLostException"/> class.
        /// </summary>
        public DeviceLostException()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceLostException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DeviceLostException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceLostException"/> with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">
        /// The exception that is the cause of the current exception,
        /// or a null reference if no inner exception is specified.
        /// </param>
        public DeviceLostException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
