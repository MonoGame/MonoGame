namespace MonoGame.Utilities
{
    partial class Imaging
    {
        public static uint[] crc_table = new uint[]
        {
            (uint) (0x00000000), (uint) (0x77073096), (uint) (0xEE0E612C), (uint) (0x990951BA), (uint) (0x076DC419),
            (uint) (0x706AF48F), (uint) (0xE963A535), (uint) (0x9E6495A3), (uint) (0x0eDB8832), (uint) (0x79DCB8A4),
            (uint) (0xE0D5E91E), (uint) (0x97D2D988), (uint) (0x09B64C2B), (uint) (0x7EB17CBD), (uint) (0xE7B82D07),
            (uint) (0x90BF1D91), (uint) (0x1DB71064), (uint) (0x6AB020F2), (uint) (0xF3B97148), (uint) (0x84BE41DE),
            (uint) (0x1ADAD47D), (uint) (0x6DDDE4EB), (uint) (0xF4D4B551), (uint) (0x83D385C7), (uint) (0x136C9856),
            (uint) (0x646BA8C0), (uint) (0xFD62F97A), (uint) (0x8A65C9EC), (uint) (0x14015C4F), (uint) (0x63066CD9),
            (uint) (0xFA0F3D63), (uint) (0x8D080DF5), (uint) (0x3B6E20C8), (uint) (0x4C69105E), (uint) (0xD56041E4),
            (uint) (0xA2677172), (uint) (0x3C03E4D1), (uint) (0x4B04D447), (uint) (0xD20D85FD), (uint) (0xA50AB56B),
            (uint) (0x35B5A8FA), (uint) (0x42B2986C), (uint) (0xDBBBC9D6), (uint) (0xACBCF940), (uint) (0x32D86CE3),
            (uint) (0x45DF5C75), (uint) (0xDCD60DCF), (uint) (0xABD13D59), (uint) (0x26D930AC), (uint) (0x51DE003A),
            (uint) (0xC8D75180), (uint) (0xBFD06116), (uint) (0x21B4F4B5), (uint) (0x56B3C423), (uint) (0xCFBA9599),
            (uint) (0xB8BDA50F), (uint) (0x2802B89E), (uint) (0x5F058808), (uint) (0xC60CD9B2), (uint) (0xB10BE924),
            (uint) (0x2F6F7C87), (uint) (0x58684C11), (uint) (0xC1611DAB), (uint) (0xB6662D3D), (uint) (0x76DC4190),
            (uint) (0x01DB7106), (uint) (0x98D220BC), (uint) (0xEFD5102A), (uint) (0x71B18589), (uint) (0x06B6B51F),
            (uint) (0x9FBFE4A5), (uint) (0xE8B8D433), (uint) (0x7807C9A2), (uint) (0x0F00F934), (uint) (0x9609A88E),
            (uint) (0xE10E9818), (uint) (0x7F6A0DBB), (uint) (0x086D3D2D), (uint) (0x91646C97), (uint) (0xE6635C01),
            (uint) (0x6B6B51F4), (uint) (0x1C6C6162), (uint) (0x856530D8), (uint) (0xF262004E), (uint) (0x6C0695ED),
            (uint) (0x1B01A57B), (uint) (0x8208F4C1), (uint) (0xF50FC457), (uint) (0x65B0D9C6), (uint) (0x12B7E950),
            (uint) (0x8BBEB8EA), (uint) (0xFCB9887C), (uint) (0x62DD1DDF), (uint) (0x15DA2D49), (uint) (0x8CD37CF3),
            (uint) (0xFBD44C65), (uint) (0x4DB26158), (uint) (0x3AB551CE), (uint) (0xA3BC0074), (uint) (0xD4BB30E2),
            (uint) (0x4ADFA541), (uint) (0x3DD895D7), (uint) (0xA4D1C46D), (uint) (0xD3D6F4FB), (uint) (0x4369E96A),
            (uint) (0x346ED9FC), (uint) (0xAD678846), (uint) (0xDA60B8D0), (uint) (0x44042D73), (uint) (0x33031DE5),
            (uint) (0xAA0A4C5F), (uint) (0xDD0D7CC9), (uint) (0x5005713C), (uint) (0x270241AA), (uint) (0xBE0B1010),
            (uint) (0xC90C2086), (uint) (0x5768B525), (uint) (0x206F85B3), (uint) (0xB966D409), (uint) (0xCE61E49F),
            (uint) (0x5EDEF90E), (uint) (0x29D9C998), (uint) (0xB0D09822), (uint) (0xC7D7A8B4), (uint) (0x59B33D17),
            (uint) (0x2EB40D81), (uint) (0xB7BD5C3B), (uint) (0xC0BA6CAD), (uint) (0xEDB88320), (uint) (0x9ABFB3B6),
            (uint) (0x03B6E20C), (uint) (0x74B1D29A), (uint) (0xEAD54739), (uint) (0x9DD277AF), (uint) (0x04DB2615),
            (uint) (0x73DC1683), (uint) (0xE3630B12), (uint) (0x94643B84), (uint) (0x0D6D6A3E), (uint) (0x7A6A5AA8),
            (uint) (0xE40ECF0B), (uint) (0x9309FF9D), (uint) (0x0A00AE27), (uint) (0x7D079EB1), (uint) (0xF00F9344),
            (uint) (0x8708A3D2), (uint) (0x1E01F268), (uint) (0x6906C2FE), (uint) (0xF762575D), (uint) (0x806567CB),
            (uint) (0x196C3671), (uint) (0x6E6B06E7), (uint) (0xFED41B76), (uint) (0x89D32BE0), (uint) (0x10DA7A5A),
            (uint) (0x67DD4ACC), (uint) (0xF9B9DF6F), (uint) (0x8EBEEFF9), (uint) (0x17B7BE43), (uint) (0x60B08ED5),
            (uint) (0xD6D6A3E8), (uint) (0xA1D1937E), (uint) (0x38D8C2C4), (uint) (0x4FDFF252), (uint) (0xD1BB67F1),
            (uint) (0xA6BC5767), (uint) (0x3FB506DD), (uint) (0x48B2364B), (uint) (0xD80D2BDA), (uint) (0xAF0A1B4C),
            (uint) (0x36034AF6), (uint) (0x41047A60), (uint) (0xDF60EFC3), (uint) (0xA867DF55), (uint) (0x316E8EEF),
            (uint) (0x4669BE79), (uint) (0xCB61B38C), (uint) (0xBC66831A), (uint) (0x256FD2A0), (uint) (0x5268E236),
            (uint) (0xCC0C7795), (uint) (0xBB0B4703), (uint) (0x220216B9), (uint) (0x5505262F), (uint) (0xC5BA3BBE),
            (uint) (0xB2BD0B28), (uint) (0x2BB45A92), (uint) (0x5CB36A04), (uint) (0xC2D7FFA7), (uint) (0xB5D0CF31),
            (uint) (0x2CD99E8B), (uint) (0x5BDEAE1D), (uint) (0x9B64C2B0), (uint) (0xEC63F226), (uint) (0x756AA39C),
            (uint) (0x026D930A), (uint) (0x9C0906A9), (uint) (0xEB0E363F), (uint) (0x72076785), (uint) (0x05005713),
            (uint) (0x95BF4A82), (uint) (0xE2B87A14), (uint) (0x7BB12BAE), (uint) (0x0CB61B38), (uint) (0x92D28E9B),
            (uint) (0xE5D5BE0D), (uint) (0x7CDCEFB7), (uint) (0x0BDBDF21), (uint) (0x86D3D2D4), (uint) (0xF1D4E242),
            (uint) (0x68DDB3F8), (uint) (0x1FDA836E), (uint) (0x81BE16CD), (uint) (0xF6B9265B), (uint) (0x6FB077E1),
            (uint) (0x18B74777), (uint) (0x88085AE6), (uint) (0xFF0F6A70), (uint) (0x66063BCA), (uint) (0x11010B5C),
            (uint) (0x8F659EFF), (uint) (0xF862AE69), (uint) (0x616BFFD3), (uint) (0x166CCF45), (uint) (0xA00AE278),
            (uint) (0xD70DD2EE), (uint) (0x4E048354), (uint) (0x3903B3C2), (uint) (0xA7672661), (uint) (0xD06016F7),
            (uint) (0x4969474D), (uint) (0x3E6E77DB), (uint) (0xAED16A4A), (uint) (0xD9D65ADC), (uint) (0x40DF0B66),
            (uint) (0x37D83BF0), (uint) (0xA9BCAE53), (uint) (0xDEBB9EC5), (uint) (0x47B2CF7F), (uint) (0x30B5FFE9),
            (uint) (0xBDBDF21C), (uint) (0xCABAC28A), (uint) (0x53B39330), (uint) (0x24B4A3A6), (uint) (0xBAD03605),
            (uint) (0xCDD70693), (uint) (0x54DE5729), (uint) (0x23D967BF), (uint) (0xB3667A2E), (uint) (0xC4614AB8),
            (uint) (0x5D681B02), (uint) (0x2A6F2B94), (uint) (0xB40BBE37), (uint) (0xC30C8EA1), (uint) (0x5A05DF1B),
            (uint) (0x2D02EF8D)
        };

        public unsafe static void stbiw__write3(stbi__write_context s, byte a, byte b, byte c)
        {
            byte* arr = stackalloc byte[3];
            ((byte*) (arr))[0] = (byte) (a);
            ((byte*) (arr))[1] = (byte) (b);
            ((byte*) (arr))[2] = (byte) (c);
            s.func(s.context, ((byte*) (arr)), (int) (3));
        }

        public unsafe static void stbiw__write_pixel(stbi__write_context s, int rgb_dir, int comp, int write_alpha,
            int expand_mono, byte* d)
        {
            byte* bg = stackalloc byte[3];
            bg[0] = (byte) (255);
            bg[1] = (byte) (0);
            bg[2] = (byte) (255);
            byte* px = stackalloc byte[3];
            int k;
            if ((write_alpha) < (0)) s.func(s.context, &d[comp - 1], (int) (1));
            switch (comp)
            {
                case 1:
                    s.func(s.context, d, (int) (1));
                    break;
                case 2:
                    if ((expand_mono) != 0) stbiw__write3(s, (byte) (d[0]), (byte) (d[0]), (byte) (d[0]));
                    else s.func(s.context, d, (int) (1));
                    break;
                case 3:
                case 4:
                    if (((comp) == (4)) && (write_alpha == 0))
                    {
                        for (k = (int) (0); (k) < (3); ++k)
                        {
                            ((byte*) (px))[k] = (byte) (((byte*) (bg))[k] + ((d[k] - ((byte*) (bg))[k])*d[3])/255);
                        }
                        stbiw__write3(s, (byte) (((byte*) (px))[1 - rgb_dir]), (byte) (((byte*) (px))[1]),
                            (byte) (((byte*) (px))[1 + rgb_dir]));
                        break;
                    }
                    stbiw__write3(s, (byte) (d[1 - rgb_dir]), (byte) (d[1]), (byte) (d[1 + rgb_dir]));
                    break;
            }

            if ((write_alpha) > (0)) s.func(s.context, &d[comp - 1], (int) (1));
        }

        public unsafe static void stbiw__write_pixels(stbi__write_context s, int rgb_dir, int vdir, int x, int y,
            int comp,
            void* data, int write_alpha, int scanline_pad, int expand_mono)
        {
            uint zero = (uint) (0);
            int i;
            int j;
            int j_end;
            if (y <= 0) return;
            if ((vdir) < (0))
            {
                j_end = (int) (-1);
                j = (int) (y - 1);
            }
            else
            {
                j_end = (int) (y);
                j = (int) (0);
            }

            for (; j != j_end; j += (int) (vdir))
            {
                {
                    for (i = (int) (0); (i) < (x); ++i)
                    {
                        {
                            byte* d = (byte*) (data) + (j*x + i)*comp;
                            stbiw__write_pixel(s, (int) (rgb_dir), (int) (comp), (int) (write_alpha),
                                (int) (expand_mono), d);
                        }
                    }
                    s.func(s.context, &zero, (int) (scanline_pad));
                }
            }
        }

        public unsafe static int stbi_write_bmp_core(stbi__write_context s, int x, int y, int comp, void* data)
        {
            int pad = (int) ((-x*3) & 3);
            return
                (int)
                    (stbiw__outfile(s, (int) (-1), (int) (-1), (int) (x), (int) (y), (int) (comp), (int) (1), data,
                        (int) (0),
                        (int) (pad), "11 4 22 44 44 22 444444", (int) ('B'), (int) ('M'),
                        (int) (14 + 40 + (x*3 + pad)*y), (int) (0),
                        (int) (0), (int) (14 + 40), (int) (40), (int) (x), (int) (y), (int) (1), (int) (24), (int) (0),
                        (int) (0),
                        (int) (0), (int) (0), (int) (0), (int) (0)));
        }

        public unsafe static int stbi_write_tga_core(stbi__write_context s, int x, int y, int comp, void* data)
        {
            int has_alpha = (((comp) == (2)) || ((comp) == (4))) ? 1 : 0;
            int colorbytes = (int) ((has_alpha) != 0 ? comp - 1 : comp);
            int format = (int) ((colorbytes) < (2) ? 3 : 2);
            if (((y) < (0)) || ((x) < (0))) return (int) (0);
            if (stbi_write_tga_with_rle == 0)
            {
                return
                    (int)
                        (stbiw__outfile(s, (int) (-1), (int) (-1), (int) (x), (int) (y), (int) (comp), (int) (0), data,
                            (int) (has_alpha),
                            (int) (0), "111 221 2222 11", (int) (0), (int) (0), (int) (format), (int) (0), (int) (0),
                            (int) (0), (int) (0),
                            (int) (0), (int) (x), (int) (y), (int) ((colorbytes + has_alpha)*8), (int) (has_alpha*8)));
            }
            else
            {
                int i;
                int j;
                int k;
                stbiw__writef(s, "111 221 2222 11", (int) (0), (int) (0), (int) (format + 8), (int) (0), (int) (0),
                    (int) (0),
                    (int) (0), (int) (0), (int) (x), (int) (y), (int) ((colorbytes + has_alpha)*8), (int) (has_alpha*8));
                for (j = (int) (y - 1); (j) >= (0); --j)
                {
                    {
                        byte* row = (byte*) (data) + j*x*comp;
                        int len;
                        for (i = (int) (0); (i) < (x); i += (int) (len))
                        {
                            {
                                byte* begin = row + i*comp;
                                int diff = (int) (1);
                                len = (int) (1);
                                if ((i) < (x - 1))
                                {
                                    ++len;
                                    diff = (int) (memcmp(begin, row + (i + 1)*comp, (ulong) (comp)));
                                    if ((diff) != 0)
                                    {
                                        byte* prev = begin;
                                        for (k = (int) (i + 2); ((k) < (x)) && ((len) < (128)); ++k)
                                        {
                                            {
                                                if ((memcmp(prev, row + k*comp, (ulong) (comp))) != 0)
                                                {
                                                    prev += comp;
                                                    ++len;
                                                }
                                                else
                                                {
                                                    --len;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (k = (int) (i + 2); ((k) < (x)) && ((len) < (128)); ++k)
                                        {
                                            {
                                                if (memcmp(begin, row + k*comp, (ulong) (comp)) == 0)
                                                {
                                                    ++len;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                if ((diff) != 0)
                                {
                                    byte header = (byte) ((len - 1) & 0xff);
                                    s.func(s.context, &header, (int) (1));
                                    for (k = (int) (0); (k) < (len); ++k)
                                    {
                                        {
                                            stbiw__write_pixel(s, (int) (-1), (int) (comp), (int) (has_alpha), (int) (0),
                                                begin + k*comp);
                                        }
                                    }
                                }
                                else
                                {
                                    byte header = (byte) ((len - 129) & 0xff);
                                    s.func(s.context, &header, (int) (1));
                                    stbiw__write_pixel(s, (int) (-1), (int) (comp), (int) (has_alpha), (int) (0), begin);
                                }
                            }
                        }
                    }
                }
            }

            return (int) (1);
        }

        public unsafe static void stbiw__linear_to_rgbe(byte* rgbe, float* linear)
        {
            int exponent;
            float maxcomp =
                (float)
                    ((linear[0]) > ((linear[1]) > (linear[2]) ? (linear[1]) : (linear[2]))
                        ? (linear[0])
                        : ((linear[1]) > (linear[2]) ? (linear[1]) : (linear[2])));
            if ((maxcomp) < (1e-32f))
            {
                rgbe[0] = (byte) (rgbe[1] = (byte) (rgbe[2] = (byte) (rgbe[3] = (byte) (0))));
            }
            else
            {
                float normalize = (float) (frexp((double) (maxcomp), &exponent))*256.0f/maxcomp;
                rgbe[0] = (byte) ((byte) (linear[0]*normalize));
                rgbe[1] = (byte) ((byte) (linear[1]*normalize));
                rgbe[2] = (byte) ((byte) (linear[2]*normalize));
                rgbe[3] = (byte) ((byte) (exponent + 128));
            }

        }

        public unsafe static void stbiw__write_run_data(stbi__write_context s, int length, byte databyte)
        {
            byte lengthbyte = (byte) ((length + 128) & 0xff);
            s.func(s.context, &lengthbyte, (int) (1));
            s.func(s.context, &databyte, (int) (1));
        }

        public unsafe static void stbiw__write_dump_data(stbi__write_context s, int length, byte* data)
        {
            byte lengthbyte = (byte) ((length) & 0xff);
            s.func(s.context, &lengthbyte, (int) (1));
            s.func(s.context, data, (int) (length));
        }

        public unsafe static void* stbiw__sbgrowf(void** arr, int increment, int itemsize)
        {
            int m = (int) (*arr != null ? 2*((int*) (*arr) - 2)[0] + increment : increment + 1);
            void* p = realloc(*arr != null ? ((int*) (*arr) - 2) : ((int*) (0)), (ulong) (itemsize*m + sizeof (int)*2));
            if ((p) != null)
            {
                if (*arr == null) ((int*) (p))[1] = (int) (0);
                *arr = (void*) ((int*) (p) + 2);
                ((int*) (*arr) - 2)[0] = (int) (m);
            }

            return *arr;
        }

        public unsafe static byte* stbiw__zlib_flushf(byte* data, uint* bitbuffer, int* bitcount)
        {
            while ((*bitcount) >= (8))
            {
                {
                    if ((((data) == ((byte*) (0))) || ((((int*) (data) - 2)[1] + (1)) >= (((int*) (data) - 2)[0]))))
                    {
                        stbiw__sbgrowf((void**) (&(data)), (int) (1), sizeof (byte));
                    }
                    ;
                    (data)[((int*) (data) - 2)[1]++] = (byte) ((byte) ((*bitbuffer) & 0xff));
                    ;
                    *bitbuffer >>= 8;
                    *bitcount -= (int) (8);
                }
            }
            return data;
        }

        public unsafe static int stbiw__zlib_bitrev(int code, int codebits)
        {
            int res = (int) (0);
            while ((codebits--) != 0)
            {
                {
                    res = (int) ((res << 1) | (code & 1));
                    code >>= 1;
                }
            }
            return (int) (res);
        }

        public unsafe static uint stbiw__zlib_countm(byte* a, byte* b, int limit)
        {
            int i;
            for (i = (int) (0); ((i) < (limit)) && ((i) < (258)); ++i)
            {
                if (a[i] != b[i]) break;
            }
            return (uint) (i);
        }

        public unsafe static uint stbiw__zhash(byte* data)
        {
            uint hash = (uint) (data[0] + (data[1] << 8) + (data[2] << 16));
            hash ^= (uint) (hash << 3);
            hash += (uint) (hash >> 5);
            hash ^= (uint) (hash << 4);
            hash += (uint) (hash >> 17);
            hash ^= (uint) (hash << 25);
            hash += (uint) (hash >> 6);
            return (uint) (hash);
        }

        public unsafe static byte* stbi_zlib_compress(byte* data, int data_len, int* out_len, int quality)
        {
            ushort* lengthc = stackalloc ushort[30];
            lengthc[0] = (ushort) (3);
            lengthc[1] = (ushort) (4);
            lengthc[2] = (ushort) (5);
            lengthc[3] = (ushort) (6);
            lengthc[4] = (ushort) (7);
            lengthc[5] = (ushort) (8);
            lengthc[6] = (ushort) (9);
            lengthc[7] = (ushort) (10);
            lengthc[8] = (ushort) (11);
            lengthc[9] = (ushort) (13);
            lengthc[10] = (ushort) (15);
            lengthc[11] = (ushort) (17);
            lengthc[12] = (ushort) (19);
            lengthc[13] = (ushort) (23);
            lengthc[14] = (ushort) (27);
            lengthc[15] = (ushort) (31);
            lengthc[16] = (ushort) (35);
            lengthc[17] = (ushort) (43);
            lengthc[18] = (ushort) (51);
            lengthc[19] = (ushort) (59);
            lengthc[20] = (ushort) (67);
            lengthc[21] = (ushort) (83);
            lengthc[22] = (ushort) (99);
            lengthc[23] = (ushort) (115);
            lengthc[24] = (ushort) (131);
            lengthc[25] = (ushort) (163);
            lengthc[26] = (ushort) (195);
            lengthc[27] = (ushort) (227);
            lengthc[28] = (ushort) (258);
            lengthc[29] = (ushort) (259);

            byte* lengtheb = stackalloc byte[29];
            lengtheb[0] = (byte) (0);
            lengtheb[1] = (byte) (0);
            lengtheb[2] = (byte) (0);
            lengtheb[3] = (byte) (0);
            lengtheb[4] = (byte) (0);
            lengtheb[5] = (byte) (0);
            lengtheb[6] = (byte) (0);
            lengtheb[7] = (byte) (0);
            lengtheb[8] = (byte) (1);
            lengtheb[9] = (byte) (1);
            lengtheb[10] = (byte) (1);
            lengtheb[11] = (byte) (1);
            lengtheb[12] = (byte) (2);
            lengtheb[13] = (byte) (2);
            lengtheb[14] = (byte) (2);
            lengtheb[15] = (byte) (2);
            lengtheb[16] = (byte) (3);
            lengtheb[17] = (byte) (3);
            lengtheb[18] = (byte) (3);
            lengtheb[19] = (byte) (3);
            lengtheb[20] = (byte) (4);
            lengtheb[21] = (byte) (4);
            lengtheb[22] = (byte) (4);
            lengtheb[23] = (byte) (4);
            lengtheb[24] = (byte) (5);
            lengtheb[25] = (byte) (5);
            lengtheb[26] = (byte) (5);
            lengtheb[27] = (byte) (5);
            lengtheb[28] = (byte) (0);

            ushort* distc = stackalloc ushort[31];
            distc[0] = (ushort) (1);
            distc[1] = (ushort) (2);
            distc[2] = (ushort) (3);
            distc[3] = (ushort) (4);
            distc[4] = (ushort) (5);
            distc[5] = (ushort) (7);
            distc[6] = (ushort) (9);
            distc[7] = (ushort) (13);
            distc[8] = (ushort) (17);
            distc[9] = (ushort) (25);
            distc[10] = (ushort) (33);
            distc[11] = (ushort) (49);
            distc[12] = (ushort) (65);
            distc[13] = (ushort) (97);
            distc[14] = (ushort) (129);
            distc[15] = (ushort) (193);
            distc[16] = (ushort) (257);
            distc[17] = (ushort) (385);
            distc[18] = (ushort) (513);
            distc[19] = (ushort) (769);
            distc[20] = (ushort) (1025);
            distc[21] = (ushort) (1537);
            distc[22] = (ushort) (2049);
            distc[23] = (ushort) (3073);
            distc[24] = (ushort) (4097);
            distc[25] = (ushort) (6145);
            distc[26] = (ushort) (8193);
            distc[27] = (ushort) (12289);
            distc[28] = (ushort) (16385);
            distc[29] = (ushort) (24577);
            distc[30] = (ushort) (32768);

            byte* disteb = stackalloc byte[30];
            disteb[0] = (byte) (0);
            disteb[1] = (byte) (0);
            disteb[2] = (byte) (0);
            disteb[3] = (byte) (0);
            disteb[4] = (byte) (1);
            disteb[5] = (byte) (1);
            disteb[6] = (byte) (2);
            disteb[7] = (byte) (2);
            disteb[8] = (byte) (3);
            disteb[9] = (byte) (3);
            disteb[10] = (byte) (4);
            disteb[11] = (byte) (4);
            disteb[12] = (byte) (5);
            disteb[13] = (byte) (5);
            disteb[14] = (byte) (6);
            disteb[15] = (byte) (6);
            disteb[16] = (byte) (7);
            disteb[17] = (byte) (7);
            disteb[18] = (byte) (8);
            disteb[19] = (byte) (8);
            disteb[20] = (byte) (9);
            disteb[21] = (byte) (9);
            disteb[22] = (byte) (10);
            disteb[23] = (byte) (10);
            disteb[24] = (byte) (11);
            disteb[25] = (byte) (11);
            disteb[26] = (byte) (12);
            disteb[27] = (byte) (12);
            disteb[28] = (byte) (13);
            disteb[29] = (byte) (13);

            uint bitbuf = (uint) (0);
            int i;
            int j;
            int bitcount = (int) (0);
            byte* _out_ = ((byte*) ((void*) (0)));
            byte*** hash_table = (byte***) (malloc((ulong) (16384*sizeof (byte**))));
            if ((quality) < (5)) quality = (int) (5);
            if ((((_out_) == ((byte*) (0))) || ((((int*) (_out_) - 2)[1] + (1)) >= (((int*) (_out_) - 2)[0]))))
            {
                stbiw__sbgrowf((void**) (&(_out_)), (int) (1), sizeof (byte));
            }

            (_out_)[((int*) (_out_) - 2)[1]++] = (byte) (0x78);
            if ((((_out_) == ((byte*) (0))) || ((((int*) (_out_) - 2)[1] + (1)) >= (((int*) (_out_) - 2)[0]))))
            {
                stbiw__sbgrowf((void**) (&(_out_)), (int) (1), sizeof (byte));
            }

            (_out_)[((int*) (_out_) - 2)[1]++] = (byte) (0x5e);
            {
                bitbuf |= (uint) ((1) << bitcount);
                bitcount += (int) (1);
                _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                ;
            }

            {
                bitbuf |= (uint) ((1) << bitcount);
                bitcount += (int) (2);
                _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                ;
            }

            for (i = (int) (0); (i) < (16384); ++i)
            {
                hash_table[i] = ((byte**) ((void*) (0)));
            }
            i = (int) (0);
            while ((i) < (data_len - 3))
            {
                {
                    int h = (int) (stbiw__zhash(data + i) & (16384 - 1));
                    int best = (int) (3);
                    byte* bestloc = ((byte*) (0));
                    byte** hlist = hash_table[h];
                    int n = (int) (hlist != ((byte**) (0)) ? ((int*) (hlist) - 2)[1] : 0);
                    for (j = (int) (0); (j) < (n); ++j)
                    {
                        {
                            if ((hlist[j] - data) > (i - 32768))
                            {
                                int d = (int) (stbiw__zlib_countm(hlist[j], data + i, (int) (data_len - i)));
                                if ((d) >= (best))
                                {
                                    best = (int) (d);
                                    bestloc = hlist[j];
                                }
                            }
                        }
                    }
                    if (((hash_table[h]) != null) && ((((int*) (hash_table[h]) - 2)[1]) == (2*quality)))
                    {
                        memmove(hash_table[h], hash_table[h] + quality, (ulong) (sizeof (byte*)*quality));
                        ((int*) (hash_table[h]) - 2)[1] = (int) (quality);
                    }
                    if ((((hash_table[h]) == ((byte**) (0))) ||
                         ((((int*) (hash_table[h]) - 2)[1] + (1)) >= (((int*) (hash_table[h]) - 2)[0]))))
                    {
                        stbiw__sbgrowf((void**) (&(hash_table[h])), (int) (1), sizeof (byte*));
                    }
                    ;
                    (hash_table[h])[((int*) (hash_table[h]) - 2)[1]++] = (data + i);
                    ;
                    if ((bestloc) != null)
                    {
                        h = (int) (stbiw__zhash(data + i + 1) & (16384 - 1));
                        hlist = hash_table[h];
                        n = (int) (hlist != ((byte**) (0)) ? ((int*) (hlist) - 2)[1] : 0);
                        for (j = (int) (0); (j) < (n); ++j)
                        {
                            {
                                if ((hlist[j] - data) > (i - 32767))
                                {
                                    int e = (int) (stbiw__zlib_countm(hlist[j], data + i + 1, (int) (data_len - i - 1)));
                                    if ((e) > (best))
                                    {
                                        bestloc = ((byte*) ((void*) (0)));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if ((bestloc) != null)
                    {
                        int d = (int) (data + i - bestloc);
                        ;
                        for (j = (int) (0); (best) > (((ushort*) (lengthc))[j + 1] - 1); ++j)
                        {
                            ;
                        }
                        if (j + 257 <= 143)
                        {
                            bitbuf |= (uint) ((stbiw__zlib_bitrev((int) (0x30 + (j + 257)), (int) (8))) << bitcount);
                            bitcount += (int) (8);
                            _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                            ;
                        }
                        else if (j + 257 <= 255)
                        {
                            bitbuf |=
                                (uint) ((stbiw__zlib_bitrev((int) (0x190 + (j + 257) - 144), (int) (9))) << bitcount);
                            bitcount += (int) (9);
                            _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                            ;
                        }
                        else if (j + 257 <= 279)
                        {
                            bitbuf |= (uint) ((stbiw__zlib_bitrev((int) (0 + (j + 257) - 256), (int) (7))) << bitcount);
                            bitcount += (int) (7);
                            _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                            ;
                        }
                        else
                        {
                            bitbuf |=
                                (uint) ((stbiw__zlib_bitrev((int) (0xc0 + (j + 257) - 280), (int) (8))) << bitcount);
                            bitcount += (int) (8);
                            _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                            ;
                        }
                        ;
                        if ((((byte*) (lengtheb))[j]) != 0)
                        {
                            bitbuf |= (uint) ((best - ((ushort*) (lengthc))[j]) << bitcount);
                            bitcount += (int) (((byte*) (lengtheb))[j]);
                            _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                            ;
                        }
                        ;
                        for (j = (int) (0); (d) > (((ushort*) (distc))[j + 1] - 1); ++j)
                        {
                            ;
                        }
                        {
                            bitbuf |= (uint) ((stbiw__zlib_bitrev((int) (j), (int) (5))) << bitcount);
                            bitcount += (int) (5);
                            _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                            ;
                        }
                        ;
                        if ((((byte*) (disteb))[j]) != 0)
                        {
                            bitbuf |= (uint) ((d - ((ushort*) (distc))[j]) << bitcount);
                            bitcount += (int) (((byte*) (disteb))[j]);
                            _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                            ;
                        }
                        ;
                        i += (int) (best);
                    }
                    else
                    {
                        if (data[i] <= 143)
                        {
                            bitbuf |= (uint) ((stbiw__zlib_bitrev((int) (0x30 + (data[i])), (int) (8))) << bitcount);
                            bitcount += (int) (8);
                            _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                            ;
                        }
                        else
                        {
                            bitbuf |=
                                (uint) ((stbiw__zlib_bitrev((int) (0x190 + (data[i]) - 144), (int) (9))) << bitcount);
                            bitcount += (int) (9);
                            _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                            ;
                        }
                        ;
                        ++i;
                    }
                }
            }
            for (; (i) < (data_len); ++i)
            {
                if (data[i] <= 143)
                {
                    bitbuf |= (uint) ((stbiw__zlib_bitrev((int) (0x30 + (data[i])), (int) (8))) << bitcount);
                    bitcount += (int) (8);
                    _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                    ;
                }
                else
                {
                    bitbuf |= (uint) ((stbiw__zlib_bitrev((int) (0x190 + (data[i]) - 144), (int) (9))) << bitcount);
                    bitcount += (int) (9);
                    _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                    ;
                }
            }
            if (256 <= 143)
            {
                bitbuf |= (uint) ((stbiw__zlib_bitrev((int) (0x30 + (256)), (int) (8))) << bitcount);
                bitcount += (int) (8);
                _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                ;
            }
            else if (256 <= 255)
            {
                bitbuf |= (uint) ((stbiw__zlib_bitrev((int) (0x190 + (256) - 144), (int) (9))) << bitcount);
                bitcount += (int) (9);
                _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                ;
            }
            else if (256 <= 279)
            {
                bitbuf |= (uint) ((stbiw__zlib_bitrev((int) (0 + (256) - 256), (int) (7))) << bitcount);
                bitcount += (int) (7);
                _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                ;
            }
            else
            {
                bitbuf |= (uint) ((stbiw__zlib_bitrev((int) (0xc0 + (256) - 280), (int) (8))) << bitcount);
                bitcount += (int) (8);
                _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                ;
            }

            while ((bitcount) != 0)
            {
                {
                    bitbuf |= (uint) ((0) << bitcount);
                    bitcount += (int) (1);
                    _out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
                    ;
                }
            }
            for (i = (int) (0); (i) < (16384); ++i)
            {
                if ((hash_table[i]) != null)
                {
                    free(((int*) (hash_table[i]) - 2));
                }
            }
            free(hash_table);
            {
                uint s1 = (uint) (1);
                uint s2 = (uint) (0);
                int blocklen = (int) (data_len%5552);
                j = (int) (0);
                while ((j) < (data_len))
                {
                    {
                        for (i = (int) (0); (i) < (blocklen); ++i)
                        {
                            {
                                s1 += (uint) (data[j + i]);
                                s2 += (uint) (s1);
                            }
                        }
                        s1 %= (uint) (65521);
                        s2 %= (uint) (65521);
                        j += (int) (blocklen);
                        blocklen = (int) (5552);
                    }
                }
                if ((((_out_) == ((byte*) (0))) || ((((int*) (_out_) - 2)[1] + (1)) >= (((int*) (_out_) - 2)[0]))))
                {
                    stbiw__sbgrowf((void**) (&(_out_)), (int) (1), sizeof (byte));
                }
                ;
                (_out_)[((int*) (_out_) - 2)[1]++] = (byte) ((byte) ((s2 >> 8) & 0xff));
                ;
                if ((((_out_) == ((byte*) (0))) || ((((int*) (_out_) - 2)[1] + (1)) >= (((int*) (_out_) - 2)[0]))))
                {
                    stbiw__sbgrowf((void**) (&(_out_)), (int) (1), sizeof (byte));
                }
                ;
                (_out_)[((int*) (_out_) - 2)[1]++] = (byte) ((byte) ((s2) & 0xff));
                ;
                if ((((_out_) == ((byte*) (0))) || ((((int*) (_out_) - 2)[1] + (1)) >= (((int*) (_out_) - 2)[0]))))
                {
                    stbiw__sbgrowf((void**) (&(_out_)), (int) (1), sizeof (byte));
                }
                ;
                (_out_)[((int*) (_out_) - 2)[1]++] = (byte) ((byte) ((s1 >> 8) & 0xff));
                ;
                if ((((_out_) == ((byte*) (0))) || ((((int*) (_out_) - 2)[1] + (1)) >= (((int*) (_out_) - 2)[0]))))
                {
                    stbiw__sbgrowf((void**) (&(_out_)), (int) (1), sizeof (byte));
                }
                ;
                (_out_)[((int*) (_out_) - 2)[1]++] = (byte) ((byte) ((s1) & 0xff));
                ;
            }

            *out_len = (int) (((int*) (_out_) - 2)[1]);
            memmove(((int*) (_out_) - 2), _out_, (ulong) (*out_len));
            return (byte*) ((int*) (_out_) - 2);
        }

        public unsafe static uint stbiw__crc32(byte* buffer, int len)
        {
            uint crc = (uint) (~0u);
            int i;
            for (i = (int) (0); (i) < (len); ++i)
            {
                crc = (uint) ((crc >> 8) ^ ((crc_table))[buffer[i] ^ (crc & 0xff)]);
            }
            return (uint) (~crc);
        }

        public unsafe static void stbiw__wpcrc(byte** data, int len)
        {
            uint crc = (uint) (stbiw__crc32(*data - len - 4, (int) (len + 4)));
            (*data)[0] = (byte) ((byte) (((crc) >> 24) & 0xff));
            (*data)[1] = (byte) ((byte) (((crc) >> 16) & 0xff));
            (*data)[2] = (byte) ((byte) (((crc) >> 8) & 0xff));
            (*data)[3] = (byte) ((byte) ((crc) & 0xff));
            (*data) += 4;
        }

        public unsafe static byte stbiw__paeth(int a, int b, int c)
        {
            int p = (int) (a + b - c);
            int pa = (int) (abs((int) (p - a)));
            int pb = (int) (abs((int) (p - b)));
            int pc = (int) (abs((int) (p - c)));
            if ((pa <= pb) && (pa <= pc)) return (byte) ((a) & 0xff);
            if (pb <= pc) return (byte) ((b) & 0xff);
            return (byte) ((c) & 0xff);
        }

        public unsafe static byte* stbi_write_png_to_mem(byte* pixels, int stride_bytes, int x, int y, int n,
            int* out_len)
        {
            int* ctype = stackalloc int[5];
            ctype[0] = (int) (-1);
            ctype[1] = (int) (0);
            ctype[2] = (int) (4);
            ctype[3] = (int) (2);
            ctype[4] = (int) (6);

            byte* sig = stackalloc byte[8];
            sig[0] = (byte) (137);
            sig[1] = (byte) (80);
            sig[2] = (byte) (78);
            sig[3] = (byte) (71);
            sig[4] = (byte) (13);
            sig[5] = (byte) (10);
            sig[6] = (byte) (26);
            sig[7] = (byte) (10);

            byte* _out_;
            byte* o;
            byte* filt;
            byte* zlib;
            sbyte* line_buffer;
            int i;
            int j;
            int k;
            int p;
            int zlen;
            if ((stride_bytes) == (0)) stride_bytes = (int) (x*n);
            filt = (byte*) (malloc((ulong) ((x*n + 1)*y)));
            if (filt == null) return ((byte*) (0));
            line_buffer = (sbyte*) (malloc((ulong) (x*n)));
            if (line_buffer == null)
            {
                free(filt);
                return ((byte*) (0));
            }

            for (j = (int) (0); (j) < (y); ++j)
            {
                {
                    int* mapping = stackalloc int[5];
                    mapping[0] = (int) (0);
                    mapping[1] = (int) (1);
                    mapping[2] = (int) (2);
                    mapping[3] = (int) (3);
                    mapping[4] = (int) (4);
                    int* firstmap = stackalloc int[5];
                    firstmap[0] = (int) (0);
                    firstmap[1] = (int) (1);
                    firstmap[2] = (int) (0);
                    firstmap[3] = (int) (5);
                    firstmap[4] = (int) (6);
                    int* mymap = (j) != 0 ? ((int*) (mapping)) : ((int*) (firstmap));
                    int best = (int) (0);
                    int bestval = (int) (0x7fffffff);
                    for (p = (int) (0); (p) < (2); ++p)
                    {
                        {
                            for (k = (int) ((p) != 0 ? best : 0); (k) < (5); ++k)
                            {
                                {
                                    int type = (int) (mymap[k]);
                                    int est = (int) (0);
                                    byte* z = pixels + stride_bytes*j;
                                    for (i = (int) (0); (i) < (n); ++i)
                                    {
                                        switch (type)
                                        {
                                            case 0:
                                                line_buffer[i] = (sbyte) (z[i]);
                                                break;
                                            case 1:
                                                line_buffer[i] = (sbyte) (z[i]);
                                                break;
                                            case 2:
                                                line_buffer[i] = (sbyte) (z[i] - z[i - stride_bytes]);
                                                break;
                                            case 3:
                                                line_buffer[i] = (sbyte) (z[i] - (z[i - stride_bytes] >> 1));
                                                break;
                                            case 4:
                                                line_buffer[i] =
                                                    (sbyte)
                                                        ((sbyte)
                                                            (z[i] -
                                                             stbiw__paeth((int) (0), (int) (z[i - stride_bytes]),
                                                                 (int) (0))));
                                                break;
                                            case 5:
                                                line_buffer[i] = (sbyte) (z[i]);
                                                break;
                                            case 6:
                                                line_buffer[i] = (sbyte) (z[i]);
                                                break;
                                        }
                                    }
                                    for (i = (int) (n); (i) < (x*n); ++i)
                                    {
                                        {
                                            switch (type)
                                            {
                                                case 0:
                                                    line_buffer[i] = (sbyte) (z[i]);
                                                    break;
                                                case 1:
                                                    line_buffer[i] = (sbyte) (z[i] - z[i - n]);
                                                    break;
                                                case 2:
                                                    line_buffer[i] = (sbyte) (z[i] - z[i - stride_bytes]);
                                                    break;
                                                case 3:
                                                    line_buffer[i] =
                                                        (sbyte) (z[i] - ((z[i - n] + z[i - stride_bytes]) >> 1));
                                                    break;
                                                case 4:
                                                    line_buffer[i] =
                                                        (sbyte)
                                                            (z[i] -
                                                             stbiw__paeth((int) (z[i - n]), (int) (z[i - stride_bytes]),
                                                                 (int) (z[i - stride_bytes - n])));
                                                    break;
                                                case 5:
                                                    line_buffer[i] = (sbyte) (z[i] - (z[i - n] >> 1));
                                                    break;
                                                case 6:
                                                    line_buffer[i] =
                                                        (sbyte)
                                                            (z[i] - stbiw__paeth((int) (z[i - n]), (int) (0), (int) (0)));
                                                    break;
                                            }
                                        }
                                    }
                                    if ((p) != 0) break;
                                    for (i = (int) (0); (i) < (x*n); ++i)
                                    {
                                        est += (int) (abs((int) (line_buffer[i])));
                                    }
                                    if ((est) < (bestval))
                                    {
                                        bestval = (int) (est);
                                        best = (int) (k);
                                    }
                                }
                            }
                        }
                    }
                    filt[j*(x*n + 1)] = (byte) ((byte) (best));
                    memmove(filt + j*(x*n + 1) + 1, line_buffer, (ulong) (x*n));
                }
            }
            free(line_buffer);
            zlib = stbi_zlib_compress(filt, (int) (y*(x*n + 1)), &zlen, (int) (8));
            free(filt);
            if (zlib == null) return ((byte*) (0));
            _out_ = (byte*) (malloc((ulong) (8 + 12 + 13 + 12 + zlen + 12)));
            if (_out_ == null) return ((byte*) (0));
            *out_len = (int) (8 + 12 + 13 + 12 + zlen + 12);
            o = _out_;
            memmove(o, ((byte*) (sig)), (ulong) (8));
            o += 8;
            (o)[0] = (byte) ((byte) (((13) >> 24) & 0xff));
            (o)[1] = (byte) ((byte) (((13) >> 16) & 0xff));
            (o)[2] = (byte) ((byte) (((13) >> 8) & 0xff));
            (o)[3] = (byte) ((byte) ((13) & 0xff));
            (o) += 4;
            (o)[0] = (byte) ((byte) (("IHDR"[0]) & 0xff));
            (o)[1] = (byte) ((byte) (("IHDR"[1]) & 0xff));
            (o)[2] = (byte) ((byte) (("IHDR"[2]) & 0xff));
            (o)[3] = (byte) ((byte) (("IHDR"[3]) & 0xff));
            (o) += 4;
            (o)[0] = (byte) ((byte) (((x) >> 24) & 0xff));
            (o)[1] = (byte) ((byte) (((x) >> 16) & 0xff));
            (o)[2] = (byte) ((byte) (((x) >> 8) & 0xff));
            (o)[3] = (byte) ((byte) ((x) & 0xff));
            (o) += 4;
            (o)[0] = (byte) ((byte) (((y) >> 24) & 0xff));
            (o)[1] = (byte) ((byte) (((y) >> 16) & 0xff));
            (o)[2] = (byte) ((byte) (((y) >> 8) & 0xff));
            (o)[3] = (byte) ((byte) ((y) & 0xff));
            (o) += 4;
            *o++ = (byte) (8);
            *o++ = (byte) ((byte) ((((int*) (ctype))[n]) & 0xff));
            *o++ = (byte) (0);
            *o++ = (byte) (0);
            *o++ = (byte) (0);
            stbiw__wpcrc(&o, (int) (13));
            (o)[0] = (byte) ((byte) (((zlen) >> 24) & 0xff));
            (o)[1] = (byte) ((byte) (((zlen) >> 16) & 0xff));
            (o)[2] = (byte) ((byte) (((zlen) >> 8) & 0xff));
            (o)[3] = (byte) ((byte) ((zlen) & 0xff));
            (o) += 4;
            (o)[0] = (byte) ((byte) (("IDAT"[0]) & 0xff));
            (o)[1] = (byte) ((byte) (("IDAT"[1]) & 0xff));
            (o)[2] = (byte) ((byte) (("IDAT"[2]) & 0xff));
            (o)[3] = (byte) ((byte) (("IDAT"[3]) & 0xff));
            (o) += 4;
            memmove(o, zlib, (ulong) (zlen));
            o += zlen;
            free(zlib);
            stbiw__wpcrc(&o, (int) (zlen));
            (o)[0] = (byte) ((byte) (((0) >> 24) & 0xff));
            (o)[1] = (byte) ((byte) (((0) >> 16) & 0xff));
            (o)[2] = (byte) ((byte) (((0) >> 8) & 0xff));
            (o)[3] = (byte) ((byte) ((0) & 0xff));
            (o) += 4;
            (o)[0] = (byte) ((byte) (("IEND"[0]) & 0xff));
            (o)[1] = (byte) ((byte) (("IEND"[1]) & 0xff));
            (o)[2] = (byte) ((byte) (("IEND"[2]) & 0xff));
            (o)[3] = (byte) ((byte) (("IEND"[3]) & 0xff));
            (o) += 4;
            stbiw__wpcrc(&o, (int) (0));
            return _out_;
        }
    }
}