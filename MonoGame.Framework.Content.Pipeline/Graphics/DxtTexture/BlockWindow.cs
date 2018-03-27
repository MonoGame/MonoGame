using System;

using Vec3 = Microsoft.Xna.Framework.Vector3;

namespace TextureSquish
{
    sealed class BlockWindow
    {
        public BlockWindow(Byte[] array, CompressionMode flags)
        {
            _Array = array;

            flags = flags.FixFlags();

            _BytesPerBlock = ((flags & CompressionMode.Dxt1) != 0) ? 8 : 16;
            _ColourOffset = (flags & (CompressionMode.Dxt3 | CompressionMode.Dxt5)) != 0 ? 8 : 0;
        }

        private readonly Byte[] _Array;
        private readonly int _BytesPerBlock;
        private readonly int _ColourOffset;

        public int Offset { get; set; }

        public int ByteLength { get { return _BytesPerBlock; } }

        public Byte this[int index]
        {
            get { System.Diagnostics.Debug.Assert(index >= 0 && index < _BytesPerBlock); return _Array[Offset + index]; }
            set { System.Diagnostics.Debug.Assert(index >= 0 && index < _BytesPerBlock); _Array[Offset + index] = value; }
        }

        private void WriteColourBlock(int a, int b, byte[] indices)
        {
            // write the endpoints
            this[_ColourOffset + 0] = (Byte)(a & 0xff);
            this[_ColourOffset + 1] = (Byte)(a >> 8);
            this[_ColourOffset + 2] = (Byte)(b & 0xff);
            this[_ColourOffset + 3] = (Byte)(b >> 8);

            // write the indices
            for (int i = 0; i < 4; ++i)
            {
                var ind = 4 * i;
                this[_ColourOffset + 4 + i] = (Byte)(indices[ind + 0] | (indices[ind + 1] << 2) | (indices[ind + 2] << 4) | (indices[ind + 3] << 6));
            }
        }

        private void WriteAlphaBlock(int alpha0, int alpha1, Byte[] indices)
        {
            // write the first two bytes
            this[0] = (byte)alpha0;
            this[1] = (byte)alpha1;

            // pack the indices with 3 bits each
            int destIdx = 2;
            int indIdx = 0;

            for (int i = 0; i < 2; ++i)
            {
                // pack 8 3-bit values
                int value = 0;
                for (int j = 0; j < 8; ++j)
                {
                    int index = indices[indIdx++];
                    value |= (index << 3 * j);
                }

                // store in 3 bytes
                for (int j = 0; j < 3; ++j)
                {
                    int val = (value >> 8 * j) & 0xff;

                    this[destIdx++] = (Byte)val;
                }
            }
        }

        public void WriteColourBlock3(Vec3 start, Vec3 end, Byte[] indices)
        {
            // get the packed values
            int a = start.ToPackedInt565();
            int b = end.ToPackedInt565();

            // remap the indices
            var remapped = new Byte[16];

            if (a <= b)
            {
                // use the indices directly
                for (int i = 0; i < 16; ++i)
                    remapped[i] = indices[i];
            }
            else
            {
                // swap a and b
                var c = a;
                a = b;
                b = c;

                for (int i = 0; i < 16; ++i)
                {
                    if (indices[i] == 0) remapped[i] = 1;
                    else if (indices[i] == 1) remapped[i] = 0;
                    else remapped[i] = indices[i];
                }
            }

            // write the block
            WriteColourBlock(a, b, remapped);
        }

        public void WriteColourBlock4(Vec3 start, Vec3 end, Byte[] indices)
        {
            // get the packed values
            int a = start.ToPackedInt565();
            int b = end.ToPackedInt565();

            // remap the indices
            var remapped = new Byte[16];
            if (a < b)
            {
                // swap a and b
                var c = a;
                a = b;
                b = c;
                for (int i = 0; i < 16; ++i)
                    remapped[i] = (Byte)((indices[i] ^ 0x1) & 0x3);
            }
            else if (a == b)
            {
                // use index 0
                for (int i = 0; i < 16; ++i)
                    remapped[i] = 0;
            }
            else
            {
                // use the indices directly
                for (int i = 0; i < 16; ++i)
                    remapped[i] = indices[i];
            }

            // write the block
            WriteColourBlock(a, b, remapped);
        }

