using Microsoft.Xna.Framework.Graphics;

namespace TwoMGFX
{
	internal partial class ShaderData
	{
		public bool IsVertexShader;

		public struct Sampler
		{
			public MojoShader.MOJOSHADER_samplerType type;
			public int textureSlot;
            public int samplerSlot;
			public string samplerName;
			public string parameterName;
			public int parameter;
			public SamplerState state;
		}

		public struct Attribute
		{
			public int index;
			public string name;
			public VertexElementUsage usage;
			public short format;
		}

		/// <summary>
		/// The index to the constant buffers which are 
		/// required by this shader at runtime.
		/// </summary>
		public int[] _cbuffers;

		public Sampler[] _samplers;

		public Attribute[] _attributes;

		public byte[] ShaderCode { get; private set; }


#region Non-Serialized Stuff

		public byte[] Bytecode { get; private set; }

		// The index of the shader in the shared list.
		public int SharedIndex { get; private set; }

#endregion // Non-Serialized Stuff

	}
}

