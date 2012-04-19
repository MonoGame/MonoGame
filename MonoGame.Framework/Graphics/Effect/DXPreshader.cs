using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXPreshader
	{
		private MojoShader.MOJOSHADER_preshader _preshader;
        private MojoShader.MOJOSHADER_symbol[] _symbols;
        private MOJOSHADER_preshaderInstruction[] _instructions;
        private double[] _literals;
        private float[] _inRegs;

        public static DXPreshader CreatePreshader(byte[] expressionData)
        {
            var parseDataPtr = MojoShader.NativeMethods.MOJOSHADER_parseExpression(
                expressionData,
                expressionData.Length,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero
            );

            var parseData = DXHelper.Unmarshal<MojoShader.MOJOSHADER_parseData>(parseDataPtr);
            if (parseData.error_count > 0)
            {
                var errors = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_error>(parseData.errors, parseData.error_count);
                throw new Exception(errors[0].error);
            }

            return CreatePreshader(parseData.preshader);
        }

        public static DXPreshader CreatePreshader(IntPtr preshaderPtr)
        {
            var preshader = DXHelper.Unmarshal<MojoShader.MOJOSHADER_preshader>(preshaderPtr);

            var symbols = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_symbol>(
                    preshader.symbols, (int)preshader.symbol_count);

            var instructions = DXHelper.UnmarshalArray<MOJOSHADER_preshaderInstruction>(
                preshader.instructions, (int)preshader.instruction_count);

            var literals = DXHelper.UnmarshalArray<double>(
                preshader.literals, (int)preshader.literal_count);

            return new DXPreshader(preshader, symbols, instructions, literals);
        }

		//TODO: Fix mojoshader to handle output registers properly
        // TODO TODO:  What does this todo mean?

        private DXPreshader(    MojoShader.MOJOSHADER_preshader preshader, 
                                MojoShader.MOJOSHADER_symbol [] symbols,
                                MOJOSHADER_preshaderInstruction [] instructions,
                                double [] literals )
		{
            _preshader = preshader;
            _symbols = symbols;
            _instructions = instructions;
            _literals = literals;

			var input_count = 0;
            foreach (var symbol in _symbols)
				input_count = Math.Max(input_count, (int)(symbol.register_index+symbol.register_count));

            _inRegs = new float[4 * input_count];
		}
		
		public void Run(EffectParameterCollection parameters, float[] outRegs) 
        {
			//todo: cleanup and merge with DXShader :/
			
			//only populate modified stuff?
            foreach (var symbol in _symbols) 
            {
				if (symbol.register_set != MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4)
					throw new NotImplementedException();

                var parameter = parameters[symbol.name];
				var data = parameter.GetValueSingleArray();
				
				switch (parameter.ParameterClass) 
                {
				case EffectParameterClass.Scalar:
					for (var i=0; i<data.Length; i++) 
                    {
						// Preshader scalar arrays map one to each vec4 register
                        _inRegs[symbol.register_index * 4 + i * 4] = (float)data[i];
					}
					break;
				case EffectParameterClass.Vector:
				case EffectParameterClass.Matrix:
					if (parameter.Elements.Count > 0) 
						throw new NotImplementedException();

					for (var y=0; y<Math.Min (symbol.register_count, parameter.RowCount); y++)
                        for (var x = 0; x < parameter.ColumnCount; x++)
                            _inRegs[(symbol.register_index + y) * 4 + x] = (float)data[y * 4 + x];
					break;
				default:
					throw new NotImplementedException();
				}
			}

            RunPreshader(_preshader, _inRegs, outRegs);
            //MojoShader.NativeMethods.MOJOSHADER_runPreshader(ref _preshader, _inRegs, outRegs);
		}

        enum MOJOSHADER_preshaderOpcode
        {
            NOP,
            MOV,
            NEG,
            RCP,
            FRC,
            EXP,
            LOG,
            RSQ,
            SIN,
            COS,
            ASIN,
            ACOS,
            ATAN,
            MIN,
            MAX,
            LT,
            GE,
            ADD,
            MUL,
            ATAN2,
            DIV,
            CMP,
            MOVC,
            DOT,
            NOISE,
            SCALAR_OPS,
            MIN_SCALAR = SCALAR_OPS,
            MAX_SCALAR,
            LT_SCALAR,
            GE_SCALAR,
            ADD_SCALAR,
            MUL_SCALAR,
            ATAN2_SCALAR,
            DIV_SCALAR,
            DOT_SCALAR,
            NOISE_SCALAR,
        };

        enum MOJOSHADER_preshaderOperandType
        {
            MOJOSHADER_PRESHADEROPERAND_LITERAL = 1,
            MOJOSHADER_PRESHADEROPERAND_INPUT = 2,
            MOJOSHADER_PRESHADEROPERAND_OUTPUT = 4,
            MOJOSHADER_PRESHADEROPERAND_TEMP = 7,
            MOJOSHADER_PRESHADEROPERAND_UNKN = 0xff,
        };

        [StructLayoutAttribute(LayoutKind.Sequential)]
        struct MOJOSHADER_preshaderOperand
        {
            public MOJOSHADER_preshaderOperandType type;
            public uint index;
	        public int indexingType;
            public uint indexingIndex;
        };

        [StructLayoutAttribute(LayoutKind.Sequential)]
        struct MOJOSHADER_preshaderInstruction
        {
            public MOJOSHADER_preshaderOpcode opcode;
            public uint element_count;
            public uint operand_count;
            public MOJOSHADER_preshaderOperand operand0;
            public MOJOSHADER_preshaderOperand operand1;
            public MOJOSHADER_preshaderOperand operand2;

            public MOJOSHADER_preshaderOperand operand(int index)
            {
                switch (index)
                {
                    case 0:
                        return operand0;
                    case 1:
                        return operand1;
                    default:
                    case 2:
                        return operand2;
                }
            }
        };

        private void RunPreshader(MojoShader.MOJOSHADER_preshader preshader, float[] inRegs, float[] outRegs)
        {
            // this is fairly straightforward, as there aren't any branching
            //  opcodes in the preshader instruction set (at the moment, at least).
            const int scalarstart = (int)MOJOSHADER_preshaderOpcode.SCALAR_OPS;

            var temps = new double[preshader.temp_count];
            
            var dst = new double [] { 0, 0, 0, 0 };
            var src = new double [,] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };

            for (var ip = 0; ip < _instructions.Length; ip++)
            {
                var inst = _instructions[ip];

                //MOJOSHADER_preshaderOperand operand = inst.operands;
                var elems = inst.element_count;
                //var elemsbytes = sizeof (double) * elems;
                var isscalarop = ((int)inst.opcode >= scalarstart);

                //assert(elems >= 0);
                //assert(elems <= 4);

                var operand = new MOJOSHADER_preshaderOperand();

                // load up our operands...
                int opiter, elemiter;
                for (opiter = 0; opiter < inst.operand_count-1; opiter++)
                {
                    operand = inst.operand(opiter);

                    var isscalar = ((isscalarop) && (opiter == 0));
                    var index = operand.index;
                    switch (operand.type)
                    {
                        case MOJOSHADER_preshaderOperandType.MOJOSHADER_PRESHADEROPERAND_LITERAL:
                        {
                            Debug.Assert((index + elems) <= preshader.literal_count);

                            if (!isscalar)
                                for ( var cpy=0; cpy < elems; cpy++ )
                                    src[opiter, cpy] = _literals[index + cpy];
                            else
                            {
                                for (elemiter = 0; elemiter < elems; elemiter++)
                                    src[opiter, elemiter] = _literals[index];
                            }

                            break;
                        }

                        case MOJOSHADER_preshaderOperandType.MOJOSHADER_PRESHADEROPERAND_INPUT:
                            if (operand.indexingType == 2)
                                index += (uint)inRegs[operand.indexingIndex] * 4;
                            if (isscalar)
                                src[opiter, 0] = inRegs[index];
                            else
                            {
                                for (var cpy = 0; cpy < elems; cpy++)
                                    src[opiter, cpy] = inRegs[index + cpy];
                            }
                            break;

                        case MOJOSHADER_preshaderOperandType.MOJOSHADER_PRESHADEROPERAND_OUTPUT:
                            if (isscalar)
                                src[opiter,0] = outRegs[index];
                            else
                            {
                                int cpy;
                                for (cpy = 0; cpy < elems; cpy++)
                                    src[opiter,cpy] = outRegs[index+cpy];
                            }
                            break;

                        case MOJOSHADER_preshaderOperandType.MOJOSHADER_PRESHADEROPERAND_TEMP:
                            if (temps != null)
                            {
                                if (isscalar)
                                    src[opiter,0] = temps[index];
                                else
                                    for ( var cpy=0; cpy < elems; cpy++ )
                                        src[opiter,cpy] = temps[index+cpy];
                            } // if
                            break;

                        default:
                            Debug.Assert(false, "unexpected preshader operand type.");
                            return;

                    }
                }

                // run the actual instruction, store result to dst.
                switch (inst.opcode)
                {
                    case MOJOSHADER_preshaderOpcode.NOP:
                        break;

                    case MOJOSHADER_preshaderOpcode.MOV:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.NEG:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = -src[0,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.RCP:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = 1.0 / src[0,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.FRC:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,i] - Math.Floor(src[0,i]);
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.EXP:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = Math.Exp(src[0,i]);
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.LOG:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = Math.Log(src[0,i]);
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.RSQ:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = 1.0 / Math.Sqrt(src[0,i]);
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.SIN:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = Math.Sin(src[0,i]);
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.COS:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = Math.Cos(src[0,i]);
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.ASIN:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = Math.Asin(src[0,i]);
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.ACOS:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = Math.Acos(src[0,i]);
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.ATAN:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = Math.Atan(src[0,i]);
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.MIN:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,i] < src[1,i] ? src[0,i] : src[1,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.MAX:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,i] > src[1,i] ? src[0,i] : src[1,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.LT:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,i] < src[1,i] ? 1.0 : 0.0;
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.GE:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,i] >= src[1,i] ? 1.0 : 0.0;
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.ADD:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,i] + src[1,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.MUL:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,i] * src[1,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.DIV:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,i] / src[1,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.ATAN2:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = Math.Atan2(src[0,i], src[1,i]);
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.CMP:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,i] >= 0.0 ? src[1,i] : src[2,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.MIN_SCALAR:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,0] < src[1,i] ? src[0,0] : src[1,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.MAX_SCALAR:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,0] > src[1,i] ? src[0,0] : src[1,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.LT_SCALAR:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,0] < src[1,i] ? 1.0 : 0.0;
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.GE_SCALAR:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,0] >= src[1,i] ? 1.0 : 0.0;
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.ADD_SCALAR:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,0] + src[1,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.MUL_SCALAR:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,0] * src[1,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.DIV_SCALAR:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = src[0,0] / src[1,i];
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.ATAN2_SCALAR:
                    {
                        for (var i = 0; i < elems; i++) 
                            dst[i] = Math.Atan2(src[0,0], src[1,i]);
                        break; 
                    }

                    case MOJOSHADER_preshaderOpcode.DOT:
                    {
                        var final = 0.0;
                        for (var i = 0; i < elems; i++)
                            final += src[0,i] * src[1,i];
                        for (var i = 0; i < elems; i++)
                            dst[i] = final;  // !!! FIXME: is this right?

                        break;
                    }

                    case MOJOSHADER_preshaderOpcode.MOVC:
                    case MOJOSHADER_preshaderOpcode.NOISE:
                    case MOJOSHADER_preshaderOpcode.DOT_SCALAR: // Just a MUL?
                    case MOJOSHADER_preshaderOpcode.NOISE_SCALAR:
                    Debug.Fail("Unimplemented preshader opcode!");
                    break;

                    default:
                        Debug.Fail("Unknown preshader opcode!");
                        break;
                }

                operand = inst.operand(opiter);

                // Figure out where dst wants to be stored.
                if (operand.type == MOJOSHADER_preshaderOperandType.MOJOSHADER_PRESHADEROPERAND_TEMP)
                {
                    //assert(preshader->temp_count >= operand->index + (elemsbytes / sizeof (double)));

                    for (var i = 0; i < elems; i++)
                        temps[operand.index + i] = dst[i];
                }
                else
                {
                    Debug.Assert(operand.type == MOJOSHADER_preshaderOperandType.MOJOSHADER_PRESHADEROPERAND_OUTPUT);

                    for (var i = 0; i < elems; i++)
                        outRegs[operand.index + i] = (float)dst[i];
                }
            }
        }
	}
}

