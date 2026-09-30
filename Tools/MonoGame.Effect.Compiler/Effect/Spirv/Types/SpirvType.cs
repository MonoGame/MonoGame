// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Effect.Compiler.Effect.Spirv.Types;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#_type_declaration_instructions
    internal enum SpirvType
    {
        Void,
        Bool,
        Int,
        Float,
        Vector,
        Matrix,
        Image,
        Sampler,
        SampledImage,
        Array,
        RuntimeArray,
        Struct,
        Pointer,
        Function
    }

    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#_type_declaration_instructions
    internal abstract class SpirvTypeBase
    {
        public abstract SpirvType Type { get; }

        public required string Id { get; init; }

        public required string? Name { get; init; }

        public override string ToString() => $"{Type} {Name ?? Id}";

        public virtual void ApplyDecoration(SpirvDecoration spirvDecoration)
        {
        }

        protected virtual void ParseArgs(string[] args, SpirvReflectionInfo.SpirvParseContext context)
        {
        }

        internal static SpirvTypeBase? ParseType(string[] parts, SpirvReflectionInfo.SpirvParseContext context) => parts[2] switch
            {
                "OpTypeVoid" => SpirvTypeVoid.Parse(parts, context),
                "OpTypeBool" => SpirvTypeBool.Parse(parts, context),
                "OpTypeInt" => SpirvTypeInt.Parse(parts, context),
                "OpTypeFloat" => SpirvTypeFloat.Parse(parts, context),
                "OpTypeVector" => SpirvTypeVector.Parse(parts, context),
                "OpTypeMatrix" => SpirvTypeMatrix.Parse(parts, context),
                "OpTypeImage" => SpirvTypeImage.Parse(parts, context),
                "OpTypeSampler" => SpirvTypeSampler.Parse(parts, context),
                "OpTypeSampledImage" => SpirvTypeSampledImage.Parse(parts, context),
                "OpTypeArray" => SpirvTypeArray.Parse(parts, context),
                "OpTypeRuntimeArray" => SpirvTypeRuntimeArray.Parse(parts, context),
                "OpTypeStruct" => SpirvTypeStruct.Parse(parts, context),
                "OpTypePointer" => SpirvTypePointer.Parse(parts, context),
                _ => null
            };
    }
}
