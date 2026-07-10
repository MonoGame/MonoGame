// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpConstant
    internal class SpirvConstant
    {
        public string Id { get; private set; }
        public SpirvTypeScalar Type { get; private set; }
        // This can be an int or a floating point value. Just use a float here and cast to int when required.
        public float Value { get; private set; }

        internal static SpirvConstant ParseConstant(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
        {
            if (!context.Types.TryGetValue(parts[3], out SpirvTypeBase type))
            {
                return null;
            }
            else if (type is not SpirvTypeScalar)
            {
                return null;
            }

            float value = float.Parse(parts[4]);

            return new SpirvConstant
            {
                Id = parts[0],
                Type = type as SpirvTypeScalar,
                Value = value
            };
        }
    }
}
