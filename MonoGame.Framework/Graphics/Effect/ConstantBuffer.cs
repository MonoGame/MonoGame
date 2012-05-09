using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class ConstantBuffer : GraphicsResource
    {
        private readonly byte[] _buffer;

        private readonly int[] _parameters;

        private readonly int[] _offsets;

#if DIRECTX
        private SharpDX.Direct3D11.Buffer _cbuffer;
#endif

        public ConstantBuffer(ConstantBuffer cloneSource)
        {
            graphicsDevice = cloneSource.graphicsDevice;

            // Share the immutable types.
            _parameters = cloneSource._parameters;
            _offsets = cloneSource._offsets;

            // Clone the mutable types.
            _buffer = (byte[])cloneSource._buffer.Clone();
            Initialize();
        }

        public ConstantBuffer(GraphicsDevice device, BinaryReader reader)
        {
            graphicsDevice = device;

            // Create the backing system memory buffer.
            var size = (int)reader.ReadInt16();
            _buffer = new byte[size];

            // Read the parameter index values.
            _parameters = new int[reader.ReadByte()];
            _offsets = new int[_parameters.Length];
            for (var i = 0; i < _parameters.Length; i++)
            {
                _parameters[i] = (int)reader.ReadByte();
                _offsets[i] = (int)reader.ReadUInt16();
            }

            Initialize();
        }

        private void Initialize()
        {
#if DIRECTX

            // Allocate the hardware constant buffer.
            var desc = new SharpDX.Direct3D11.BufferDescription();
            desc.SizeInBytes = _buffer.Length;
            desc.Usage = SharpDX.Direct3D11.ResourceUsage.Default;
            desc.BindFlags = SharpDX.Direct3D11.BindFlags.ConstantBuffer;
            desc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None;
            _cbuffer = new SharpDX.Direct3D11.Buffer(graphicsDevice._d3dDevice, desc);

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

        public void Apply(bool vertexStage, int slot, EffectParameterCollection parameters)
        {
            // TODO:  We should be doing some sort of dirty state 
            // testing here.
            //
            // It should let us skip all parameter updates if
            // nothing has changed.  It should not be per-parameter
            // as that is why you should use multiple constant
            // buffers.

            var dirty = false;

            for (var p = 0; p < _parameters.Length; p++)
            {
                var index = _parameters[p];
                var offset = _offsets[p];
                var param = parameters[p];

                switch (param.ParameterType)
                {
                    case EffectParameterType.Single:
                        SetData(offset, param.RowCount, param.ColumnCount, param.Data);                        
                        break;

                    default:
                        throw new NotImplementedException("Not supported!");
                }

                dirty = true;
            }

#if DIRECTX
            var d3dContext = graphicsDevice._d3dContext;

            // Update the hardware buffer.
            if ( dirty )
                d3dContext.UpdateSubresource(_buffer, _cbuffer);

            // Set the constant buffer.
            if (vertexStage)
                d3dContext.VertexShader.SetConstantBuffer(slot, _cbuffer);
            else
                d3dContext.PixelShader.SetConstantBuffer(slot, _cbuffer);

#endif
        }
    }
}
