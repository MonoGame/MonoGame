using System.IO;

using MonoGame.Framework.Utilities;

namespace MonoGame.Effect
{
    internal partial class ConstantBufferData
    {
        public void Write(BinaryWriterEx writer, Options options)
        {
            writer.Write(Name);

            writer.Write((ushort)Size);

            writer.WriteCount(ParameterIndex.Count);
            for (var i=0; i < ParameterIndex.Count; i++)
            {
                writer.WriteIndex(ParameterIndex[i]);
                writer.Write((ushort)ParameterOffset[i]);
            }
        }
    }
}
