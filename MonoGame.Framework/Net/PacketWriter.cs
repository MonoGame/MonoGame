using System.IO;

namespace Microsoft.Xna.Framework.Net
{
    public class PacketWriter : BinaryWriter
    {
        public PacketWriter() : base(new MemoryStream())
        { }

        public PacketWriter(int capacity) : base(new MemoryStream(capacity))
        { }

        public int Length { get { return (int)BaseStream.Length; } }
        public int Position { get { return (int)BaseStream.Position; } set { BaseStream.Position = value; } }

        public void Write(Color value)
        {
            Write(value.PackedValue);
        }

        public override void Write(double value)
        {
            base.Write(value);
        }

        public void Write(Matrix value)
        {
            Write(value.M11);
            Write(value.M12);
            Write(value.M13);
            Write(value.M14);
            Write(value.M21);
            Write(value.M22);
            Write(value.M23);
            Write(value.M24);
            Write(value.M31);
            Write(value.M32);
            Write(value.M33);
            Write(value.M34);
            Write(value.M41);
            Write(value.M42);
            Write(value.M43);
            Write(value.M44);
        }

        public void Write(Quaternion value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }

        public override void Write(float value)
        {
            base.Write(value);
        }

        public void Write(Vector2 value)
        {
            Write(value.X);
            Write(value.Y);
        }

        public void Write(Vector3 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
        }

        public void Write(Vector4 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }
    }
}