        private void WriteAlphaBlock5(int alpha0, int alpha1, Byte[] indices)
        {
            // check the relative values of the endpoints
            if (alpha0 > alpha1)
            {
                // swap the indices
                var swapped = new Byte[16];
                for (int i = 0; i < 16; ++i)
                {
                    int index = indices[i];

                    if (index == 0) swapped[i] = 1;
                    else if (index == 1) swapped[i] = 0;
                    else if (index <= 5) swapped[i] = (Byte)(7 - index);
                    else swapped[i] = (Byte)index;
                }

                // write the block
                WriteAlphaBlock(alpha1, alpha0, swapped);
            }
            else
            {
                // write the block
                WriteAlphaBlock(alpha0, alpha1, indices);
            }
        }

        private void WriteAlphaBlock7(int alpha0, int alpha1, Byte[] indices)
        {
            // check the relative values of the endpoints
            if (alpha0 < alpha1)
            {
                // swap the indices
                var swapped = new Byte[16];
                for (int i = 0; i < 16; ++i)
                {
                    int index = indices[i];
                    if (index == 0) swapped[i] = 1;
                    else if (index == 1) swapped[i] = 0;
                    else swapped[i] = (Byte)(9 - index);
                }

                // write the block
                WriteAlphaBlock(alpha1, alpha0, swapped);
            }
            else
            {
                // write the block
                WriteAlphaBlock(alpha0, alpha1, indices);
            }
        }

        public void CompressAlphaDxt3(Byte[] rgba, int mask)
        {
            // quantise and pack the alpha values pairwise
            for (int i = 0; i < 8; ++i)
            {
                // quantise down to 4 bits
                float alpha1 = (float)rgba[8 * i + 3] * (15.0f / 255.0f);
                float alpha2 = (float)rgba[8 * i + 7] * (15.0f / 255.0f);
                int quant1 = alpha1.FloatToInt(15);
                int quant2 = alpha2.FloatToInt(15);

                // set alpha to zero where masked
                int bit1 = 1 << (2 * i);
                int bit2 = 1 << (2 * i + 1);
                if ((mask & bit1) == 0)
                    quant1 = 0;
                if ((mask & bit2) == 0)
                    quant2 = 0;

                // pack into the byte
                this[i] = (Byte)(quant1 | (quant2 << 4));
            }
        }

        public void DecompressAlphaDxt3(Byte[] rgba)
        {
            // unpack the alpha values pairwise
            for (int i = 0; i < 8; ++i)
            {
                // quantise down to 4 bits
                int quant = this[i];

                // unpack the values
                int lo = (quant & 0x0f);
                int hi = (quant & 0xf0);

                // convert back up to bytes
                rgba[8 * i + 3] = (Byte)(lo | (lo << 4));
                rgba[8 * i + 7] = (Byte)(hi | (hi >> 4));
            }
        }

        private static void FixRange(ref int min, ref int max, int steps)
        {
            if (max - min < steps) max = Math.Min(min + steps, 255);
            if (max - min < steps) min = Math.Max(0, max - steps);
        }

        private static int FitCodes(Byte[] rgba, int mask, byte[] codes, byte[] indices)
        {
            // fit each alpha value to the codebook
            int err = 0;
            for (int i = 0; i < 16; ++i)
            {
                // check this pixel is valid
                int bit = 1 << i;
                if ((mask & bit) == 0)
                {
                    // use the first code
                    indices[i] = 0;
                    continue;
                }

                // find the least error and corresponding index
                int value = rgba[4 * i + 3];
                int least = int.MaxValue;
                int index = 0;
                for (int j = 0; j < 8; ++j)
                {
                    // get the squared error from this code
                    int dist = (int)value - (int)codes[j];
                    dist *= dist;

                    // compare with the best so far
                    if (dist < least)
                    {
                        least = dist;
                        index = j;
                    }
                }

                // save this index and accumulate the error
                indices[i] = (Byte)index;
                err += least;
            }

            // return the total error
            return err;
        }

