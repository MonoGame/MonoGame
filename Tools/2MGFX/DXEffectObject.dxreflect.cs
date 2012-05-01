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
                    var shaderIndex = CreateShader(variable);
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

        private int CreateShader(SharpDX.Direct3D11.EffectVariable variable)
        {
            var shaderDesc = ((SharpDX.Direct3D11.EffectShaderVariable)variable).GetShaderDescription(0);

            // Get the shader bytecode.
            var bytecode = new byte[shaderDesc.Bytecode.Data.Length];
            shaderDesc.Bytecode.Data.Read(bytecode, 0, bytecode.Length);

            // First look to see if we already created this same shader.
            foreach (var shader in Shaders)
            {
                if (bytecode.SequenceEqual(shader.Bytecode))
                    return shader.SharedIndex;
            }

            var shaderVar = variable.AsShader();

            var isVertexShader = variable.TypeInfo.Description.Type == SharpDX.D3DCompiler.ShaderVariableType.Vertexshader;

            DXShader.Attribute[] attributes = new DXShader.Attribute[0];

            if (isVertexShader)
            {
                var componentX = SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX;
                var componentXY = SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX |
                                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentY;
                var componentXYZ = SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX |
                                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentY |
                                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentZ;
                var componentXYZW = SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentX |
                                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentY |
                                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentZ |
                                    SharpDX.D3DCompiler.RegisterComponentMaskFlags.ComponentW;

                attributes = new DXShader.Attribute[shaderDesc.InputParameterCount];
                var offset = 0;
                for (var i = 0; i < attributes.Length; i++)
                {
                    var element = shaderVar.GetInputSignatureElementDescription(0, i);

                    attributes[i].name = element.SemanticName;
                    attributes[i].index = offset;
                    //attributes[i].usage = ???

                    var isX = (element.UsageMask & componentX) == componentX;
                    var isXY = (element.UsageMask & componentXY) == componentXY;
                    var isXYZ = (element.UsageMask & componentXYZ) == componentXYZ;
                    var isXYZW = (element.UsageMask & componentXYZW) == componentXYZW;

                    // Increment the offset.
                    offset += isXYZW ? 4 : isXYZ ? 3 : isXY ? 2 : 1;

                    SharpDX.DXGI.Format format;
                    switch (element.ComponentType)
                    {
                        case SharpDX.D3DCompiler.RegisterComponentType.Float32:
                            if (isXYZW)
                                format = SharpDX.DXGI.Format.R32G32B32A32_Float;
                            else if (isXYZ)
                                format = SharpDX.DXGI.Format.R32G32B32_Float;
                            else if (isXY)
                                format = SharpDX.DXGI.Format.R32G32_Float;
                            else if (isX)
                                format = SharpDX.DXGI.Format.R32_Float;
                            else 
                                throw new NotImplementedException("Got unknown vertex shader input!");
                            break;

                        case SharpDX.D3DCompiler.RegisterComponentType.Sint32:
                            if (isXYZW)
                                format = SharpDX.DXGI.Format.R32G32B32A32_SInt;
                            else if (isXYZ)
                                format = SharpDX.DXGI.Format.R32G32B32_SInt;
                            else if (isXY)
                                format = SharpDX.DXGI.Format.R32G32_SInt;
                            else if (isX)
                                format = SharpDX.DXGI.Format.R32_SInt;
                            else 
                                throw new NotImplementedException("Got unknown vertex shader input!");
                            break;

                        case SharpDX.D3DCompiler.RegisterComponentType.Uint32:
                            if (isXYZW)
                                format = SharpDX.DXGI.Format.R32G32B32A32_UInt;
                            else if (isXYZ)
                                format = SharpDX.DXGI.Format.R32G32B32_UInt;
                            else if (isXY)
                                format = SharpDX.DXGI.Format.R32G32_UInt;
                            else if (isX)
                                format = SharpDX.DXGI.Format.R32_UInt;
                            else 
                                throw new NotImplementedException("Got unknown vertex shader input!");
                            break;

                        default:
                            throw new NotImplementedException("Got unknown vertex shader input!");
                    }

                    attributes[i].format = (short)format;
                }
            }
   
            // Create a new shader.
            var dxShader = new DXShader(bytecode, isVertexShader, Shaders.Count, attributes);
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
