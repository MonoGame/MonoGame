using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
    public class ConstantBuffer
    {
        public string Name { get; private set; }

        public int Size { get; private set; }

        public List<DXEffectObject.d3dx_parameter> Parameters { get; private set; }

        public List<int> ParameterIndex { get; private set; }

        public List<int> ParameterOffset { get; private set; }

        public ConstantBuffer(SharpDX.D3DCompiler.ConstantBuffer cb)
        {
            Name = cb.Description.Name ?? string.Empty;            
            Size = cb.Description.Size;

            ParameterIndex = new List<int>();
            ParameterOffset = new List<int>();

            // Gather all the parameters.
            var paramters = new List<DXEffectObject.d3dx_parameter>();
            for (var i = 0; i < cb.Description.VariableCount; i++)
            {
                var vdesc = cb.GetVariable(i);

                var param = GetParameterFromType(vdesc.GetVariableType());

                param.name = vdesc.Description.Name;
                param.bufferOffset = vdesc.Description.StartOffset;

                if (vdesc.Description.DefaultValue != IntPtr.Zero)
                    throw new NotImplementedException("No support for default values yet!");
                else
                    param.data = new byte[param.columns * param.rows * 4];

                paramters.Add(param);
            }

            // Sort the parameters by the buffer offset.
            Parameters = paramters.OrderBy(e => e.bufferOffset).ToList();
        }

        private static DXEffectObject.d3dx_parameter GetParameterFromType(SharpDX.D3DCompiler.ShaderReflectionType type)
        {
            var param = new DXEffectObject.d3dx_parameter();
            param.rows = (uint)type.Description.RowCount;
            param.columns = (uint)type.Description.ColumnCount;
            param.name = type.Description.Name ?? string.Empty;

            param.bufferOffset = type.Description.Offset;

            switch (type.Description.Class)
            {
                case SharpDX.D3DCompiler.ShaderVariableClass.Scalar:
                    param.class_ = DXEffectObject.D3DXPARAMETER_CLASS.SCALAR;
                    break;

                case SharpDX.D3DCompiler.ShaderVariableClass.Vector:
                    param.class_ = DXEffectObject.D3DXPARAMETER_CLASS.VECTOR;
                    break;

                case SharpDX.D3DCompiler.ShaderVariableClass.MatrixColumns:
                    param.class_ = DXEffectObject.D3DXPARAMETER_CLASS.MATRIX_COLUMNS;
                    break;

                default:
                    throw new Exception("Unsupported parameter class!");
            }

            switch (type.Description.Type)
            {
                case SharpDX.D3DCompiler.ShaderVariableType.Bool:
                    param.type = DXEffectObject.D3DXPARAMETER_TYPE.BOOL;
                    break;

                case SharpDX.D3DCompiler.ShaderVariableType.Float:
                    param.type = DXEffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case SharpDX.D3DCompiler.ShaderVariableType.Int:
                    param.type = DXEffectObject.D3DXPARAMETER_TYPE.INT;
                    break;

                default:
                    throw new Exception("Unsupported parameter type!");
            }

            param.element_count = (uint)type.Description.ElementCount;
            param.member_count = (uint)type.Description.MemberCount;

            var count = Math.Max(param.element_count, param.member_count);

            param.member_handles = new DXEffectObject.d3dx_parameter[count];
            for (var i=0; i < count; i++)
            {
                var mparam = GetParameterFromType(type.GetMemberType(i));
                mparam.name = type.GetMemberTypeName(i) ?? string.Empty;
                param.member_handles[i] = mparam;
            }

            return param;
        }

        public bool SameAs(ConstantBuffer other)
        {
            // If the names of the constant buffers don't
            // match then consider them different right off 
            // the bat... even if their parameters are the same.
            if (Name != other.Name)
                return false;

            // Do we have the same count of parameters and size?
            if (    Size != other.Size ||
                    Parameters.Count != other.Parameters.Count)
                return false;
            
            // Compare the parameters themselves.
            for (var i = 0; i < Parameters.Count; i++)
            {
                var p1 = Parameters[i];
                var p2 = other.Parameters[i];

                // Check the importaint bits.
                if (    p1.name != p2.name ||
                        p1.rows != p2.rows ||
                        p1.columns != p2.columns ||
                        p1.class_ != p2.class_ ||
                        p1.type != p2.type ||
                        p1.bufferOffset != p2.bufferOffset)
                    return false;
            }

            // These are equal.
            return true;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((ushort)Size);

            writer.Write((byte)ParameterIndex.Count);
            for (var i=0; i < ParameterIndex.Count; i++)
            {
                writer.Write((byte)ParameterIndex[i]);
                writer.Write((ushort)ParameterOffset[i]);
            }
        }
    }
}
