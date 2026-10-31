// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Globalization;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpConstant
    internal class SpirvConstant
    {
        public required string Id { get; init; }
        public required SpirvTypeScalar Type { get; init; }
        // This can be an int or a floating point value. Just use a float here and cast to int when required.
        public required float Value { get; init; }

        internal static SpirvConstant? ParseConstant(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
        {
            if (parts.Length < 5)
                return null;

            if (!context.Types.TryGetValue(parts[3], out var type))
                return null;

            if (type is not SpirvTypeScalar scalar)
                return null;

            if (!float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var floatVal))
                return null;

            return new SpirvConstant
            {
                Id = parts[0],
                Type = scalar,
                Value = floatVal
            };
        }
    }
}
