// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;
using System.Globalization;

namespace MonoGame.Effect.Compiler.Effect.Spirv;

// https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeMatrix
internal class SpirvTypeMatrix : SpirvTypeBase
{
    public override SpirvType Type => SpirvType.Matrix;

    public required SpirvTypeVector ColumnType { get; init; }

    public required uint Columns { get; init; }

    public static SpirvTypeMatrix? Parse(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
    {
        if (parts.Length < 5)
            return null;

        var id = parts[0];
        context.Names.TryGetValue(id, out var name);

        if (!context.Types.TryGetValue(parts[3], out var opTypeBase))
        {
            Debug.WriteLine($"OpTypeMatrix {name ?? id} uses columns of unencountered type: {parts[3]}");
            return null;
        }

        if (opTypeBase is not SpirvTypeVector vector)
        {
            Debug.WriteLine($"OpTypeMatrix {name ?? id} specifies type {parts[3]} for its columns, which is not a vector.");
            return null;
        }

        if (!uint.TryParse(parts[4], CultureInfo.InvariantCulture, out uint columns))
            return null;

        return new SpirvTypeMatrix
        {
            Id = id,
            Name = name,
            ColumnType = vector,
            Columns = columns
        };
    }
}
