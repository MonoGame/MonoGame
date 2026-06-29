// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    internal class SpirvTypeStructMember
    {
        public int Index { get; init; }
        public SpirvTypeBase Type { get; init; }
        public string Name { get; init; }
        public uint? Offset { get; private set; }
        public uint? MatrixStride { get; private set; }

        internal void ApplyDecoration(SpirvDecoration decoration)
        {
            switch (decoration.Type)
            {
                case SpirvDecorationType.Offset:
                    Offset = uint.Parse(decoration.Args[0]);
                    break;
                case SpirvDecorationType.MatrixStride:
                    MatrixStride = uint.Parse(decoration.Args[0]);
                    break;
            }
        }
    }

    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeStruct
    internal class SpirvTypeStruct : SpirvTypeBase
    {
        public override SpirvType Type => SpirvType.Struct;
        public List<SpirvTypeStructMember> Members { get; private set; }

        protected override void ParseArgs(string[] args, SpirvReflectionInfo.SpirvParseContext context)
        {
            Members = [];

            if (!context.MemberNames.TryGetValue(Id, out Dictionary<int, string> memberNames))
            {
                memberNames = [];
            }

            for (int memberIdx = 0; memberIdx < args.Length; memberIdx++)
            {
                string memberTypeId = args[memberIdx];

                if (!context.Types.TryGetValue(memberTypeId, out SpirvTypeBase type))
                {
                    Debug.WriteLine($"OpTypeStruct {Name ?? Id} uses a member of unencountered type: {memberTypeId}");
                }

                if (!memberNames.TryGetValue(memberIdx, out string memberName))
                {
                    Debug.WriteLine($"Could not find name for member {memberIdx} in SpirvTypeStruct {Name ?? Id}");
                    memberName = memberTypeId;
                }

                Members.Add(new SpirvTypeStructMember
                {
                    Index = memberIdx,
                    Type = type,
                    Name = memberName
                });
            }
        }
    }
}
