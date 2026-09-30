// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv;

// https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#Storage_Class
internal enum StorageClass
{
    UniformConstant = 0,
    Input = 1,
    Uniform = 2,
    Output = 3
}

// https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypePointer
internal class SpirvTypePointer : SpirvTypeBase
{
    public override SpirvType Type => SpirvType.Pointer;

    public required StorageClass StorageClass { get; init; }

    public required SpirvTypeBase PointerType { get; init; }

    public static SpirvTypePointer? Parse(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
    {
        if (parts.Length < 5)
            return null;

        var id = parts[0];
        context.Names.TryGetValue(id, out var name);

        if (!Enum.TryParse(parts[3], false, out StorageClass storageClass))
        {
            Debug.WriteLine($"OpTypePointer {name ?? id} uses an unexpected storage type {parts[3]}");
            return null;
        }

        if (!context.Types.TryGetValue(parts[4], out var type))
        {
            Debug.WriteLine($"OpTypeStruct {name ?? id} uses a member of unencountered type {parts[4]}");
            return null;
        }

        return new SpirvTypePointer
        {
            Id = id,
            Name = name,
            StorageClass = storageClass,
            PointerType = type
        };
    }
}
