using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
	internal partial class DXPreshader
	{
        private MojoShader.MOJOSHADER_symbol[] _symbols;
        private MOJOSHADER_preshaderInstruction[] _instructions;
        private double[] _literals;
        private float[] _inRegs;
        private double[] _temps;
        		
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
            public MOJOSHADER_preshaderOperand operand3;

            public MOJOSHADER_preshaderOperand operand(int index)
            {
                switch (index)
                {
                    case 0:
                        return operand0;
                    case 1:
                        return operand1;
                    case 2:
                        return operand2;
                    default:
                    case 3:
                        return operand3;
                }
            }

            public void operand(int index, MOJOSHADER_preshaderOperand op)
            {
                switch (index)
                {
                    case 0:
                        operand0 = op;
                        break;
                    case 1:
                        operand1 = op;
                        break;
                    case 2:
                        operand2 = op;
                        break;
                    default:
                    case 3:
                        operand3 = op;
                        break;
                }
            }
        };
	}
}

