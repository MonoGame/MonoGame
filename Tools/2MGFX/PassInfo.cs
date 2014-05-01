using System;
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
		
        private static readonly Regex _hlslPixelShaderRegex = new Regex(@"^ps_(?<major>1|2|3|4|5)_(?<minor>0|1|)(_level_(9_1|9_2|9_3))?$", RegexOptions.Compiled);
        private static readonly Regex _hlslVertexShaderRegex = new Regex(@"^vs_(?<major>1|2|3|4|5)_(?<minor>0|1|)(_level_(9_1|9_2|9_3))?$", RegexOptions.Compiled);

        private static readonly Regex _glslPixelShaderRegex = new Regex(@"^ps_(?<major>1|2|3|4|5)_(?<minor>0|1|)$", RegexOptions.Compiled);
        private static readonly Regex _glslVertexShaderRegex = new Regex(@"^vs_(?<major>1|2|3|4|5)_(?<minor>0|1|)$", RegexOptions.Compiled);


        public static void ParseShaderModel(string text, Regex regex, out int major, out int minor)
        {
            var match = regex.Match(text);
            if (!match.Success)
            {
                major = 0;
                minor = 0;
                return;
            }

            major = int.Parse(match.Groups["major"].Value);
            minor = int.Parse(match.Groups["minor"].Value);
        }

        public void ValidateShaderModels(ShaderProfile profile)
        {
            int major, minor;

            if (!string.IsNullOrEmpty(vsFunction))
            {
                switch (profile)
                {
                    case ShaderProfile.DirectX_11:
                        ParseShaderModel(vsModel, _hlslVertexShaderRegex, out major, out minor);
                        if (major <= 3)
                            throw new Exception(String.Format("Invalid profile '{0}'. Vertex shader '{1}' must be SM 4.0 level 9.1 or higher!", vsModel, vsFunction));
                        break;
                    case ShaderProfile.OpenGL:
                        ParseShaderModel(vsModel, _glslVertexShaderRegex, out major, out minor);
                        if (major > 3)
                            throw new Exception(String.Format("Invalid profile '{0}'. Vertex shader '{1}' must be SM 3.0 or lower!", vsModel, vsFunction));
                        break;
                    case ShaderProfile.PlayStation4:
                        throw new NotSupportedException("PlayStation 4 support isn't available in this build.");
                }
            }

            if (!string.IsNullOrEmpty(psFunction))
            {
                switch (profile)
                {
                    case ShaderProfile.DirectX_11:
                        ParseShaderModel(psModel, _hlslPixelShaderRegex, out major, out minor);
                        if (major <= 3)
                            throw new Exception(String.Format("Invalid profile '{0}'. Pixel shader '{1}' must be SM 4.0 level 9.1 or higher!", vsModel, psFunction));
                        break;
                    case ShaderProfile.OpenGL:
                        ParseShaderModel(psModel, _glslPixelShaderRegex, out major, out minor);
                        if (major > 3)
                            throw new Exception(String.Format("Invalid profile '{0}'. Pixel shader '{1}' must be SM 3.0 or lower!", vsModel, psFunction));
                        break;
                    case ShaderProfile.PlayStation4:
                        throw new NotSupportedException("PlayStation 4 support isn't available in this build.");
                }
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
}