// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;
using System.Globalization;

namespace MonoGame.Effect.Compiler.Effect.Spirv;

// https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeArray
internal class SpirvTypeArray : SpirvTypeBase
{
    public override SpirvType Type => SpirvType.Array;

    public required SpirvTypeBase ElementType { get; init; }

    public required uint Length { get; init; }

    public uint ArrayStride { get; private set; }

    public static SpirvTypeArray? Parse(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
    {
        if (parts.Length < 4)
            return null;

        var id = parts[0];
        context.Names.TryGetValue(id, out var name);

        if (!context.Types.TryGetValue(parts[3], out var type))
        {
            Debug.WriteLine($"OpTypeArray {name ?? id} uses elements of unencountered type: {parts[3]}");
            return null;
        }

        if (!context.Constants.TryGetValue(parts[4], out var constant))
        {
            Debug.WriteLine($"OpTypeArray {name ?? id} specified unparsed constant for length {parts[4]}");
            return null;
        }

        return new SpirvTypeArray
        {
            Id = id,
            Name = name,
            ElementType = type,
            Length =  (uint)constant.Value
        };
    }

    public override void ApplyDecoration(SpirvDecoration spirvDecoration)
    {
        if (spirvDecoration.Type == SpirvDecorationType.ArrayStride)
        {
            ArrayStride = uint.Parse(spirvDecoration.Args[0], CultureInfo.InvariantCulture);
        }
    }
}
