// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#Decoration
    internal enum SpirvDecorationType
    {
        RelaxedPrecision = 0,
        SpecId = 1,
        Block = 2,
        BufferBlock = 3,
        RowMajor = 4,
        ColMajor = 5,
        ArrayStride = 6,
        MatrixStride = 7,
        GLSLShared = 8,
        GLSLPacked = 9,
        CPacked = 10,
        BuiltIn = 11,
        NoPerspective = 13,
        Flat = 14,
        Patch = 15,
        Centroid = 16,
        Sample = 17,
        Invariant = 18,
        Restrict = 19,
        Aliased = 20,
        Volatile = 21,
        Constant = 22,
        Coherent = 23,
        NonWritable = 24,
        NonReadable = 25,
        Uniform = 26,
        UniformId = 27,
        SaturatedConversion = 28,
        Stream = 29,
        Location = 30,
        Component = 31,
        Index = 32,
        Binding = 33,
        DescriptorSet = 34,
        Offset = 35,
        XfbBuffer = 36,
        XfbStride = 37,
        FuncParamAttr = 38,
        FPRoundingMode = 39,
        FPFastMathMode = 40,
        LinkageAttributes = 41,
        NoContraction = 42,
        InputAttachmentIndex = 43,
        Alignment = 44,
        MaxByteOffset = 45,
        AlignmentId = 46,
        MaxByteOffsetId = 47,
        UserSemantic = 5635
    }

    internal class SpirvDecoration
    {
        public SpirvDecorationType Type { get; private set; }
        public string[] Args { get; private set; }

        internal static SpirvDecoration ParseDecorator(string[] definition)
        {
            if (!Enum.TryParse(definition[0], out SpirvDecorationType decorationType))
            {
                return null;
            }

            SpirvDecoration decoration = new();
            decoration.Type = decorationType;
            decoration.Args = definition[1..];

            return decoration;
        }
    }
}
