// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;
using System.Globalization;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpVariable
    internal class SpirvVariable
    {
        public required string Id { get; init; }

        public required string? Name { get; set; }

        public required SpirvTypePointer Pointer { get; init; }

        public required StorageClass StorageClass { get; init; }

        public uint? BindingSlot { get; private set; }

        public uint? DescriptorSet { get; private set; }

        public uint? Location { get; private set; }

        public string? HlslSemantic { get; private set; }

        internal static SpirvVariable? Parse(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
        {
            var id = parts[0];
            context.Names.TryGetValue(id, out var name);

            if (!context.Types.TryGetValue(parts[3], out var type))
            {
                Debug.WriteLine($"OpVariable {name ?? id} references an unencountered type {parts[3]}");
                return null;
            }

            if (type is not SpirvTypePointer pointer)
            {
                Debug.WriteLine($"[OpVariable {name ?? id} references a non-pointer type {parts[3]}");
                return null;
            }

            if (!Enum.TryParse(parts[4], false, out StorageClass storageClass))
            {
                Debug.WriteLine($"OpTypePointer {name ?? id} uses an unexpected storage type {parts[4]}");
                return null;
            }

            return new SpirvVariable()
            {
                Id = id,
                Name = name,
                Pointer = pointer,
                StorageClass = storageClass
            };
        }

        internal void ApplyDecoration(SpirvDecoration spirvDecoration)
        {
            // If we start needing to care about more kinds of decoration, we probably want to make a more generic system.
            // for now, only a few are important.
            switch (spirvDecoration.Type)
            {
                case SpirvDecorationType.Binding:
                    BindingSlot = uint.Parse(spirvDecoration.Args[0], CultureInfo.InvariantCulture);
                    break;
                case SpirvDecorationType.DescriptorSet:
                    DescriptorSet = uint.Parse(spirvDecoration.Args[0], CultureInfo.InvariantCulture);
                    break;
                case SpirvDecorationType.Location:
                    Location = uint.Parse(spirvDecoration.Args[0], CultureInfo.InvariantCulture);
                    break;
                case SpirvDecorationType.UserSemantic:
                    HlslSemantic = spirvDecoration.Args[0].Trim('\"');
                    break;
            }
        }
    }
}
