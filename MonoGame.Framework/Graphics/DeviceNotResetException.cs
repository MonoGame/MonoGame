using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics
{
    [DataContract]
    public sealed class DeviceNotResetException : Exception
    {
        public DeviceNotResetException()
            : base()
        {

        }

        public DeviceNotResetException(string message)
            : base(message)
        {

        }

        public DeviceNotResetException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
