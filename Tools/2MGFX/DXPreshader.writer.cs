using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	internal partial class DXPreshader
	{
        private int _temp_count;
        private int _input_count;

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

            var preshader = DXHelper.Unmarshal<MojoShader.MOJOSHADER_preshader>(parseData.preshader);

            return CreatePreshader(preshader);
        }

        public static DXPreshader CreatePreshader(MojoShader.MOJOSHADER_preshader preshaderData)
        {
            var symbols = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_symbol>(
                    preshaderData.symbols, (int)preshaderData.symbol_count);

            var instructions = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_preshaderInstruction>(
                preshaderData.instructions, (int)preshaderData.instruction_count);

            var literals = DXHelper.UnmarshalArray<double>(
                preshaderData.literals, (int)preshaderData.literal_count);


            var preshader = new DXPreshader();

            preshader._temp_count = (int)preshaderData.temp_count;
            preshader._symbols = symbols;
            preshader._instructions = instructions;
            preshader._literals = literals;

            var input_count = 0;
            foreach (var symbol in symbols)
                input_count = Math.Max(input_count, (int)(symbol.register_index + symbol.register_count));            
            preshader._input_count = input_count;

            return preshader;
        }

        //TODO: Fix mojoshader to handle output registers properly
        // TODO TODO:  What does this todo mean?

        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)_input_count);
            writer.Write((byte)_temp_count);

            writer.Write((byte)_symbols.Length);
            foreach (var symbol in _symbols)
            {
		        writer.Write(symbol.name);
		        writer.Write((byte)symbol.register_set);
		        writer.Write((ushort)symbol.register_index);
		        writer.Write((byte)symbol.register_count);
            }

            writer.Write((byte)_literals.Length);
            foreach (var literal in _literals)
                writer.Write(literal);

            writer.Write((byte)_instructions.Length);
            foreach (var inst in _instructions)
            {
                writer.Write((byte)inst.opcode);
                writer.Write((byte)inst.element_count);
                writer.Write((byte)inst.operand_count);

                for (var o = 0; o < inst.operand_count; o++)
                {
                    var op = inst.operands[o];
                    writer.Write((byte)op.type);
                    writer.Write((byte)op.index);
                    writer.Write((byte)op.indexingType);
                    writer.Write((byte)op.indexingIndex);
                }
            }
        }
	}
}

