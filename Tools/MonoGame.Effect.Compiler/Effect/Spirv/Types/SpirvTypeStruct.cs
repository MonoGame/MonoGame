// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;
using System.Globalization;

namespace MonoGame.Effect.Compiler.Effect.Spirv;

internal class SpirvTypeStructMember
{
    public required int Index { get; init; }
    public required SpirvTypeBase Type { get; init; }
    public required string Name { get; init; }
    public uint Offset { get; private set; }
    public uint MatrixStride { get; private set; }

    internal void ApplyDecoration(SpirvDecoration decoration)
    {
        switch (decoration.Type)
        {
            case SpirvDecorationType.Offset:
                Offset = uint.Parse(decoration.Args[0], CultureInfo.InvariantCulture);
                break;
            case SpirvDecorationType.MatrixStride:
                MatrixStride = uint.Parse(decoration.Args[0], CultureInfo.InvariantCulture);
                break;
        }
    }
}

// https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeStruct
internal class SpirvTypeStruct : SpirvTypeBase
{
    public override SpirvType Type => SpirvType.Struct;

    public required List<SpirvTypeStructMember> Members { get; init; }

    public static SpirvTypeStruct? Parse(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
    {
        if (parts.Length < 1)
            return null;

        var id = parts[0];
        context.Names.TryGetValue(id, out var name);

        var members = new List<SpirvTypeStructMember>();

        if (!context.MemberNames.TryGetValue(id, out var memberNames))
        {
            memberNames = [];
        }

        for (int partsIdx = 3; partsIdx < parts.Length; partsIdx++)
        {
            var memberTypeId = parts[partsIdx];
            var memberIdx = partsIdx - 3;

            if (!context.Types.TryGetValue(memberTypeId, out var type))
            {
                Debug.WriteLine($"OpTypeStruct {name ?? id} uses a member of unencountered type: {memberTypeId}");
                return null;
            }

            if (!memberNames.TryGetValue(memberIdx, out var memberName))
            {
                Debug.WriteLine($"Could not find name for member {memberIdx} in SpirvTypeStruct {name ?? id}");
                memberName = memberTypeId;
            }

            members.Add(new SpirvTypeStructMember
            {
                Index = memberIdx,
                Type = type,
                Name = memberName
            });
        }

        return new SpirvTypeStruct
        {
            Id = id,
            Name = name,
            Members = members
        };
    }
}
