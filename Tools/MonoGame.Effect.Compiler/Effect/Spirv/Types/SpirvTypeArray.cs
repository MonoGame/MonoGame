// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeArray
    internal class SpirvTypeArray : SpirvTypeBase
    {
        public override SpirvType Type => SpirvType.Array;
        public SpirvTypeBase ElementType { get; private set; }
        public uint Length { get; private set; }
        public uint? ArrayStride { get; private set; }

        protected override void ParseArgs(string[] args, SpirvReflectionInfo.SpirvParseContext context)
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

        public override void ApplyDecoration(SpirvDecoration spirvDecoration)
        {
            if (spirvDecoration.Type == SpirvDecorationType.ArrayStride)
            {
                ArrayStride = uint.Parse(spirvDecoration.Args[0]);
            }
        }
    }
}
