using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
	internal partial class DXPreshader
	{
        private MojoShader.MOJOSHADER_symbol[] _symbols;
        private MojoShader.MOJOSHADER_preshaderInstruction[] _instructions;
        private double[] _literals;
        private float[] _inRegs;
        private double[] _temps;
	}
}

