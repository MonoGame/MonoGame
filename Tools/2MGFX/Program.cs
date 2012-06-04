using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace TwoMGFX
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var options = new Options();
            var parser = new Utilities.CommandLineParser(options);
            parser.Title = "2MGFX - Converts Microsoft FX files to a compiled MonoGame Effect.";

            if (!parser.ParseCommandLine(args))
                return 1;

            // Validate the input file exits.
            if (!File.Exists(options.SourceFile))
            {
                Console.Error.WriteLine("The input file '{0}' was not found!", options.SourceFile);
                return 1;
            }

            // Compile the effect file.
            SharpDX.D3DCompiler.ShaderBytecode effectByteCode;
            try
            {
                var profile = "fx_2_0";
                SharpDX.Direct3D.ShaderMacro[] macros = null;

                if (options.DX11Profile)
                {
                    profile = "fx_5_0";
                    macros = new[] { new SharpDX.Direct3D.ShaderMacro("SM4", 1) };
                }

                SharpDX.D3DCompiler.ShaderFlags shaderFlags = 0;
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.EnableBackwardsCompatibility;
                shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.OptimizationLevel3;
                shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.NoPreshader;
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.PackMatrixRowMajor;
                shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.WarningsAreErrors;
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.SkipValidation;
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.Debug;

                SharpDX.D3DCompiler.EffectFlags effectFlags = SharpDX.D3DCompiler.EffectFlags.None;

                // First compile the effect into bytecode.                
                using (var includer = new CompilerInclude())
                {
                    var result = SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile(options.SourceFile, profile, shaderFlags, effectFlags, macros, includer);
                    if (result.HasErrors)
                        throw new Exception(result.Message);

                    effectByteCode = result.Bytecode;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to compile the input file '{0}'!", options.SourceFile);
                Console.Error.WriteLine(ex.Message);
                return 1;
            }

            // Parse the effect byte code.
            DXEffectObject effect;
            try
            {
                if (options.DX11Profile)
                    effect = DXEffectObject.FromDX10Effect(effectByteCode);
                else
                {
                    var byteCode = DXHelper.UnmarshalArray(effectByteCode.BufferPointer, effectByteCode.BufferSize);
                    effect = DXEffectObject.FromCompiledD3DXEffect(byteCode);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Fatal exception when parsing the compiled Microsoft Effect!");
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }

            // Get the output file path.
            if ( options.OutputFile == string.Empty )
                options.OutputFile = Path.GetFileNameWithoutExtension(options.SourceFile) + ".mgfxo";

            // Write out the effect to a runtime format.
            try
            {
                using (var stream = new FileStream(options.OutputFile, FileMode.Create, FileAccess.Write))
                using (var writer = new BinaryWriter(stream))
                    effect.Write(writer, options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to write the output file '{0}'!", options.OutputFile);
                Console.Error.WriteLine(ex.Message);
                return 1;
            }

            // We finished succesfully.
            Console.WriteLine("Compiled '{0}' to '{1}'.", options.SourceFile, options.OutputFile);
            return 0;
        }

    }
}
