using System;
using System.Runtime.InteropServices;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	internal partial class Shader
	{
        protected enum SamplerType
        {
            Sampler2D,
            SamplerCube,
            SamplerVolume,
        }

        protected struct Sampler
        {
            public SamplerType type;
            public int index;
            public int parameter;

            public string name;
        }

        protected Sampler[] _samplers;

        protected int[] _cbuffers;

		/// <summary>
		/// A hash value which can be used to compare shaders.
		/// </summary>
		public int HashKey { get; protected set; }

	}
}