        public void CompressAlphaDxt5(Byte[] rgba, int mask)
        {
            // get the range for 5-alpha and 7-alpha interpolation
            int min5 = 255;
            int max5 = 0;
            int min7 = 255;
            int max7 = 0;
            for (int i = 0; i < 16; ++i)
            {
                // check this pixel is valid
                int bit = 1 << i;
                if ((mask & bit) == 0) continue;

                // incorporate into the min/max
                int value = rgba[4 * i + 3];
                if (value < min7) min7 = value;
                if (value > max7) max7 = value;
                if (value != 0 && value < min5) min5 = value;
                if (value != 255 && value > max5) max5 = value;
            }

            // handle the case that no valid range was found
            if (min5 > max5) min5 = max5;
            if (min7 > max7) min7 = max7;

            // fix the range to be the minimum in each case
            FixRange(ref min5, ref max5, 5);

            FixRange(ref min7, ref max7, 7);

            // set up the 5-alpha code book
            var codes5 = new Byte[8];
            codes5[0] = (Byte)min5;
            codes5[1] = (Byte)max5;
            for (int i = 1; i < 5; ++i)
                codes5[1 + i] = (byte)(((5 - i) * min5 + i * max5) / 5);
            codes5[6] = 0;
            codes5[7] = 255;

            // set up the 7-alpha code book
            var codes7 = new Byte[8];
            codes7[0] = (Byte)min7;
            codes7[1] = (Byte)max7;
            for (int i = 1; i < 7; ++i)
                codes7[1 + i] = (Byte)(((7 - i) * min7 + i * max7) / 7);

            // fit the data to both code books
            var indices5 = new Byte[16];
            var indices7 = new Byte[16];
            int err5 = FitCodes(rgba, mask, codes5, indices5);
            int err7 = FitCodes(rgba, mask, codes7, indices7);

            // save the block with least error
            if (err5 <= err7) this.WriteAlphaBlock5(min5, max5, indices5);
            else this.WriteAlphaBlock7(min7, max7, indices7);
        }

        public void DecompressAlphaDxt5(Byte[] rgba)
        {
            // get the two alpha values

            int alpha0 = this[0];
            int alpha1 = this[1];

            // compare the values to build the codebook
            var codes = new Byte[8];
            codes[0] = (Byte)alpha0;
            codes[1] = (Byte)alpha1;
            if (alpha0 <= alpha1)
            {
                // use 5-alpha codebook
                for (int i = 1; i < 5; ++i)
                    codes[1 + i] = (Byte)(((5 - i) * alpha0 + i * alpha1) / 5);
                codes[6] = 0;
                codes[7] = 255;
            }
            else
            {
                // use 7-alpha codebook
                for (int i = 1; i < 7; ++i)
                    codes[1 + i] = (Byte)(((7 - i) * alpha0 + i * alpha1) / 7);
            }

            // decode the indices
            var indices = new Byte[16];
            int bytesIdx = 2;
            var indIdx = 0; // u8* dest = indices;
            for (int i = 0; i < 2; ++i)
            {
                // grab 3 bytes
                int value = 0;
                for (int j = 0; j < 3; ++j)
                {
                    int val = this[bytesIdx++];
                    value |= (val << 8 * j);
                }

                // unpack 8 3-bit values from it
                for (int j = 0; j < 8; ++j)
                {
                    int index = (value >> 3 * j) & 0x7;

                    indices[indIdx++] = (Byte)index;
                }
            }

            // write out the indexed codebook values
            for (int i = 0; i < 16; ++i)
                rgba[4 * i + 3] = codes[indices[i]];
        }

