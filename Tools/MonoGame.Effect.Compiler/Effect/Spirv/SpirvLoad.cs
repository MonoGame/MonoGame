// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpLoad
    internal class SpirvLoad
    {
        public required string Id { get; init; }
        public required SpirvTypeBase ResultType { get; init; }
        public required SpirvVariable Variable { get; init; }

        internal static SpirvLoad? ParseLoad(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
        {
            if (parts.Length < 5)
                return null;
            
            if (!context.Types.TryGetValue(parts[3], out var spirvTypeBase))
            {
                Debug.WriteLine($"OpLoad referenced unparsed type {parts[3]}");
                return null;
            }

            if (!context.Variables.TryGetValue(parts[4], out var spirvVariable))
                return null;

            return new SpirvLoad
            {
                Id = parts[0],
                ResultType = spirvTypeBase,
                Variable = spirvVariable
            };
        }
    }
}
