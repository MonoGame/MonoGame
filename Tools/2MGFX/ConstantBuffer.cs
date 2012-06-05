using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TwoMGFX;

namespace Microsoft.Xna.Framework.Graphics
{
    public class ConstantBuffer
    {
        public string Name { get; private set; }

        public int Size { get; private set; }

        public List<int> ParameterIndex { get; private set; }

        public List<int> ParameterOffset { get; private set; }

        public List<DXEffectObject.d3dx_parameter> Parameters { get; private set; }

        public ConstantBuffer(SharpDX.D3DCompiler.ConstantBuffer cb)
        {
            Name = cb.Description.Name ?? string.Empty;            
            Size = cb.Description.Size;

            ParameterIndex = new List<int>();

            var parameters = new List<DXEffectObject.d3dx_parameter>();

            // Gather all the parameters.
            for (var i = 0; i < cb.Description.VariableCount; i++)
            {
                var vdesc = cb.GetVariable(i);

                var param = GetParameterFromType(vdesc.GetVariableType());

                param.name = vdesc.Description.Name;
                param.semantic = string.Empty;
                param.bufferOffset = vdesc.Description.StartOffset;

                if (vdesc.Description.DefaultValue != IntPtr.Zero)
                    throw new NotImplementedException("No support for default values yet!");
                else
                    param.data = new byte[param.columns * param.rows * 4];

                parameters.Add(param);
            }

            // Sort them by the offset for some consistant results.
            Parameters = parameters.OrderBy(e => e.bufferOffset).ToList();

            // Store the parameter offsets.
            ParameterOffset = new List<int>();
            foreach (var param in Parameters)
                ParameterOffset.Add(param.bufferOffset);
        }

        public ConstantBuffer(  string name,
                                MojoShader.MOJOSHADER_symbolRegisterSet set, 
                                MojoShader.MOJOSHADER_symbol[] symbols)
        {
            Name = name ?? string.Empty;

            ParameterIndex = new List<int>();
            ParameterOffset = new List<int>();
            Parameters = new List<DXEffectObject.d3dx_parameter>();

            int minRegister = short.MaxValue;
            int maxRegister = 0;

            var registerSize = (set == MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL ? 1 : 4 ) * 4;

            foreach (var symbol in symbols)
            {
                if (symbol.register_set != set)
                    continue;

                // Create the parameter.
                var parm = GetParameterFromSymbol(symbol);

                var offset = (int)symbol.register_index * registerSize;
                parm.bufferOffset = offset;

                Parameters.Add(parm);
                ParameterOffset.Add(offset);

                minRegister = Math.Min(minRegister, (int)symbol.register_index);
                maxRegister = Math.Max(maxRegister, (int)(symbol.register_index + symbol.register_count));
            }

            Size = Math.Max(maxRegister - minRegister, 0) * registerSize;
        }

        private static DXEffectObject.d3dx_parameter GetParameterFromSymbol(MojoShader.MOJOSHADER_symbol symbol)
        {
            var param = new DXEffectObject.d3dx_parameter();
            param.rows = symbol.info.rows;
            param.columns = symbol.info.columns;
            param.name = symbol.name ?? string.Empty;
            param.semantic = string.Empty; // TODO: How do i do this with only MojoShader?

            var registerSize = (symbol.register_set == MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL ? 1 : 4) * 4;
            var offset = (int)symbol.register_index * registerSize;
            param.bufferOffset = offset;

            switch (symbol.info.parameter_class)
            {
                case MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_SCALAR:
                    param.class_ = DXEffectObject.D3DXPARAMETER_CLASS.SCALAR;
                    break;

                case MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_VECTOR:
                    param.class_ = DXEffectObject.D3DXPARAMETER_CLASS.VECTOR;
                    break;

                case MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_MATRIX_COLUMNS:
                    param.class_ = DXEffectObject.D3DXPARAMETER_CLASS.MATRIX_COLUMNS;
                    break;

                default:
                    throw new Exception("Unsupported parameter class!");
            }

            switch (symbol.info.parameter_type)
            {
                case MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_BOOL:
                    param.type = DXEffectObject.D3DXPARAMETER_TYPE.BOOL;
                    break;

                case MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_FLOAT:
                    param.type = DXEffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_INT:
                    param.type = DXEffectObject.D3DXPARAMETER_TYPE.INT;
                    break;

                default:
                    throw new Exception("Unsupported parameter type!");
            }

            // HACK: We don't have real default parameters from mojoshader! 
            param.data = new byte[param.rows * param.columns * 4];

            param.member_count = symbol.info.member_count;
            param.element_count = symbol.info.elements > 1 ? symbol.info.elements : 0;

            if (param.member_count > 0)
            {
                param.member_handles = new DXEffectObject.d3dx_parameter[param.member_count];

                var members = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_symbol>(
                    symbol.info.members, (int)symbol.info.member_count);

                for (var i = 0; i < param.member_count; i++)
                {
                    var mparam = GetParameterFromSymbol(members[i]);
                    param.member_handles[i] = mparam;
                }
            }
            else
            {
                param.member_handles = new DXEffectObject.d3dx_parameter[param.element_count];
                for (var i = 0; i < param.element_count; i++)
                {
                    var mparam = new DXEffectObject.d3dx_parameter();

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

            return param;
        }

        private static DXEffectObject.d3dx_parameter GetParameterFromType(SharpDX.D3DCompiler.ShaderReflectionType type)
        {
            var param = new DXEffectObject.d3dx_parameter();
            param.rows = (uint)type.Description.RowCount;
            param.columns = (uint)type.Description.ColumnCount;
            param.name = type.Description.Name ?? string.Empty;
            param.semantic = string.Empty;
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

            param.member_count = (uint)type.Description.MemberCount;
            param.element_count = (uint)type.Description.ElementCount;

            if (param.member_count > 0)
            {
                param.member_handles = new DXEffectObject.d3dx_parameter[param.member_count];
                for (var i = 0; i < param.member_count; i++)
                {
                    var mparam = GetParameterFromType(type.GetMemberType(i));
                    mparam.name = type.GetMemberTypeName(i) ?? string.Empty;
                    param.member_handles[i] = mparam;
                }
            }
            else
            {
                param.member_handles = new DXEffectObject.d3dx_parameter[param.element_count];
                for (var i = 0; i < param.element_count; i++)
                {
                    var mparam = new DXEffectObject.d3dx_parameter();

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

        public void Write(BinaryWriter writer, Options options)
        {
            if (!options.DX11Profile)
                writer.Write(Name);

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
