using System.Collections.Generic;
using System.IO;

namespace MonoGame.Effect
{
    public enum ShaderResourceType
    {
        StructuredBuffer = 0,
        RWStructuredBuffer = 1,
        ByteBuffer = 2,
        RWByteBuffer = 3,
        RWTexture = 4,
    }

    public struct ShaderResourceData
    {
        public string Name;
        public string InstanceName;
        public int Size;
        public int Slot;
        public ShaderResourceType Type;
        public int Parameter;

        public void Write(BinaryWriter writer, Options options)
        {
            writer.Write(Name);
            writer.Write(InstanceName);
            writer.Write((ushort)Size);
            writer.Write((byte)Slot);
            writer.Write((byte)Type);
            writer.Write((byte)Parameter);
        }
    }
}
