// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeMatrix
    internal class SpirvTypeMatrix : SpirvTypeBase
    {
        public override SpirvType Type => SpirvType.Matrix;
        public SpirvTypeVector ColumnType { get; private set; }
        public uint Columns { get; private set; }
        public uint Width => ColumnType.Width * Columns;

        internal override void ParseArgs(string[] args, SpirvReflectionInfo.SpirvParseContext context)
        {
            if (!context.Types.TryGetValue(args[0], out SpirvTypeBase opTypeBase))
            {
                Debug.WriteLine($"OpTypeMatrix {Name ?? Id} uses columns of unencountered type: {args[0]}");
                return;
            }
            else if (opTypeBase is not SpirvTypeVector)
            {
                Debug.WriteLine($"OpTypeMatrix {Name ?? Id} specifies type {args[0]} for its columns, which is not a vector.");
                return;
            }

            ColumnType = opTypeBase as SpirvTypeVector;
            Columns = uint.Parse(args[1]);
        }
    }
}
