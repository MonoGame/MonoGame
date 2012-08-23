using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
#elif PSS
using Sce.PlayStation.Core.Graphics;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    internal class ConstantBuffer : GraphicsResource
    {
        private readonly byte[] _buffer;

        private readonly int[] _parameters;

        private readonly int[] _offsets;

        private readonly string _name;

        private ulong _stateKey;

#if DIRECTX
        private SharpDX.Direct3D11.Buffer _cbuffer;
#endif

        public ConstantBuffer(ConstantBuffer cloneSource)
        {
            graphicsDevice = cloneSource.graphicsDevice;

            // Share the immutable types.
            _name = cloneSource._name;
            _parameters = cloneSource._parameters;
            _offsets = cloneSource._offsets;

            // Clone the mutable types.
            _buffer = (byte[])cloneSource._buffer.Clone();
            Initialize();
        }

        public ConstantBuffer(GraphicsDevice device,
                              int sizeInBytes,
                              int[] parameterIndexes,
                              int[] parameterOffsets,
                              string name)
        {
            graphicsDevice = device;

            _buffer = new byte[sizeInBytes];

            _parameters = parameterIndexes;
            _offsets = parameterOffsets;

            _name = name;

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
                // TODO: Consider storing all data in arrays to avoid
                // having to generate this temp array on every set.
                byte[] bytes;

                if (data is float)
                    bytes = BitConverter.GetBytes((float)data);
                else
                    bytes = BitConverter.GetBytes(((float[])data)[0]);

                Buffer.BlockCopy(bytes, 0, _buffer, offset, elementSize);
            }

            // Take care of the single copy case!
            else if (rows == 1 || (rows == 4 && columns == 4))
                Buffer.BlockCopy(data as Array, 0, _buffer, offset, rows * columns * elementSize);

            else
            {
                var source = data as Array;

                var stride = (columns * elementSize);
                for (var y = 0; y < rows; y++)
                    Buffer.BlockCopy(source, stride * y, _buffer, offset + (rowSize * y), columns * elementSize);
            }
        }

        void SetParameter(int offset, EffectParameter param)
        {
            if (param.Data != null)
            {
                int elementSize = 4;
                var rowSize = elementSize * 4;

                if (param.Elements.Count > 0)
                {
                    foreach (var subparam in param.Elements)
                    {
                        SetParameter(offset, subparam);
                        //TODO: Sometimes directx decides to transpose matricies
                        //to fit in fewer registers.
                        offset += subparam.RowCount * rowSize;
                    }
                }
                else
                {
                    switch (param.ParameterType)
                    {
                        case EffectParameterType.Single:
                            SetData(offset, param.RowCount, param.ColumnCount, param.Data);
                            break;

                        default:
                            throw new NotImplementedException("Not supported!");
                    }
                }
            }
        }

#if DIRECTX
        public void Apply(bool vertexStage, int slot, EffectParameterCollection parameters)
#elif OPENGL
        public unsafe void Apply(int program, EffectParameterCollection parameters)
#elif PSS
        public void Apply(ShaderProgram program, EffectParameterCollection parameters)
#endif
        {
            // TODO:  We should be doing some sort of dirty state 
            // testing here.
            //
            // It should let us skip all parameter updates if
            // nothing has changed.  It should not be per-parameter
            // as that is why you should use multiple constant
            // buffers.

            // If our state key becomes larger than the 
            // next state key then the keys have rolled 
            // over and we need to reset.
            if (_stateKey > EffectParameter.NextStateKey)
                _stateKey = 0;

            var dirty = false;

            for (var p = 0; p < _parameters.Length; p++)
            {
                var index = _parameters[p];
                var param = parameters[index];

                if (param.StateKey < _stateKey)
                    continue;

                var offset = _offsets[p];
                dirty = true;

                SetParameter(offset, param);
            }

            _stateKey = EffectParameter.NextStateKey;

#if DIRECTX

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.
            var d3dContext = graphicsDevice._d3dContext;

            // Update the hardware buffer.
            if (dirty)
                d3dContext.UpdateSubresource(_buffer, _cbuffer);

            // Set the constant buffer.
            if (vertexStage)
                d3dContext.VertexShader.SetConstantBuffer(slot, _cbuffer);
            else
                d3dContext.PixelShader.SetConstantBuffer(slot, _cbuffer);
#endif

#if OPENGL
            var location = GL.GetUniformLocation(program, _name);
            fixed (byte* bytePtr = _buffer)
            {
                // TODO: We need to know the type of buffer float/int/bool
                // and cast this correctly... else it doesn't work as i guess
                // GL is checking the type of the uniform.

                GL.Uniform4(location, _buffer.Length / 16, (float*)bytePtr);
            }
#endif

#if PSS
#warning Unimplemented
            //TODO
#endif
        }
    }
}
