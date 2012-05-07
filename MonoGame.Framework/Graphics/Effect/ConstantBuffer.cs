using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class ConstantBuffer : GraphicsResource
    {
        private byte[] _buffer;

#if DIRECTX
        private SharpDX.Direct3D11.Buffer _cbuffer;
#endif

        public ConstantBuffer(ConstantBuffer cloneSource)
            : this(cloneSource.graphicsDevice, cloneSource._buffer.Length)
        {
            // Copy the current data state.
            Array.Copy(cloneSource._buffer, _buffer, _buffer.Length);
        }

        public ConstantBuffer(GraphicsDevice device, int size)
        {
            graphicsDevice = device;
            
            // Create the system memory buffer.
            _buffer = new byte[size];

#if DIRECTX

            // Allocate the hardware constant buffer.
            var desc = new SharpDX.Direct3D11.BufferDescription();
            desc.SizeInBytes = size;
            desc.Usage = SharpDX.Direct3D11.ResourceUsage.Default;
            desc.BindFlags = SharpDX.Direct3D11.BindFlags.ConstantBuffer;
            desc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None;
            _cbuffer = new SharpDX.Direct3D11.Buffer(device._d3dDevice, desc);

#endif
        }

        public void SetData(int offset, int rows, int columns, object data)
        {
            // TODO: Should i pass the element size in?
            var elementSize = 4;
            var rowSize = elementSize * 4;

            // Take care of a single data type.
            if (rows == 1 && columns == 1)
            {
                var bytes = BitConverter.GetBytes((float)data);
                Buffer.BlockCopy(bytes, 0, _buffer, offset, elementSize);
            }

            // Take care of the single copy case!
            else if (rows == 1 || (rows == 4 && columns == 4))
                Buffer.BlockCopy(data as Array, 0, _buffer, offset, rows * columns * elementSize);

            else
            {
                var stride = (columns * elementSize);
                for (var y = 0; y < rows; y++)
                    Buffer.BlockCopy(data as Array, stride * y, _buffer, offset + (rowSize * y), columns * elementSize);
            }
        }

        public void Apply()
        {
#if DIRECTX
            var d3dContext = graphicsDevice._d3dContext;

            // Update the buffer.
            d3dContext.UpdateSubresource(_buffer, _cbuffer);

            // TODO: How do we know what slot and which buffers
            // are needed for what shader?

            // Set the constant buffer.
            d3dContext.VertexShader.SetConstantBuffer(0, _cbuffer);
            d3dContext.PixelShader.SetConstantBuffer(0, _cbuffer);
#endif
        }
    }
}