        public void DecompressColour(byte[] rgba, bool isDxt1)
        {
            // unpack the endpoints
            var codes = new Byte[16];
            int a = Unpack565(_Array, Offset + _ColourOffset + 0, codes, 0);
            int b = Unpack565(_Array, Offset + _ColourOffset + 2, codes, 4);

            // generate the midpoints
            for (int i = 0; i < 3; ++i)
            {
                int c = codes[i];
                int d = codes[4 + i];

                if (isDxt1 && a <= b)
                {
                    codes[8 + i] = (Byte)((c + d) / 2);
                    codes[12 + i] = 0;
                }
                else
                {
                    codes[8 + i] = (Byte)((2 * c + d) / 3);
                    codes[12 + i] = (Byte)((c + 2 * d) / 3);
                }
            }

            // fill in alpha for the intermediate values
            codes[8 + 3] = 255;
            codes[12 + 3] = (Byte)((isDxt1 && a <= b) ? 0 : 255);

            // unpack the indices
            var indices = new Byte[16];
            for (int i = 0; i < 4; ++i)
            {
                int ind = 4 * i;
                var packed = _Array[Offset+_ColourOffset+ 4 + i];

                indices[ind + 0] = (Byte)(packed & 0x3);
                indices[ind + 1] = (Byte)((packed >> 2) & 0x3);
                indices[ind + 2] = (Byte)((packed >> 4) & 0x3);
                indices[ind + 3] = (Byte)((packed >> 6) & 0x3);
            }

            // store out the colours
            for (int i = 0; i < 16; ++i)
            {
                var offset = 4 * indices[i];
                for (int j = 0; j < 4; ++j)
                    rgba[4 * i + j] = codes[offset + j];
            }
        }

        private static int Unpack565(byte[] packed, int packedOffset, byte[] colour, int colourOffset)
        {
            // build the packed value
            int value = (int)packed[packedOffset + 0] | ((int)packed[packedOffset + 1] << 8);

            // get the components in the stored range
            int red = (Byte)((value >> 11) & 0x1f);
            int green = (Byte)((value >> 5) & 0x3f);
            int blue = (Byte)(value & 0x1f);

            // scale up to 8 bits
            colour[colourOffset + 0] = (Byte)((red << 3) | (red >> 2));
            colour[colourOffset + 1] = (Byte)((green << 2) | (green >> 4));
            colour[colourOffset + 2] = (Byte)((blue << 3) | (blue >> 2));
            colour[colourOffset + 3] = 255;

            // return the value
            return value;
        }

        /// <summary>
        /// Compresses a 4x4 block of pixels.
        /// </summary>
        /// <remarks>
        /// The source pixels should be presented as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this should be:
        /// 
        ///   { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        /// 
        /// The flags parameter should specify either kDxt1, kDxt3 or kDxt5 compression, 
        /// however, DXT1 will be used by default if none is specified. When using DXT1 
        /// compression, 8 bytes of storage are required for the compressed DXT block. 
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        /// The flags parameter can also specify a preferred colour compressor and 
        /// colour error metric to use when fitting the RGB components of the data. 
        /// Possible colour compressors are: kColourClusterFit (the default), 
        /// kColourRangeFit or kColourIterativeClusterFit. Possible colour error metrics 
        /// are: kColourMetricPerceptual (the default) or kColourMetricUniform. If no 
        /// flags are specified in any particular category then the default will be 
        /// used. Unknown flags are ignored.
        /// 
        /// When using kColourClusterFit, an additional flag can be specified to
        /// weight the colour of each pixel by its alpha value. For images that are
        /// rendered using alpha blending, this can significantly increase the 
        /// perceived quality.
        /// 
        /// </remarks>
        /// <param name="rgba">The rgba values of the 16 source pixels.</param>        
        /// <param name="flags">Compression flags.</param>
        public void Compress(Byte[] rgba, CompressionMode flags)
        {
            // compress with full mask
            CompressMasked(rgba, 0xffff, flags);
        }
        
