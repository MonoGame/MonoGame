using System;

namespace Microsoft.Xna.Framework.Graphics
{
    [SerializableAttribute]
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
