using System;
using System.Collections.Generic;

namespace MonoGame.Effect
{
    internal partial class ConstantBufferData
    {
        public ConstantBufferData (string name,
                                MojoShader.MOJOSHADER_symbolRegisterSet set, 
                                MojoShader.MOJOSHADER_symbol[] symbols)
		{
			Name = name ?? string.Empty;

			ParameterIndex = new List<int> ();
			ParameterOffset = new List<int> ();
			Parameters = new List<EffectObject.d3dx_parameter> ();

			int minRegister = short.MaxValue;
			int maxRegister = 0;

			var registerSize = (set == MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL ? 1 : 4) * 4;

			foreach (var symbol in symbols) {
				if (symbol.register_set != set)
					continue;

				// Create the parameter.
				var parm = GetParameterFromSymbol (symbol);

				var offset = (int)symbol.register_index * registerSize;
				parm.bufferOffset = offset;

				Parameters.Add (parm);
                ParameterOffset.Add(offset);

                minRegister = Math.Min(minRegister, (int)symbol.register_index);
                maxRegister = Math.Max(maxRegister, (int)(symbol.register_index + symbol.register_count));
            }

            Size = Math.Max(maxRegister - minRegister, 0) * registerSize;
        }

        private static EffectObject.d3dx_parameter GetParameterFromSymbol(MojoShader.MOJOSHADER_symbol symbol)
        {
            var param = new EffectObject.d3dx_parameter();
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
                    param.class_ = EffectObject.D3DXPARAMETER_CLASS.SCALAR;
                    break;

                case MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_VECTOR:
                    param.class_ = EffectObject.D3DXPARAMETER_CLASS.VECTOR;
                    break;

                case MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_MATRIX_COLUMNS:
                    param.class_ = EffectObject.D3DXPARAMETER_CLASS.MATRIX_COLUMNS;

                    // MojoShader optimizes matrices to occupy less registers.
                    // This effectively convert a Matrix4x4 into Matrix4x3, Matrix4x2 or Matrix4x1.
                    param.columns = Math.Min(param.columns, symbol.register_count);

                    break;

                default:
                    throw new Exception("Unsupported parameter class!");
            }

            switch (symbol.info.parameter_type)
            {
                case MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_BOOL:
                    param.type = EffectObject.D3DXPARAMETER_TYPE.BOOL;
                    break;

                case MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_FLOAT:
                    param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;

                case MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_INT:
                    param.type = EffectObject.D3DXPARAMETER_TYPE.INT;
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
                param.member_handles = new EffectObject.d3dx_parameter[param.member_count];

                var members = MarshalHelper.UnmarshalArray<MojoShader.MOJOSHADER_symbol>(
                    symbol.info.members, (int)symbol.info.member_count);

                for (var i = 0; i < param.member_count; i++)
                {
                    var mparam = GetParameterFromSymbol(members[i]);
                    param.member_handles[i] = mparam;
                }
            }
            else
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

            return param;
        }
    }
}