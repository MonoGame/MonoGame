// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace MonoGame.Effect.Compiler.Effect.Spirv
{
    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#Dim
    internal enum ImageDimensionality
    {
        OneD,
        TwoD,
        ThreeD,
        Cube,
        Unknown
    }

    // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#OpTypeImage
    internal class SpirvTypeImage : SpirvTypeBase
    {
        public override SpirvType Type => SpirvType.Image;
        public ImageDimensionality Dimensionality { get; private set; }
        public SpirvTypeScalar SampleType { get; private set; }

        // Can be true, false, or unspecified
        public bool? Depth { get; private set; }
        public bool Arrayed { get; private set; }
        public bool Multisampled { get; private set; }

        // These properties are technically enumerations, but they're not used internally so I didn't bother defining them.
        public int Sampled { get; private set; }

        // https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#Image_Format
        public string ImageFormat { get; private set; }

        protected override void ParseArgs(string[] args, SpirvReflectionInfo.SpirvParseContext context)
        {
            if (!context.Types.TryGetValue(args[0], out SpirvTypeBase sampleType))
            {
                Debug.WriteLine($"OpTypeImage {Name ?? Id} has a sample type of unencountered type: {args[0]}");
                return;
            }

            if (!TryParseDimensionality(args[1], out ImageDimensionality dimensionality))
            {
                Debug.WriteLine($"OpTypeImage {Name ?? Id} has unsupported dimensionality {args[1]}");
                return;
            }

            Dimensionality = dimensionality;

            if (args[2] == "0") Depth = false;
            else if (args[2] == "1") Depth = true;

            Arrayed = args[3] == "1";
            Multisampled = args[4] == "1";
            Sampled = int.Parse(args[5]);
            ImageFormat = args[6];
        }


        private static bool TryParseDimensionality(string str, out ImageDimensionality dimensionality)
        {
            if (str == "1D") dimensionality = ImageDimensionality.OneD;
            else if (str == "2D") dimensionality = ImageDimensionality.TwoD;
            else if (str == "3D") dimensionality = ImageDimensionality.ThreeD;
            else if (str == "Cube") dimensionality = ImageDimensionality.Cube;
            else
            {
                dimensionality = ImageDimensionality.Unknown;
                return false;
            }

            return true;
        }
    }
}
