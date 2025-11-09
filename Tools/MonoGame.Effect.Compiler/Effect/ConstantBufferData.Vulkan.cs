// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Effect.Compiler.Effect.Spirv;
using System;
using System.Linq;

namespace MonoGame.Effect
{
    internal partial class ConstantBufferData
    {
        static EffectObject.D3DXPARAMETER_TYPE ToParamType(SpirvTypeBase spirvType)
        {
            if (spirvType is SpirvTypeVector vector)
            {
                return ToParamType(vector.ElementType);
            }
            else if (spirvType is SpirvTypeMatrix matrix)
            {
                return ToParamType(matrix.ColumnType.ElementType);
            }
            else if (spirvType is SpirvTypeArray array)
            {
                return ToParamType(array.ElementType);
            }

            switch (spirvType.Type)
            {
                case SpirvType.Float:
                    return EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                case SpirvType.Int:
                    return EffectObject.D3DXPARAMETER_TYPE.INT;
                case SpirvType.Bool:
                    return EffectObject.D3DXPARAMETER_TYPE.BOOL;
                default:
                    throw new Exception("Unknown data type: " + spirvType);
            };
        }

        public void AddParameter(SpirvTypeStructMember member)
        {
            // Has this parameter already been added?
            var found = Parameters.FirstOrDefault(p => p.name == member.Name);
            if (found != null)
                return;

            // Create the new parameter.
            var param = new EffectObject.d3dx_parameter();
            param.name = member.Name;
            param.semantic = string.Empty;
            param.bufferOffset = member.Offset.Value;

            if (member.Type is SpirvTypeMatrix matrix)
            {
                param.columns = matrix.Columns;
                param.rows = matrix.ColumnType.Dimensions;
                param.type = ToParamType(matrix.ColumnType.ElementType);
                param.class_ = EffectObject.D3DXPARAMETER_CLASS.MATRIX_COLUMNS;
            }
            else if (member.Type is SpirvTypeVector vector)
            {
                param.rows = 1;
                param.columns = vector.Dimensions;
                param.type = ToParamType(vector.ElementType);
                param.class_ = EffectObject.D3DXPARAMETER_CLASS.VECTOR;
            }
            else if (member.Type is SpirvTypeArray array)
            {
                // TODO: Add array support here.
                param.rows = 1;
                param.columns = 1;
                param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                param.class_ = EffectObject.D3DXPARAMETER_CLASS.SCALAR;
            }
            else
            {
                param.rows = 1;
                param.columns = 1;
                param.type = ToParamType(member.Type);
                param.class_ = EffectObject.D3DXPARAMETER_CLASS.SCALAR;
            }

            var byteSize = param.rows * param.columns * 4;

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
