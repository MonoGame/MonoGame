// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv.Types;

// https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeSampledImage
internal class SpirvTypeSampledImage : SpirvTypeBase
{
    public override SpirvType Type => SpirvType.SampledImage;

    public required SpirvTypeImage ImageType { get; init; }

    public static SpirvTypeSampledImage? Parse(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
    {
        if (parts.Length < 4)
            return null;

        var id = parts[0];
        context.Names.TryGetValue(id, out var name);

        if (!context.Types.TryGetValue(parts[3], out var type))
        {
            Debug.WriteLine($"Could not find type {parts[3]} reference by OpTypeSampledImage {name ?? id}.");
            return null;
        }

        if (type is not SpirvTypeImage image)
        {
            Debug.WriteLine($"Id {type.Name ?? type.Id} referenced by OpTypeSampledImage {name ?? id} is not an image type.");
            return null;
        }

        return new SpirvTypeSampledImage
        {
            Id = id,
            Name = name,
            ImageType = image
        };
    }
}
