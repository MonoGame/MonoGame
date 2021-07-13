using System.Collections.Generic;
using System.IO;

namespace MonoGame.Effect
{
    public enum BufferType
    {
        Structured = 0,
        RWStructured = 1,
    }

    public struct BufferResourceData
    {
        public string Name;
        public string InstanceName;
        public int Size;
        public int Slot;
        public BufferType Type;
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
