using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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
            public string samplerName;
            public string parameterName;
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

        public Sampler[] _samplers;

        private Attribute[] _attributes;

        public byte[] ShaderCode { get; private set; }


#region Non-Serialized Stuff

        public byte[] Bytecode { get; private set; }

        // The index of the shader in the shared list.
        public int SharedIndex { get; private set; }

        private MojoShader.MOJOSHADER_symbol[] _symbols;

#endregion // Non-Serialized Stuff

        public static DXShader CreateHLSL(byte[] byteCode, bool isVertexShader, List<ConstantBuffer> cbuffers, int sharedIndex)
        {
            var dxshader = new DXShader();
            dxshader.IsVertexShader = isVertexShader;
            dxshader.SharedIndex = sharedIndex;
            dxshader.Bytecode = (byte[])byteCode.Clone();

            // Strip the bytecode we're gonna save!
            const SharpDX.D3DCompiler.StripFlags stripFlags =   SharpDX.D3DCompiler.StripFlags.CompilerStripDebugInformation |
                                                                SharpDX.D3DCompiler.StripFlags.CompilerStripReflectionData |
                                                                SharpDX.D3DCompiler.StripFlags.CompilerStripTestBlobs;

            using (var original = new SharpDX.D3DCompiler.ShaderBytecode(byteCode))
            {
                // Strip the bytecode for saving to disk.
                using (var stripped = original.Strip(stripFlags))
                {
                    // Only SM4 and above works with strip... so this can return null!
                    if (stripped != null)
                    {
                        var shaderCode = new byte[stripped.BufferSize];
                        stripped.Data.Read(shaderCode, 0, shaderCode.Length);
                        dxshader.ShaderCode = shaderCode;
                    }
                    else
                    {
                        // TODO: There is a way to strip SM3 and below
                        // but we have to write the method ourselves.
                        // 
                        // If we need to support it then consider porting
                        // this code over...
                        //
                        // http://entland.homelinux.com/blog/2009/01/15/stripping-comments-from-shader-bytecodes/
                        //
                        dxshader.ShaderCode = (byte[])dxshader.Bytecode.Clone();
                    }
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
                                parameterName = rdesc.Name,

                                // TODO: Detect the sampler type for realz.
                                type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D
                            });
                        }
                    }
                    dxshader._samplers = samplers.ToArray();

                    // Gather all the constant buffers used by this shader.
                    dxshader._cbuffers = new int[refelect.Description.ConstantBuffers];
                    for (var i = 0; i < refelect.Description.ConstantBuffers; i++)
                    {
                        var cb = new ConstantBuffer(refelect.GetConstantBuffer(i));

                        // Look for a duplicate cbuffer in the list.
                        for (var c = 0; c < cbuffers.Count; c++)
                        {
                            if (cb.SameAs(cbuffers[c]))
                            {
                                cb = null;
                                dxshader._cbuffers[i] = c;
                                break;
                            }
                        }

                        // Add a new cbuffer.
                        if (cb != null)
                        {
                            dxshader._cbuffers[i] = cbuffers.Count;
                            cbuffers.Add(cb);
                        }
                    }
                }
            }

            return dxshader;
        }

        public static DXShader CreateGLSL(byte[] byteCode, bool isVertexShader, List<ConstantBuffer> cbuffers, int sharedIndex)
        {
            var dxshader = new DXShader();
            dxshader.IsVertexShader = isVertexShader;
            dxshader.SharedIndex = sharedIndex;
            dxshader.Bytecode = (byte[])byteCode.Clone();

            // Use MojoShader to convert the HLSL bytecode to GLSL.

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

            // Conver the attributes.
            //
            // TODO: Could this be done using DX shader reflection?
            //
            {
                var attributes = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_attribute>(
                        parseData.attributes, parseData.attribute_count);

                dxshader._attributes = new Attribute[attributes.Length];
                for (var i = 0; i < attributes.Length; i++)
                {
                    dxshader._attributes[i].name = attributes[i].name;
                    dxshader._attributes[i].index = attributes[i].index;
                    dxshader._attributes[i].usage = DXEffectObject.ToVertexElementUsage(attributes[i].usage);
                }
            }

            var symbols = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_symbol>(
                    parseData.symbols, parseData.symbol_count);

            //try to put the symbols in the order they are eventually packed into the uniform arrays
            //this /should/ be done by pulling the info from mojoshader
            Array.Sort(symbols, delegate(MojoShader.MOJOSHADER_symbol a, MojoShader.MOJOSHADER_symbol b)
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

                for (var i = 0; i < symbols.Length; i++)
                {
                    switch (symbols[i].register_set)
                    {
                        case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
                            symbols[i].register_index = bool_index;
                            bool_index += symbols[i].register_count;
                            break;

                        case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
                            symbols[i].register_index = float4_index;
                            float4_index += symbols[i].register_count;
                            break;

                        case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
                            symbols[i].register_index = int4_index;
                            int4_index += symbols[i].register_count;
                            break;
                    }
                }
            }

            // Get the samplers.
            var samplers = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_sampler>(
                    parseData.samplers, parseData.sampler_count);
            dxshader._samplers = new Sampler[samplers.Length];
            for (var i = 0; i < samplers.Length; i++)
            {
                // GLSL needs the sampler name.
                dxshader._samplers[i].samplerName = samplers[i].name;

                // We need the parameter name for creating the parameter
                // listing for the effect... look for that in the symbols.
                dxshader._samplers[i].parameterName =
                    symbols.First(e =>  e.register_set == MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER &&
                                        e.register_index == samplers[i].index).name;

                // Set the rest of the sampler info.
                dxshader._samplers[i].type = samplers[i].type;
                dxshader._samplers[i].index = samplers[i].index;                    
            }

            // Gather all the parameters used by this shader.
            var symbol_types = new [] { 
                new { name = isVertexShader ? "vs_uniforms_bool" : "ps_uniforms_bool", set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL, },
                new { name = isVertexShader ? "vs_uniforms_ivec4" : "ps_uniforms_ivec4", set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4, },
                new { name = isVertexShader ? "vs_uniforms_vec4" : "ps_uniforms_vec4", set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4, },
            };

            var cbuffer_index = new List<int>();
            for (var i = 0; i < symbol_types.Length; i++)
            {
                var cbuffer = new ConstantBuffer(   symbol_types[i].name,
                                                    symbol_types[i].set,
                                                    symbols);
                if (cbuffer.Size == 0)
                    continue;

                var match = cbuffers.FindIndex(e => e.SameAs(cbuffer));
                if (match == -1)
                {
                    cbuffer_index.Add(cbuffers.Count);
                    cbuffers.Add(cbuffer);
                }
                else
                    cbuffer_index.Add(match);
            }
            dxshader._cbuffers = cbuffer_index.ToArray();

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
            dxshader.ShaderCode = Encoding.ASCII.GetBytes(glslCode);

            return dxshader;
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
                    writer.Write(sampler.samplerName);

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

