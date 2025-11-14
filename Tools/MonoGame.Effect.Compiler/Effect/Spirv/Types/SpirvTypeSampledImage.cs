// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv.Types
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeSampledImage
    internal class SpirvTypeSampledImage : SpirvTypeBase
    {
        public override SpirvType Type => SpirvType.SampledImage;
        public SpirvTypeImage ImageType { get; private set; }

        internal override void ParseArgs(string[] args, SpirvReflectionInfo.SpirvParseContext context)
        {
            if (!context.Types.TryGetValue(args[0], out SpirvTypeBase type))
            {
                Debug.WriteLine($"Could not find type {args[0]} reference by OpTypeSampledImage {Name ?? Id}.");
            }
            else if (type is not SpirvTypeImage)
            {
                Debug.WriteLine($"Id {type.Name ?? type.Id} referenced by OpTypeSampledImage {Name ?? Id} is not an image type.");
            }

            ImageType = type as SpirvTypeImage;
        }
    }
}
