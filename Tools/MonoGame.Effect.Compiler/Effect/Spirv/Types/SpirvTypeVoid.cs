// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // I'm not sure this will ever be used.
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeVoid
    internal class SpirvTypeVoid : SpirvTypeBase
    {
        public override SpirvType Type => SpirvType.Void;
    }
}
