// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    public class SpirvLoad
    {
        public string Id { get; private set; }
        public SpirvTypeBase ResultType { get; private set; }
        public SpirvVariable Variable { get; private set; }

        internal static SpirvLoad ParseLoad(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
        {
            if (!context.Types.TryGetValue(parts[3], out SpirvTypeBase spirvTypeBase))
            {
                Debug.WriteLine($"OpLoad referenced unparsed type {parts[3]}");
                return null;
            }

            if (!context.Variables.TryGetValue(parts[4], out SpirvVariable spirvVariable))
            {
                return null;
            }

            return new SpirvLoad
            {
                Id = parts[0],
                ResultType = spirvTypeBase,
                Variable = spirvVariable
            };
        }
    }
}
