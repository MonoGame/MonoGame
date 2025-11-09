// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Effect.Compiler.Effect.Spirv.Types;
using System.Collections.Generic;

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
        public string Id { get; private set; }
        public string Name { get; private set; }
        public List<SpirvDecoration> Decorations { get; } = [];

        private void Parse(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
        {
            Id = parts[0];

            if (context.Names.TryGetValue(Id, out string name))
            {
                Name = name;
            }

            ParseArgs(parts[3..], context);
        }

        internal virtual void ParseArgs(string[] args, SpirvReflectionInfo.SpirvParseContext context)
        {
        }

        public override string ToString()
        {
            return $"{Type} {Name ?? Id}";
        }

        internal static SpirvTypeBase ParseType(string[] parts, SpirvReflectionInfo.SpirvParseContext context)
        {
            SpirvTypeBase type = null;

            switch (parts[2])
            {
                case "OpTypeVoid":
                    type = new SpirvTypeVoid();
                    break;
                case "OpTypeBool":
                    type = new SpirvTypeBool();
                    break;
                case "OpTypeInt":
                    type = new SpirvTypeInt();
                    break;
                case "OpTypeFloat":
                    type = new SpirvTypeFloat();
                    break;
                case "OpTypeVector":
                    type = new SpirvTypeVector();
                    break;
                case "OpTypeMatrix":
                    type = new SpirvTypeMatrix();
                    break;
                case "OpTypeImage":
                    type = new SpirvTypeImage();
                    break;
                case "OpTypeSampler":
                    type = new SpirvTypeSampler();
                    break;
                case "OpTypeSampledImage":
                    type = new SpirvTypeSampledImage();
                    break;
                case "OpTypeArray":
                    type = new SpirvTypeArray();
                    break;
                case "OpTypeRuntimeArray":
                    type = new SpirvTypeRuntimeArray();
                    break;
                case "OpTypeStruct":
                    type = new SpirvTypeStruct();
                    break;
                case "OpTypePointer":
                    type = new SpirvTypePointer();
                    break;
                default:
                    return null;
            }

            type.Parse(parts, context);
            return type;
        }
    }
}
