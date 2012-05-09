using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Text;
using TwoMGFX;

namespace Microsoft.Xna.Framework.Graphics
{
	public class DXShader
	{
        private bool IsVertexShader;

        public struct Sampler
        {
            public MojoShader.MOJOSHADER_samplerType type;
            public int index;
            public string name;
            public int parameter;
        }

        public struct Attribute
        {
            public int index;
            public string name;
            public VertexElementUsage usage;
            public short format;
        }

        /// <summary>
        /// The index to the constant buffers which are 
        /// required by this shader at runtime.
        /// </summary>
        private int[] _cbuffers;

        public readonly Sampler[] _samplers;

        private readonly Attribute[] _attributes;

        public byte[] ShaderCode { get; private set; }


#region Non-Serialized Stuff

        public byte[] Bytecode { get; private set; }

        // The index of the shader in the shared list.
        public int SharedIndex { get; private set; }

        private readonly MojoShader.MOJOSHADER_symbol[] _symbols;

#endregion // Non-Serialized Stuff

        public DXShader(byte[] byteCode, SharpDX.Direct3D11.EffectShaderVariable variable, List<ConstantBuffer> cbuffers, int sharedIndex)
        {
            var shaderDesc = variable.GetShaderDescription(0);

            if (variable.TypeInfo.Description.Type != SharpDX.D3DCompiler.ShaderVariableType.Vertexshader)
                IsVertexShader = false;
            else
                IsVertexShader = true;

            SharedIndex = sharedIndex;

            // Store the original bytecode here for comparison.
            Bytecode = byteCode;

            // Strip the bytecode we're gonna save!
            const SharpDX.D3DCompiler.StripFlags stripFlags =   SharpDX.D3DCompiler.StripFlags.CompilerStripDebugInformation | 
                                                                SharpDX.D3DCompiler.StripFlags.CompilerStripReflectionData | 
                                                                SharpDX.D3DCompiler.StripFlags.CompilerStripTestBlobs;

            using (var original = new SharpDX.D3DCompiler.ShaderBytecode(byteCode))
            {
                // Strip the bytecode for saving to disk.
                using (var stripped = original.Strip(stripFlags))
                {
                    ShaderCode = new byte[stripped.BufferSize];
                    stripped.Data.Read(ShaderCode, 0, ShaderCode.Length);
                }

                // Use reflection to get details of the shader.
                using (var refelect = new SharpDX.D3DCompiler.ShaderReflection(original))
                {
                    // Get the samplers.
                    var samplers = new List<Sampler>();
                    for (var i = 0; i < refelect.Description.BoundResources; i++)
                    {
                        var rdesc = refelect.GetResourceBindingDescription(i);
                        if (rdesc.Type == SharpDX.D3DCompiler.ShaderInputType.Texture)
                        {
                            samplers.Add(new Sampler 
                            { 
                                index = rdesc.BindPoint, 
                                name = rdesc.Name,

                                // TODO: Detect the sampler type for realz.
                                type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D 
                            });
                        }
                    }
                    _samplers = samplers.ToArray();

                    // Gather all the constant buffers used by this shader.
                    _cbuffers = new int[refelect.Description.ConstantBuffers];
                    for (var i = 0; i < refelect.Description.ConstantBuffers; i++)
                    {
                        var cb = new ConstantBuffer(refelect.GetConstantBuffer(i));
                        
                        // Look for a duplicate cbuffer in the list.
                        for (var c = 0; c < cbuffers.Count; c++)
                        {
                            if (cb.SameAs(cbuffers[c]))
                            {
                                cb = null;
                                _cbuffers[i] = c;
                                break;
                            }
                        }

                        // Add a new cbuffer.
                        if (cb != null)
                        {
                            _cbuffers[i] = cbuffers.Count;
                            cbuffers.Add(cb);
                        }
                    }
                }
            }

            _symbols = new MojoShader.MOJOSHADER_symbol[0];
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


            // Convert the samplers.
            {
                var samplers = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_sampler>(
                        parseData.samplers, parseData.sampler_count);
                _samplers = new Sampler[samplers.Length];
                for (var i = 0; i < samplers.Length; i++)
                {
                    _samplers[i].name = samplers[i].name;
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

            _symbols = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_symbol>(
                    parseData.symbols, parseData.symbol_count);

            //try to put the symbols in the order they are eventually packed into the uniform arrays
            //this /should/ be done by pulling the info from mojoshader
            Array.Sort(_symbols, delegate(MojoShader.MOJOSHADER_symbol a, MojoShader.MOJOSHADER_symbol b)
            {
                uint va = a.register_index;
                if (a.info.elements == 1) va += 1024; //hax. mojoshader puts array objects first
                uint vb = b.register_index;
                if (b.info.elements == 1) vb += 1024;
                return va.CompareTo(vb);
            });//(a, b) => ((int)(a.info.elements > 1))a.register_index.CompareTo(b.register_index));


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

        public void CreateConstantBuffer(List<DXEffectObject.d3dx_parameter> parameters, List<ConstantBuffer> cbuffers)
        {
            // Build a constant buffer for each register set.
            var cbs = new List<int>();

            var cbuffer = new ConstantBuffer(   IsVertexShader ? "vs_uniforms_bool" : "ps_uniforms_bool", 
                                                MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL, 
                                                _symbols,
                                                parameters);
            if (cbuffer.Size > 0)
            {
                var match = cbuffers.FindIndex(e => e.SameAs(cbuffer));
                if (match == -1)
                {
                    cbs.Add(cbuffers.Count);
                    cbuffers.Add(cbuffer);
                }
                else
                    cbs.Add(match);
            }

            cbuffer = new ConstantBuffer(   IsVertexShader ? "vs_uniforms_ivec4" : "ps_uniforms_ivec4", 
                                            MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4,
                                            _symbols,
                                            parameters);
            if (cbuffer.Size > 0)
            {
                var match = cbuffers.FindIndex(e => e.SameAs(cbuffer));
                if (match == -1)
                {
                    cbs.Add(cbuffers.Count);
                    cbuffers.Add(cbuffer);
                }
                else
                    cbs.Add(match);
            }

            cbuffer = new ConstantBuffer(   IsVertexShader ? "vs_uniforms_vec4" : "ps_uniforms_vec4", 
                                            MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4, 
                                            _symbols,
                                            parameters);
            if (cbuffer.Size > 0)
            {
                var match = cbuffers.FindIndex(e => e.SameAs(cbuffer));
                if (match == -1)
                {
                    cbs.Add(cbuffers.Count);
                    cbuffers.Add(cbuffer);
                }
                else
                    cbs.Add(match);
            }

            _cbuffers = cbs.ToArray();
        }

        public void SetSamplerParameters(Dictionary<string, DXEffectObject.d3dx_parameter> samplers, List<DXEffectObject.d3dx_parameter> parameters)
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
                if (samplerSymbol.HasValue && samplers.TryGetValue(samplerSymbol.Value.name, out param))
                {
                    var samplerState = (DXEffectObject.d3dx_sampler)param.data;
                    if (samplerState != null && samplerState.state_count > 0)
                    {
                        var textureName = samplerState.states[0].parameter.name;
                        var index = parameters.FindIndex(e => e.name == textureName);
                        if (index != -1)
                            _samplers[i].parameter = index;
                    }
                }
            }
        }

        public void Write(BinaryWriter writer, Options options)
        {
            writer.Write(IsVertexShader);

            writer.Write((ushort)ShaderCode.Length);
            writer.Write(ShaderCode);

            writer.Write((byte)_samplers.Length);
            foreach (var sampler in _samplers)
            {
                writer.Write((byte)sampler.type);
                writer.Write((byte)sampler.index);

                if (!options.DX11Profile)
                    writer.Write(sampler.name);

                writer.Write((byte)sampler.parameter);
            }

            writer.Write((byte)_cbuffers.Length);
            foreach (var cb in _cbuffers)
                writer.Write((byte)cb);

            if (options.DX11Profile)
                return;

            // The rest of this is for GL only!

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

