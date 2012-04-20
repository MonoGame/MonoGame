using System;
using System.Runtime.InteropServices;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	public class DXShader
	{
        public readonly bool IsVertexShader;

        private readonly DXPreshader _preshader;

        private readonly string _glslCode;

        private readonly int _uniforms_float4_count = 0;
        private readonly int _uniforms_int4_count = 0;
        private readonly int _uniforms_bool_count = 0;

        private readonly MojoShader.MOJOSHADER_symbol[] _symbols;
        private readonly MojoShader.MOJOSHADER_sampler[] _samplers;
        private readonly MojoShader.MOJOSHADER_attribute[] _attributes;
		
		public DXShader(byte[] byteCode)
		{
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
			
			_samplers = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_sampler>(
					parseData.samplers, parseData.sampler_count);
			
			_attributes = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_attribute>(
					parseData.attributes, parseData.attribute_count);

            foreach (var symbol in _symbols)
            {
                switch (symbol.register_set)
                {
                    case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
                        _uniforms_bool_count += (int)symbol.register_count;
                        break;
                    case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
                        _uniforms_float4_count += (int)symbol.register_count;
                        break;
                    case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
                        _uniforms_int4_count += (int)symbol.register_count;
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

        public void Write(BinaryWriter writer)
        {
            writer.Write(IsVertexShader);

            writer.Write(_preshader != null);
            if (_preshader != null)
                _preshader.Write(writer);

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
            foreach (var samples in _samplers)
            {
                writer.Write(samples.name);
                writer.Write((byte)samples.type);
                writer.Write((byte)samples.index);
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

