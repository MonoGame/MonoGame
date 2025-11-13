// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpVariable
    internal class SpirvVariable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public SpirvTypePointer Pointer { get; set; }
        public StorageClass StorageClass { get; set; }
        public List<SpirvDecoration> Decorations { get; private set; } = [];
        public int? BindingSlot { get; private set; }
        public uint? DescriptorSet { get; private set; }
        public uint? Location { get; private set; }
        public string HlslSemantic { get; private set; }

        internal static SpirvVariable ParseVariable(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
        {
            SpirvVariable result = new()
            {
                Id = parts[0]
            };

            if (context.Names.TryGetValue(result.Id, out string name))
            {
                result.Name = name;
            }

            if (!context.Types.TryGetValue(parts[3], out SpirvTypeBase type))
            {
                Debug.WriteLine($"OpVariable {result.Id ?? result.Name} references an unencountered type {parts[3]}");
            }
            else if (type is not SpirvTypePointer)
            {
                Debug.WriteLine($"[OpVariable {result.Id ?? result.Name} references a non-pointer type {parts[3]}");
            }

            result.Pointer = type as SpirvTypePointer;

            if (!Enum.TryParse(parts[4], false, out StorageClass storageClass))
            {
                Debug.WriteLine($"OpTypePointer {result.Name ?? result.Id} uses an unexpected storage type {parts[4]}");
            }

            result.StorageClass = storageClass;

            return result;
        }

        internal void ApplyDecoration(SpirvDecoration spirvDecoration)
        {
            // If we start needing to care about more kinds of decoration, we probably want to make a more generic system.
            // for now, only a few are important.
            switch (spirvDecoration.Type)
            {
                case SpirvDecorationType.Binding:
                    BindingSlot = int.Parse(spirvDecoration.Args[0]);
                    break;
                case SpirvDecorationType.DescriptorSet:
                    DescriptorSet = uint.Parse(spirvDecoration.Args[0]);
                    break;
                case SpirvDecorationType.Location:
                    Location = uint.Parse(spirvDecoration.Args[0]);
                    break;
                case SpirvDecorationType.UserSemantic:
                    HlslSemantic = spirvDecoration.Args[0].Trim('\"');
                    break;
            }
        }
    }
}
