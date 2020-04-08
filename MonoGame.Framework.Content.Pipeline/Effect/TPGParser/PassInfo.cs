using System;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Effect.TPGParser
{
    internal class PassInfo
    {
        public string name;

        public string vsModel;
        public string vsFunction;

        public string psModel;
        public string psFunction;

        public BlendStateData blendState;
        public RasterizerStateData rasterizerState;
        public DepthStencilStateData depthStencilState;
		
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
                        blendState = new BlendStateData();
                        blendState.ColorSourceBlend = Blend.One;
                        blendState.AlphaSourceBlend = Blend.One;
                        blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
                        blendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
                    }
                }
                else if (!value)
                {
                    if (blendState == null)
                        blendState = new BlendStateData();
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
                    rasterizerState = new RasterizerStateData();
                rasterizerState.FillMode = value;             
            }
        }

        public CullMode CullMode
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerStateData();
                rasterizerState.CullMode = value;
            }
        }

        public bool ZEnable
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilStateData();
                depthStencilState.DepthBufferEnable = value;
            }
        }

        public bool ZWriteEnable
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilStateData();
                depthStencilState.DepthBufferWriteEnable = value;
            }
        }

        public CompareFunction DepthBufferFunction
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilStateData();
                depthStencilState.DepthBufferFunction = value;
            }
        }

        public bool MultiSampleAntiAlias
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerStateData();
                rasterizerState.MultiSampleAntiAlias = value;
            }
        }

        public bool ScissorTestEnable
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerStateData();
                rasterizerState.ScissorTestEnable = value;
            }
        }

        public bool StencilEnable
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilStateData();
                depthStencilState.StencilEnable = value;
            }
        }

        public StencilOperation StencilFail
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilStateData();
                depthStencilState.StencilFail = value;
            }
        }

        public CompareFunction StencilFunc
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilStateData();
                depthStencilState.StencilFunction = value;
            }
        }

        public int StencilMask
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilStateData();
                depthStencilState.StencilMask = value;
            }
        }

        public StencilOperation StencilPass
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilStateData();
                depthStencilState.StencilPass = value;
            }
        }

        public int StencilRef
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilStateData();
                depthStencilState.ReferenceStencil = value;
            }
        }

        public int StencilWriteMask
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilStateData();
                depthStencilState.StencilWriteMask = value;
            }
        }

        public StencilOperation StencilZFail
        {
            set
            {
                if (depthStencilState == null)
                    depthStencilState = new DepthStencilStateData();
                depthStencilState.StencilDepthBufferFail = value;
            }
        }

        public Blend SrcBlend
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendStateData();
                blendState.ColorSourceBlend = value;
                blendState.AlphaSourceBlend = ToAlphaBlend(value);
            }
        }

        public Blend DestBlend
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendStateData();
                blendState.ColorDestinationBlend = value;
                blendState.AlphaDestinationBlend = ToAlphaBlend(value);
            }
        }

        public BlendFunction BlendOp
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendStateData();
                blendState.AlphaBlendFunction = value;
            }
        }

        public ColorWriteChannels ColorWriteEnable
        {
            set
            {
                if (blendState == null)
                    blendState = new BlendStateData();
                blendState.ColorWriteChannels = value;
            }    
        }

        public float DepthBias
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerStateData();
                rasterizerState.DepthBias = value;
            }
        }

        public float SlopeScaleDepthBias
        {
            set
            {
                if (rasterizerState == null)
                    rasterizerState = new RasterizerStateData();
                rasterizerState.SlopeScaleDepthBias = value;
            }
        }
    }
}