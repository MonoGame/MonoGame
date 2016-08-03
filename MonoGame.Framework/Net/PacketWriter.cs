using System.IO;

namespace Microsoft.Xna.Framework.Net
{
    public class PacketWriter : BinaryWriter
    {
        public PacketWriter()
        { }

        public PacketWriter(int capacity)
        { }

        public int Length { get; }
        public int Position { get; set; }

        public void Write(Color value)
        { }

        public override void Write(double value)
        { }

        public void Write(Matrix value)
        { }

        public void Write(Quaternion value)
        { }

        public override void Write(float value)
        { }

        public void Write(Vector2 value)
        { }

        public void Write(Vector3 value)
        { }

        public void Write(Vector4 value)
        { }
    }
}