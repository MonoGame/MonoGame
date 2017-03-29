using System.Runtime.InteropServices;

namespace MonoGame.Utilities
{
    partial class Imaging
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct TJEJPEGHeader
        {
            public ushort SOI;
            public ushort APP0;
            public ushort jfif_len;
            public fixed byte jfif_id [5];
            public ushort version;
            public byte units;
            public ushort x_density;
            public ushort y_density;
            public byte x_thumb;
            public byte y_thumb;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct TJEComponentSpec
        {
            public byte component_id;
            public byte sampling_factors;
            public byte qt;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct TJEFrameHeader
        {
            public ushort SOF;
            public ushort len;
            public byte precision;
            public ushort height;
            public ushort width;
            public byte num_components;
            public TJEComponentSpec component_spec0;
            public TJEComponentSpec component_spec1;
            public TJEComponentSpec component_spec2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct TJEFrameComponentSpec
        {
            public byte component_id;
            public byte dc_ac;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct TJEScanHeader
        {
            public ushort SOS;
            public ushort len;
            public byte num_components;
            public TJEFrameComponentSpec component_spec0;
            public TJEFrameComponentSpec component_spec1;
            public TJEFrameComponentSpec component_spec2;
            public byte first;
            public byte last;
            public byte ah_al;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct TJEProcessedQT
        {
            public fixed float chroma [64];
            public fixed float luma [64];
        }

        public const int TJEI_DC = 0;
        public const int TJEI_AC = 1;
        public const int TJEI_LUMA_DC = 0;
        public const int TJEI_LUMA_AC = 1;
        public const int TJEI_CHROMA_DC = 2;
        public const int TJEI_CHROMA_AC = 3;

        public static PinnedArray<byte> tjei_default_qt_luma_from_spec =
            new PinnedArray<byte>(new byte[]
            {
                (byte) (16), (byte) (11), (byte) (10), (byte) (16), (byte) (24), (byte) (40), (byte) (51), (byte) (61),
                (byte) (12),
                (byte) (12), (byte) (14), (byte) (19), (byte) (26), (byte) (58), (byte) (60), (byte) (55), (byte) (14),
                (byte) (13),
                (byte) (16), (byte) (24), (byte) (40), (byte) (57), (byte) (69), (byte) (56), (byte) (14), (byte) (17),
                (byte) (22),
                (byte) (29), (byte) (51), (byte) (87), (byte) (80), (byte) (62), (byte) (18), (byte) (22), (byte) (37),
                (byte) (56),
                (byte) (68), (byte) (109), (byte) (103), (byte) (77), (byte) (24), (byte) (35), (byte) (55), (byte) (64),
                (byte) (81), (byte) (104), (byte) (113), (byte) (92), (byte) (49), (byte) (64), (byte) (78), (byte) (87),
                (byte) (103), (byte) (121), (byte) (120), (byte) (101), (byte) (72), (byte) (92), (byte) (95),
                (byte) (98),
                (byte) (112), (byte) (100), (byte) (103), (byte) (99)
            });

        public static PinnedArray<byte> tjei_default_qt_chroma_from_paper =
            new PinnedArray<byte>(new byte[]
            {
                (byte) (16), (byte) (12), (byte) (14), (byte) (14), (byte) (18), (byte) (24), (byte) (49), (byte) (72),
                (byte) (11),
                (byte) (10), (byte) (16), (byte) (24), (byte) (40), (byte) (51), (byte) (61), (byte) (12), (byte) (13),
                (byte) (17),
                (byte) (22), (byte) (35), (byte) (64), (byte) (92), (byte) (14), (byte) (16), (byte) (22), (byte) (37),
                (byte) (55),
                (byte) (78), (byte) (95), (byte) (19), (byte) (24), (byte) (29), (byte) (56), (byte) (64), (byte) (87),
                (byte) (98),
                (byte) (26), (byte) (40), (byte) (51), (byte) (68), (byte) (81), (byte) (103), (byte) (112), (byte) (58),
                (byte) (57), (byte) (87), (byte) (109), (byte) (104), (byte) (121), (byte) (100), (byte) (60),
                (byte) (69),
                (byte) (80), (byte) (103), (byte) (113), (byte) (120), (byte) (103), (byte) (55), (byte) (56),
                (byte) (62),
                (byte) (77), (byte) (92), (byte) (101), (byte) (99)
            });

        public static PinnedArray<byte> tjei_default_ht_luma_dc_len =
            new PinnedArray<byte>(new byte[]
            {
                (byte) (0), (byte) (1), (byte) (5), (byte) (1), (byte) (1), (byte) (1), (byte) (1), (byte) (1),
                (byte) (1),
                (byte) (0), (byte) (0), (byte) (0), (byte) (0), (byte) (0), (byte) (0), (byte) (0)
            });

        public static PinnedArray<byte> tjei_default_ht_luma_dc =
            new PinnedArray<byte>(new byte[]
            {
                (byte) (0), (byte) (1), (byte) (2), (byte) (3), (byte) (4), (byte) (5), (byte) (6), (byte) (7),
                (byte) (8),
                (byte) (9), (byte) (10), (byte) (11)
            });

        public static PinnedArray<byte> tjei_default_ht_chroma_dc_len =
            new PinnedArray<byte>(new byte[]
            {
                (byte) (0), (byte) (3), (byte) (1), (byte) (1), (byte) (1), (byte) (1), (byte) (1), (byte) (1),
                (byte) (1),
                (byte) (1), (byte) (1), (byte) (0), (byte) (0), (byte) (0), (byte) (0), (byte) (0)
            });

        public static PinnedArray<byte> tjei_default_ht_chroma_dc =
            new PinnedArray<byte>(new byte[]
            {
                (byte) (0), (byte) (1), (byte) (2), (byte) (3), (byte) (4), (byte) (5), (byte) (6), (byte) (7),
                (byte) (8),
                (byte) (9), (byte) (10), (byte) (11)
            });

        public static PinnedArray<byte> tjei_default_ht_luma_ac_len =
            new PinnedArray<byte>(new byte[]
            {
                (byte) (0), (byte) (2), (byte) (1), (byte) (3), (byte) (3), (byte) (2), (byte) (4), (byte) (3),
                (byte) (5),
                (byte) (5), (byte) (4), (byte) (4), (byte) (0), (byte) (0), (byte) (1), (byte) (0x7d)
            });

        public static PinnedArray<byte> tjei_default_ht_luma_ac =
            new PinnedArray<byte>(new byte[]
            {
                (byte) (0x01), (byte) (0x02), (byte) (0x03), (byte) (0x00), (byte) (0x04), (byte) (0x11), (byte) (0x05),
                (byte) (0x12), (byte) (0x21), (byte) (0x31), (byte) (0x41), (byte) (0x06), (byte) (0x13), (byte) (0x51),
                (byte) (0x61), (byte) (0x07), (byte) (0x22), (byte) (0x71), (byte) (0x14), (byte) (0x32), (byte) (0x81),
                (byte) (0x91), (byte) (0xA1), (byte) (0x08), (byte) (0x23), (byte) (0x42), (byte) (0xB1), (byte) (0xC1),
                (byte) (0x15), (byte) (0x52), (byte) (0xD1), (byte) (0xF0), (byte) (0x24), (byte) (0x33), (byte) (0x62),
                (byte) (0x72), (byte) (0x82), (byte) (0x09), (byte) (0x0A), (byte) (0x16), (byte) (0x17), (byte) (0x18),
                (byte) (0x19), (byte) (0x1A), (byte) (0x25), (byte) (0x26), (byte) (0x27), (byte) (0x28), (byte) (0x29),
                (byte) (0x2A), (byte) (0x34), (byte) (0x35), (byte) (0x36), (byte) (0x37), (byte) (0x38), (byte) (0x39),
                (byte) (0x3A), (byte) (0x43), (byte) (0x44), (byte) (0x45), (byte) (0x46), (byte) (0x47), (byte) (0x48),
                (byte) (0x49), (byte) (0x4A), (byte) (0x53), (byte) (0x54), (byte) (0x55), (byte) (0x56), (byte) (0x57),
                (byte) (0x58), (byte) (0x59), (byte) (0x5A), (byte) (0x63), (byte) (0x64), (byte) (0x65), (byte) (0x66),
                (byte) (0x67), (byte) (0x68), (byte) (0x69), (byte) (0x6A), (byte) (0x73), (byte) (0x74), (byte) (0x75),
                (byte) (0x76), (byte) (0x77), (byte) (0x78), (byte) (0x79), (byte) (0x7A), (byte) (0x83), (byte) (0x84),
                (byte) (0x85), (byte) (0x86), (byte) (0x87), (byte) (0x88), (byte) (0x89), (byte) (0x8A), (byte) (0x92),
                (byte) (0x93), (byte) (0x94), (byte) (0x95), (byte) (0x96), (byte) (0x97), (byte) (0x98), (byte) (0x99),
                (byte) (0x9A), (byte) (0xA2), (byte) (0xA3), (byte) (0xA4), (byte) (0xA5), (byte) (0xA6), (byte) (0xA7),
                (byte) (0xA8), (byte) (0xA9), (byte) (0xAA), (byte) (0xB2), (byte) (0xB3), (byte) (0xB4), (byte) (0xB5),
                (byte) (0xB6), (byte) (0xB7), (byte) (0xB8), (byte) (0xB9), (byte) (0xBA), (byte) (0xC2), (byte) (0xC3),
                (byte) (0xC4), (byte) (0xC5), (byte) (0xC6), (byte) (0xC7), (byte) (0xC8), (byte) (0xC9), (byte) (0xCA),
                (byte) (0xD2), (byte) (0xD3), (byte) (0xD4), (byte) (0xD5), (byte) (0xD6), (byte) (0xD7), (byte) (0xD8),
                (byte) (0xD9), (byte) (0xDA), (byte) (0xE1), (byte) (0xE2), (byte) (0xE3), (byte) (0xE4), (byte) (0xE5),
                (byte) (0xE6), (byte) (0xE7), (byte) (0xE8), (byte) (0xE9), (byte) (0xEA), (byte) (0xF1), (byte) (0xF2),
                (byte) (0xF3), (byte) (0xF4), (byte) (0xF5), (byte) (0xF6), (byte) (0xF7), (byte) (0xF8), (byte) (0xF9),
                (byte) (0xFA)
            });

        public static PinnedArray<byte> tjei_default_ht_chroma_ac_len =
            new PinnedArray<byte>(new byte[]
            {
                (byte) (0), (byte) (2), (byte) (1), (byte) (2), (byte) (4), (byte) (4), (byte) (3), (byte) (4),
                (byte) (7),
                (byte) (5), (byte) (4), (byte) (4), (byte) (0), (byte) (1), (byte) (2), (byte) (0x77)
            });

        public static PinnedArray<byte> tjei_default_ht_chroma_ac =
            new PinnedArray<byte>(new byte[]
            {
                (byte) (0x00), (byte) (0x01), (byte) (0x02), (byte) (0x03), (byte) (0x11), (byte) (0x04), (byte) (0x05),
                (byte) (0x21), (byte) (0x31), (byte) (0x06), (byte) (0x12), (byte) (0x41), (byte) (0x51), (byte) (0x07),
                (byte) (0x61), (byte) (0x71), (byte) (0x13), (byte) (0x22), (byte) (0x32), (byte) (0x81), (byte) (0x08),
                (byte) (0x14), (byte) (0x42), (byte) (0x91), (byte) (0xA1), (byte) (0xB1), (byte) (0xC1), (byte) (0x09),
                (byte) (0x23), (byte) (0x33), (byte) (0x52), (byte) (0xF0), (byte) (0x15), (byte) (0x62), (byte) (0x72),
                (byte) (0xD1), (byte) (0x0A), (byte) (0x16), (byte) (0x24), (byte) (0x34), (byte) (0xE1), (byte) (0x25),
                (byte) (0xF1), (byte) (0x17), (byte) (0x18), (byte) (0x19), (byte) (0x1A), (byte) (0x26), (byte) (0x27),
                (byte) (0x28), (byte) (0x29), (byte) (0x2A), (byte) (0x35), (byte) (0x36), (byte) (0x37), (byte) (0x38),
                (byte) (0x39), (byte) (0x3A), (byte) (0x43), (byte) (0x44), (byte) (0x45), (byte) (0x46), (byte) (0x47),
                (byte) (0x48), (byte) (0x49), (byte) (0x4A), (byte) (0x53), (byte) (0x54), (byte) (0x55), (byte) (0x56),
                (byte) (0x57), (byte) (0x58), (byte) (0x59), (byte) (0x5A), (byte) (0x63), (byte) (0x64), (byte) (0x65),
                (byte) (0x66), (byte) (0x67), (byte) (0x68), (byte) (0x69), (byte) (0x6A), (byte) (0x73), (byte) (0x74),
                (byte) (0x75), (byte) (0x76), (byte) (0x77), (byte) (0x78), (byte) (0x79), (byte) (0x7A), (byte) (0x82),
                (byte) (0x83), (byte) (0x84), (byte) (0x85), (byte) (0x86), (byte) (0x87), (byte) (0x88), (byte) (0x89),
                (byte) (0x8A), (byte) (0x92), (byte) (0x93), (byte) (0x94), (byte) (0x95), (byte) (0x96), (byte) (0x97),
                (byte) (0x98), (byte) (0x99), (byte) (0x9A), (byte) (0xA2), (byte) (0xA3), (byte) (0xA4), (byte) (0xA5),
                (byte) (0xA6), (byte) (0xA7), (byte) (0xA8), (byte) (0xA9), (byte) (0xAA), (byte) (0xB2), (byte) (0xB3),
                (byte) (0xB4), (byte) (0xB5), (byte) (0xB6), (byte) (0xB7), (byte) (0xB8), (byte) (0xB9), (byte) (0xBA),
                (byte) (0xC2), (byte) (0xC3), (byte) (0xC4), (byte) (0xC5), (byte) (0xC6), (byte) (0xC7), (byte) (0xC8),
                (byte) (0xC9), (byte) (0xCA), (byte) (0xD2), (byte) (0xD3), (byte) (0xD4), (byte) (0xD5), (byte) (0xD6),
                (byte) (0xD7), (byte) (0xD8), (byte) (0xD9), (byte) (0xDA), (byte) (0xE2), (byte) (0xE3), (byte) (0xE4),
                (byte) (0xE5), (byte) (0xE6), (byte) (0xE7), (byte) (0xE8), (byte) (0xE9), (byte) (0xEA), (byte) (0xF2),
                (byte) (0xF3), (byte) (0xF4), (byte) (0xF5), (byte) (0xF6), (byte) (0xF7), (byte) (0xF8), (byte) (0xF9),
                (byte) (0xFA)
            });

        public static PinnedArray<byte> tjei_zig_zag =
            new PinnedArray<byte>(new byte[]
            {
                (byte) (0), (byte) (1), (byte) (5), (byte) (6), (byte) (14), (byte) (15), (byte) (27), (byte) (28),
                (byte) (2),
                (byte) (4), (byte) (7), (byte) (13), (byte) (16), (byte) (26), (byte) (29), (byte) (42), (byte) (3),
                (byte) (8),
                (byte) (12), (byte) (17), (byte) (25), (byte) (30), (byte) (41), (byte) (43), (byte) (9), (byte) (11),
                (byte) (18),
                (byte) (24), (byte) (31), (byte) (40), (byte) (44), (byte) (53), (byte) (10), (byte) (19), (byte) (23),
                (byte) (32),
                (byte) (39), (byte) (45), (byte) (52), (byte) (54), (byte) (20), (byte) (22), (byte) (33), (byte) (38),
                (byte) (46),
                (byte) (51), (byte) (55), (byte) (60), (byte) (21), (byte) (34), (byte) (37), (byte) (47), (byte) (50),
                (byte) (56),
                (byte) (59), (byte) (61), (byte) (35), (byte) (36), (byte) (48), (byte) (49), (byte) (57), (byte) (58),
                (byte) (62),
                (byte) (63)
            });

        public unsafe static ushort tjei_be_word(ushort le_word)
        {
            ushort lo = (ushort) (le_word & 0x00ff);
            ushort hi = (ushort) ((le_word & 0xff00) >> 8);
            return (ushort) ((lo << 8) | hi);
        }

        public unsafe static void tjei_write(TJEState state, void* data, ulong num_bytes, ulong num_elements)
        {
            ulong to_write = (ulong) (num_bytes*num_elements);

            ulong capped_count =
                (ulong)
                    (((to_write) < (1024 - 1 - state.output_buffer_count))
                        ? (to_write)
                        : (1024 - 1 - state.output_buffer_count));
            memcpy((byte*) state.output_buffer + state.output_buffer_count, data, (ulong) (capped_count));
            state.output_buffer_count += (ulong) (capped_count);

            if ((state.output_buffer_count) == (1024 - 1))
            {
                state.write_context.func(state.write_context.context, state.output_buffer,
                    (int) (state.output_buffer_count));
                state.output_buffer_count = (ulong) (0);
            }

            if ((capped_count) < (to_write))
            {
                tjei_write(state, (byte*) (data) + capped_count, (ulong) (to_write - capped_count), (ulong) (1));
            }

        }

        public unsafe static void tjei_write_DQT(TJEState state, byte* matrix, byte id)
        {
            ushort DQT = (ushort) (tjei_be_word((ushort) (0xffdb)));
            tjei_write(state, &DQT, (ulong) (sizeof (ushort)), (ulong) (1));
            ushort len = (ushort) (tjei_be_word((ushort) (0x0043)));
            tjei_write(state, &len, (ulong) (sizeof (ushort)), (ulong) (1));
            byte precision_and_id = (byte) (id);
            tjei_write(state, &precision_and_id, (ulong) (sizeof (byte)), (ulong) (1));
            tjei_write(state, matrix, (ulong) (64*sizeof (byte)), (ulong) (1));
        }

        public unsafe static void tjei_write_DHT(TJEState state, byte* matrix_len, byte* matrix_val, int ht_class,
            byte id)
        {
            int num_values = (int) (0);
            for (int i = (int) (0); (i) < (16); ++i)
            {
                {
                    num_values += (int) (matrix_len[i]);
                }
            }
            ushort DHT = (ushort) (tjei_be_word((ushort) (0xffc4)));
            ushort len = (ushort) (tjei_be_word((ushort) (2 + 1 + 16 + (ushort) (num_values))));
            byte tc_th = (byte) ((((byte) (ht_class)) << 4) | id);
            tjei_write(state, &DHT, (ulong) (sizeof (ushort)), (ulong) (1));
            tjei_write(state, &len, (ulong) (sizeof (ushort)), (ulong) (1));
            tjei_write(state, &tc_th, (ulong) (sizeof (byte)), (ulong) (1));
            tjei_write(state, matrix_len, (ulong) (sizeof (byte)), (ulong) (16));
            tjei_write(state, matrix_val, (ulong) (sizeof (byte)), (ulong) (num_values));
        }

        public unsafe static byte* tjei_huff_get_code_lengths(byte* huffsize, byte* bits)
        {
            int k = (int) (0);
            for (int i = (int) (0); (i) < (16); ++i)
            {
                {
                    for (int j = (int) (0); (j) < (bits[i]); ++j)
                    {
                        {
                            huffsize[k++] = (byte) ((byte) (i + 1));
                        }
                    }
                    huffsize[k] = (byte) (0);
                }
            }
            return huffsize;
        }

        public unsafe static ushort* tjei_huff_get_codes(ushort* codes, byte* huffsize, int count)
        {
            ushort code = (ushort) (0);
            int k = (int) (0);
            byte sz = (byte) (huffsize[0]);
            for (;;)
            {
                {
                    do
                    {
                        {
                            ;
                            codes[k++] = (ushort) (code++);
                        }
                    } while ((huffsize[k]) == (sz));
                    if ((huffsize[k]) == (0))
                    {
                        return codes;
                    }
                    do
                    {
                        {
                            code = (ushort) ((ushort) (code << 1));
                            ++sz;
                        }
                    } while (huffsize[k] != sz);
                }
            }
        }

        public unsafe static void tjei_huff_get_extended(byte* out_ehuffsize, ushort* out_ehuffcode, byte* huffval,
            byte* huffsize, ushort* huffcode, int count)
        {
            int k = (int) (0);
            do
            {
                {
                    byte val = (byte) (huffval[k]);
                    out_ehuffcode[val] = (ushort) (huffcode[k]);
                    out_ehuffsize[val] = (byte) (huffsize[k]);
                    k++;
                }
            } while ((k) < (count));
        }

        public unsafe static void tjei_calculate_variable_length_int(int value, ushort* _out_)
        {
            int abs_val = (int) (value);
            if ((value) < (0))
            {
                abs_val = (int) (-abs_val);
                --value;
            }

            _out_[1] = (ushort) (1);
            while ((abs_val >>= 1) != 0)
            {
                {
                    ++_out_[1];
                }
            }
            _out_[0] = (ushort) ((ushort) (value & ((1 << _out_[1]) - 1)));
        }

        public unsafe static void tjei_write_bits(TJEState state, uint* bitbuffer, uint* location, ushort num_bits,
            ushort bits)
        {
            uint nloc = (uint) (*location + num_bits);
            var ib = *bitbuffer;
            *bitbuffer |= (uint) ((uint) (bits << (int) (32 - nloc)));
            *location = (uint) (nloc);
            while ((*location) >= (8))
            {
                {
                    byte c = (byte) ((*bitbuffer) >> 24);
                    tjei_write(state, &c, (ulong) (1), (ulong) (1));
                    if ((c) == (0xff))
                    {
                        sbyte z = (sbyte) (0);
                        tjei_write(state, &z, (ulong) (1), (ulong) (1));
                    }
                    *bitbuffer <<= 8;
                    *location -= (uint) (8);
                }
            }
        }

        public unsafe static void tjei_fdct(float* data)
        {
            float tmp0;
            float tmp1;
            float tmp2;
            float tmp3;
            float tmp4;
            float tmp5;
            float tmp6;
            float tmp7;
            float tmp10;
            float tmp11;
            float tmp12;
            float tmp13;
            float z1;
            float z2;
            float z3;
            float z4;
            float z5;
            float z11;
            float z13;
            float* dataptr;
            int ctr;
            dataptr = data;
            for (ctr = (int) (7); (ctr) >= (0); ctr--)
            {
                {
                    tmp0 = (float) (dataptr[0] + dataptr[7]);
                    tmp7 = (float) (dataptr[0] - dataptr[7]);
                    tmp1 = (float) (dataptr[1] + dataptr[6]);
                    tmp6 = (float) (dataptr[1] - dataptr[6]);
                    tmp2 = (float) (dataptr[2] + dataptr[5]);
                    tmp5 = (float) (dataptr[2] - dataptr[5]);
                    tmp3 = (float) (dataptr[3] + dataptr[4]);
                    tmp4 = (float) (dataptr[3] - dataptr[4]);
                    tmp10 = (float) (tmp0 + tmp3);
                    tmp13 = (float) (tmp0 - tmp3);
                    tmp11 = (float) (tmp1 + tmp2);
                    tmp12 = (float) (tmp1 - tmp2);
                    dataptr[0] = (float) (tmp10 + tmp11);
                    dataptr[4] = (float) (tmp10 - tmp11);
                    z1 = (float) ((tmp12 + tmp13)*((float) (0.707106781)));
                    dataptr[2] = (float) (tmp13 + z1);
                    dataptr[6] = (float) (tmp13 - z1);
                    tmp10 = (float) (tmp4 + tmp5);
                    tmp11 = (float) (tmp5 + tmp6);
                    tmp12 = (float) (tmp6 + tmp7);
                    z5 = (float) ((tmp10 - tmp12)*((float) (0.382683433)));
                    z2 = (float) (((float) (0.541196100))*tmp10 + z5);
                    z4 = (float) (((float) (1.306562965))*tmp12 + z5);
                    z3 = (float) (tmp11*((float) (0.707106781)));
                    z11 = (float) (tmp7 + z3);
                    z13 = (float) (tmp7 - z3);
                    dataptr[5] = (float) (z13 + z2);
                    dataptr[3] = (float) (z13 - z2);
                    dataptr[1] = (float) (z11 + z4);
                    dataptr[7] = (float) (z11 - z4);
                    dataptr += 8;
                }
            }
            dataptr = data;
            for (ctr = (int) (8 - 1); (ctr) >= (0); ctr--)
            {
                {
                    tmp0 = (float) (dataptr[8*0] + dataptr[8*7]);
                    tmp7 = (float) (dataptr[8*0] - dataptr[8*7]);
                    tmp1 = (float) (dataptr[8*1] + dataptr[8*6]);
                    tmp6 = (float) (dataptr[8*1] - dataptr[8*6]);
                    tmp2 = (float) (dataptr[8*2] + dataptr[8*5]);
                    tmp5 = (float) (dataptr[8*2] - dataptr[8*5]);
                    tmp3 = (float) (dataptr[8*3] + dataptr[8*4]);
                    tmp4 = (float) (dataptr[8*3] - dataptr[8*4]);
                    tmp10 = (float) (tmp0 + tmp3);
                    tmp13 = (float) (tmp0 - tmp3);
                    tmp11 = (float) (tmp1 + tmp2);
                    tmp12 = (float) (tmp1 - tmp2);
                    dataptr[8*0] = (float) (tmp10 + tmp11);
                    dataptr[8*4] = (float) (tmp10 - tmp11);
                    z1 = (float) ((tmp12 + tmp13)*((float) (0.707106781)));
                    dataptr[8*2] = (float) (tmp13 + z1);
                    dataptr[8*6] = (float) (tmp13 - z1);
                    tmp10 = (float) (tmp4 + tmp5);
                    tmp11 = (float) (tmp5 + tmp6);
                    tmp12 = (float) (tmp6 + tmp7);
                    z5 = (float) ((tmp10 - tmp12)*((float) (0.382683433)));
                    z2 = (float) (((float) (0.541196100))*tmp10 + z5);
                    z4 = (float) (((float) (1.306562965))*tmp12 + z5);
                    z3 = (float) (tmp11*((float) (0.707106781)));
                    z11 = (float) (tmp7 + z3);
                    z13 = (float) (tmp7 - z3);
                    dataptr[8*5] = (float) (z13 + z2);
                    dataptr[8*3] = (float) (z13 - z2);
                    dataptr[8*1] = (float) (z11 + z4);
                    dataptr[8*7] = (float) (z11 - z4);
                    dataptr++;
                }
            }
        }

        public unsafe static void tjei_encode_and_write_MCU(TJEState state, float* mcu, float* qt, byte* huff_dc_len,
            ushort* huff_dc_code, byte* huff_ac_len, ushort* huff_ac_code, int* pred, uint* bitbuffer, uint* location)
        {
            int* du = stackalloc int[64];
            float* dct_mcu = stackalloc float[64];
            memcpy(dct_mcu, mcu, (ulong) (64*sizeof (float)));
            tjei_fdct(dct_mcu);

            for (int i = (int) (0); (i) < (64); ++i)
            {
                {
                    state.fval = (float) (dct_mcu[i]);
                    state.fval *= (float) (qt[i]);
                    state.fval = (float) (floorf((float) (state.fval + 1024 + 0.5f)));
                    state.fval -= (float) (1024);
                    int val = (int) (state.fval);
                    du[tjei_zig_zag[i]] = (int) (val);
                }
            }
            ushort* vli = stackalloc ushort[2];
            int diff = (int) (du[0] - *pred);
            *pred = (int) (du[0]);
            if (diff != 0)
            {
                tjei_calculate_variable_length_int((int) (diff), vli);
                tjei_write_bits(state, bitbuffer, location, (ushort) (huff_dc_len[vli[1]]),
                    (ushort) (huff_dc_code[vli[1]]));
                tjei_write_bits(state, bitbuffer, location, (ushort) (vli[1]), (ushort) (vli[0]));
            }
            else
            {
                tjei_write_bits(state, bitbuffer, location, (ushort) (huff_dc_len[0]), (ushort) (huff_dc_code[0]));
            }

            int last_non_zero_i = (int) (0);
            for (int i = (int) (63); (i) > (0); --i)
            {
                {
                    if (du[i] != 0)
                    {
                        last_non_zero_i = (int) (i);
                        break;
                    }
                }
            }
            for (int i = (int) (1); i <= last_non_zero_i; ++i)
            {
                {
                    int zero_count = (int) (0);
                    while ((du[i]) == (0))
                    {
                        {
                            ++zero_count;
                            ++i;
                            if ((zero_count) == (16))
                            {
                                tjei_write_bits(state, bitbuffer, location, (ushort) (huff_ac_len[0xf0]),
                                    (ushort) (huff_ac_code[0xf0]));
                                zero_count = (int) (0);
                            }
                        }
                    }
                    tjei_calculate_variable_length_int((int) (du[i]), vli);

                    ushort sym1 = (ushort) (((ushort) (zero_count) << 4) | vli[1]);
                    tjei_write_bits(state, bitbuffer, location, (ushort) (huff_ac_len[sym1]),
                        (ushort) (huff_ac_code[sym1]));
                    tjei_write_bits(state, bitbuffer, location, (ushort) (vli[1]), (ushort) (vli[0]));
                }
            }
            if (last_non_zero_i != 63)
            {
                tjei_write_bits(state, bitbuffer, location, (ushort) (huff_ac_len[0]), (ushort) (huff_ac_code[0]));
            }

            return;
        }

        public unsafe static int tjei_encode_main(TJEState state, byte* src_data, int width, int height,
            int src_num_components)
        {
            if ((src_num_components != 3) && (src_num_components != 4))
            {
                return (int) (0);
            }

            if (((width) > (0xffff)) || ((height) > (0xffff)))
            {
                return (int) (0);
            }

            TJEProcessedQT pqt = new TJEProcessedQT();
            float* aan_scales = stackalloc float[8];
            aan_scales[0] = (float) (1.0f);
            aan_scales[1] = (float) (1.387039845f);
            aan_scales[2] = (float) (1.306562965f);
            aan_scales[3] = (float) (1.175875602f);
            aan_scales[4] = (float) (1.0f);
            aan_scales[5] = (float) (0.785694958f);
            aan_scales[6] = (float) (0.541196100f);
            aan_scales[7] = (float) (0.275899379f);

            for (int y = (int) (0); (y) < (8); y++)
            {
                {
                    for (int x = (int) (0); (x) < (8); x++)
                    {
                        {
                            int i = (int) (y*8 + x);
                            pqt.luma[y*8 + x] =
                                (float) (1.0f/(8*aan_scales[x]*aan_scales[y]*state.qt_luma[tjei_zig_zag[i]]));
                            pqt.chroma[y*8 + x] =
                                (float) (1.0f/(8*aan_scales[x]*aan_scales[y]*state.qt_chroma[tjei_zig_zag[i]]));
                        }
                    }
                }
            }
            {
                TJEJPEGHeader header = new TJEJPEGHeader();
                header.SOI = (ushort) (tjei_be_word((ushort) (0xffd8)));
                header.APP0 = (ushort) (tjei_be_word((ushort) (0xffe0)));
                ushort jfif_len = (ushort) (20 - 4);
                header.jfif_len = (ushort) (tjei_be_word((ushort) (jfif_len)));
                memcpy(header.jfif_id, (void*) (tjeik_jfif_id), (ulong) (5));
                header.version = (ushort) (tjei_be_word((ushort) (0x0102)));
                header.units = (byte) (0x01);
                header.x_density = (ushort) (tjei_be_word((ushort) (0x0060)));
                header.y_density = (ushort) (tjei_be_word((ushort) (0x0060)));
                header.x_thumb = (byte) (0);
                header.y_thumb = (byte) (0);
                tjei_write(state, &header, (ulong) sizeof (TJEJPEGHeader), (ulong) (1));
            }

            {
                TJEJPEGComment com = new TJEJPEGComment();
                ushort com_len = (ushort) (2 + tjeik_com_str.Count);
                com.com = (ushort) (tjei_be_word((ushort) (0xfffe)));
                com.com_len = (ushort) (tjei_be_word((ushort) (com_len)));
                memcpy(com.com_str, (void*) (tjeik_com_str), (ulong) (tjeik_com_str.Count));
                tjei_write(state, &com, (ulong) sizeof (TJEJPEGComment), 1);
            }

            tjei_write_DQT(state, state.qt_luma, (byte) (0x00));
            tjei_write_DQT(state, state.qt_chroma, (byte) (0x01));
            {
                TJEFrameHeader header = new TJEFrameHeader();
                header.SOF = (ushort) (tjei_be_word((ushort) (0xffc0)));
                header.len = (ushort) (tjei_be_word((ushort) (8 + 3*3)));
                header.precision = (byte) (8);
                ;
                header.width = (ushort) (tjei_be_word((ushort) (width)));
                header.height = (ushort) (tjei_be_word((ushort) (height)));
                header.num_components = (byte) (3);
                byte* tables = stackalloc byte[3];
                tables[0] = (byte) (0);
                tables[1] = (byte) (1);
                tables[2] = (byte) (1);
                TJEComponentSpec spec = new TJEComponentSpec();
                spec.component_id = (byte) ((byte) (0 + 1));
                spec.sampling_factors = (byte) ((byte) (0x11));
                spec.qt = (byte) (tables[0]);
                header.component_spec0 = (TJEComponentSpec) (spec);
                spec.component_id = (byte) ((byte) (1 + 1));
                spec.qt = (byte) (tables[1]);
                header.component_spec1 = (TJEComponentSpec) (spec);
                spec.component_id = (byte) ((byte) (2 + 1));
                spec.qt = (byte) (tables[2]);
                header.component_spec2 = (TJEComponentSpec) (spec);
                tjei_write(state, &header, (ulong) sizeof (TJEFrameHeader), 1);
            }

            tjei_write_DHT(state, state.ht_bits[TJEI_LUMA_DC],
                state.ht_vals[TJEI_LUMA_DC],
                (int) (TJEI_DC), (byte) (0));
            tjei_write_DHT(state, state.ht_bits[TJEI_LUMA_AC],
                state.ht_vals[TJEI_LUMA_AC],
                (int) (TJEI_AC), (byte) (0));
            tjei_write_DHT(state, state.ht_bits[TJEI_CHROMA_DC],
                state.ht_vals[TJEI_CHROMA_DC],
                (int) (TJEI_DC), (byte) (1));
            tjei_write_DHT(state, state.ht_bits[TJEI_CHROMA_AC],
                state.ht_vals[TJEI_CHROMA_AC],
                (int) (TJEI_AC), (byte) (1));
            {
                TJEScanHeader header = new TJEScanHeader();
                header.SOS = (ushort) (tjei_be_word((ushort) (0xffda)));
                header.len = (ushort) (tjei_be_word((ushort) (6 + sizeof (TJEFrameComponentSpec)*3)));
                header.num_components = (byte) (3);
                byte* tables = stackalloc byte[3];
                tables[0] = (byte) (0x00);
                tables[1] = (byte) (0x11);
                tables[2] = (byte) (0x11);
                TJEFrameComponentSpec cs = new TJEFrameComponentSpec();
                cs.component_id = (byte) ((byte) (0 + 1));
                cs.dc_ac = (byte) (tables[0]);
                header.component_spec0 = (TJEFrameComponentSpec) (cs);
                cs.component_id = (byte) ((byte) (1 + 1));
                cs.dc_ac = (byte) (tables[1]);
                header.component_spec1 = (TJEFrameComponentSpec) (cs);
                cs.component_id = (byte) ((byte) (2 + 1));
                cs.dc_ac = (byte) (tables[2]);
                header.component_spec2 = (TJEFrameComponentSpec) (cs);
                header.first = (byte) (0);
                header.last = (byte) (63);
                header.ah_al = (byte) (0);
                tjei_write(state, &header, (ulong) sizeof (TJEScanHeader), 1);
            }

            float* du_y = stackalloc float[64];
            float* du_b = stackalloc float[64];
            float* du_r = stackalloc float[64];
            int pred_y = (int) (0);
            int pred_b = (int) (0);
            int pred_r = (int) (0);
            uint bitbuffer = (uint) (0);
            uint location = (uint) (0);
            for (int y = (int) (0); (y) < (height); y += (int) (8))
            {
                {
                    for (int x = (int) (0); (x) < (width); x += (int) (8))
                    {
                        {
                            for (int off_y = (int) (0); (off_y) < (8); ++off_y)
                            {
                                {
                                    for (int off_x = (int) (0); (off_x) < (8); ++off_x)
                                    {
                                        {
                                            int block_index = (int) (off_y*8 + off_x);
                                            int src_index =
                                                (int) ((((y + off_y)*width) + (x + off_x))*src_num_components);
                                            int col = (int) (x + off_x);
                                            int row = (int) (y + off_y);
                                            if ((row) >= (height))
                                            {
                                                src_index -= (int) ((width*(row - height + 1))*src_num_components);
                                            }
                                            if ((col) >= (width))
                                            {
                                                src_index -= (int) ((col - width + 1)*src_num_components);
                                            }
                                            ;
                                            byte r = (byte) (src_data[src_index + 0]);
                                            byte g = (byte) (src_data[src_index + 1]);
                                            byte b = (byte) (src_data[src_index + 2]);
                                            float luma = (float) (0.299f*r + 0.587f*g + 0.114f*b - 128);
                                            float cb = (float) (-0.1687f*r - 0.3313f*g + 0.5f*b);
                                            float cr = (float) (0.5f*r - 0.4187f*g - 0.0813f*b);
                                            du_y[block_index] = (float) (luma);
                                            du_b[block_index] = (float) (cb);
                                            du_r[block_index] = (float) (cr);
                                        }
                                    }
                                }
                            }
                            tjei_encode_and_write_MCU(state, du_y, pqt.luma,
                                ((byte*) state.ehuffsize + TJEI_LUMA_DC*257),
                                ((ushort*) state.ehuffcode + TJEI_LUMA_DC*256),
                                ((byte*) state.ehuffsize + TJEI_LUMA_AC*257),
                                ((ushort*) state.ehuffcode + TJEI_LUMA_AC*256),
                                &pred_y, &bitbuffer, &location);
                            tjei_encode_and_write_MCU(state, du_b, pqt.chroma,
                                ((byte*) state.ehuffsize + TJEI_CHROMA_DC*257),
                                ((ushort*) state.ehuffcode + TJEI_CHROMA_DC*256),
                                ((byte*) state.ehuffsize + TJEI_CHROMA_AC*257),
                                ((ushort*) state.ehuffcode + TJEI_CHROMA_AC*256), &pred_b,
                                &bitbuffer, &location);
                            tjei_encode_and_write_MCU(state, du_r, pqt.chroma,
                                ((byte*) state.ehuffsize + TJEI_CHROMA_DC*257),
                                ((ushort*) state.ehuffcode + TJEI_CHROMA_DC*256),
                                ((byte*) state.ehuffsize + TJEI_CHROMA_AC*257),
                                ((ushort*) state.ehuffcode + TJEI_CHROMA_AC*256), &pred_r,
                                &bitbuffer, &location);
                        }
                    }
                }
            }
            {
                if (((location) > (0)) && ((location) < (8)))
                {
                    tjei_write_bits(state, &bitbuffer, &location, (ushort) (8 - location), (ushort) (0));
                }
            }

            ushort EOI = (ushort) (tjei_be_word((ushort) (0xffd9)));
            tjei_write(state, &EOI, (ulong) (sizeof (ushort)), (ulong) (1));
            if ((state.output_buffer_count) != 0)
            {
                state.write_context.func(state.write_context.context, state.output_buffer,
                    (int) (state.output_buffer_count));
                state.output_buffer_count = (ulong) (0);
            }

            return (int) (1);
        }
    }
}