// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
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
        public StorageClass StorageClass { get; private set; }
        public SpirvTypeBase PointerType { get; private set; }

        protected override void ParseArgs(string[] args, SpirvReflectionInfo.SpirvParseContext context)
        {
            if (!Enum.TryParse(args[0], false, out StorageClass storageClass))
            {
                Debug.WriteLine($"OpTypePointer {Name ?? Id} uses an unexpected storage type {args[0]}");
                return;
            }

            StorageClass = storageClass;

            if (!context.Types.TryGetValue(args[1], out SpirvTypeBase type))
            {
                Debug.WriteLine($"OpTypeStruct {Name ?? Id} uses a member of unencountered type {args[1]}");
                return;
            }

            PointerType = type;
        }
    }
}
