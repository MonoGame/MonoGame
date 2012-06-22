
namespace Microsoft.Xna.Framework.Graphics
{
    public partial class DXShader
    {
        public static DXShader CreateHLSL(byte[] byteCode, bool isVertexShader, List<ConstantBuffer> cbuffers, int sharedIndex)
        {
            var dxshader = new DXShaderData();
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