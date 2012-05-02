using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    partial class DXEffectObject
    {
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

                for (var i = 0; i < desc.ConstantBufferCount; i++)
                {
                    var cbuffer = dxeffect.GetConstantBufferByIndex(i);
                }

                effect.Shaders = new List<DXShader>();

                effect.Objects = new d3dx_parameter[0];
                effect.Parameters = new d3dx_parameter[desc.GlobalVariableCount];
                for (var i = 0; i < desc.GlobalVariableCount; i++)
                    effect.Parameters[i] = effect.GetParameter( dxeffect.GetVariableByIndex(i) );

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

                        if (dxpass.PixelShaderDescription.Variable.IsValid)
                        {                            
                            pass.states[pass.state_count] = effect.GetState(dxpass.PixelShaderDescription.Variable);
                            pass.state_count++;
                        }

                        if (dxpass.VertexShaderDescription.Variable.IsValid)
                        {
                            pass.states[pass.state_count] = effect.GetState(dxpass.VertexShaderDescription.Variable); ;
                            pass.state_count++;
                        }

                        technique.pass_handles[p] = pass;
                    }

                    effect.Techniques[i] = technique;
                }
                
                /*
                var desc = dxeffect.Description.TechniqueCount;
                var vsarray = dxeffect.GetVariableByName("VSArray");
                if (vsarray != null)
                {
                    var elem = vsarray.GetElement(0);
                    var shader = elem.AsShader();
                    for (var v = 0; v < vsarray.TypeInfo.Description.Elements; v++)
                    {
                        var shaderDesc = shader.GetShaderDescription(v);
                        var signature = shaderDesc.Signature;
                        //var shader = new SharpDX.Direct3D11.VertexShader(  );
                    }
                }

                if (desc.TechniqueCount > 0)
                {
                    var technique = dxeffect.GetTechniqueByIndex(0);
                    var techniqueDesc = technique.Description;
                    var pass = technique.GetPassByIndex(0);
                    var passDesc = pass.Description;
                    var pixelShader = pass.PixelShaderDescription.Variable.AsShader();
                    var vertexShader = pass.VertexShaderDescription.Variable.AsShader();
                }
                */

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
                    var shaderIndex = CreateShader(variable as SharpDX.Direct3D11.EffectShaderVariable);
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

                    var size = param.rows * param.columns;
                    var buffer = new byte[size * 4];
                    param.data = buffer;

                    break;
                }

                case D3DXPARAMETER_TYPE.PIXELFRAGMENT:
                case D3DXPARAMETER_TYPE.VERTEXFRAGMENT:
                    throw new Exception("We don't support shader fragments!");
            }

            //param.member_count = variable.TypeInfo.Description.Members;                    
            //param.member_handles = variable.TypeInfo.nm
            //param.member_count =
            //param.members = 

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
            var dxShader = new DXShader(bytecode, variable, Shaders.Count);
            Shaders.Add(dxShader);
            return dxShader.SharedIndex;
        }

        private d3dx_state GetState(SharpDX.Direct3D11.EffectVariable variable)
        {
            var state = new d3dx_state();

            var dxsampler = variable.AsSampler();
            var dxshader = variable.AsShader();

            if (dxsampler != null)
            {
                var samplerState = dxsampler.GetSampler().Description;

                state.index = 0;
                state.operation = 164; // DXEffectObject.state_table texture!
                state.type = STATE_TYPE.PARAMETER;

                state.parameter = new d3dx_parameter();
                state.parameter.object_id = 1;
            }
            else if (dxshader != null)
            {
                state.index = 0; // (uint)dxpass.PixelShaderDescription.Index;
                state.type = STATE_TYPE.CONSTANT;

                // This is from DXEffectObject.state_table! 
                state.operation = variable.TypeInfo.Description.Type == SharpDX.D3DCompiler.ShaderVariableType.Vertexshader ? (uint)146 : (uint)147;

                state.parameter = GetParameter(variable);
                //state.parameter.object_id = 1;
            }

            return state;
        }
    }
}
