using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkSessionJoinException : NetworkException
    {
        protected NetworkSessionJoinError joinError;

        public NetworkSessionJoinException()
        { }

        public NetworkSessionJoinException(string message) : base(message)
        { }

        public NetworkSessionJoinException(string message, Exception innerException) : base(message, innerException)
        { }

        public NetworkSessionJoinException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }

        public NetworkSessionJoinException(string message, NetworkSessionJoinError joinError) : base(message)
        {
            this.joinError = joinError;
        }

        public NetworkSessionJoinError JoinError { get; set; }
    }
}