// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    public class SpirvTypeArray : SpirvTypeBase
    {
        public override SpirvType Type => SpirvType.Array;
        public SpirvTypeBase ElementType { get; private set; }
        public uint Length { get; private set; }

        internal override void ParseArgs(string[] args, SpirvReflectionInfo.SpirvParseContext context)
        {
            if (!context.Types.TryGetValue(args[0], out SpirvTypeBase type))
            {     
                Debug.WriteLine($"OpTypeArray {Name ?? Id} uses elements of unencountered type: {args[3]}");
                return;
            }

            if (!context.Constants.TryGetValue(args[1], out SpirvConstant constant))
            {
                Debug.WriteLine($"OpTypeArray {Name ?? Id} specified unparsed constant for length {args[1]}");
                return;
            }

            ElementType = type;
            Length = (uint)constant.Value;
        }
    }
}
