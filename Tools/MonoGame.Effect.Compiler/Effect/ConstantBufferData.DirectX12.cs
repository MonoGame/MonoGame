// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGame.Effect
{
    internal partial class ConstantBufferData
    {
        string ParseArrayParameter(string name, List<uint> dims)
        {
            dims.Clear();

            // Is this an array based parameter?
            var arrayStartIndex = name.IndexOf('[');
            if (arrayStartIndex != -1)
            {
                // Extract complete array expression.
                var arrayEndIndex = name.LastIndexOf(']');
                if (arrayEndIndex == -1 || arrayEndIndex <= arrayStartIndex)
                {
                    throw new Exception(String.Format("Invalid array parameter syntax, {0}", name));
                }

                var arrayExpression = name.Substring(arrayStartIndex, arrayEndIndex - arrayStartIndex + 1);
                var arrayExpressionLength = arrayExpression.Length;

                StringBuilder currentDim = new StringBuilder();

                // Parse all array dimensions.
                for (int i = 0; i < arrayExpressionLength; ++i)
                {
                    switch (arrayExpression[i])
                    {
                        case '[' :
                            {
                                break;
                            }
                        case ']':
                            {
                                if (currentDim.Length != 0)
                                    dims.Add(uint.Parse(currentDim.ToString()));
                                currentDim.Clear();
                                break;
                            }

                        default:
                            {
                                if (!Char.IsNumber(arrayExpression[i]))
                                    throw new Exception(String.Format("Invalid array parameter syntax, {0}", name));
                                currentDim.Append(arrayExpression[i]);
                                break;
                            }
                    }
                }

                if (currentDim.Length != 0)
                    throw new Exception(String.Format("Invalid array parameter syntax, {0}", name));

                // Remove array specifier from identifier.
                name = name.Remove(arrayStartIndex).Trim();
            }

            return name;
        }

        public void SetSize(int size)
        {
            Size = size;
        }

        public void AddParameter(string name, string dataType, int byteOffset)
        {
            // Parse parameter dims if this is an array identifier.
            List<uint> dims = new List<uint>();
            name = ParseArrayParameter(name, dims);

            // Has this parameter already been added?
            var found = Parameters.FirstOrDefault(p => p.name == name);
            if (found != null)
                return;

            // Create the new parameter.
            var param = new EffectObject.d3dx_parameter();
            param.name = name;
            param.semantic = string.Empty;
            param.bufferOffset = byteOffset;

            switch (dataType)
            {
                case "bool":
                    param.rows = 1;
                    param.columns = 1;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.BOOL;
                    break;

                case "uint":
                case "int":
                    param.rows = 1;
                    param.columns = 1;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.INT;
                    break;

                case "float":
                    param.rows = 1;
                    param.columns = 1;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float2":
                    param.rows = 1;
                    param.columns = 2;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float3":
                    param.rows = 1;
                    param.columns = 3;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float4":
                    param.rows = 1;
                    param.columns = 4;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float1x2":
                    param.rows = 1;
                    param.columns = 2;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float2x2":
                    param.rows = 2;
                    param.columns = 2;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float3x2":
                    param.rows = 3;
                    param.columns = 2;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float4x2":
                    param.rows = 4;
                    param.columns = 2;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float1x3":
                    param.rows = 1;
                    param.columns = 3;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float2x3":
                    param.rows = 2;
                    param.columns = 3;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float3x3":
                    param.rows = 3;
                    param.columns = 3;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float4x3":
                    param.rows = 4;
                    param.columns = 3;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float1x4":
                    param.rows = 1;
                    param.columns = 4;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float2x4":
                    param.rows = 2;
                    param.columns = 4;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float3x4":
                    param.rows = 3;
                    param.columns = 4;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case "float4x4":
                    param.rows = 4;
                    param.columns = 4;
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                default:
                    throw new Exception("Unknown data type: " + dataType);
            }

            if (param.rows > 1)
                param.class_ = EffectObject.D3DXPARAMETER_CLASS.MATRIX_COLUMNS;
            else if (param.columns > 1)
                param.class_ = EffectObject.D3DXPARAMETER_CLASS.VECTOR;
            else
                param.class_ = EffectObject.D3DXPARAMETER_CLASS.SCALAR;

            param.member_count = 0;
            param.element_count = (dims.Count() != 0) ? dims[0] : 0;

            if (param.member_count > 0)
            {
                param.member_handles = new EffectObject.d3dx_parameter[param.member_count];
                for (var i = 0; i < param.member_count; i++)
                {
                    throw new NotImplementedException("Struct parameter members currently not supported.");
                }
            }
            else if (param.element_count > 0)
            {
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
                    mparam.data = new byte[param.columns * param.rows * 4];

                    param.member_handles[i] = mparam;
                }
            }

            // Size passed to this method can include padding size and is not always the size we would like to use for the buffer, since the
            // reader/deserializer will always read a fixed set of bytes depending on type and class. All data types is currently of size 4
            // and that's what expected by the deserializer as well.
            var data = new byte[param.rows * param.columns * 4];

            // TODO: Default value?

            param.data = data;

            // Add the new parameter and resort by the
            // offset for some consistent results.
            Parameters.Add(param);
            Parameters = Parameters.OrderBy(e => e.bufferOffset).ToList();

            // Recreate the parameter offsets.
            ParameterOffset.Clear();
            foreach (var p in Parameters)
                ParameterOffset.Add(p.bufferOffset);
        }
    }
}
