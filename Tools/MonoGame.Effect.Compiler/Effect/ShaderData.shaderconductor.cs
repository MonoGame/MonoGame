using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using MonoGame.Effect.TPGParser;

namespace MonoGame.Effect
{
	internal partial class ShaderData
	{
        public static ShaderData CreateGLSL(string sourceCode, string shaderFunction, ShaderStage shaderStage, int shaderModelMajor, int shaderModelMinor, List<ConstantBufferData> cbuffers, int sharedIndex, Dictionary<string, SamplerStateInfo> samplerStates, bool debug)
        {
            var shaderData = new ShaderData(shaderStage, sharedIndex, new byte[0]);

            var sourceDesc = new ShaderConductor.SourceDesc()
            {
                entryPoint = shaderFunction,
                source = sourceCode,
            };

            switch (shaderStage)
            {
                case ShaderStage.VertexShader:
                    sourceDesc.stage = ShaderConductor.ShaderStage.VertexShader;
                    break;
                case ShaderStage.PixelShader:
                    sourceDesc.stage = ShaderConductor.ShaderStage.PixelShader;
                    break;
                case ShaderStage.HullShader:
                    sourceDesc.stage = ShaderConductor.ShaderStage.HullShader;
                    break;
                case ShaderStage.DomainShader:
                    sourceDesc.stage = ShaderConductor.ShaderStage.DomainShader;
                    break;
                case ShaderStage.GeometryShader:
                    sourceDesc.stage = ShaderConductor.ShaderStage.GeometryShader;
                    break;
            }

            var options = ShaderConductor.OptionsDesc.Default;
            options.shaderModel = new ShaderConductor.ShaderModel(4, 0);  
            options.enableDebugInfo = true;

            var target = new ShaderConductor.TargetDesc
            {
                language = ShaderConductor.ShadingLanguage.Glsl,
                version = "330",
                asModule = false,
            };

            // change working directory otherwise ShaderConductor can't load dlls
            string saveWorkingDir = System.IO.Directory.GetCurrentDirectory();
            string newWorkingDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            System.IO.Directory.SetCurrentDirectory(newWorkingDir);

            ShaderConductor.Compile(ref sourceDesc, ref options, ref target, out ShaderConductor.ResultDesc result);

            // restore previous working directory
            System.IO.Directory.SetCurrentDirectory(saveWorkingDir);

            if (result.hasError)
            {
                IntPtr errorBlob = ShaderConductor.GetShaderConductorBlobData(result.errorWarningMsg);
                int errorBlobSize = ShaderConductor.GetShaderConductorBlobSize(result.errorWarningMsg);
                string errorMsg = Marshal.PtrToStringAnsi(errorBlob, errorBlobSize);
                throw new Exception(errorMsg);
            }
            else
            {
                if (!result.isText)
                    throw new Exception("ShaderConductor result is not text");

                var targetBlob = ShaderConductor.GetShaderConductorBlobData(result.target);
                int targetBlobSize = ShaderConductor.GetShaderConductorBlobSize(result.target);
                string glsl = Marshal.PtrToStringAnsi(targetBlob, targetBlobSize);

                glsl = PostProcessGLSL(glsl, shaderStage);
                shaderData.ShaderCode = Encoding.ASCII.GetBytes(glsl);

                // Add attributes
                if (shaderStage == ShaderStage.VertexShader)
                    shaderData._attributes = ShaderConductor.GetStageInputs(result, shaderStage == ShaderStage.VertexShader);
                else
                    shaderData._attributes = new Attribute[0];

                // Add constant buffers
                var uniformBuffers = ShaderConductor.GetUniformBuffers(result);
                shaderData._cbuffers = new int[uniformBuffers.Count];

                for (var i = 0; i < uniformBuffers.Count; i++)
                {
                    var cb = new ConstantBufferData(uniformBuffers[i]);

                    // Look for a duplicate cbuffer in the list
                    for (var c = 0; c < cbuffers.Count; c++)
                    {
                        if (cb.SameAs(cbuffers[c]))
                        {
                            cb = null;
                            shaderData._cbuffers[i] = c;
                            break;
                        }
                    }

                    // Add a new cbuffer
                    if (cb != null)
                    {
                        shaderData._cbuffers[i] = cbuffers.Count;
                        cbuffers.Add(cb);
                    }
                }

                // Add samplers              
                var samplers = ShaderConductor.GetSamplers(result);
                shaderData._samplers = new Sampler[samplers.Count];

                for (int i = 0; i < samplers.Count; i++)
                {
                    shaderData._samplers[i].samplerName = samplers[i].name;
                    shaderData._samplers[i].parameterName = samplers[i].textureName;
                    shaderData._samplers[i].samplerSlot = samplers[i].slot;
                    shaderData._samplers[i].textureSlot = samplers[i].slot;
                    shaderData._samplers[i].type = samplers[i].type;
                    shaderData._samplers[i].parameter = -1; //sampler mapping to parameter is unknown atm

                    if (samplerStates.TryGetValue(samplers[i].parameterName, out SamplerStateInfo state))
                    {
                        shaderData._samplers[i].state = state.State;
                    }
                }
            }

            // Cleanup
            ShaderConductor.DestroyShaderConductorBlob(result.target);
            ShaderConductor.DestroyShaderConductorBlob(result.errorWarningMsg);

            return shaderData;
        }

        private static string PostProcessGLSL(string glsl, ShaderStage shaderStage)
        {
            // Warning: sloppy text manipulation code ahead!
            // This should be done properly using regular expressions or something.
            if (shaderStage == ShaderStage.VertexShader)
            {
                // The following gl_PerVertex declaration needs to be removed from the vertex shader, otherwise OpenGL will crash, no idea why.
                string gl_PerVertex = "\nout gl_PerVertex\n{\n    vec4 gl_Position;\n};\n";

                // This posFixup parameter will be added to the vertex shader, so we can compensate for differences btw DirectX and OpenGL
                string posFixupParameter = "uniform vec4 posFixup;";

                // Two birds - one stone!
                glsl = glsl.Replace(gl_PerVertex, posFixupParameter);

                // Add posFixup code to the end of the vertex shader.
                // OpenGL uses flipped y-coordinates when rendering to a render target, in this case posFixup.y will be -1.
                // posFixup.zw is for emulating the DX9 half-pixel-offset.
                // The final change to gl_Position.z is needed because OpenGL uses a -1..1 clipspace, while DX uses 0..1
                string posFixupCode = @"
    gl_Position.y = gl_Position.y * posFixup.y;
	gl_Position.xy += posFixup.zw * gl_Position.ww;
    gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
";
                // The assumption here is that the final closing brace belongs to the main vertex shader function, I have a bad feeling about this.
                int closingBracePosition = glsl.LastIndexOf('}');
                glsl = glsl.Insert(closingBracePosition, posFixupCode);
            }

            return glsl;
        }
    }
}
