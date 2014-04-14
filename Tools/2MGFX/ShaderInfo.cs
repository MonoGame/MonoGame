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

	    private static Blend ToAlphaBlend(Blend blend)
	    {
	        switch (blend)
	        {
	            case Blend.SourceColor:
	                return Blend.SourceAlpha;
	            case Blend.InverseSourceColor:
	                return Blend.InverseSourceAlpha;
	            case Blend.DestinationColor:
	                return Blend.DestinationAlpha;
	            case Blend.InverseDestinationColor:
	                return Blend.InverseDestinationAlpha;
	        }
	        return blend;
	    }

        public void ParseRenderState(string name, string value)
        {
            Blend blend;

            switch (name.ToLower())
            {
                case "alphablendenable":
                    if (!ParseTreeTools.ParseBool(value))
                    {
                        if (blendState == null)
                            blendState = new BlendState();
                        blendState.ColorSourceBlend = Blend.One;
                        blendState.AlphaSourceBlend = Blend.One;
                        blendState.ColorDestinationBlend = Blend.Zero;
                        blendState.AlphaDestinationBlend = Blend.Zero;
                    }
                    break;
                case "srcblend":
                    blend = ParseTreeTools.ParseBlend(value);
                    if (blendState == null)
                        blendState = new BlendState();
                    blendState.ColorSourceBlend = blend;
                    blendState.AlphaSourceBlend = ToAlphaBlend(blend);
                    break;
                case "destblend":
                    blend = ParseTreeTools.ParseBlend(value);
                    if (blendState == null)
                        blendState = new BlendState();
                    blendState.ColorDestinationBlend = blend;
                    blendState.AlphaDestinationBlend = ToAlphaBlend(blend);
                    break;
                case "blendop":
                    if (blendState == null)
                        blendState = new BlendState();
                    blendState.AlphaBlendFunction = ParseTreeTools.ParseBlendFunction(value);
                    break;
                case "zenable":
                    if (depthStencilState == null)
                        depthStencilState = new DepthStencilState();
                    depthStencilState.DepthBufferEnable = ParseTreeTools.ParseBool(value);
                    break;
                case "zwriteenable":
                    if (depthStencilState == null)
                        depthStencilState = new DepthStencilState();
                    depthStencilState.DepthBufferWriteEnable = ParseTreeTools.ParseBool(value);
                    break;
                case "depthbias":
                    if (rasterizerState == null)
                        rasterizerState = new RasterizerState();
                    rasterizerState.DepthBias = float.Parse(value);
                    break;
                case "cullmode":
                    if (rasterizerState == null)
                        rasterizerState = new RasterizerState();
                    rasterizerState.CullMode = ParseTreeTools.ParseCullMode(value);
                    break;
                case "fillmode":
                    if (rasterizerState == null)
                        rasterizerState = new RasterizerState();
                    rasterizerState.FillMode = ParseTreeTools.ParseFillMode(value);
                    break;
                case "multisampleantialias":
                    if (rasterizerState == null)
                        rasterizerState = new RasterizerState();
                    rasterizerState.MultiSampleAntiAlias = ParseTreeTools.ParseBool(value);
                    break;
                case "slopescaledepthbias":
                    if (rasterizerState == null)
                        rasterizerState = new RasterizerState();
                    rasterizerState.SlopeScaleDepthBias = float.Parse(value);
                    break;
            }            
        }
	}

	public enum TextureFilterType
	{
		Linear, Anisotropic, Point
	}

	public class SamplerStateInfo
	{
        private SamplerState _state;
        
        private bool _dirty;

        private TextureFilterType _minFilter;
        private TextureFilterType _magFilter;
        private TextureFilterType _mipFilter;

        private TextureAddressMode _addressU;
        private TextureAddressMode _addressV;
        private TextureAddressMode _addressW;

	    private int _maxAnisotropy;
        private int _maxMipLevel;
	    private float _mipMapLevelOfDetailBias;

        public string Name { get; set; }

        public string TextureName { get; set; }

	    public TextureFilterType MinFilter
	    {
            set
            {
                _minFilter = value;
                _dirty = true;
            }
	    }

        public TextureFilterType MagFilter
        {
            set
            {
                _magFilter = value;
                _dirty = true;
            }
        }

        public TextureFilterType MipFilter
        {
            set
            {
                _mipFilter = value;
                _dirty = true;
            }
        }

        public TextureAddressMode AddressU
        {
            set
            {
                _addressU = value;
                _dirty = true;
            }
        }

        public TextureAddressMode AddressV
        {
            set
            {
                _addressV = value;
                _dirty = true;
            }
        }

        public TextureAddressMode AddressW
        {
            set
            {
                _addressW = value;
                _dirty = true;
            }
        }

        public int MaxAnisotropy
        {
            set
            {
                _maxAnisotropy = value;
                _dirty = true;
            }
        }

        public int MaxMipLevel
        {
            set
            {
                _maxMipLevel = value;
                _dirty = true;
            }
        }

        public float MipMapLevelOfDetailBias
        {
            set
            {
                _mipMapLevelOfDetailBias = value;
                _dirty = true;
            }
        }

        private void UpdateSamplerState()
        {
            // Get the existing state or create it.
            if (_state == null)
                _state = new SamplerState();

            _state.AddressU = _addressU;
            _state.AddressV = _addressV;
            _state.AddressW = _addressW;

            _state.MaxAnisotropy = _maxAnisotropy;
            _state.MaxMipLevel = _maxMipLevel;
            _state.MipMapLevelOfDetailBias = _mipMapLevelOfDetailBias;

            // Figure out what kind of filter to set based on each
            // individual min, mag, and mip filter settings.
            if (_minFilter == TextureFilterType.Anisotropic)
                _state.Filter = TextureFilter.Anisotropic;
            else if (_minFilter == TextureFilterType.Linear && _magFilter == TextureFilterType.Linear && _mipFilter == TextureFilterType.Linear)
                _state.Filter = TextureFilter.Linear;
            else if (_minFilter == TextureFilterType.Linear && _magFilter == TextureFilterType.Linear && _mipFilter == TextureFilterType.Point)
                _state.Filter = TextureFilter.LinearMipPoint;
            else if (_minFilter == TextureFilterType.Linear && _magFilter == TextureFilterType.Point && _mipFilter == TextureFilterType.Linear)
                _state.Filter = TextureFilter.MinLinearMagPointMipLinear;
            else if (_minFilter == TextureFilterType.Linear && _magFilter == TextureFilterType.Point && _mipFilter == TextureFilterType.Point)
                _state.Filter = TextureFilter.MinLinearMagPointMipPoint;
            else if (_minFilter == TextureFilterType.Point && _magFilter == TextureFilterType.Linear && _mipFilter == TextureFilterType.Linear)
                _state.Filter = TextureFilter.MinPointMagLinearMipLinear;
            else if (_minFilter == TextureFilterType.Point && _magFilter == TextureFilterType.Linear && _mipFilter == TextureFilterType.Point)
                _state.Filter = TextureFilter.MinPointMagLinearMipPoint;
            else if (_minFilter == TextureFilterType.Point && _magFilter == TextureFilterType.Point && _mipFilter == TextureFilterType.Point)
                _state.Filter = TextureFilter.Point;
            else if (_minFilter == TextureFilterType.Point && _magFilter == TextureFilterType.Point && _mipFilter == TextureFilterType.Linear)
                _state.Filter = TextureFilter.PointMipLinear;

            _dirty = false;
        }

        public void Parse(string name, string value)
        {
            switch (name.ToLower())
            {
                case "texture":
                    TextureName = value;
                    break;
                case "minfilter":
                    MinFilter = ParseTreeTools.ParseTextureFilterType(value);
                    break;
                case "magfilter":
                    MagFilter = ParseTreeTools.ParseTextureFilterType(value);
                    break;
                case "mipfilter":
                    MipFilter = ParseTreeTools.ParseTextureFilterType(value);
                    break;
                case "filter":
                    MinFilter = MagFilter = MipFilter = ParseTreeTools.ParseTextureFilterType(value);
                    break;
                case "addressu":
                    AddressU = ParseTreeTools.ParseAddressMode(value);
                    break;
                case "addressv":
                    AddressV = ParseTreeTools.ParseAddressMode(value);
                    break;
                case "addressw":
                    AddressW = ParseTreeTools.ParseAddressMode(value);
                    break;
                case "maxanisotropy":
                    MaxAnisotropy = int.Parse(value);
                    break;
                case "maxlod":
                    MaxMipLevel = int.Parse(value);
                    break;
                case "miplodbias":
                    MipMapLevelOfDetailBias = float.Parse(value);
                    break;
            }            
        }

        public SamplerState State
	    {
	        get
	        {
	            if (_dirty)
                    UpdateSamplerState();

	            return _state;
	        }
	    }
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

			result.DX11Profile = options.DX11Profile;
			result.Debug = options.Debug;

			return result;
		}

	}
}
