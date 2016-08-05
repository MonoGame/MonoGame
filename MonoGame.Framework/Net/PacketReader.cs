using System;
using System.IO;

namespace Microsoft.Xna.Framework.Net
{
    public class PacketReader : BinaryReader
    {
        public PacketReader() : base(new MemoryStream())
        {
        }

        public PacketReader(int capacity) : base(new MemoryStream(capacity))
        { }

        public int Length { get; }
        public int Position { get; set; }
        
        public Color ReadColor()
        {
            throw new NotImplementedException();
        }

        public override double ReadDouble()
        {
            throw new NotImplementedException();
        }

        public Matrix ReadMatrix()
        {
            throw new NotImplementedException();
        }

        public Quaternion ReadQuaternion()
        {
            throw new NotImplementedException();
        }

        public override float ReadSingle()
        {
            throw new NotImplementedException();
        }

        public Vector2 ReadVector2()
        {
            throw new NotImplementedException();
        }

        public Vector3 ReadVector3()
        {
            throw new NotImplementedException();
        }

        public Vector4 ReadVector4()
        {
            throw new NotImplementedException();
        }
    }
}