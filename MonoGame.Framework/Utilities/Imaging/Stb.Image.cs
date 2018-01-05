using System;
using System.Runtime.InteropServices;

namespace MonoGame.Utilities
{
    internal unsafe partial class Imaging
    {
        public static string LastError;

        public const int STBI__ZFAST_BITS = 9;

        public delegate int ReadCallback(void* user, sbyte* data, int size);

        public delegate int SkipCallback(void* user, int n);

        public delegate int EofCallback(void* user);

        public delegate void idct_block_kernel(byte* output, int out_stride, short* data);

        public delegate void YCbCr_to_RGB_kernel(
            byte* output, byte* y, byte* pcb, byte* pcr, int count, int step);

        public delegate byte* Resampler(byte* a, byte* b, byte* c, int d, int e);

        public static string stbi__g_failure_reason;
        public static int stbi__vertically_flip_on_load;

        public class stbi_io_callbacks
        {
            public ReadCallback read;
            public SkipCallback skip;
            public EofCallback eof;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct img_comp
        {
            public int id;
            public int h, v;
            public int tq;
            public int hd, ha;
            public int dc_pred;

            public int x, y, w2, h2;
            public byte* data;
            public void* raw_data;
            public void* raw_coeff;
            public byte* linebuf;
            public short* coeff; // progressive only
            public int coeff_w, coeff_h; // number of 8x8 coefficient blocks
        }

        public class stbi__jpeg
        {
            public stbi__context s;
            public readonly PinnedArray<stbi__huffman> huff_dc = new PinnedArray<stbi__huffman>(4);
            public readonly PinnedArray<stbi__huffman> huff_ac = new PinnedArray<stbi__huffman>(4);
            public readonly PinnedArray<ushort>[] dequant;

            public readonly PinnedArray<short>[] fast_ac;

// sizes for components, interleaved MCUs
            public int img_h_max, img_v_max;
            public int img_mcu_x, img_mcu_y;
            public int img_mcu_w, img_mcu_h;

// definition of jpeg image component
            public img_comp[] img_comp = new img_comp[4];

            public uint code_buffer; // jpeg entropy-coded buffer
            public int code_bits; // number of valid bits
            public byte marker; // marker seen while filling entropy buffer
            public int nomore; // flag if we saw a marker so must stop

            public int progressive;
            public int spec_start;
            public int spec_end;
            public int succ_high;
            public int succ_low;
            public int eob_run;
            public int jfif;
            public int app14_color_transform; // Adobe APP14 tag
            public int rgb;

            public int scan_n;
            public PinnedArray<int> order = new PinnedArray<int>(4);
            public int restart_interval, todo;

// kernels
            public idct_block_kernel idct_block_kernel;
            public YCbCr_to_RGB_kernel YCbCr_to_RGB_kernel;
            public Resampler resample_row_hv_2_kernel;

            public stbi__jpeg()
            {
                for (var i = 0; i < 4; ++i)
                {
                    huff_ac[i] = new stbi__huffman();
                    huff_dc[i] = new stbi__huffman();
                }

                for (var i = 0; i < img_comp.Length; ++i)
                {
                    img_comp[i] = new img_comp();
                }

                fast_ac = new PinnedArray<short>[4];
                for (var i = 0; i < fast_ac.Length; ++i)
                {
                    fast_ac[i] = new PinnedArray<short>(1 << STBI__ZFAST_BITS);
                }

                dequant = new PinnedArray<ushort>[4];
                for (var i = 0; i < dequant.Length; ++i)
                {
                    dequant[i] = new PinnedArray<ushort>(64);
                }
            }
        };

        public class stbi__resample
        {
            public Resampler resample;
            public byte* line0;
            public byte* line1;
            public int hs;
            public int vs;
            public int w_lores;
            public int ystep;
            public int ypos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct stbi__gif_lzw
        {
            public short prefix;
            public byte first;
            public byte suffix;
        }

        public class stbi__gif
        {
            public int w;
            public int h;
            public byte* _out_;
            public byte* old_out;
            public int flags;
            public int bgindex;
            public int ratio;
            public int transparent;
            public int eflags;
            public int delay;
            public PinnedArray<byte> pal;
            public PinnedArray<byte> lpal;
            public PinnedArray<stbi__gif_lzw> codes;
            public byte* color_table;
            public int parse;
            public int step;
            public int lflags;
            public int start_x;
            public int start_y;
            public int max_x;
            public int max_y;
            public int cur_x;
            public int cur_y;
            public int line_size;

