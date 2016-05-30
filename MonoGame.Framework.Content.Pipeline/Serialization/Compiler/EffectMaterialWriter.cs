using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class EffectMaterialWriter : BuiltInContentWriter<EffectMaterialContent>
    {
        protected internal override void Write(ContentWriter output, EffectMaterialContent value)
        {
            output.WriteExternalReference(value.CompiledEffect);
            var dict = new Dictionary<string, object>();
            foreach (var item in value.Textures)
            {
                dict.Add(item.Key, item.Value);
            }
            foreach (var item in value.OpaqueData)
            {
                dict.Add(item.Key, item.Value);
            }
            output.WriteObject(dict);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var type = typeof(ContentReader);
            var readerType = type.Namespace + ".EffectMaterialReader, " + type.Assembly.FullName;
            return readerType;
        }
    }
}