// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;
using System.Globalization;

namespace MonoGame.Effect.Compiler.Effect.Spirv;

// https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeVector
internal class SpirvTypeVector : SpirvTypeBase
{
    public override SpirvType Type => SpirvType.Vector;

    public required SpirvTypeScalar ElementType { get; init; }

    public required uint Dimensions { get; init; }

    public static SpirvTypeVector? Parse(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
    {
        if (parts.Length < 5)
            return null;

        var id = parts[0];
        context.Names.TryGetValue(id, out var name);

        if (!context.Types.TryGetValue(parts[3], out var type) || type is not SpirvTypeScalar value)
        {
            Debug.WriteLine($"OpTypeVector {name ?? id} uses elements of unencountered type {parts[3]}");
            return null;
        }

        if (!uint.TryParse(parts[4], CultureInfo.InvariantCulture, out uint dimensions))
            return null;

        return new SpirvTypeVector
        {
            Id = id,
            Name = name,
            ElementType = value,
            Dimensions = dimensions
        };
    }
}
