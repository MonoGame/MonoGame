using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The exception that is thrown when the device has been lost, but can be reset at this time.
    /// </summary>
    [DataContract]
    public sealed class DeviceNotResetException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceNotResetException"/> class.
        /// </summary>
        public DeviceNotResetException()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceNotResetException"/>
        /// class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DeviceNotResetException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceNotResetException"/> with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">
        /// The exception that is the cause of the current exception,
        /// or a null reference if no inner exception is specified.
        /// </param>
        public DeviceNotResetException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
