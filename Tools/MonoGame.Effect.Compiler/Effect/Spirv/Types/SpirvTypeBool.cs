// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeBool
    internal class SpirvTypeBool : SpirvTypeScalar
    {
        public override SpirvType Type => SpirvType.Bool;
        public override uint Width { get; protected set; }

        internal override void ParseArgs(string[] args, SpirvReflectionInfo.SpirvParseContext context)
        {
            // TODO: Validate what this should be
            Width = 8;
        }
    }
}
