#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2013 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
#if DIRECTX
    using SharpDX.DXGI;
    using SharpDX.Direct3D11;
    using Device = SharpDX.Direct3D11.Device;
#elif OPENGL
    //using OpenGL4NET;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Compute surface which content will be calculated on GPU.
    /// </summary>
    public sealed class ComputeSurface2D : IDisposable
    {
        private int width;
        private int height;
#if DIRECTX
        internal Format format;
        internal Device device;
        internal DeviceContext context;
        internal SharpDX.Direct3D11.Texture2D texture;
        internal UnorderedAccessView view;
#elif OPENGL
        internal uint texture = 0;

        public static uint GetBoundTexture2D()
        { 
            //int[] data = new int[1];
            //gl.GetIntegeri_v(GL.TEXTURE_BINDING_2D, 0U, data);
            //return (uint)data[0];

            return 0;
        }

        private Color[] ConvertByteArrayToColorArray(byte[] arr)
        {
            int cl = arr.Length / 4;
            Color[] carr = new Color[cl];

            for (int i = 0; i < cl; i += 4)
            {
                carr[i].R = arr[i];
                carr[i].G = arr[i + 1];
                carr[i].B = arr[i + 2];
                carr[i].A = arr[i + 3];
            }

            return carr;
        }
#endif
        /// <summary>
        /// Gets the width of <see cref="ComputeSurface2D"/>.
        /// </summary>
        public int Width { get { return width; } }
        /// <summary>
        /// Gets the height of <see cref="ComputeSurface2D"/>.
        /// </summary>
        public int Height { get { return height; } }


        /// <summary>
        /// Creates a new instance of <see cref="ComputeSurface2D"/> class.
        /// </summary>
        /// <param name="graphics">Valid <see cref="GraphicsDevice"/>.</param>
        /// <param name="width">Width of new surface.</param>
        /// <param name="height">Height of new surface.</param>
        public ComputeSurface2D(GraphicsDevice graphics, int width, int height)
        {
            this.width = width;
            this.height = height;
#if DIRECTX
            device = (Device)graphics.Handle;
            context = device.ImmediateContext;
            Texture2DDescription desc = new Texture2DDescription();
            desc.ArraySize = 1;
            desc.BindFlags = BindFlags.UnorderedAccess;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            desc.Format = format = Format.R8G8B8A8_UNorm;
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = 1;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;
            desc.Usage = ResourceUsage.Default;
            texture = new SharpDX.Direct3D11.Texture2D(device, desc);
            view = new UnorderedAccessView(device, texture);
#elif OPENGL  
            //// Store the current bound texture.
            //var prevTexture = GetBoundTexture2D();

            //texture = gl.GenTexture();
            //gl.BindTexture(GL.TEXTURE_2D, this.texture);
            //gl.TexStorage2D(GL.TEXTURE_2D, 1, GL.RGBA32F, width, height);
               
            //// Restore the bound texture.
            //gl.BindTexture(GL.TEXTURE_2D, prevTexture);  
#endif
        }

        public void GetData(int level, Rectangle? rect, Color[] data, int startIndex, int elementCount)
        {
#if DIRECTX
            if (data == null || data.Length == 0)
                throw new ArgumentException("data cannot be null");
            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data passed has a length of " + data.Length + " but " + elementCount + " pixels have been requested.");

            var desc = new SharpDX.Direct3D11.Texture2DDescription();
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = 1;
            desc.ArraySize = 1;
            desc.Format = format;
            desc.BindFlags = SharpDX.Direct3D11.BindFlags.None;
            desc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;
            desc.Usage = SharpDX.Direct3D11.ResourceUsage.Staging;
            desc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;

            var d3dContext = context;
            using (var stagingTex = new SharpDX.Direct3D11.Texture2D(device, desc))
                lock (d3dContext)
                {
                    // Copy the data from the GPU to the staging texture.
                    if (rect.HasValue)
                    {
                        // TODO: Need to deal with subregion copies!
                        throw new NotImplementedException();
                    }
                    else
                        d3dContext.CopySubresourceRegion(texture, level, null, stagingTex, 0, 0, 0, 0);

                    // Copy the data to the array.
                    SharpDX.DataStream stream;
                    d3dContext.MapSubresource(stagingTex, 0, SharpDX.Direct3D11.MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out stream);
                    stream.ReadRange(data, startIndex, elementCount);
                    stream.Dispose();
                }
#elif OPENGL
            //var temp = new T[this.width * this.height];

            //IntPtr temp = Utility.AllocateMemory(this.width*this.height);

            //gl.BindTexture(GL.TEXTURE_2D, this.texture);
            //gl.GetTexImage(GL.TEXTURE_2D, level, GL.RGBA32F, GL.FLOAT, temp);

            //byte[] managedArray = new byte[this.width * this.height];
            //Marshal.Copy(temp, managedArray, 0, this.width * this.height);

            //if (rect.HasValue)
            //{
            //    int z = 0, w = 0;

            //    for (int y = rect.Value.Y; y < rect.Value.Y + rect.Value.Height; y++)
            //    {
            //        for (int x = rect.Value.X; x < rect.Value.X + rect.Value.Width; x++)
            //        {
            //            data[z * rect.Value.Width + w].R = managedArray[(y * width) + x];
            //            data[z * rect.Value.Width + w + 1].R = managedArray[(y * width) + x + 1];
            //            data[z * rect.Value.Width + w + 2].G = managedArray[(y * width) + x + 2];
            //            data[z * rect.Value.Width + w + 3].B = managedArray[(y * width) + x + 3];
            //            w++;
            //        }
            //        z++;
            //    }
            //}
            //else
            //{
            //    data = ConvertByteArrayToColorArray(managedArray);
            //}
#endif
        }

        public void GetData(Color[] data, int startIndex, int elementCount)
        {
            this.GetData(0, null, data, startIndex, elementCount);
        }

        /// <summary>
        /// Fills the array of <see cref="Color"/> with computation result.
        /// </summary>
        /// <param name="data"><see cref="Color"/> array.</param>
        public void GetData(Color[] data)
        {
            this.GetData(0, null, data, 0, data.Length);
        }

        ~ComputeSurface2D()
        {
#if DIRECTX
            if (view != null)
            {
                view.Dispose();
            }
            if (texture != null)
            {
                texture.Dispose();
            }
#elif OPENGL
            //if (texture != uint.MaxValue)
            //{
            //    gl.DeleteTexture(texture);
            //}
#endif
        }

        /// <summary>
        /// Releases internal unmanaged resources.
        /// </summary>
        public void Dispose()
        {
#if DIRECTX
            if (view != null)
            {
                view.Dispose();
            }
            if (texture != null)
            {
                texture.Dispose();
            }
#elif OPENGL
            //if (texture != uint.MaxValue)
            //{
            //    gl.DeleteTexture(texture); 
            //}
#endif
            GC.SuppressFinalize(this);
        }
    }
}
