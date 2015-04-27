using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;

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

            major = int.Parse(match.Groups["major"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            minor = int.Parse(match.Groups["minor"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
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

        public bool AlphaBlendEnable
        {
            set
            {
                if (value)
                {
                    if (blendState == null)
                    {
                        blendState = new BlendState();
                        blendState.ColorSourceBlend = Blend.One;
                        blendState.AlphaSourceBlend = Blend.One;
                        blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
                        blendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
                    }
                }
                else if (!value)
                {
                    if (blendState == null)
                        blendState = new BlendState();
                    blendState.ColorSourceBlend = Blend.One;
                    blendState.AlphaSourceBlend = Blend.One;
                    blendState.ColorDestinationBlend = Blend.Zero;
                    blendState.AlphaDestinationBlend = Blend.Zero;
                }
            }
        }

        public FillMode FillMode
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerState();
                rasterizerState.FillMode = value;             
            }
        }

        public CullMode CullMode
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerState();
                rasterizerState.CullMode = value;
            }
        }

        public bool ZEnable
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.DepthBufferEnable = value;
            }
        }

        public bool ZWriteEnable
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.DepthBufferWriteEnable = value;
            }
        }

        public CompareFunction DepthBufferFunction
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.DepthBufferFunction = value;
            }
        }

        public bool MultiSampleAntiAlias
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerState();
                rasterizerState.MultiSampleAntiAlias = value;
            }
        }

        public bool ScissorTestEnable
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerState();
                rasterizerState.ScissorTestEnable = value;
            }
        }

        public bool StencilEnable
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilEnable = value;
            }
        }

        public StencilOperation StencilFail
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilFail = value;
            }
        }

        public CompareFunction StencilFunc
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilFunction = value;
            }
        }

        public int StencilMask
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilMask = value;
            }
        }

        public StencilOperation StencilPass
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilPass = value;
            }
        }

        public int StencilRef
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.ReferenceStencil = value;
            }
        }

        public int StencilWriteMask
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilWriteMask = value;
            }
        }

        public StencilOperation StencilZFail
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilState();
                depthStencilState.StencilDepthBufferFail = value;
            }
        }

        public Blend SrcBlend
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendState();
                blendState.ColorSourceBlend = value;
                blendState.AlphaSourceBlend = ToAlphaBlend(value);
            }
        }

        public Blend DestBlend
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendState();
                blendState.ColorDestinationBlend = value;
                blendState.AlphaDestinationBlend = ToAlphaBlend(value);
            }
        }

        public BlendFunction BlendOp
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendState();
                blendState.AlphaBlendFunction = value;
            }
        }

        public ColorWriteChannels ColorWriteEnable
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendState();
                blendState.ColorWriteChannels = value;
            }    
        }

        public float DepthBias
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerState();
                rasterizerState.DepthBias = value;
            }
        }

        public float SlopeScaleDepthBias
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerState();
                rasterizerState.SlopeScaleDepthBias = value;
            }
        }
    }
}