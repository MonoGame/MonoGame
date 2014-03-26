// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Imaging;
using PssTexture2D = Sce.PlayStation.Core.Graphics.Texture2D;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
		internal PssTexture2D _texture2D;

        private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
        {
            PixelBufferOption option = PixelBufferOption.None;
            if (type == SurfaceType.RenderTarget)
			    option = PixelBufferOption.Renderable;
            _texture2D = new Sce.PlayStation.Core.Graphics.Texture2D(width, height, mipmap, PSSHelper.ToFormat(format),option);
        }

        private Texture2D(GraphicsDevice graphicsDevice, Stream stream)
        {            
            if (graphicsDevice == null)
                throw new ArgumentNullException("Graphics Device Cannot Be Null");
           
            GraphicsDevice = graphicsDevice;
                       
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            _texture2D = new PssTexture2D(bytes, false);
            width = _texture2D.Width;
            height = _texture2D.Height;
            this._format = SurfaceFormat.Color; //FIXME HACK
            this._levelCount = 1;
        }

        private void PlatformSetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            int x, y, w, h;
            if (rect.HasValue)
            {
                x = rect.Value.X;
                y = rect.Value.Y;
                w = rect.Value.Width;
                h = rect.Value.Height;
            }
            else
            {
                x = 0;
                y = 0;
                w = Math.Max(width >> level, 1);
                h = Math.Max(height >> level, 1);

                // For DXT textures the width and height of each level is a multiple of 4.
                // OpenGL only: The last two mip levels require the width and height to be 
                // passed as 2x2 and 1x1, but there needs to be enough data passed to occupy 
                // a 4x4 block. 
                // Ref: http://www.mentby.com/Group/mac-opengl/issue-with-dxt-mipmapped-textures.html 
                if (_format == SurfaceFormat.Dxt1 ||
                    _format == SurfaceFormat.Dxt1a ||
                    _format == SurfaceFormat.Dxt3 ||
                    _format == SurfaceFormat.Dxt5)
                {
                        if (w > 4)
                            w = (w + 3) & ~3;
                        if (h > 4)
                            h = (h + 3) & ~3;
                }
            }
            _texture2D.SetPixels(level, data, _texture2D.Format, startIndex, 0, x, y, w, h);
        }

        private void PlatformGetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            Rectangle r;
            if (rect.HasValue)
            {
                r = rect.Value;
            }
            else
            {
                r = new Rectangle(0, 0, Width, Height);
            }
            
            int rWidth = r.Width;
            int rHeight = r.Height;
            
            var sz = 4;         
            
            // Loop through and extract the data but we need to load it 
            var dataRowColOffset = 0;
            
            var pixelOffset = 0;
            var result = new Color(0, 0, 0, 0);
            
            byte[] imageInfo = new byte[(rWidth * rHeight) * sz];
            
            ImageRect old_scissor = GraphicsDevice.Context.GetScissor();
            ImageRect old_viewport = GraphicsDevice.Context.GetViewport();
            FrameBuffer old_frame_buffer = GraphicsDevice.Context.GetFrameBuffer();

            ColorBuffer color_buffer = new ColorBuffer(rWidth, rHeight, PixelFormat.Rgba);
            FrameBuffer frame_buffer = new FrameBuffer();
            frame_buffer.SetColorTarget(color_buffer);
             
            GraphicsDevice.Context.SetFrameBuffer(frame_buffer);

            GraphicsDevice.Context.SetTexture(0, this._texture2D);
            GraphicsDevice.Context.ReadPixels(imageInfo, PixelFormat.Rgba, 0, 0, rWidth, rHeight);

            GraphicsDevice.Context.SetFrameBuffer(old_frame_buffer);
            GraphicsDevice.Context.SetScissor(old_scissor);
            GraphicsDevice.Context.SetViewport(old_viewport);
            
            for (int y = r.Top; y < rHeight; y++)
            {
                for (int x = r.Left; x < rWidth; x++)
                {
                    dataRowColOffset = ((y * r.Width) + x);
                    
                    pixelOffset = dataRowColOffset * sz;
                    result.R = imageInfo[pixelOffset];
                    result.G = imageInfo[pixelOffset + 1];
                    result.B = imageInfo[pixelOffset + 2];
                    result.A = imageInfo[pixelOffset + 3];
                    
                    data[dataRowColOffset] = (T)(object)result;
                }
            }
        }

        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            return new Texture2D(graphicsDevice, stream);
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
            throw new NotImplementedException();
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
            // TODO: We need to find a simple stand alone
            // PNG encoder if we want to support this.
            throw new NotImplementedException();
        }

        private void PlatformReload(Stream textureStream)
        {
        }
	}
}

