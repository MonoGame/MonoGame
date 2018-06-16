using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Net
{
#if WINDOWS_UAP
    [DataContract]
#else
    [Serializable]
#endif
    public class NetworkException : Exception
    {
        public NetworkException()
        { }

        public NetworkException(string message) : base(message)
        { }

        public NetworkException(string message, Exception innerException) : base(message, innerException)
        { }

        public NetworkException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
