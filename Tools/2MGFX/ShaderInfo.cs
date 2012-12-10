using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;

namespace TwoMGFX
{
	public class PassInfo
	{
		public string name;

		public string vsModel;
		public string vsFunction;

		public string psModel;
		public string psFunction;

		public BlendState blendState;
		public RasterizerState rasterizerState;
		public DepthStencilState depthStencilState;
		
        private static readonly Regex _shaderModelRegex = new Regex(@"(vs_|ps_)(1|2|3|4|5)(_)(0|1|)((_level_)(9_1|9_2|9_3))?", RegexOptions.Compiled);

        public static void ParseShaderModel(string text, out int major, out int minor)
        {
            var match = _shaderModelRegex.Match(text);
            if (match.Groups.Count < 5)
            {
                major = 0;
                minor = 0;
                return;
            }

            major = int.Parse(match.Groups[2].Value);
            minor = int.Parse(match.Groups[4].Value);
        }

        public void ValidateShaderModels(bool dx11Profile)
        {
            int major, minor;

            if (!string.IsNullOrEmpty(vsFunction))
            {
                ParseShaderModel(vsModel, out major, out minor);
                if (dx11Profile && major <= 3)
                    throw new Exception(String.Format("Vertex shader '{0}' must be SM 4.0 level 9.1 or higher!", vsFunction));
                if (!dx11Profile && major > 3)
                    throw new Exception(String.Format("Vertex shader '{0}' must be SM 3.0 or lower!", vsFunction));
            }

            if (!string.IsNullOrEmpty(psFunction))
            {
                ParseShaderModel(psModel, out major, out minor);
                if (dx11Profile && major <= 3)
                    throw new Exception(String.Format("Pixel shader '{0}' must be SM 4.0 level 9.1 or higher!", psFunction));
                if (!dx11Profile && major > 3)
                    throw new Exception(String.Format("Pixel shader '{0}' must be SM 3.0 or lower!", psFunction));
            }
        }
	}

	public enum TextureFilterType
	{
		Linear, Anisotropic, Point
	}

	public class SamplerStateInfo
	{
		public string name;
		public SamplerState state;
		public TextureFilterType MinFilter;
		public TextureFilterType MagFilter;
		public TextureFilterType MipFilter;
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
		public string fileName { get; private set; }
		public string fileContent { get; private set; }

		public bool DX11Profile { get; private set; }

		public bool Debug { get; private set; }

		public List<TechniqueInfo> Techniques = new List<TechniqueInfo>();
		public Dictionary<string, SamplerState> SamplerStates = new Dictionary<string, SamplerState>();

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

			// If we're building shaders for debug set that flag too.
			if (options.Debug)
				macros.Add(new SharpDX.Direct3D.ShaderMacro("DEBUG", 1));

			// Use the D3DCompiler to pre-process the file resolving 
			// all #includes and macros.... this even works for GLSL.
			string newFile;
		    var full = Path.GetFullPath(filePath);
		    var dir = Path.GetDirectoryName(full);
			using (var includer = new CompilerInclude(Path.GetDirectoryName(Path.GetFullPath(filePath))))
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

			result.DX11Profile = options.DX11Profile;
			result.Debug = options.Debug;

			return result;
		}

	}
}
