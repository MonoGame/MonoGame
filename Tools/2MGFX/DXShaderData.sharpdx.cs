using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class DXShaderData
    {
        public static DXShaderData CreateHLSL(byte[] byteCode, bool isVertexShader, List<DXConstantBufferData> cbuffers, int sharedIndex, Dictionary<string, SamplerState> samplerStates, bool debug)
        {
            var dxshader = new DXShaderData();
            dxshader.IsVertexShader = isVertexShader;
            dxshader.SharedIndex = sharedIndex;
            dxshader.Bytecode = (byte[])byteCode.Clone();

            // Strip the bytecode we're gonna save!
            var stripFlags = SharpDX.D3DCompiler.StripFlags.CompilerStripReflectionData |
                             SharpDX.D3DCompiler.StripFlags.CompilerStripTestBlobs;

            if (!debug)
                stripFlags |= SharpDX.D3DCompiler.StripFlags.CompilerStripDebugInformation;

            using (var original = new SharpDX.D3DCompiler.ShaderBytecode(byteCode))
            {
                // Strip the bytecode for saving to disk.
                var stripped = original.Strip(stripFlags);
                {
                    // Only SM4 and above works with strip... so this can return null!
                    if (stripped != null)
                    {
                        dxshader.ShaderCode = stripped;
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
                using (var refelect = new SharpDX.D3DCompiler.ShaderReflection(byteCode))
                {
                    // Get the samplers.
                    var samplers = new List<Sampler>();
                    for (var i = 0; i < refelect.Description.BoundResources; i++)
                    {
                        var rdesc = refelect.GetResourceBindingDescription(i);
                        if (rdesc.Type == SharpDX.D3DCompiler.ShaderInputType.Texture)
                        {
							SamplerState state = null;
							samplerStates.TryGetValue(rdesc.Name, out state);
                            samplers.Add(new Sampler
                            {
                                index = rdesc.BindPoint,
                                parameterName = rdesc.Name,
								state = state,
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
                        var cb = new DXConstantBufferData(refelect.GetConstantBuffer(i));

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
    }
}