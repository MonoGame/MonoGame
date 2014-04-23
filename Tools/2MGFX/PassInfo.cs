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

        public void ValidateShaderModels(ShaderProfile profile)
        {
            int major, minor;

            var dx11Profile = profile != ShaderProfile.OpenGL;

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
}