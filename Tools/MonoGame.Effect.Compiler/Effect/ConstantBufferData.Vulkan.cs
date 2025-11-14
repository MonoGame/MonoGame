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
            }
        }

        static (uint rows, uint columns, EffectObject.D3DXPARAMETER_CLASS paramClass) DimensionsForType(SpirvTypeBase spirvType)
        {
            if (spirvType is SpirvTypeArray array)
            {
                return DimensionsForType(array.ElementType);
            }
            else if (spirvType is SpirvTypeVector vector)
            {
                return (1, vector.Dimensions, EffectObject.D3DXPARAMETER_CLASS.VECTOR);
            }
            else if (spirvType is SpirvTypeMatrix matrix)
            {
                return (matrix.ColumnType.Dimensions, matrix.Columns, EffectObject.D3DXPARAMETER_CLASS.MATRIX_COLUMNS);
            }
            else
            {
                return (1, 1, EffectObject.D3DXPARAMETER_CLASS.SCALAR);
            }
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

            (uint rows, uint cols, var paramClass) = DimensionsForType(member.Type);
            param.rows = rows;
            param.columns = cols;
            param.class_ = paramClass;
            param.type = ToParamType(member.Type);

            if (member.Type is SpirvTypeArray array)
            {
                param.element_count = array.Length;
                param.member_handles = new EffectObject.d3dx_parameter[param.element_count];

                for (uint i = 0; i < array.Length; i++)
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
