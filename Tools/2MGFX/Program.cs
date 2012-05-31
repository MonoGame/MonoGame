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
            var options = new Options();
            var parser = new Utilities.CommandLineParser(options);
            parser.Title = "2MGFX - Converts Microsoft FX files to a compiled MonoGame Effect.";

            if (!parser.ParseCommandLine(args))
                return 1;

            // Validate the input file exits.
            if (!File.Exists(options.SourceFile))
            {
                Console.WriteLine("The input file '{0}' was not found!", options.SourceFile);
                return 1;
            }
            
            // TODO: This would be where we would decide the user
            // is trying to convert an FX file to a MGFX glsl file.
            //
            // For now we assume we're going right to a compiled MGFXO file.

            var macros = new List<SharpDX.Direct3D.ShaderMacro>();
            macros.Add(new SharpDX.Direct3D.ShaderMacro("MGFX", 1));

            // Under the DX11 profile we pass a few more macros.
            if (options.DX11Profile)
            {
                macros.Add(new SharpDX.Direct3D.ShaderMacro("HLSL", 1));
                macros.Add(new SharpDX.Direct3D.ShaderMacro("SM4", 1));
            }

            // Parse the MGFX file expanding includes, macros, and returning the techniques.
            ShaderInfo shaderInfo;
            try
            {
                shaderInfo = ParseFile(options.SourceFile, macros);
                shaderInfo.DX11Profile = options.DX11Profile;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to parse the input file '{0}'!", options.SourceFile);
                Console.WriteLine(ex.Message);
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
                Console.WriteLine("Fatal exception when creating the effect!");
                Console.WriteLine(ex.ToString());
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
                Console.WriteLine("Failed to write the output file '{0}'!", options.OutputFile);
                Console.WriteLine(ex.Message);
                return 1;
            }

            // We finished succesfully.
            Console.WriteLine("Compiled '{0}' to '{1}'.", options.SourceFile, options.OutputFile);
            return 0;
        }

        static private ShaderInfo ParseFile(string path, List<SharpDX.Direct3D.ShaderMacro> macros)
        {
            // Use the D3DCompiler to pre-process the file resolving 
            // all #includes and macros.... this even works for GLSL.
            string newFile;
            using (var includer = new CompilerInclude())
                newFile = SharpDX.D3DCompiler.ShaderBytecode.PreprocessFromFile(path, macros.ToArray(), includer);

            // Parse the resulting file for techniques and passes.
            var tree = new Parser(new Scanner()).Parse(newFile);
            if (tree.Errors.Count > 0)
            {
                // TODO: Make the error info pretty!
                var errors = String.Empty;
                foreach (var error in tree.Errors)
                {
                    int line, col;
                    FindLineAndCol(newFile, error.Position, out line, out col);
                    errors += string.Format("{0}({1},{2}) : {3}\r\n", path, line, col, error.Message);
                }

                throw new Exception(errors);
            }

            // Evaluate the results of the parse tree.
            var result = tree.Eval() as ShaderInfo;
            result.fileName = path;
            result.fileContent = newFile;

            // Finally remove the techniques from the file.
            //
            // TODO: Do we really need to do this, or will the HLSL 
            // compiler just ignore it as we compile shaders?
            //
            /*
            var extra = 2;
            var offset = 0;
            foreach (var tech in result.Techniques)
            {
                // Remove the technique from the file.
                newFile = newFile.Remove(tech.startPos + offset, tech.length + extra);
                offset -= tech.length + extra;

                techniques.Add(tech);
            }
            */

            return result;
        }

        public static void FindLineAndCol(string src, int pos, out int line, out int col)
        {
            line = 1;
            col = 1;

            for (var i = 0; i < pos; i++)
            {
                if (src[i] == '\n')
                {
                    line++;
                    col = 1;
                }
                else
                {
                    col++;
                }
            }
        }

    }
}
