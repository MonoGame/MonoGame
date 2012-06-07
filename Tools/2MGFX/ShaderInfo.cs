using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TwoMGFX
{
    public class PassInfo
    {
        public string name;

        public string vsModel;
        public string vsFunction;

        public string psModel;
        public string psFunction;
    }

    public class TechniqueInfo
    {
        public int startPos;
        public int length;

        public string name;
        public List<PassInfo> Passes = new List<PassInfo>();
    }

    public class ShaderInfo
    {        
        public string fileName;
        public string fileContent;

        public bool DX11Profile;

        public List<TechniqueInfo> Techniques = new List<TechniqueInfo>();


        static public ShaderInfo FromFile(string path, Options options)
        {
            var effectSource = File.ReadAllText(path);
            return FromString(effectSource, path, options);
        }

        static public ShaderInfo FromString(string effectSource, string filePath, Options options)
        {
            var macros = new List<SharpDX.Direct3D.ShaderMacro>();
            macros.Add(new SharpDX.Direct3D.ShaderMacro("MGFX", 1));

            // Under the DX11 profile we pass a few more macros.
            if (options.DX11Profile)
            {
                macros.Add(new SharpDX.Direct3D.ShaderMacro("HLSL", 1));
                macros.Add(new SharpDX.Direct3D.ShaderMacro("SM4", 1));
            }

            // Use the D3DCompiler to pre-process the file resolving 
            // all #includes and macros.... this even works for GLSL.
            string newFile;
            using (var includer = new CompilerInclude())
                newFile = SharpDX.D3DCompiler.ShaderBytecode.Preprocess(effectSource, macros.ToArray(), includer);

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
                    errors += string.Format("{0}({1},{2}) : {3}\r\n", filePath, line, col, error.Message);
                }

                throw new Exception(errors);
            }

            // Evaluate the results of the parse tree.
            var result = tree.Eval() as ShaderInfo;
            result.fileName = filePath;
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

            result.DX11Profile = options.DX11Profile;
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
