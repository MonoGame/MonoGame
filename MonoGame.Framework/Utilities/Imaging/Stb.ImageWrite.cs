using System.Text;

namespace MonoGame.Utilities
{
    unsafe partial class Imaging
    {
        public static int stbi_write_tga_with_rle = 1;

        public delegate int WriteCallback(void* context, void* data, int size);

        public class stbi__write_context
        {
            public WriteCallback func;
            public void* context;
        }

        public static void stbi__start_write_callbacks(stbi__write_context s, WriteCallback c, void* context)
        {
            s.func = c;
            s.context = context;
        }

        public static void stbiw__writefv(stbi__write_context s, string fmt, params object[] v)
        {
            var vindex = 0;
            for (var i = 0; i < fmt.Length; ++i)
            {
                var c = fmt[i];
                switch (c)
                {
                    case ' ':
                        break;
                    case '1':
                    {
                        var x = (byte) ((int) v[vindex++] & 0xff);
                        s.func(s.context, &x, 1);
                        break;
                    }
                    case '2':
                    {
                        var x = (int) v[vindex++];
                        var b = stackalloc byte[2];
                        b[0] = (byte) (x & 0xff);
                        b[1] = (byte) ((x >> 8) & 0xff);
                        s.func(s.context, b, 2);
                        break;
                    }
                    case '4':
                    {
                        var x = (int) v[vindex++];
                        var b = stackalloc byte[4];
                        b[0] = (byte) (x & 0xff);
                        b[1] = (byte) ((x >> 8) & 0xff);
                        b[2] = (byte) ((x >> 16) & 0xff);
                        b[3] = (byte) ((x >> 24) & 0xff);
                        s.func(s.context, b, 4);
                        break;
                    }
                }
            }
        }

        public static void stbiw__writef(stbi__write_context s, string fmt, params object[] v)
        {
            stbiw__writefv(s, fmt, v);
        }

        public static int stbiw__outfile(stbi__write_context s, int rgb_dir, int vdir, int x, int y, int comp,
            int expand_mono, void* data, int alpha, int pad, string fmt, params object[] v)
        {
            if ((y < 0) || (x < 0))
            {
                return 0;
            }

            stbiw__writefv(s, fmt, v);
            stbiw__write_pixels(s, rgb_dir, vdir, x, y, comp, data, alpha, pad, expand_mono);
            return 1;
        }

        public static int stbi_write_bmp_to_func(WriteCallback func,
            void* context,
            int x,
            int y,
            int comp,
            void* data
            )
        {
            var s = new stbi__write_context();
            stbi__start_write_callbacks(s, func, context);
            return stbi_write_bmp_core(s, x, y, comp, data);
        }

        public static int stbi_write_tga_to_func(WriteCallback func,
            void* context,
            int x,
            int y,
            int comp,
            void* data
            )
        {
            var s = new stbi__write_context();
            stbi__start_write_callbacks(s, func, context);
            return stbi_write_tga_core(s, x, y, comp, data);
        }

        public static int stbi_write_png_to_func(WriteCallback func,
            void* context,
            int x,
            int y,
            int comp,
            void* data,
            int stride_bytes
            )
        {
            int len;
            var png = stbi_write_png_to_mem((byte*) (data), stride_bytes, x, y, comp, &len);
            if (png == null) return 0;
            func(context, png, len);
            free(png);
            return 1;
        }

        public static int stbi_write_jpg_to_func(WriteCallback func,
            void* context,
            int x,
            int y,
            int comp,
            void* data,
            int quality
            )
        {
            stbi__write_context s = new stbi__write_context();
            stbi__start_write_callbacks(s, func, context);
            return stbi_write_jpg_core(s, x, y, comp, data, quality);
        }
    }
}