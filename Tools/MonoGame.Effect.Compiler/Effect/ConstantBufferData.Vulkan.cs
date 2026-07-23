// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
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
                return ToParamType(vector.ElementType);
            else if (spirvType is SpirvTypeMatrix matrix)
                return ToParamType(matrix.ColumnType.ElementType);
            else if (spirvType is SpirvTypeArray array)
                return ToParamType(array.ElementType);

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
                return DimensionsForType(array.ElementType);
            else if (spirvType is SpirvTypeVector vector)
                return (1, vector.Dimensions, EffectObject.D3DXPARAMETER_CLASS.VECTOR);
            else if (spirvType is SpirvTypeMatrix matrix)
                return (matrix.ColumnType.Dimensions, matrix.Columns, EffectObject.D3DXPARAMETER_CLASS.MATRIX_COLUMNS);
            else
                return (1, 1, EffectObject.D3DXPARAMETER_CLASS.SCALAR);
        }

        // This one calculates how large we need to make the bit array of data for this specific parameter
        static uint DataSizeForMember(SpirvTypeBase type)
        {
            if (type is SpirvTypeScalar svScalar)
                // SPIR-V scalar widths are bit-sized.
                return svScalar.Width / 8;
            else if (type is SpirvTypeVector svVector)
                return DataSizeForMember(svVector.ElementType) * svVector.Dimensions;
            else if (type is SpirvTypeMatrix svMatrix)
                return DataSizeForMember(svMatrix.ColumnType) * svMatrix.Columns;
            else if (type is SpirvTypeArray svArray)
                return DataSizeForMember(svArray.ElementType);
            else
                return 4;
        }

        // And this one calculates the size of the parameter with padding
        static uint PaddingSizeForMember(SpirvTypeStructMember member)
        {
            if (member.Type is SpirvTypeScalar svScalar)
                // SPIR-V scalar widths are bit-sized.
                return svScalar.Width / 8;
            else if (member.Type is SpirvTypeVector svVector)
                return svVector.Dimensions * svVector.ElementType.Width / 8;
            else if (member.Type is SpirvTypeMatrix svMatrix)
                return member.MatrixStride * svMatrix.Columns;
            else if (member.Type is SpirvTypeArray svArray)
                return svArray.ArrayStride * svArray.Length;
            else
                return 4;
        }

        public static ConstantBufferData BuildFromSpirvStruct(SpirvTypeStruct svStruct)
        {
            var cbuffer = new ConstantBufferData(svStruct.Name ?? svStruct.Id);
            var byOffset = svStruct.Members.OrderBy(m => m.Offset);

            foreach (var member in byOffset)
            {
                var param = new EffectObject.d3dx_parameter();
                param.name = member.Name;
                param.semantic = string.Empty;
                param.bufferOffset = (int)member.Offset;

                (param.rows, param.columns, param.class_) = DimensionsForType(member.Type);
                param.type = ToParamType(member.Type);
                var dataSize = DataSizeForMember(member.Type);

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
                        mparam.data = new byte[dataSize];

                        param.member_handles[i] = mparam;
                    }
                }
                else
                {
                    // TODO: Default value?
                    var data = new byte[dataSize];
                    param.data = data;
                }

                cbuffer.Parameters.Add(param);
                cbuffer.ParameterOffset.Add(param.bufferOffset);
            }

            var lastItem = svStruct.Members.MaxBy(mem => mem.Offset);
            if (lastItem != null)
            {
                cbuffer.Size = (int)(lastItem.Offset + PaddingSizeForMember(lastItem));
                cbuffer.Size = ((cbuffer.Size + 15) / 16) * 16;
            }

            return cbuffer;
        }
    }
}
