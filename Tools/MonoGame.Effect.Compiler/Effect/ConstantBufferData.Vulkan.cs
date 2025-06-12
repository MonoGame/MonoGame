// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;

namespace MonoGame.Effect
{
    internal partial class ConstantBufferData
    {
        static EffectObject.D3DXPARAMETER_TYPE ToParamType(string dataType)
        {
            switch(dataType)
            {
                case "float":
                    return EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                case "uint":
                case "int":
                    return EffectObject.D3DXPARAMETER_TYPE.INT;
                case "bool":
                    return EffectObject.D3DXPARAMETER_TYPE.BOOL;
                default:
                    throw new Exception("Unknown data type: " + dataType);
            };
        }

        public void AddParameter(string name, string dataType, int sizeOfArray, int byteOffset)
        {
            // Has this parameter already been added?
            var found = Parameters.FirstOrDefault(p => p.name == name);
            if (found != null)
                return;

            // Create the new parameter.
            var param = new EffectObject.d3dx_parameter();
            param.name = name;
            param.semantic = string.Empty;
            param.bufferOffset = byteOffset;

            if (dataType.StartsWith("%mat"))
            {
                param.columns = (uint)char.GetNumericValue(dataType[4]);
                param.rows = (uint)char.GetNumericValue(dataType[6]);
                param.type = ToParamType(dataType.Substring(7));
            }
            else if (dataType.StartsWith("%v"))
            {
                param.rows = 1;
                param.columns = (uint)char.GetNumericValue(dataType[2]);
                param.type = ToParamType(dataType.Substring(3));
            }
            else if (dataType.StartsWith("%_arr_"))
            {
                // TODO: Support arrays.... %_arr_mat4v3float_uint_72
                param.rows = 1;
                param.columns = 1;
                param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
            }
            else
            {
                param.rows = 1;
                param.columns = 1;
                param.type = ToParamType(dataType.Substring(1));
            }

            if (param.rows > 1)
                param.class_ = EffectObject.D3DXPARAMETER_CLASS.MATRIX_COLUMNS;
            else if (param.columns > 1)
                param.class_ = EffectObject.D3DXPARAMETER_CLASS.VECTOR;
            else
                param.class_ = EffectObject.D3DXPARAMETER_CLASS.SCALAR;

            var byteSize = param.rows * param.columns * 4;

            if (sizeOfArray > 0)
            {
                param.element_count = (uint)sizeOfArray;
                param.member_handles = new EffectObject.d3dx_parameter[param.element_count];
                for (var i = 0; i < param.element_count; i++)
                {
                    var mparam = new EffectObject.d3dx_parameter();

                    mparam.name = string.Empty;
                    mparam.semantic = string.Empty;
                    mparam.type = param.type;
                    mparam.class_ = param.class_;
                    mparam.rows = param.rows;
                    mparam.columns = param.columns;
                    mparam.data = new byte[byteSize];

                    param.member_handles[i] = mparam;
                }
            }

            var data = new byte[byteSize];

            // TODO: Default value?

            param.data = data;
                        
            // Add the new parameter and resort by the
            // offset for some consistent results.
            Parameters.Add(param);
            Parameters = Parameters.OrderBy(e => e.bufferOffset).ToList();

            // Recreate the parameter offsets and calculate the size.
            Size = 0;
            ParameterOffset.Clear();
            foreach (var p in Parameters)
            {
                ParameterOffset.Add(p.bufferOffset);

                var esize = p.rows * p.columns * 4;
                if (p.element_count > 0)
                    esize = (esize + (16 - (esize % 16))) * p.element_count;

                Size = p.bufferOffset + (int)esize;
            }
        }
    }
}
