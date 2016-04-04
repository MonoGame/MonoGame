// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class BinaryEffectReader : IEffectReader
    {
        private int _effectKey;
        private BinaryConstantBuffer[] _binaryConstantBuffers;
        private BinaryShaderReader[] _shaders;
        private BinaryParameter[] _parameters;
        private BinaryTechnique[] _techniques;

        #region Internal File Reading and State

        struct MGFXHeader
        {
            /// <summary>
            /// The MonoGame Effect file format header identifier ("MGFX"). 
            /// </summary>
            public static readonly int MGFXSignature = (BitConverter.IsLittleEndian) ? 0x5846474D : 0x4D474658;

            /// <summary>
            /// The current MonoGame Effect file format versions
            /// used to detect old packaged content.
            /// </summary>
            /// <remarks>
            /// We should avoid supporting old versions for very long if at all 
            /// as users should be rebuilding content when packaging their game.
            /// </remarks>
            public const int MGFXVersion = 8;

            public int Signature;
            public int Version;
            public int Profile;
            public int EffectKey;
            public int HeaderSize;
        }

        private struct BinaryConstantBuffer
        {
            public int SizeInBytes { get; set; }
            public int[] Parameters { get; set; }
            public int[] Offsets { get; set; }
            public string Name { get; set; }
        }

        private struct BinaryParameter
        {
            public EffectParameterClass Class { get; set; }
            public EffectParameterType Type { get; set; }
            public string Name { get; set; }
            public int RowCount { get; set; }
            public int ColumnCount { get; set; }
            public string Semantic { get; set; }
            public BinaryAnnotation[] Annotations { get; set; }
            public BinaryParameter[] Elements { get; set; }
            public BinaryParameter[] StructMembers { get; set; }
            public object Data { get; set; }
        }

        private struct BinaryTechnique
        {
            public string Name { get; set; }
            public BinaryAnnotation[] Annotations { get; set; }
            public BinaryPass[] Passes { get; set; }
        }

        private struct BinaryPass
        {
            public string Name { get; set; }
            public int? VertexShaderIndex { get; set; }
            public int? PixelShaderIndex { get; set; }
            public BlendState BlendState { get; set; }
            public DepthStencilState DepthStencilState { get; set; }
            public RasterizerState RasterizerState { get; set; }
            public BinaryAnnotation[] Annotations { get; set; }
        }

        private struct BinaryAnnotation
        {
            
        }

        public BinaryEffectReader(byte[] effectCode, int index, int count)
        {
            //Read the header
            MGFXHeader header = ReadHeader(effectCode, index);
            _effectKey = header.EffectKey;
            int headerSize = header.HeaderSize;
            
            using (var stream = new MemoryStream(effectCode, index + headerSize, count - headerSize, false))
            using (var reader = new BinaryReader(stream))
            {
                // Read in all the constant buffers.
                var buffers = (int)reader.ReadByte();
                _binaryConstantBuffers = new BinaryConstantBuffer[buffers];
                for (var c = 0; c < buffers; c++)
                {
                    string name = reader.ReadString();

                    // Create the backing system memory buffer.
                    var sizeInBytes = (int)reader.ReadInt16();

                    // Read the parameter index values.
                    var parameters = new int[reader.ReadByte()];
                    var offsets = new int[parameters.Length];
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        parameters[i] = (int)reader.ReadByte();
                        offsets[i] = (int)reader.ReadUInt16();
                    }

                    _binaryConstantBuffers[c].SizeInBytes = sizeInBytes;
                    _binaryConstantBuffers[c].Parameters = parameters;
                    _binaryConstantBuffers[c].Offsets = offsets;
                    _binaryConstantBuffers[c].Name = name;
                }

                // Read in all the shader objects.
                var shaders = (int)reader.ReadByte();
                _shaders = new BinaryShaderReader[shaders];
                for (var s = 0; s < shaders; s++)
                    _shaders[s] = new BinaryShaderReader(reader);

                // Read in the parameters.
                _parameters = ReadParameters(reader);

                // Read the techniques.
                var techniqueCount = (int)reader.ReadByte();
                _techniques = new BinaryTechnique[techniqueCount];
                for (var t = 0; t < techniqueCount; t++)
                {
                    var name = reader.ReadString();

                    var annotations = ReadAnnotations(reader);

                    var passes = ReadPasses(reader);

                    _techniques[t] = new BinaryTechnique
                    {
                        Name = name,
                        Annotations = annotations,
                        Passes = passes
                    };
                }
            }
        }

        private MGFXHeader ReadHeader(byte[] effectCode, int index)
        {
            MGFXHeader header;
            header.Signature = BitConverter.ToInt32(effectCode, index); index += 4;
            header.Version = (int)effectCode[index++];
            header.Profile = (int)effectCode[index++];
            header.EffectKey = BitConverter.ToInt32(effectCode, index); index += 4;
            header.HeaderSize = index;

            if (header.Signature != MGFXHeader.MGFXSignature)
                throw new Exception("This does not appear to be a MonoGame MGFX file!");
            if (header.Version < MGFXHeader.MGFXVersion)
                throw new Exception("This MGFX effect is for an older release of MonoGame and needs to be rebuilt.");
            if (header.Version > MGFXHeader.MGFXVersion)
                throw new Exception("This MGFX effect seems to be for a newer release of MonoGame.");
            
            if (header.Profile != Shader.Profile)
                throw new Exception("This MGFX effect was built for a different platform!");

            return header;
        }

        private static BinaryAnnotation[] ReadAnnotations(BinaryReader reader)
        {
            var count = (int)reader.ReadByte();
            if (count == 0)
                return new BinaryAnnotation[0];

            var annotations = new BinaryAnnotation[count];

            // TODO: Annotations are not implemented!

            return annotations;
        }

        private static BinaryPass[] ReadPasses(BinaryReader reader)
        {
            var count = (int)reader.ReadByte();
            var passes = new BinaryPass[count];

            for (var i = 0; i < count; i++)
            {
                var name = reader.ReadString();
                var annotations = ReadAnnotations(reader);

                // Get the vertex shader.
                int? vertexShader = null;
                var shaderIndex = (int)reader.ReadByte();
                if (shaderIndex != 255)
                    vertexShader = shaderIndex;

                // Get the pixel shader.
                int? pixelShader = null;
                shaderIndex = (int)reader.ReadByte();
                if (shaderIndex != 255)
                    pixelShader = shaderIndex;

                BlendState blend = null;
                DepthStencilState depth = null;
                RasterizerState raster = null;
                if (reader.ReadBoolean())
                {
                    blend = new BlendState
                    {
                        AlphaBlendFunction = (BlendFunction)reader.ReadByte(),
                        AlphaDestinationBlend = (Blend)reader.ReadByte(),
                        AlphaSourceBlend = (Blend)reader.ReadByte(),
                        BlendFactor = new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
                        ColorBlendFunction = (BlendFunction)reader.ReadByte(),
                        ColorDestinationBlend = (Blend)reader.ReadByte(),
                        ColorSourceBlend = (Blend)reader.ReadByte(),
                        ColorWriteChannels = (ColorWriteChannels)reader.ReadByte(),
                        ColorWriteChannels1 = (ColorWriteChannels)reader.ReadByte(),
                        ColorWriteChannels2 = (ColorWriteChannels)reader.ReadByte(),
                        ColorWriteChannels3 = (ColorWriteChannels)reader.ReadByte(),
                        MultiSampleMask = reader.ReadInt32(),
                    };
                }
                if (reader.ReadBoolean())
                {
                    depth = new DepthStencilState
                    {
                        CounterClockwiseStencilDepthBufferFail = (StencilOperation)reader.ReadByte(),
                        CounterClockwiseStencilFail = (StencilOperation)reader.ReadByte(),
                        CounterClockwiseStencilFunction = (CompareFunction)reader.ReadByte(),
                        CounterClockwiseStencilPass = (StencilOperation)reader.ReadByte(),
                        DepthBufferEnable = reader.ReadBoolean(),
                        DepthBufferFunction = (CompareFunction)reader.ReadByte(),
                        DepthBufferWriteEnable = reader.ReadBoolean(),
                        ReferenceStencil = reader.ReadInt32(),
                        StencilDepthBufferFail = (StencilOperation)reader.ReadByte(),
                        StencilEnable = reader.ReadBoolean(),
                        StencilFail = (StencilOperation)reader.ReadByte(),
                        StencilFunction = (CompareFunction)reader.ReadByte(),
                        StencilMask = reader.ReadInt32(),
                        StencilPass = (StencilOperation)reader.ReadByte(),
                        StencilWriteMask = reader.ReadInt32(),
                        TwoSidedStencilMode = reader.ReadBoolean(),
                    };
                }
                if (reader.ReadBoolean())
                {
                    raster = new RasterizerState
                    {
                        CullMode = (CullMode)reader.ReadByte(),
                        DepthBias = reader.ReadSingle(),
                        FillMode = (FillMode)reader.ReadByte(),
                        MultiSampleAntiAlias = reader.ReadBoolean(),
                        ScissorTestEnable = reader.ReadBoolean(),
                        SlopeScaleDepthBias = reader.ReadSingle(),
                    };
                }

                passes[i] = new BinaryPass
                {
                    Name = name,
                    VertexShaderIndex = vertexShader,
                    PixelShaderIndex = pixelShader,
                    BlendState = blend,
                    DepthStencilState = depth,
                    RasterizerState = raster,
                    Annotations = annotations,
                };
            }

            return passes;
        }

        private static BinaryParameter[] ReadParameters(BinaryReader reader)
        {
            var count = (int)reader.ReadByte();
            if (count == 0)
                return new BinaryParameter[0];

            var parameters = new BinaryParameter[count];
            for (var i = 0; i < count; i++)
            {
                var class_ = (EffectParameterClass)reader.ReadByte();
                var type = (EffectParameterType)reader.ReadByte();
                var name = reader.ReadString();
                var semantic = reader.ReadString();
                var annotations = ReadAnnotations(reader);
                var rowCount = (int)reader.ReadByte();
                var columnCount = (int)reader.ReadByte();

                var elements = ReadParameters(reader);
                var structMembers = ReadParameters(reader);

                object data = null;
                if (elements.Length == 0 && structMembers.Length == 0)
                {
                    switch (type)
                    {
                        case EffectParameterType.Bool:
                        case EffectParameterType.Int32:
#if DIRECTX
                            // Under DirectX we properly store integers and booleans
                            // in an integer type.
                            //
                            // MojoShader on the otherhand stores everything in float
                            // types which is why this code is disabled under OpenGL.
					        {
					            var buffer = new int[rowCount * columnCount];								
                                for (var j = 0; j < buffer.Length; j++)
                                    buffer[j] = reader.ReadInt32();
                                data = buffer;
                                break;
					        }
#endif

                        case EffectParameterType.Single:
                            {
                                var buffer = new float[rowCount * columnCount];
                                for (var j = 0; j < buffer.Length; j++)
                                    buffer[j] = reader.ReadSingle();
                                data = buffer;
                                break;
                            }

                        case EffectParameterType.String:
                            // TODO: We have not investigated what a string
                            // type should do in the parameter list.  Till then
                            // throw to let the user know.
                            throw new NotSupportedException();

                        default:
                            // NOTE: We skip over all other types as they 
                            // don't get added to the constant buffer.
                            break;
                    }
                }

                parameters[i] = new BinaryParameter
                {
                    Class = class_,
                    Type = type,
                    Name = name,
                    RowCount = rowCount,
                    ColumnCount = columnCount,
                    Semantic = semantic,
                    Annotations = annotations,
                    Elements = elements,
                    StructMembers = structMembers,
                    Data = data
                };
            }

            return parameters;
        }

        #endregion

        public int GetEffectKey()
        {
            return _effectKey;
        }

        public int GetConstantBufferCount()
        {
            return _binaryConstantBuffers.Length;
        }

        public string GetConstantBufferName(int constantBufferIndex)
        {
            return _binaryConstantBuffers[constantBufferIndex].Name;
        }

        public int GetConstantBufferSize(int constantBufferIndex)
        {
            return _binaryConstantBuffers[constantBufferIndex].SizeInBytes;
        }

        public int GetConstantBufferParameterCount(int constantBufferIndex)
        {
            return _binaryConstantBuffers[constantBufferIndex].Parameters.Length;
        }

        public int GetConstantBufferParameterValue(int constantBufferIndex, int parameterIndex)
        {
            return _binaryConstantBuffers[constantBufferIndex].Parameters[parameterIndex];
        }

        public int GetConstantBufferParameterOffset(int constantBufferIndex, int parameterIndex)
        {
            return _binaryConstantBuffers[constantBufferIndex].Offsets[parameterIndex];
        }

        public int GetShaderCount()
        {
            return _shaders.Length;
        }

        public IShaderReader GetShaderReader(int shaderIndex)
        {
            return _shaders[shaderIndex];
        }

        public int GetParameterCount(object parameterContext)
        {
            var parameters = parameterContext as BinaryParameter[] ?? _parameters;
            return parameters.Length;
        }

        public EffectParameterClass GetParameterClass(object parameterContext, int parameterIndex)
        {
            var parameters = parameterContext as BinaryParameter[] ?? _parameters;
            return parameters[parameterIndex].Class;
        }

        public EffectParameterType GetParameterType(object parameterContext, int parameterIndex)
        {
            var parameters = parameterContext as BinaryParameter[] ?? _parameters;
            return parameters[parameterIndex].Type;
        }

        public string GetParameterName(object parameterContext, int parameterIndex)
        {
            var parameters = parameterContext as BinaryParameter[] ?? _parameters;
            return parameters[parameterIndex].Name;
        }

        public string GetParameterSemantic(object parameterContext, int parameterIndex)
        {
            var parameters = parameterContext as BinaryParameter[] ?? _parameters;
            return parameters[parameterIndex].Semantic;
        }

        public int GetParameterAnnotationCount(object parameterContext, int parameterIndex)
        {
            var parameters = parameterContext as BinaryParameter[] ?? _parameters;
            return parameters[parameterIndex].Annotations.Length;
        }

        public int GetParameterRowCount(object parameterContext, int parameterIndex)
        {
            var parameters = parameterContext as BinaryParameter[] ?? _parameters;
            return parameters[parameterIndex].RowCount;
        }

        public int GetParameterColumnCount(object parameterContext, int parameterIndex)
        {
            var parameters = parameterContext as BinaryParameter[] ?? _parameters;
            return parameters[parameterIndex].ColumnCount;
        }

        public int GetParameterInt32Buffer(object parameterContext, int parameterIndex, int bufferIndex)
        {
            var parameters = parameterContext as BinaryParameter[] ?? _parameters;
            return ((int[]) parameters[parameterIndex].Data)[bufferIndex];
        }

        public float GetParameterFloatBuffer(object parameterContext, int parameterIndex, int bufferIndex)
        {
            var parameters = parameterContext as BinaryParameter[] ?? _parameters;
            return ((float[]) parameters[parameterIndex].Data)[bufferIndex];
        }

        public object GetParameterElementsContext(object parameterContext, int parameterIndex)
        {
            var parameters = parameterContext as BinaryParameter[] ?? _parameters;
            return parameters[parameterIndex].Elements;
        }

        public object GetParameterStructMembersContext(object parameterContext, int parameterIndex)
        {
            var parameters = parameterContext as BinaryParameter[] ?? _parameters;
            return parameters[parameterIndex].StructMembers;
        }

        public int GetTechniqueCount()
        {
            return _techniques.Length;
        }

        public string GetTechniqueName(int techniqueIndex)
        {
            return _techniques[techniqueIndex].Name;
        }

        public int GetTechniqueAnnotationCount(int techniqueIndex)
        {
            return _techniques[techniqueIndex].Annotations.Length;
        }

        public int GetPassCount(int techniqueIndex)
        {
            return _techniques[techniqueIndex].Passes.Length;
        }

        public string GetPassName(int techniqueIndex, int passIndex)
        {
            return _techniques[techniqueIndex].Passes[passIndex].Name;
        }

        public int GetPassAnnotationCount(int techniqueIndex, int passIndex)
        {
            return _techniques[techniqueIndex].Passes[passIndex].Annotations.Length;
        }

        public int? GetPassVertexShaderIndex(int techniqueIndex, int passIndex)
        {
            return _techniques[techniqueIndex].Passes[passIndex].VertexShaderIndex;
        }

        public int? GetPassPixelShaderIndex(int techniqueIndex, int passIndex)
        {
            return _techniques[techniqueIndex].Passes[passIndex].PixelShaderIndex;
        }

        public BlendState GetPassBlendState(int techniqueIndex, int passIndex)
        {
            return _techniques[techniqueIndex].Passes[passIndex].BlendState;
        }

        public DepthStencilState GetPassDepthStencilState(int techniqueIndex, int passIndex)
        {
            return _techniques[techniqueIndex].Passes[passIndex].DepthStencilState;
        }

        public RasterizerState GetPassRasterizerState(int techniqueIndex, int passIndex)
        {
            return _techniques[techniqueIndex].Passes[passIndex].RasterizerState;
        }
    }
}
