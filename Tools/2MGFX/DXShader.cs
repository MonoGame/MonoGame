using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
	public class DXShader
	{
        public static bool IsDirectX = false;

        // The index of the shader in the shared list.
        public int SharedIndex { get; private set; }

        private bool IsVertexShader;

        private readonly int _uniforms_float4_count = 0;
        private readonly int _uniforms_int4_count = 0;
        private readonly int _uniforms_bool_count = 0;

        private struct Sampler
        {
            public MojoShader.MOJOSHADER_samplerType type;
            public int index;
            public string name;
            public string parameter;
        }

        public struct Attribute
        {
            public int index;
            public string name;
            public VertexElementUsage usage;
            public short format;
        }

        private readonly MojoShader.MOJOSHADER_symbol[] _symbols;
        private readonly Sampler[] _samplers;
        private readonly Attribute[] _attributes;

        public byte[] Bytecode { get; private set; }

        public byte[] ShaderCode { get; private set; }

        public DXShader(byte[] byteCode, SharpDX.Direct3D11.EffectShaderVariable variable, int sharedIndex)
        {
            var shaderDesc = variable.GetShaderDescription(0);
            var shaderVar = variable.AsShader();

            if (variable.TypeInfo.Description.Type != SharpDX.D3DCompiler.ShaderVariableType.Vertexshader)
            {
                IsVertexShader = false;
                _attributes = new Attribute[0];
            }
            else
            {
                IsVertexShader = true;

                var componentX = SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX;
                var componentXY = SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX |
                                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentY;
                var componentXYZ = SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX |
                                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentY |
                                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentZ;
                var componentXYZW = SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX |
                                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentY |
                                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentZ |
                                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentW;

                _attributes = new DXShader.Attribute[shaderDesc.InputParameterCount];
                var offset = 0;
                for (var i = 0; i < _attributes.Length; i++)
                {
                    var element = shaderVar.GetInputSignatureElementDescription(0, i);

                    _attributes[i].name = element.SemanticName;
                    _attributes[i].index = offset;
                    //_attributes[i].usage = ???

                    var isX = (element.UsageMask & componentX) == componentX;
                    var isXY = (element.UsageMask & componentXY) == componentXY;
                    var isXYZ = (element.UsageMask & componentXYZ) == componentXYZ;
                    var isXYZW = (element.UsageMask & componentXYZW) == componentXYZW;

                    // Increment the offset.
                    offset += isXYZW ? 4 : isXYZ ? 3 : isXY ? 2 : 1;

                    SharpDX.DXGI.Format format;
                    switch (element.ComponentType)
                    {
                        case SharpDX.D3DCompiler.RegisterComponentType.Float32:
                            if (isXYZW)
                                format = SharpDX.DXGI.Format.R32G32B32A32_Float;
                            else if (isXYZ)
                                format = SharpDX.DXGI.Format.R32G32B32_Float;
                            else if (isXY)
                                format = SharpDX.DXGI.Format.R32G32_Float;
                            else if (isX)
                                format = SharpDX.DXGI.Format.R32_Float;
                            else
                                throw new NotImplementedException("Got unknown vertex shader input!");
                            break;

                        case SharpDX.D3DCompiler.RegisterComponentType.Sint32:
                            if (isXYZW)
                                format = SharpDX.DXGI.Format.R32G32B32A32_SInt;
                            else if (isXYZ)
                                format = SharpDX.DXGI.Format.R32G32B32_SInt;
                            else if (isXY)
                                format = SharpDX.DXGI.Format.R32G32_SInt;
                            else if (isX)
                                format = SharpDX.DXGI.Format.R32_SInt;
                            else
                                throw new NotImplementedException("Got unknown vertex shader input!");
                            break;

                        case SharpDX.D3DCompiler.RegisterComponentType.Uint32:
                            if (isXYZW)
                                format = SharpDX.DXGI.Format.R32G32B32A32_UInt;
                            else if (isXYZ)
                                format = SharpDX.DXGI.Format.R32G32B32_UInt;
                            else if (isXY)
                                format = SharpDX.DXGI.Format.R32G32_UInt;
                            else if (isX)
                                format = SharpDX.DXGI.Format.R32_UInt;
                            else
                                throw new NotImplementedException("Got unknown vertex shader input!");
                            break;

                        default:
                            throw new NotImplementedException("Got unknown vertex shader input!");
                    }

                    _attributes[i].format = (short)format;
                }
            }
            
            SharedIndex = sharedIndex;
            Bytecode = byteCode;
            ShaderCode = byteCode;

            _symbols = new MojoShader.MOJOSHADER_symbol[0];
            _samplers = new Sampler[0];
        }

        public DXShader(byte[] byteCode, int sharedIndex)
		{
            SharedIndex = sharedIndex;
            Bytecode = byteCode;

			var parseDataPtr = MojoShader.NativeMethods.MOJOSHADER_parse(
					"glsl",
                    byteCode,
                    byteCode.Length,
					IntPtr.Zero,
					0,
					IntPtr.Zero,
					IntPtr.Zero,
					IntPtr.Zero);

            var parseData = DXHelper.Unmarshal<MojoShader.MOJOSHADER_parseData>(parseDataPtr);			
			if (parseData.error_count > 0) 
            {
				var errors = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_error>(parseData.errors, parseData.error_count);
				throw new Exception(errors[0].error);
			}

            switch (parseData.shader_type)
            {
                case MojoShader.MOJOSHADER_shaderType.MOJOSHADER_TYPE_PIXEL:
                    IsVertexShader = false;
                    break;

                case MojoShader.MOJOSHADER_shaderType.MOJOSHADER_TYPE_VERTEX:
                    IsVertexShader = true;
                    break;

                default:
                    throw new NotSupportedException();
            }

			_symbols = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_symbol>(
					parseData.symbols, parseData.symbol_count);
			
			//try to put the symbols in the order they are eventually packed into the uniform arrays
			//this /should/ be done by pulling the info from mojoshader
			Array.Sort (_symbols, delegate (MojoShader.MOJOSHADER_symbol a, MojoShader.MOJOSHADER_symbol b) 
            {
				uint va = a.register_index;
				if (a.info.elements == 1) va += 1024; //hax. mojoshader puts array objects first
				uint vb = b.register_index;
				if (b.info.elements == 1) vb += 1024;
				return va.CompareTo(vb);
			});//(a, b) => ((int)(a.info.elements > 1))a.register_index.CompareTo(b.register_index));
			
            // Convert the attributes.
            {
			    var samplers = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_sampler>(
					    parseData.samplers, parseData.sampler_count);
                _samplers = new Sampler[samplers.Length];
                for (var i=0; i < samplers.Length; i++)
                {
                    _samplers[i].name = samplers[i].name;
                    _samplers[i].parameter = string.Empty;
                    _samplers[i].type = samplers[i].type;
                    _samplers[i].index = samplers[i].index;
                }
            }

            // Conver the attributes.
            {
			    var attributes = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_attribute>(
				    	parseData.attributes, parseData.attribute_count);

                _attributes = new Attribute[attributes.Length];
                for (var i = 0; i < attributes.Length; i++)
                {
                    _attributes[i].name = attributes[i].name;
                    _attributes[i].index = attributes[i].index;
                    _attributes[i].usage = DXEffectObject.ToVertexElementUsage(attributes[i].usage);
                }
            }

            // For whatever reason the register indexing is 
            // incorrect from MojoShader.
            {
                uint bool_index = 0;
                uint float4_index = 0;
                uint int4_index = 0;

                for (var i = 0; i < _symbols.Length; i++)
                {
                    switch (_symbols[i].register_set)
                    {
                        case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
                            _symbols[i].register_index = bool_index;
                            bool_index += _symbols[i].register_count;
                            break;

                        case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
                            _symbols[i].register_index = float4_index;
                            float4_index += _symbols[i].register_count;
                            break;

                        case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
                            _symbols[i].register_index = int4_index;
                            int4_index += _symbols[i].register_count;
                            break;
                    }
                }
            }

            foreach (var symbol in _symbols)
            {
                switch (symbol.register_set)
                {
                    case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
                        _uniforms_bool_count = Math.Max(_uniforms_bool_count, (int)(symbol.register_index + symbol.register_count) );
                        break;
                    case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
                        _uniforms_float4_count = Math.Max(_uniforms_float4_count, (int)(symbol.register_index + symbol.register_count) );
                        break;
                    case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
                        _uniforms_int4_count = Math.Max(_uniforms_int4_count, (int)(symbol.register_index + symbol.register_count));
                        break;
                    default:
                        break;
                }
            }
		
			var glslCode = parseData.output;
			
#if GLSLOPTIMIZER
			//glslCode = GLSLOptimizer.Optimize(glslCode, ShaderType);
#endif

            // TODO: This sort of sucks... why does MojoShader not produce
            // code valid for GLES out of the box?

            // GLES platforms do not like this.
            glslCode = glslCode.Replace("#version 110\r\n", "");

            // Add the required precision specifiers for GLES.
            glslCode = "#ifdef GL_ES\r\n" +
                        "precision highp float;\r\n" +
                        "precision mediump int;\r\n" +
                        "#endif\r\n" +
                        glslCode;

            // Store the code for serialization.
            ShaderCode = Encoding.ASCII.GetBytes(glslCode);
		}

        public void SetSamplerParameters(Dictionary<string, DXEffectObject.d3dx_parameter> parameters)
        {
            for (var i = 0; i < _samplers.Length; i++ )
            {
                MojoShader.MOJOSHADER_symbol? samplerSymbol = null;
                foreach (var symbol in _symbols)
                {
                    if (symbol.register_set ==
                            MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER &&
                        symbol.register_index == _samplers[i].index)
                    {
                        samplerSymbol = symbol;
                        break;
                    }
                }

                DXEffectObject.d3dx_parameter param;
                if (samplerSymbol.HasValue && parameters.TryGetValue(samplerSymbol.Value.name, out param))
                {
                    var samplerState = (DXEffectObject.d3dx_sampler)param.data;
                    if (samplerState != null && samplerState.state_count > 0)
                    {
                        var textureName = samplerState.states[0].parameter.name;
                        _samplers[i].parameter = textureName;
                    }
                }
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(IsVertexShader);

            writer.Write((ushort)ShaderCode.Length);
            writer.Write(ShaderCode);

            writer.Write((byte)_uniforms_bool_count);
            writer.Write((byte)_uniforms_int4_count);
            writer.Write((byte)_uniforms_float4_count);

            writer.Write((byte)_symbols.Length);
            foreach (var symbol in _symbols)
            {
                writer.Write(symbol.name);
                writer.Write((byte)symbol.register_set);
                writer.Write((byte)symbol.register_index);
                writer.Write((byte)symbol.register_count);
            }

            writer.Write((byte)_samplers.Length);
            foreach (var sampler in _samplers)
            {
                writer.Write(sampler.name);
                writer.Write(sampler.parameter);
                writer.Write((byte)sampler.type);
                writer.Write((byte)sampler.index);
            }

            writer.Write((byte)_attributes.Length);
            foreach (var attrib in _attributes)
            {
                writer.Write(attrib.name);
                writer.Write((byte)attrib.usage);
                writer.Write((byte)attrib.index);
                writer.Write(attrib.format);
            }
        }
	}
}

