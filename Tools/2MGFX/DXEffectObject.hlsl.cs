using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Xna.Framework.Graphics
{
    partial class DXEffectObject
    {
        private DXEffectObject()
        {
        }

        static public DXEffectObject FromShaderInfo(TwoMGFX.ShaderInfo shaderInfo)
        {
            var effect = new DXEffectObject();

            // These are filled out as we process stuff.
            effect.ConstantBuffers = new List<DXConstantBufferData>();
            effect.Shaders = new List<DXShaderData>();

            // Go thru the techniques and that will find all the 
            // shaders and constant buffers.
            effect.Techniques = new d3dx_technique[shaderInfo.Techniques.Count];
            for (var t = 0; t < shaderInfo.Techniques.Count; t++)
            {
                var tinfo = shaderInfo.Techniques[t]; ;

                var technique = new d3dx_technique();
                technique.name = tinfo.name;
                technique.pass_count = (uint)tinfo.Passes.Count;
                technique.pass_handles = new d3dx_pass[tinfo.Passes.Count];

                for (var p = 0; p < tinfo.Passes.Count; p++)
                {
                    var pinfo = tinfo.Passes[p];

                    var pass = new d3dx_pass();
                    pass.name = pinfo.name ?? string.Empty;

					pass.blendState = pinfo.blendState;
					pass.depthStencilState = pinfo.depthStencilState;
					pass.rasterizerState = pinfo.rasterizerState;

                    pass.state_count = 0;
                    var tempstate = new d3dx_state[2];

                    pinfo.ValidateShaderModels(shaderInfo.DX11Profile);

                    if (!string.IsNullOrEmpty(pinfo.psFunction))
                    {
                        pass.state_count += 1;
                        tempstate[pass.state_count -1] = effect.CreateShader(shaderInfo, pinfo.psFunction, pinfo.psModel, false);
                    }

                    if (!string.IsNullOrEmpty(pinfo.vsFunction))
                    {
                        pass.state_count += 1;
                        tempstate[pass.state_count - 1] = effect.CreateShader(shaderInfo, pinfo.vsFunction, pinfo.vsModel, true);
                    }

                    pass.states = new d3dx_state[pass.state_count];
                    for (var s = 0; s < pass.state_count; s++)
                        pass.states[s] = tempstate[s];

                    technique.pass_handles[p] = pass;
                }

                effect.Techniques[t] = technique;
            }
                
            // Make the list of parameters by combining all the
            // constant buffers ignoring the buffer offsets.
            var parameters = new List<d3dx_parameter>();
            for (var c = 0; c < effect.ConstantBuffers.Count; c++ )
            {
                var cb = effect.ConstantBuffers[c];

                for (var i = 0; i < cb.Parameters.Count; i++ )
                {
                    var param = cb.Parameters[i];

                    var match = parameters.FindIndex(e => e.name == param.name);
                    if (match == -1)
                    {
                        cb.ParameterIndex.Add(parameters.Count);
                        parameters.Add(param);
                    }
                    else
                    {
                        // TODO: Make sure the type and size of 
                        // the parameter match up!
                        cb.ParameterIndex.Add(match);
                    }
                }
            }

			var samplerStates = new Dictionary<string, SamplerState>();

            // Add the texture parameters from the samplers.
            foreach (var shader in effect.Shaders)
            {
                for (var s = 0; s < shader._samplers.Length; s++)
                {
                    var sampler = shader._samplers[s];

                    var match = parameters.FindIndex(e => e.name == sampler.parameterName);
                    if (match == -1)
                    {
                        shader._samplers[s].parameter = parameters.Count;

                        var param = new d3dx_parameter();
                        param.class_ = D3DXPARAMETER_CLASS.OBJECT;
                        param.type = D3DXPARAMETER_TYPE.TEXTURE2D; // TODO: Fix this right!
                        param.name = sampler.parameterName;
                        param.semantic = string.Empty;

						SamplerState state = null;
						shaderInfo.SamplerStates.TryGetValue(param.name, out state);
						samplerStates[param.name] = state;

                        parameters.Add(param);
                    }
                    else
                    {
                        // TODO: Make sure the type and size of 
                        // the parameter match up!

                        shader._samplers[s].parameter = match;
                    }
                }
            }

            // TODO: Annotations are part of the .FX format and
            // not a part of shaders... we need to implement them
            // in our mgfx parser if we want them back.
            /*
            // Now find the things we could not in the constant
            // buffer reflection interface...  semantics and annotations.
            for (var i = 0; i < parameters.Count; i++)
            {
                var vdesc = dxeffect.GetVariableByName(parameters[i].name);
                if (!vdesc.IsValid)
                    continue;

                parameters[i].semantic = vdesc.Description.Semantic ?? string.Empty;

                // TODO: Annotations!
            }
            */

			effect.SamplerStates = samplerStates;
            effect.Parameters = parameters.ToArray();

            return effect;
        }
        
        private d3dx_state CreateShader(TwoMGFX.ShaderInfo shaderInfo, string shaderFunction, string shaderProfile, bool isVertexShader)
        {
            // Compile the shader.
            SharpDX.D3DCompiler.ShaderBytecode shaderByteCode;
            try
            {
                SharpDX.D3DCompiler.ShaderFlags shaderFlags = 0;

                // While we never allow preshaders, this flag is invalid for
                // the DX11 shader compiler which doesn't allow preshaders
                // in the first place.
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.NoPreshader;

                if (shaderInfo.DX11Profile)
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.EnableBackwardsCompatibility;

                if (shaderInfo.Debug)
                {
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.SkipOptimization;
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.Debug;
                }
                else
                {
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.OptimizationLevel3;
                }

                // Compile the shader into bytecode.                
                var result = SharpDX.D3DCompiler.ShaderBytecode.Compile(
                    shaderInfo.fileContent, 
                    shaderFunction, 
                    shaderProfile, 
                    shaderFlags, 
                    0, 
                    null,
                    null,
                    shaderInfo.fileName);

                if (result.HasErrors)
                    throw new Exception(result.Message);

                shaderByteCode = result.Bytecode;
                //var source = shaderByteCode.Disassemble();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // Get a copy of the shader bytecode.
            var bytecode = shaderByteCode.Data.ToArray();

            // First look to see if we already created this same shader.
            DXShaderData dxShader = null;
            foreach (var shader in Shaders)
            {
                if (bytecode.SequenceEqual(shader.Bytecode))
                {
                    dxShader = shader;
                    break;
                }
            }

            // Create a new shader.
            if ( dxShader == null )
            {
                if (shaderInfo.DX11Profile)
                    dxShader = DXShaderData.CreateHLSL(bytecode, isVertexShader, ConstantBuffers, Shaders.Count, shaderInfo.SamplerStates, shaderInfo.Debug);
                else
                    dxShader = DXShaderData.CreateGLSL(bytecode, ConstantBuffers, Shaders.Count, shaderInfo.SamplerStates);

                Shaders.Add(dxShader);
            }
            
            //var assmbly = desc.Bytecode.Disassemble();
            //var buffer = reflection.GetConstantBuffer(0);

            var state = new d3dx_state();
            state.index = 0;
            state.type = STATE_TYPE.CONSTANT;
            state.operation = isVertexShader ? (uint)146 : (uint)147;

            state.parameter = new d3dx_parameter();
            state.parameter.name = string.Empty;
            state.parameter.semantic = string.Empty;
            state.parameter.class_ = D3DXPARAMETER_CLASS.OBJECT;
            state.parameter.type = isVertexShader ? D3DXPARAMETER_TYPE.VERTEXSHADER : D3DXPARAMETER_TYPE.PIXELSHADER;
            state.parameter.rows = 0;
            state.parameter.columns = 0;
            state.parameter.data = dxShader.SharedIndex;

            return state;
        }
    }
}
