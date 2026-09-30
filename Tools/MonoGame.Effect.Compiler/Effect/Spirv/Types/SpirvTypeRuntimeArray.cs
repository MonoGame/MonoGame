// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv;

// https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeRuntimeArray
internal class SpirvTypeRuntimeArray : SpirvTypeBase
{
    public override SpirvType Type => SpirvType.RuntimeArray;

    public required SpirvTypeBase ElementType { get; init; }

    public static SpirvTypeRuntimeArray? Parse(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
    {
        if (parts.Length < 4)
            return null;

        var id = parts[0];
        context.Names.TryGetValue(id, out var name);

        if (!context.Types.TryGetValue(parts[3], out var type))
        {
            Debug.WriteLine($"OpTypeRuntimeArray {name ?? id} uses elements of unencountered type {parts[3]}");
            return null;
        }

        return new SpirvTypeRuntimeArray
        {
            Id = id,
            Name = name,
            ElementType = type
        };
    }
}
