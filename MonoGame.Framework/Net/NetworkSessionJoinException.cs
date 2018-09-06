using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Net
{
#if WINDOWS_UAP
    [DataContract]
#else
    [Serializable]
#endif
    public class NetworkSessionJoinException : NetworkException
    {
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
            this.JoinError = joinError;
        }

        public NetworkSessionJoinError JoinError { get; set; }
    }
}
