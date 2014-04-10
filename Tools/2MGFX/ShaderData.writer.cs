using System.IO;

namespace TwoMGFX
{
    internal partial class ShaderData
    {
        public void Write(BinaryWriter writer, Options options)
        {
            writer.Write(IsVertexShader);

            writer.Write(ShaderCode.Length);
            writer.Write(ShaderCode);

            writer.Write((byte)_samplers.Length);
            foreach (var sampler in _samplers)
            {
                writer.Write((byte)sampler.type);
                writer.Write((byte)sampler.textureSlot);
                writer.Write((byte)sampler.samplerSlot);

				if (sampler.state != null)
				{
					writer.Write(true);
					writer.Write((byte)sampler.state.AddressU);
					writer.Write((byte)sampler.state.AddressV);
					writer.Write((byte)sampler.state.AddressW);
					writer.Write((byte)sampler.state.Filter);
					writer.Write(sampler.state.MaxAnisotropy);
					writer.Write(sampler.state.MaxMipLevel);
					writer.Write(sampler.state.MipMapLevelOfDetailBias);
				}
				else
					writer.Write(false);

                if (options.Profile == ShaderProfile.OpenGL)
                    writer.Write(sampler.samplerName);

                writer.Write((byte)sampler.parameter);
            }

            writer.Write((byte)_cbuffers.Length);
            foreach (var cb in _cbuffers)
                writer.Write((byte)cb);

            if (options.Profile != ShaderProfile.OpenGL)
                return;

            // The rest of this is for GL only!

            writer.Write((byte)_attributes.Length);
            foreach (var attrib in _attributes)
            {
                writer.Write(attrib.name);
                writer.Write((byte)attrib.usage);
                writer.Write((byte)attrib.index);
                writer.Write(attrib.format);
            }
        }
    }
}