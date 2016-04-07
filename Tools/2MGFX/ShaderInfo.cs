using System;
using System.Collections.Generic;
using System.IO;

namespace TwoMGFX
{
    public class ShaderInfo
	{
		public string FilePath { get; private set; }

		public string FileContent { get; private set; }

		public ShaderProfile Profile { get; private set; }

		public string OutputFilePath { get; private set; }

		public bool Debug { get; private set; }

		public List<TechniqueInfo> Techniques = new List<TechniqueInfo>();
        public Dictionary<string, SamplerStateInfo> SamplerStates = new Dictionary<string, SamplerStateInfo>();

        public List<string> Dependencies { get; private set; }

        public List<string> AdditionalOutputFiles { get; private set; }

        static public ShaderInfo FromFile(string path, Options options, IEffectCompilerOutput output)
		{
			var effectSource = File.ReadAllText(path);
			return FromString(effectSource, path, options, output);
		}

		static public ShaderInfo FromString(string effectSource, string filePath, Options options, IEffectCompilerOutput output)
		{
			var macros = new Dictionary<string, string>();
			macros.Add("MGFX", "1");

			// Under the DX11 profile we pass a few more macros.
			if (options.Profile == ShaderProfile.DirectX_11)
			{
				macros.Add("HLSL", "1");
				macros.Add("SM4", "1");
			}
            else if (options.Profile == ShaderProfile.OpenGL)
            {
                macros.Add("GLSL", "1");
                macros.Add("OPENGL", "1");
            }
            else if (options.Profile == ShaderProfile.PlayStation4)
            {
                throw new NotSupportedException("PlayStation 4 support isn't available in this build.");
            }

			// If we're building shaders for debug set that flag too.
			if (options.Debug)
				macros.Add("DEBUG", "1");

		    if (!string.IsNullOrEmpty(options.Defines))
		    {
		        var defines = options.Defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var define in defines)
                    macros.Add(define, "1");
		    }

		    // Use the D3DCompiler to pre-process the file resolving 
			// all #includes and macros.... this even works for GLSL.
			string newFile;
		    var fullPath = Path.GetFullPath(filePath);
		    var dependencies = new List<string>();
		    newFile = Preprocessor.Preprocess(effectSource, fullPath, macros, dependencies, output);

			// Parse the resulting file for techniques and passes.
            var tree = new Parser(new Scanner()).Parse(newFile, fullPath);
			if (tree.Errors.Count > 0)
			{
                var errors = String.Empty;
                foreach (var error in tree.Errors)
                    errors += string.Format("{0}({1},{2}) : {3}\r\n", error.File, error.Line, error.Column, error.Message);

				throw new Exception(errors);
			}

            // Evaluate the results of the parse tree.
            var result = tree.Eval() as ShaderInfo;

            // Remove the samplers and techniques so that the shader compiler
            // gets a clean file without any FX file syntax in it.
            var cleanFile = newFile;
            ParseTreeTools.WhitespaceNodes(TokenType.Technique_Declaration, tree.Nodes, ref cleanFile);
            ParseTreeTools.WhitespaceNodes(TokenType.Sampler_Declaration_States, tree.Nodes, ref cleanFile);

            // Setup the rest of the shader info.
            result.Dependencies = dependencies;
            result.FilePath = fullPath;
            result.FileContent = cleanFile;
            if (!string.IsNullOrEmpty(options.OutputFile))
                result.OutputFilePath = Path.GetFullPath(options.OutputFile);
            result.AdditionalOutputFiles = new List<string>();

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

			result.Profile = options.Profile;
			result.Debug = options.Debug;

			return result;
		}

	}
}
