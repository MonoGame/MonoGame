using System;

namespace Microsoft.Xna.Framework.Graphics
{
    [SerializableAttribute]
    public sealed class DeviceLostException : Exception
    {
        public DeviceLostException()
            : base()
        {

        }

        public DeviceLostException(string message)
            : base(message)
        {

        }

        public DeviceLostException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
