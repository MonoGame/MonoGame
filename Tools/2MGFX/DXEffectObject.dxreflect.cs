using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    partial class DXEffectObject
    {
        public List<ConstantBuffer> ConstantBuffers { get; private set; }
        
        private DXEffectObject()
        {
        }

        static public DXEffectObject FromDX10Effect( SharpDX.D3DCompiler.ShaderBytecode bytecode)
        {
            // Create a null device so we can initialize the effect.
            using (var device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Null))
            using (var dxeffect = new SharpDX.Direct3D11.Effect(device, bytecode))
            {
                var effect = new DXEffectObject();

                var desc = dxeffect.Description;

                // These are filled out as we process stuff.
                effect.ConstantBuffers = new List<ConstantBuffer>();
                effect.Shaders = new List<DXShader>();

                // Go thru the techniques and that will find all the 
                // shaders and constant buffers.
                effect.Techniques = new d3dx_technique[desc.TechniqueCount];
                for (var i = 0; i < desc.TechniqueCount; i++)
                {
                    var dxtechnique = dxeffect.GetTechniqueByIndex(i);

                    var technique = new d3dx_technique();
                    technique.name = dxtechnique.Description.Name;
                    technique.pass_count = (uint)dxtechnique.Description.PassCount;
                    technique.pass_handles = new d3dx_pass[technique.pass_count];

                    for (var p = 0; p < dxtechnique.Description.PassCount; p++)
                    {
                        var dxpass = dxtechnique.GetPassByIndex(p);

                        var pass = new d3dx_pass();
                        pass.name = dxpass.Description.Name ?? string.Empty;
                        pass.states = new d3dx_state[2];
                        pass.state_count = 2;

                        if (    !dxpass.PixelShaderDescription.Variable.IsValid ||
                                !dxpass.VertexShaderDescription.Variable.IsValid)
                            throw new Exception("Passed must have a vertex and pixel shader assigned!");

                        pass.states[0] = effect.GetState(dxpass.PixelShaderDescription.Variable);
                        pass.states[1] = effect.GetState(dxpass.VertexShaderDescription.Variable);

                        technique.pass_handles[p] = pass;
                    }

                    effect.Techniques[i] = technique;
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
                            cb.ParameterOffset.Add(param.bufferOffset);
                            parameters.Add(param);
                        }
                        else
                        {
                            // TODO: Make sure the type and size of 
                            // the parameter match up!
                            cb.ParameterIndex.Add(match);
                            cb.ParameterOffset.Add(param.bufferOffset);
                        }
                    }
                }

                // Add the texture parameters from the samplers.
                foreach (var shader in effect.Shaders)
                {
                    for (var s = 0; s < shader._samplers.Length; s++)
                    {
                        var sampler = shader._samplers[s];

                        var match = parameters.FindIndex(e => e.name == sampler.name);
                        if (match == -1)
                        {
                            shader._samplers[s].parameter = parameters.Count;

                            var param = new d3dx_parameter();
                            param.class_ = D3DXPARAMETER_CLASS.OBJECT;
                            param.type = D3DXPARAMETER_TYPE.TEXTURE2D; // TODO: Fix this right!
                            param.name = sampler.name;
                            param.semantic = string.Empty;

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

                effect.Parameters = parameters.ToArray();

                return effect;
            }
        }

        private d3dx_parameter GetParameter(SharpDX.Direct3D11.EffectVariable variable)
        {
            var param = new d3dx_parameter();

            var typeDesc = variable.TypeInfo.Description;

            param.name = variable.Description.Name ?? string.Empty;
            param.semantic = variable.Description.Semantic ?? string.Empty;

            // These exactly line up.
            param.class_ = (D3DXPARAMETER_CLASS)typeDesc.Class;
            param.type = (D3DXPARAMETER_TYPE)typeDesc.Type;

            param.rows = (uint)typeDesc.Rows;
            param.columns = (uint)typeDesc.Columns;

            switch (param.type)
            {
                case D3DXPARAMETER_TYPE.PIXELSHADER:
                case D3DXPARAMETER_TYPE.VERTEXSHADER:
                {
                    var shaderIndex = CreateShader(variable.AsShader());
                    param.data = shaderIndex;
					break;
                }

                case D3DXPARAMETER_TYPE.TEXTURE:
                case D3DXPARAMETER_TYPE.TEXTURE1D:
                case D3DXPARAMETER_TYPE.TEXTURE2D:
                case D3DXPARAMETER_TYPE.TEXTURE3D:
                case D3DXPARAMETER_TYPE.TEXTURECUBE:
                    // Nothing to store for this type.
                    break;

                case D3DXPARAMETER_TYPE.SAMPLER:
                case D3DXPARAMETER_TYPE.SAMPLER1D:
                case D3DXPARAMETER_TYPE.SAMPLER2D:
                case D3DXPARAMETER_TYPE.SAMPLER3D:
                case D3DXPARAMETER_TYPE.SAMPLERCUBE:
                {
                    /*
                    var sampler = new d3dx_sampler();
                    sampler.state_count = 1;
                    sampler.states = new d3dx_state[1];
                    sampler.states[0] = GetState(variable);
                    */
                    break;
                }

                default:
                {
                    if (param.rows == 0 || param.columns == 0)
                        break;

                    var size = (int)(param.rows * param.columns) * 4;
                    var buffer = new byte[size];                    
                    var raw = variable.GetRawValue(size);
                    raw.Read(buffer, 0, size);                    
                    param.data = buffer;

                    break;
                }

                case D3DXPARAMETER_TYPE.PIXELFRAGMENT:
                case D3DXPARAMETER_TYPE.VERTEXFRAGMENT:
                    throw new Exception("We don't support shader fragments!");
            }

            return param;
        }

        private int CreateShader(SharpDX.Direct3D11.EffectShaderVariable variable)
        {
            // Get the shader bytecode.
            var desc = variable.GetShaderDescription(0);
            var bytecode = new byte[desc.Bytecode.Data.Length];
            desc.Bytecode.Data.Read(bytecode, 0, bytecode.Length);

            // First look to see if we already created this same shader.
            foreach (var shader in Shaders)
            {
                if (bytecode.SequenceEqual(shader.Bytecode))
                    return shader.SharedIndex;
            }

            // Create a new shader.
            var dxShader = new DXShader(bytecode, variable, ConstantBuffers, Shaders.Count);
            Shaders.Add(dxShader);
            
            //var assmbly = desc.Bytecode.Disassemble();

            //var buffer = reflection.GetConstantBuffer(0);

            return dxShader.SharedIndex;
        }

        private d3dx_state GetState(SharpDX.Direct3D11.EffectVariable variable)
        {
            var state = new d3dx_state();

            var dxshader = variable.AsShader();
            if (dxshader != null)
            {
                state.index = 0;
                state.type = STATE_TYPE.CONSTANT;

                // This is from DXEffectObject.state_table! 
                state.operation = 
                    variable.TypeInfo.Description.Type == SharpDX.D3DCompiler.ShaderVariableType.Vertexshader ? 
                    (uint)146 : (uint)147;

                state.parameter = GetParameter(variable);
            }

            return state;
        }
    }
}
