// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeVector
    internal class SpirvTypeVector : SpirvTypeBase
    {
        public override SpirvType Type => SpirvType.Vector;
        public SpirvTypeScalar ElementType { get; private set; }
        public uint Dimensions { get; private set; }
        public uint Width => ElementType.Width * Dimensions;

        internal override void ParseArgs(string[] args, SpirvReflectionInfo.SpirvParseContext context)
        {
            if (!context.Types.TryGetValue(args[0], out SpirvTypeBase type))
            {
                Debug.WriteLine($"OpTypeVector {Name ?? Id} uses elements of unencountered type {args[0]}");
                return;
            }
            else if (type is not SpirvTypeScalar)
            {
                Debug.WriteLine($"OpTypeVector {Name ?? Id} uses elements of unencountered type {args[0]}");
                return;
            }

            ElementType = type as SpirvTypeScalar;
            Dimensions = uint.Parse(args[1]);
        }
    }
}
