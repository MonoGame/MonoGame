using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace TwoMGFX
{
    static class Program
    {
        static int Main(string[] args)
        {
            // Validate parameters.
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("2MGFX - Converts Microsoft FX files to a compiled MonoGame Effect.");
                Console.WriteLine();
                Console.WriteLine("Usage: 2mgfx <EffectFile> [OutputFile]");
                Console.WriteLine();
                return 0;
            }

            // TODO: Make me an argument!
            var isDirectX = false;

            // Validate the input file exits.
            var inputFilePath = args[0];
            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine("The input file '{0}' was not found!", inputFilePath);
                return -1;
            }

            // Compile the effect file.
            byte[] byteCode;
            SharpDX.D3DCompiler.ShaderBytecode shaderByteCode;
            try
            {
                var profile = "fx_2_0";
                SharpDX.Direct3D.ShaderMacro[] macros = null;

                if (isDirectX)
                {
                    profile = "fx_5_0";
                    macros = new[] { new SharpDX.Direct3D.ShaderMacro("SM4", 1) };
                }

                // First compile the effect into bytecode.                
                using (var includer = new CompilerInclude())
                {
                    var result = SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile(inputFilePath, profile, SharpDX.D3DCompiler.ShaderFlags.OptimizationLevel3, SharpDX.D3DCompiler.EffectFlags.None, macros, includer);
                    if (result.HasErrors)
                        throw new Exception(result.Message);

                    var shader = result.Bytecode;
                    shaderByteCode = shader;

                    //var code = shader.Disassemble();
                    //var reflect = new SharpDX.D3DCompiler.ShaderReflection(shader);

                    byteCode = DXHelper.UnmarshalArray(shader.BufferPointer, shader.BufferSize);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to compile the input file '{0}'!", inputFilePath);
                Console.WriteLine(ex.Message);
                return -1;
            }

            // Parse the effect byte code.
            DXEffectObject effect;
            try
            {
                //DXShader.IsDirectX = isDirectX;

                if (isDirectX)
                    effect = DXEffectObject.FromDX10Effect(shaderByteCode);
                else
                    effect = DXEffectObject.FromCompiledD3DXEffect(byteCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal exception when parsing the compiled Microsoft Effect!");
                Console.WriteLine(ex.ToString());
                return -1;
            }

            // Get the output file path.
            var outputFilePath = Path.GetFileNameWithoutExtension(inputFilePath) + ".mgfx";
            if (args.Length == 2)
                outputFilePath = args[1];

            // Write out the effect to a runtime format.
            try
            {
                using (var stream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                using (var writer = new BinaryWriter(stream))
                    effect.Write(writer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to write the output file '{0}'!", outputFilePath);
                Console.WriteLine(ex.Message);
                return -1;
            }

            // We finished succesfully.
            Console.WriteLine("Compiled '{0}' to '{1}'.", inputFilePath, outputFilePath);
            return 0;
        }

    }
}
