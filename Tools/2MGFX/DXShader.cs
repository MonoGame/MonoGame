using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
	public class DXShader
	{
        public static bool IsDirectX = false;

        // The index of the shader in the shared list.
        public int SharedIndex { get; private set; }

        private bool IsVertexShader;

        private readonly DXPreshader _preshader;

        private readonly string _glslCode;

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

        private struct Attribute
        {
            public VertexElementUsage usage;
            public int index;
            public string name;
        }

        private readonly MojoShader.MOJOSHADER_symbol[] _symbols;
        private readonly Sampler[] _samplers;
        private readonly Attribute[] _attributes;

        public byte[] Bytecode { get; private set; }

        public DXShader(byte[] byteCode, bool isVertexShader, int sharedIndex)
        {
            IsVertexShader = isVertexShader;
            SharedIndex = sharedIndex;
            Bytecode = byteCode;

            _symbols = new MojoShader.MOJOSHADER_symbol[0];
            _samplers = new Sampler[0];
            _attributes = new Attribute[0];
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

            if (parseData.preshader != IntPtr.Zero)
            {
                var preshader = DXHelper.Unmarshal<MojoShader.MOJOSHADER_preshader>(parseData.preshader);
                _preshader = DXPreshader.CreatePreshader(preshader);
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
		
			_glslCode = parseData.output;
			
#if GLSLOPTIMIZER
			//_glslCode = GLSLOptimizer.Optimize(_glslCode, ShaderType);
#endif

            // TODO: This sort of sucks... why does MojoShader not produce
            // code valid for GLES out of the box?

            // GLES platforms do not like this.
            _glslCode = _glslCode.Replace("#version 110\r\n", "");

            // Add the required precision specifiers for GLES.
            _glslCode = "#ifdef GL_ES\r\n" +
                        "precision highp float;\r\n" +
                        "precision mediump int;\r\n" +
                        "#endif\r\n" +
                        _glslCode;
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

            //writer.Write(_preshader != null);
            //if (_preshader != null)
                //_preshader.Write(writer);

            if (IsDirectX)
            {
                writer.Write((ushort)Bytecode.Length);
                writer.Write(Bytecode);
            }
            else
                writer.Write(_glslCode);

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
            }            
        }
	}
}