        /// <summary>
        /// Compresses a 4x4 block of pixels.
        /// </summary>
        /// <remarks>
        /// The source pixels should be presented as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this should be:
        /// 
        ///     { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        ///     
        /// The mask parameter enables only certain pixels within the block. The lowest
        /// bit enables the first pixel and so on up to the 16th bit. Bits beyond the
        /// 16th bit are ignored. Pixels that are not enabled are allowed to take
        /// arbitrary colours in the output block. An example of how this can be used
        /// is in the CompressImage function to disable pixels outside the bounds of
        /// the image when the width or height is not divisible by 4.
        /// 
        /// The flags parameter should specify either kDxt1, kDxt3 or kDxt5 compression, 
        /// however, DXT1 will be used by default if none is specified. When using DXT1 
        /// compression, 8 bytes of storage are required for the compressed DXT block. 
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        /// 
        /// The flags parameter can also specify a preferred colour compressor and 
        /// colour error metric to use when fitting the RGB components of the data.
        /// Possible colour compressors are: kColourClusterFit (the default), 
        /// kColourRangeFit or kColourIterativeClusterFit. Possible colour error metrics 
        /// are: kColourMetricPerceptual (the default) or kColourMetricUniform. If no 
        /// flags are specified in any particular category then the default will be 
        /// used. Unknown flags are ignored.
        /// 
        /// When using kColourClusterFit, an additional flag can be specified to
        /// weight the colour of each pixel by its alpha value. For images that are
        /// rendered using alpha blending, this can significantly increase the 
        /// perceived quality.
        /// </remarks>
        /// <param name="rgba">The rgba values of the 16 source pixels.</param>
        /// <param name="mask">The valid pixel mask.</param>
        /// <param name="flags">Compression flags.</param>
        public void CompressMasked(Byte[] rgba, int mask, CompressionMode flags)
        {
            System.Diagnostics.Debug.Assert(rgba != null && rgba.Length == 64, "rgba");

            // fix any bad flags
            flags = flags.FixFlags();

            // create the minimal point set
            var colours = new ColourSet(rgba, mask, flags);

            // check the compression type and compress colour
            if (colours.Count == 1)
            {
                // always do a single colour fit
                var fit = new SingleColourFit(colours, flags);
                fit.Compress(this);
            }
            else if ((flags & CompressionMode.ColourRangeFit) != 0 || colours.Count == 0)
            {
                // do a range fit
                var fit = new RangeFit(colours, flags);
                fit.Compress(this);
            }
            else
            {
                // default to a cluster fit (could be iterative or not)
                var fit = new ClusterFit(colours, flags);
                fit.Compress(this);
            }

            // compress alpha separately if necessary
            if ((flags & CompressionMode.Dxt3) != 0) this.CompressAlphaDxt3(rgba, mask);
            else if ((flags & CompressionMode.Dxt5) != 0) this.CompressAlphaDxt5(rgba, mask);
        }

        /// <summary>
        /// Decompresses a 4x4 block of pixels.
        /// </summary>
        /// <remarks>
        /// The decompressed pixels will be written as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this is:
        /// 
        ///     { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        ///     
        /// The flags parameter should specify either kDxt1, kDxt3 or kDxt5 compression, 
        /// however, DXT1 will be used by default if none is specified. All other flags
        /// are ignored.
        /// </remarks>
        /// <param name="rgba">Storage for the 16 decompressed pixels.</param>
        /// <param name="flags">Compression flags.</param>
        public void Decompress(Byte[] rgba, CompressionMode flags)
        {
            System.Diagnostics.Debug.Assert(rgba != null && rgba.Length == 64, "rgba");

            // fix any bad flags
            flags = flags.FixFlags();

            // decompress colour
            this.DecompressColour(rgba, (flags & CompressionMode.Dxt1) != 0);

            // decompress alpha separately if necessary
            if ((flags & CompressionMode.Dxt3) != 0) this.DecompressAlphaDxt3(rgba);
            else if ((flags & CompressionMode.Dxt5) != 0) this.DecompressAlphaDxt5(rgba);
        }
    }

    
}


