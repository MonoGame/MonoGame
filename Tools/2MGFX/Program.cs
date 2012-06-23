using System;
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
            
            // TODO: This would be where we would decide the user
            // is trying to convert an FX file to a MGFX glsl file.
            //
            // For now we assume we're going right to a compiled MGFXO file.

            // Parse the MGFX file expanding includes, macros, and returning the techniques.
            ShaderInfo shaderInfo;
            try
            {
                shaderInfo = ShaderInfo.FromFile(options.SourceFile, options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to parse the input file '{0}'!", options.SourceFile);
                Console.Error.WriteLine(ex.Message);
                return 1;
            }

            // Create the effect object.
            DXEffectObject effect;
            try
            {
                effect = DXEffectObject.FromShaderInfo(shaderInfo);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Fatal exception when creating the effect!");
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
