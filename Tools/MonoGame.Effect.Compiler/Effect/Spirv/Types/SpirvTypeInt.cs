// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Globalization;

namespace MonoGame.Effect.Compiler.Effect.Spirv;

// https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeInt
internal class SpirvTypeInt : SpirvTypeScalar
{
    public override SpirvType Type => SpirvType.Int;

    public override uint Width { get; protected set; }

    public bool Signed { get; private set; }

    public static SpirvTypeInt? Parse(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
    {
        if (parts.Length < 5)
            return null;

        var id = parts[0];
        context.Names.TryGetValue(id, out var name);

        if (!uint.TryParse(parts[3], CultureInfo.InvariantCulture, out uint width))
            return null;

        return new SpirvTypeInt
        {
            Id = id,
            Name = name,
            Width = width,
            Signed = parts[4] == "1"
        };
    }
}
