using System;
using System.Collections.Generic;
using System.IO;

namespace TwoMGFX
{
    public class ShaderInfo
	{
		public string fileName { get; private set; }
		public string fileContent { get; private set; }

        public ShaderProfile Profile { get; private set; }

		public bool Debug { get; private set; }

		public List<TechniqueInfo> Techniques = new List<TechniqueInfo>();
        public Dictionary<string, SamplerStateInfo> SamplerStates = new Dictionary<string, SamplerStateInfo>();

        public List<string> Dependencies { get; private set; }

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
			if (options.Profile == ShaderProfile.DirectX_11)
			{
				macros.Add(new SharpDX.Direct3D.ShaderMacro("HLSL", 1));
				macros.Add(new SharpDX.Direct3D.ShaderMacro("SM4", 1));
			}
            else if (options.Profile == ShaderProfile.OpenGL)
            {
                macros.Add(new SharpDX.Direct3D.ShaderMacro("GLSL", 1));
                macros.Add(new SharpDX.Direct3D.ShaderMacro("OPENGL", 1));
            }
            else if (options.Profile == ShaderProfile.PlayStation4)
            {
            }

			// If we're building shaders for debug set that flag too.
			if (options.Debug)
				macros.Add(new SharpDX.Direct3D.ShaderMacro("DEBUG", 1));

			// Use the D3DCompiler to pre-process the file resolving 
			// all #includes and macros.... this even works for GLSL.
			string newFile;
		    var full = Path.GetFullPath(filePath);
		    var dir = Path.GetDirectoryName(full);
		    var dependencies = new List<string>();
            using (var includer = new CompilerInclude(Path.GetDirectoryName(Path.GetFullPath(filePath)), dependencies))
                newFile = SharpDX.D3DCompiler.ShaderBytecode.Preprocess(effectSource, macros.ToArray(), includer, Path.GetFullPath(filePath));

			// Parse the resulting file for techniques and passes.
			var tree = new Parser(new Scanner()).Parse(newFile, filePath);
			if (tree.Errors.Count > 0)
			{
                var errors = String.Empty;
                foreach (var error in tree.Errors)
                    errors += string.Format("{0}({1},{2}) : {3}\r\n", error.File, error.Line, error.Column, error.Message);

				throw new Exception(errors);
			}

			// Evaluate the results of the parse tree.
			var result = tree.Eval() as ShaderInfo;
		    result.Dependencies = dependencies;
			result.fileName = filePath;
			result.fileContent = newFile;

            // Remove empty techniques.
            for (var i=0; i < result.Techniques.Count; i++)
            {
                var tech = result.Techniques[i];
                if (tech.Passes.Count <= 0)
                {
                    result.Techniques.RemoveAt(i);
                    i--;
                }
            }

            // We must have at least one technique.
            if (result.Techniques.Count <= 0)
                throw new Exception("The effect must contain at least one technique and pass!");

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

			result.Profile = options.Profile;
			result.Debug = options.Debug;

			return result;
		}

	}
}
