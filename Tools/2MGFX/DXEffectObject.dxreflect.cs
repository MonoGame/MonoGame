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

                    for (var p = 0; p < dxtechnique.Description.PassCount; p++)
                    {
                        var dxpass = dxtechnique.GetPassByIndex(p);

                        var pass = new d3dx_pass();
                        pass.name = dxpass.Description.Name;
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

            param.name = variable.Description.Name ?? string.Empty;
            param.semantic = variable.Description.Semantic ?? string.Empty;

            // These exactly line up.
            param.class_ = (D3DXPARAMETER_CLASS)variable.TypeInfo.Description.Class;
            param.type = (D3DXPARAMETER_TYPE)variable.TypeInfo.Description.Type;

            param.rows = (uint)variable.TypeInfo.Description.Rows;
            param.columns = (uint)variable.TypeInfo.Description.Columns;

            // TOOD: Look up into shared Object array!

            switch (param.type)
            {
                case D3DXPARAMETER_TYPE.PIXELSHADER:
                case D3DXPARAMETER_TYPE.VERTEXSHADER:
                {
                    var shaderDesc = ((SharpDX.Direct3D11.EffectShaderVariable)variable).GetShaderDescription(0);

                    var shaderVar = variable.AsShader();

                    for ( var i=0; i < shaderDesc.InputParameterCount; i++ )
                    {
                        var element = shaderVar.GetInputSignatureElementDescription( 0, i );
                    }

                    var bytecode = new byte[shaderDesc.Bytecode.Data.Length];
                    shaderDesc.Bytecode.Data.Read(bytecode, 0, bytecode.Length);
                    var shaderIndex = CreateShader(bytecode);
                    param.data = shaderIndex; // new DXShader(bytecode, param.type == D3DXPARAMETER_TYPE.VERTEXSHADER);                    
                    break;
                }

                case D3DXPARAMETER_TYPE.SAMPLER:
                {
                    var sampler = new d3dx_sampler();
                    sampler.state_count = 1;
                    sampler.states = new d3dx_state[1];
                    sampler.states[0] = GetState(variable);
                    break;
                }

                default:
                {
                    if ( variable.TypeInfo.Description.PackedSize > 0 )
                    {
                        var raw = variable.GetRawValue(0);
                        param.bytes = (uint)raw.Length;
                        if (param.bytes > 0)
                        {
                            var data = new byte[param.bytes];
                            raw.Read(data, 0, data.Length);
                            param.data = data;
                        }
                    }

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

                state.parameter = new d3dx_parameter();
                state.parameter.object_id = 1;
            }

            return state;
        }
    }
}
