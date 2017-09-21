using System;
using System.Runtime.InteropServices;

namespace MonoGame.Utilities
{
    partial class Imaging
    {
        public static readonly PinnedArray<byte> tjeik_jfif_id = new PinnedArray<byte>(new[]
        {
            (byte) 'J',
            (byte) 'F',
            (byte) 'I',
            (byte) 'F',
            (byte) 0
        });

        public static readonly PinnedArray<byte> tjeik_com_str = new PinnedArray<byte>(new[]
        {
            (byte) 'C',
            (byte) 'r',
            (byte) 'e',
            (byte) 'a',
            (byte) 't',
            (byte) 'e',
            (byte) 'd',
            (byte) ' ',
            (byte) 'b',
            (byte) 'y',
            (byte) ' ',
            (byte) 'T',
            (byte) 'i',
            (byte) 'n',
            (byte) 'y',
            (byte) ' ',
            (byte) 'J',
            (byte) 'P',
            (byte) 'E',
            (byte) 'G',
            (byte) ' ',
            (byte) 'E',
            (byte) 'n',
            (byte) 'c',
            (byte) 'o',
            (byte) 'd',
            (byte) 'e',
            (byte) 'r'
        });

        public unsafe delegate void WriteCallback2(void* context, void* data, int size);

        public unsafe class TJEWriteContext
        {
            public void* context = null;
            public WriteCallback2 func;
        }

        public unsafe class TJEState
        {
            public PinnedArray<byte> ehuffsize = new PinnedArray<byte>(4*257);
            public PinnedArray<ushort> ehuffcode = new PinnedArray<ushort>(4*256);
            public byte*[] ht_bits = new byte*[4];
            public byte*[] ht_vals = new byte*[4];
            public PinnedArray<byte> qt_luma = new PinnedArray<byte>(64);
            public PinnedArray<byte> qt_chroma = new PinnedArray<byte>(64);
            public TJEWriteContext write_context = new TJEWriteContext();
            public ulong output_buffer_count;
            public PinnedArray<byte> output_buffer = new PinnedArray<byte>(1024);
            public float fval;

            public TJEState()
            {
                for (var i = 0; i < 4; ++i)
                {
                    ht_bits[i] = null;
                    ht_vals[i] = null;
                }

                memset(ehuffsize.Ptr, 0, ehuffsize.Size);
                memset(ehuffcode, 0, ehuffcode.Size);
                memset(qt_luma.Ptr, 0, qt_luma.Size);
                memset(qt_chroma.Ptr, 0, qt_chroma.Size);
                memset(output_buffer.Ptr, 0, output_buffer.Size);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct TJEJPEGComment
        {
            public ushort com;
            public ushort com_len;
            public fixed sbyte com_str [28];
        }

        private static float floorf(float f)
        {
            return (float) Math.Floor(f);
        }

        public static unsafe void tjei_huff_expand(TJEState state)
        {
            state.ht_bits[TJEI_LUMA_DC] = tjei_default_ht_luma_dc_len;
            state.ht_bits[TJEI_LUMA_AC] = tjei_default_ht_luma_ac_len;
            state.ht_bits[TJEI_CHROMA_DC] = tjei_default_ht_chroma_dc_len;
            state.ht_bits[TJEI_CHROMA_AC] = tjei_default_ht_chroma_ac_len;
            state.ht_vals[TJEI_LUMA_DC] = tjei_default_ht_luma_dc;
            state.ht_vals[TJEI_LUMA_AC] = tjei_default_ht_luma_ac;
            state.ht_vals[TJEI_CHROMA_DC] = tjei_default_ht_chroma_dc;
            state.ht_vals[TJEI_CHROMA_AC] = tjei_default_ht_chroma_ac;
            var spec_tables_len = stackalloc int[4];
            for (var i = 0; i < 4; ++i)
            {
                spec_tables_len[i] = 0;
                for (var k = 0; k < 16; ++k)
                {
                    spec_tables_len[i] += state.ht_bits[i][k];
                }
            }
            var huffsize = stackalloc byte[4*257];
            var huffcode = stackalloc ushort[4*256];
            for (var i = 0; i < 4; ++i)
            {
                tjei_huff_get_code_lengths(huffsize + i*257, state.ht_bits[i]);
                tjei_huff_get_codes(huffcode + i*256, huffsize + i*257,
                    spec_tables_len[i]);
            }

            for (var i = 0; i < 4; ++i)
            {
                var count = spec_tables_len[i];
                tjei_huff_get_extended((byte*) state.ehuffsize + i*257,
                    (ushort*) state.ehuffcode + i*256,
                    state.ht_vals[i],
                    huffsize + i*257,
                    huffcode + i*256,
                    count);
            }
        }

        public static unsafe int tje_encode_with_func(WriteCallback2 func,
            void* context,
            int quality,
            int width,
            int height,
            int num_components,
            byte* src_data)
        {
            if (quality < 1 || quality > 3)
            {
                throw new Exception("[ERROR] -- Valid 'quality' values are 1 (lowest), 2, or 3 (highest)");
            }

            var state = new TJEState();

            byte qt_factor = 1;
            switch (quality)
            {
                case 3:
                    for (int i = 0; i < 64; ++i)
                    {
                        state.qt_luma[i] = 1;
                        state.qt_chroma[i] = 1;
                    }
                    break;
                case 1:
                case 2:
                    if (quality == 2)
                    {
                        qt_factor = 10;
                    }
                    // don't break. fall through.
                    for (int i = 0; i < 64; ++i)
                    {
                        state.qt_luma[i] = (byte) (tjei_default_qt_luma_from_spec[i]/qt_factor);
                        if (state.qt_luma[i] == 0)
                        {
                            state.qt_luma[i] = 1;
                        }
                        state.qt_chroma[i] = (byte) (tjei_default_qt_chroma_from_paper[i]/qt_factor);
                        if (state.qt_chroma[i] == 0)
                        {
                            state.qt_chroma[i] = 1;
                        }
                    }
                    break;
                default:
                    throw new Exception("invalid code path");
            }

            var wc = new TJEWriteContext
            {
                context = context,
                func = func
            };

            state.write_context = wc;
            tjei_huff_expand(state);

            var result = tjei_encode_main(state, src_data, width, height, num_components);

            return result;
        }
    }
}