            public stbi__gif()
            {
                codes = new PinnedArray<stbi__gif_lzw>(4096);
                pal = new PinnedArray<byte>(256*4);
                lpal = new PinnedArray<byte>(256*4);
            }
        }

        private static void* stbi__malloc(int size)
        {
            return Operations.Malloc(size);
        }

        private static void* stbi__malloc(ulong size)
        {
            return stbi__malloc((int) size);
        }

        private static void* malloc(ulong size)
        {
            return stbi__malloc(size);
        }

        private static int stbi__err(string str)
        {
            LastError = str;
            return 0;
        }

        private static void memcpy(void* a, void* b, long size)
        {
            Operations.Memcpy(a, b, size);
        }

        private static void memcpy(void* a, void* b, ulong size)
        {
            memcpy(a, b, (long) size);
        }

        private static void memmove(void* a, void* b, long size)
        {
            Operations.MemMove(a, b, size);
        }

        private static void memmove(void* a, void* b, ulong size)
        {
            memmove(a, b, (long) size);
        }

        private static int memcmp(void* a, void* b, long size)
        {
            return Operations.Memcmp(a, b, size);
        }

        private static int memcmp(void* a, void* b, ulong size)
        {
            return memcmp(a, b, (long) size);
        }

        private static void free(void* a)
        {
            Operations.Free(a);
        }

        private static void memset(void* ptr, int value, long size)
        {
            byte* bptr = (byte*) ptr;
            var bval = (byte) value;
            for (long i = 0; i < size; ++i)
            {
                *bptr++ = bval;
            }
        }

        private static void memset(void* ptr, int value, ulong size)
        {
            memset(ptr, value, (long) size);
        }

        private static uint _lrotl(uint x, int y)
        {
            return (x << y) | (x >> (32 - y));
        }

        private static void* realloc(void* ptr, long newSize)
        {
            return Operations.Realloc(ptr, newSize);
        }

        private static void* realloc(void* ptr, ulong newSize)
        {
            return realloc(ptr, (long) newSize);
        }

        private static int abs(int v)
        {
            return Math.Abs(v);
        }

        public static void stbi__gif_parse_colortable(stbi__context s, byte* pal, int num_entries, int transp)
        {
            int i;
            for (i = 0; (i) < (num_entries); ++i)
            {
                pal[i*4 + 2] = stbi__get8(s);
                pal[i*4 + 1] = stbi__get8(s);
                pal[i*4] = stbi__get8(s);
                pal[i*4 + 3] = (byte) (transp == i ? 0 : 255);
            }
        }

        public const long DBL_EXP_MASK = 0x7ff0000000000000L;
        public const int DBL_MANT_BITS = 52;
        public const long DBL_SGN_MASK = -1 - 0x7fffffffffffffffL;
        public const long DBL_MANT_MASK = 0x000fffffffffffffL;
        public const long DBL_EXP_CLR_MASK = DBL_SGN_MASK | DBL_MANT_MASK;

        /// <summary>
        /// This code had been borrowed from here: https://github.com/MachineCognitis/C.math.NET
        /// </summary>
        /// <param name="number"></param>
        /// <param name="exponent"></param>
        /// <returns></returns>
        private static double frexp(double number, int* exponent)
        {
            var bits = BitConverter.DoubleToInt64Bits(number);
            var exp = (int) ((bits & DBL_EXP_MASK) >> DBL_MANT_BITS);
            *exponent = 0;

            if (exp == 0x7ff || number == 0D)
                number += number;
            else
            {
                // Not zero and finite.
                *exponent = exp - 1022;
                if (exp == 0)
                {
                    // Subnormal, scale number so that it is in [1, 2).
                    number *= BitConverter.Int64BitsToDouble(0x4350000000000000L); // 2^54
                    bits = BitConverter.DoubleToInt64Bits(number);
                    exp = (int) ((bits & DBL_EXP_MASK) >> DBL_MANT_BITS);
                    *exponent = exp - 1022 - 54;
                }
                // Set exponent to -1 so that number is in [0.5, 1).
                number = BitConverter.Int64BitsToDouble((bits & DBL_EXP_CLR_MASK) | 0x3fe0000000000000L);
            }

            return number;
        }
    }
}