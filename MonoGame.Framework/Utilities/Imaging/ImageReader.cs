using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MonoGame.Utilities
{
    internal static unsafe class ImageReader
    {
        public static byte[] Read(Stream stream, out int x, out int y, out int comp, int req_comp)
        {
            byte[] bytes, data = null;

            // Rewind stream if it is at end
            if (stream.CanSeek && stream.Length == stream.Position)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                bytes = ms.ToArray();
            }

            int xx, yy, ccomp;
            byte* result = null;
            try
            {
                fixed (byte* b = bytes)
                {
                    result = Imaging.stbi_load_from_memory(b, bytes.Length, &xx, &yy, &ccomp, req_comp);
                }

                x = xx;
                y = yy;
                comp = ccomp;

                if (result == null)
                {
                    throw new InvalidOperationException(Imaging.LastError);
                }

                // Convert to array
                var c = req_comp != 0 ? req_comp : comp;
                data = new byte[x * y * c];
                Marshal.Copy(new IntPtr(result), data, 0, data.Length);
            }
            finally
            {
                if (result != null)
                {
                    Operations.Free(result);
                }
            }

            return data;
        }
    }
}
