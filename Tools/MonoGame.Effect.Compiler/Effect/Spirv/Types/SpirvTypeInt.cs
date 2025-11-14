// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeInt
    internal class SpirvTypeInt : SpirvTypeScalar
    {
        public override SpirvType Type => SpirvType.Int;
        public override uint Width { get; protected set; }
        public bool Signed { get; private set; }
        internal override void ParseArgs(string[] args, SpirvReflectionInfo.SpirvParseContext context)
        {
            Width = uint.Parse(args[0]);
            Signed = args[1] == "1";
        }
    }
}
