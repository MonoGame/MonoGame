using System.Runtime.InteropServices;

namespace StbSharp
{
    internal static unsafe partial class StbImage
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
            public readonly stbi__huffman[] huff_dc = new stbi__huffman[4];
            public readonly stbi__huffman[] huff_ac = new stbi__huffman[4];
            public readonly ushort[][] dequant;

            public readonly short[][] fast_ac;

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
            public int[] order = new int[4];
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

                fast_ac = new short[4][];
                for (var i = 0; i < fast_ac.Length; ++i)
                {
                    fast_ac[i] = new short[1 << STBI__ZFAST_BITS];
                }

                dequant = new ushort[4][];
                for (var i = 0; i < dequant.Length; ++i)
                {
                    dequant[i] = new ushort[64];
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
            public byte* pal;
            public byte* lpal;
            public stbi__gif_lzw* codes;
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
                codes = (stbi__gif_lzw*)stbi__malloc(4096 * sizeof(stbi__gif_lzw));
                pal = (byte*)stbi__malloc(256 * 4 * sizeof(byte));
                lpal = (byte*)stbi__malloc(256 * 4 * sizeof(byte));
            }
        }

        private static void* stbi__malloc(int size)
        {
            return CRuntime.malloc((ulong)size);
        }

        private static void* stbi__malloc(ulong size)
        {
            return stbi__malloc((int)size);
        }

        private static int stbi__err(string str)
        {
            LastError = str;
            return 0;
        }

        public static void stbi__gif_parse_colortable(stbi__context s, byte* pal, int num_entries, int transp)
        {
            int i;
            for (i = 0; (i) < (num_entries); ++i)
            {
                pal[i * 4 + 2] = stbi__get8(s);
                pal[i * 4 + 1] = stbi__get8(s);
                pal[i * 4] = stbi__get8(s);
                pal[i * 4 + 3] = (byte)(transp == i ? 0 : 255);
            }
        }
    }
}
