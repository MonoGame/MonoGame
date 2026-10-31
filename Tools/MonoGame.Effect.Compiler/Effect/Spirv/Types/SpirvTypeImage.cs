// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;
using System.Globalization;

namespace MonoGame.Effect.Compiler.Effect.Spirv;

// https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#Dim
internal enum ImageDimensionality
{
    OneD,
    TwoD,
    ThreeD,
    Cube,
    Unknown
}

// https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeImage
internal class SpirvTypeImage : SpirvTypeBase
{
    public override SpirvType Type => SpirvType.Image;

    public required ImageDimensionality Dimensionality { get; init; }

    public required SpirvTypeBase SampleType { get; init; }

    // Can be true, false, or unspecified
    public required bool? Depth { get; init; }

    public required bool Arrayed { get; init; }

    public required bool Multisampled { get; init; }

    // These properties are technically enumerations, but they're not used internally so I didn't bother defining them.
    public required int Sampled { get; init; }

    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#Image_Format
    public required string ImageFormat { get; init; }

    public static SpirvTypeImage? Parse(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
    {
        if (parts.Length < 10)
            return null;

        var id = parts[0];
        context.Names.TryGetValue(id, out var name);

        if (!context.Types.TryGetValue(parts[3], out var sampleType))
        {
            Debug.WriteLine($"OpTypeImage {name ?? id} has a sample type of unencountered type: {parts[3]}");
            return null;
        }

        if (!TryParseDimensionality(parts[4], out var dimensionality))
        {
            Debug.WriteLine($"OpTypeImage {name ?? id} has unsupported dimensionality {parts[4]}");
            return null;
        }

        if (!int.TryParse(parts[8], CultureInfo.InvariantCulture, out int sampled))
            return null;

        return new SpirvTypeImage
        {
            Id = id,
            Name = name,
            SampleType = sampleType,
            Dimensionality = dimensionality,
            Depth = parts[5] switch
            {
                "0" => false,
                "1" => true,
                _ => null
            },
            Arrayed = parts[6] == "1",
            Multisampled = parts[7] == "1",
            Sampled = sampled,
            ImageFormat = parts[9]
        };
    }

    private static bool TryParseDimensionality(string str, out ImageDimensionality dimensionality)
    {
        switch (str)
        {
            case "1D":
                dimensionality = ImageDimensionality.OneD;
                break;
            case "2D":
                dimensionality = ImageDimensionality.TwoD;
                break;
            case "3D":
                dimensionality = ImageDimensionality.ThreeD;
                break;
            case "Cube":
                dimensionality = ImageDimensionality.Cube;
                break;
            default:
                dimensionality = ImageDimensionality.Unknown;
                return false;
        }

        return true;
    }
}
