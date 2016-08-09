using System;
using System.IO;

namespace Microsoft.Xna.Framework.Net
{
    public class PacketReader : BinaryReader
    {
        public PacketReader() : base(new MemoryStream())
        { }

        public PacketReader(int capacity) : base(new MemoryStream(capacity))
        { }

        public int Length { get { return (int)BaseStream.Length; } }
        public int Position { get { return (int)BaseStream.Position; } set { BaseStream.Position = value; } }

        public Color ReadColor()
        {
            Color value = Color.TransparentBlack;
            value.PackedValue = ReadUInt32();
            return value;
        }

        public override double ReadDouble()
        {
            return base.ReadDouble();
        }

        public Matrix ReadMatrix()
        {
            Matrix value = Matrix.Identity;
            value.M11 = ReadSingle();
            value.M12 = ReadSingle();
            value.M13 = ReadSingle();
            value.M14 = ReadSingle();
            value.M21 = ReadSingle();
            value.M22 = ReadSingle();
            value.M23 = ReadSingle();
            value.M24 = ReadSingle();
            value.M31 = ReadSingle();
            value.M32 = ReadSingle();
            value.M33 = ReadSingle();
            value.M34 = ReadSingle();
            value.M41 = ReadSingle();
            value.M42 = ReadSingle();
            value.M43 = ReadSingle();
            value.M44 = ReadSingle();
            return value;
        }

        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        }

        public override float ReadSingle()
        {
            return base.ReadSingle();
        }

        public Vector2 ReadVector2()
        {
            return new Vector2(ReadSingle(), ReadSingle());
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        }

        public Vector4 ReadVector4()
        {
            return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        }
    }
}