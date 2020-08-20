using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.Effect
{
    internal partial class ConstantBufferData
    {
        public ConstantBufferData (ShaderConductor.UniformBuffer ub)
		{
            Name = ub.blockName;
            InstanceName = ub.instanceName;
            Size = ub.byteSize;

            ParameterIndex = new List<int>();

            var parameters = new List<EffectObject.d3dx_parameter>();

            // Gather all the parameters.
            foreach (ShaderConductor.Parameter p in ub.parameters)
            {
                var size = p.columns * p.rows * 4;
                var data = new byte[size];

                var dxParam = new EffectObject.d3dx_parameter();
                parameters.Add(dxParam);

                dxParam.rows = (uint)p.rows;
                dxParam.columns = (uint)p.columns;
                dxParam.name = p.name;
                dxParam.semantic = string.Empty;
                dxParam.bufferOffset = p.offset;
                dxParam.semantic = string.Empty;
                dxParam.data = data;
                dxParam.member_count = 0;
                dxParam.element_count = 0;
                dxParam.member_handles = new EffectObject.d3dx_parameter[0];

                if (p.rows == 1)
                {
                    if (p.columns == 1)
                        dxParam.class_ = EffectObject.D3DXPARAMETER_CLASS.SCALAR;
                    else
                        dxParam.class_ = EffectObject.D3DXPARAMETER_CLASS.VECTOR;
                }
                else
                    dxParam.class_ = EffectObject.D3DXPARAMETER_CLASS.MATRIX_COLUMNS;

                switch (p.type)
                {
                    case 1:
                        dxParam.type = EffectObject.D3DXPARAMETER_TYPE.BOOL;
                        break;
                    case 2:
                        dxParam.type = EffectObject.D3DXPARAMETER_TYPE.INT; 
                        break;
                    case 3:
                        dxParam.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                        break;
                    default:
                        throw new Exception("Unsupported parameter type!");
                } 
            }

            // Sort them by the offset for some consistent results.
            Parameters = parameters.OrderBy(e => e.bufferOffset).ToList();

            // Store the parameter offsets.
            ParameterOffset = new List<int>();
            foreach (var param in Parameters)
                ParameterOffset.Add(param.bufferOffset);
        }
    }
}
