// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public class TargetBlendState
	{
	    private readonly BlendState _parent;
	    private BlendFunction _alphaBlendFunction;
	    private Blend _alphaDestinationBlend;
	    private Blend _alphaSourceBlend;
	    private BlendFunction _colorBlendFunction;
	    private Blend _colorDestinationBlend;
	    private Blend _colorSourceBlend;
	    private ColorWriteChannels _colorWriteChannels;

	    internal TargetBlendState(BlendState parent)
        {
	        _parent = parent;
	        AlphaBlendFunction = BlendFunction.Add;
            AlphaDestinationBlend = Blend.Zero;
            AlphaSourceBlend = Blend.One;
            ColorBlendFunction = BlendFunction.Add;
            ColorDestinationBlend = Blend.Zero;
            ColorSourceBlend = Blend.One;
            ColorWriteChannels = ColorWriteChannels.All;
        }

	    internal TargetBlendState Clone(BlendState parent)
	    {
	        return new TargetBlendState(parent)
	        {
	            AlphaBlendFunction = AlphaBlendFunction,
                AlphaDestinationBlend = AlphaDestinationBlend,
                AlphaSourceBlend = AlphaSourceBlend,
                ColorBlendFunction = ColorBlendFunction,
                ColorDestinationBlend = ColorDestinationBlend,
                ColorSourceBlend = ColorSourceBlend,
                ColorWriteChannels = ColorWriteChannels
	        };
	    }

	    public BlendFunction AlphaBlendFunction
	    {
	        get { return _alphaBlendFunction; }
	        set
	        {
	            _parent.ThrowIfBound();
	            _alphaBlendFunction = value;
	        }
	    }

	    public Blend AlphaDestinationBlend
	    {
	        get { return _alphaDestinationBlend; }
	        set
	        {
                _parent.ThrowIfBound();
	            _alphaDestinationBlend = value;
	        }
	    }

	    public Blend AlphaSourceBlend
	    {
	        get { return _alphaSourceBlend; }
	        set
	        {
                _parent.ThrowIfBound();
	            _alphaSourceBlend = value;
	        }
	    }

	    public BlendFunction ColorBlendFunction
	    {
	        get { return _colorBlendFunction; }
	        set
	        {
                _parent.ThrowIfBound();
	            _colorBlendFunction = value;
	        }
	    }

	    public Blend ColorDestinationBlend
	    {
	        get { return _colorDestinationBlend; }
	        set
	        {
                _parent.ThrowIfBound();
	            _colorDestinationBlend = value;
	        }
	    }

	    public Blend ColorSourceBlend
	    {
	        get { return _colorSourceBlend; }
	        set
	        {
                _parent.ThrowIfBound();
	            _colorSourceBlend = value;
	        }
	    }

	    public ColorWriteChannels ColorWriteChannels
	    {
	        get { return _colorWriteChannels; }
	        set
	        {
                _parent.ThrowIfBound();
	            _colorWriteChannels = value;
	        }
	    }

#if DIRECTX

        internal void GetState(ref SharpDX.Direct3D11.RenderTargetBlendDescription desc)
        {
            // We're blending if we're not in the opaque state.
            desc.IsBlendEnabled =   !(  ColorSourceBlend == Blend.One &&
                                        ColorDestinationBlend == Blend.Zero &&
                                        AlphaSourceBlend == Blend.One &&
                                        AlphaDestinationBlend == Blend.Zero);

            desc.BlendOperation = GetBlendOperation(ColorBlendFunction);
            desc.SourceBlend = GetBlendOption(ColorSourceBlend, false);
            desc.DestinationBlend = GetBlendOption(ColorDestinationBlend, false);

            desc.AlphaBlendOperation = GetBlendOperation(AlphaBlendFunction);
            desc.SourceAlphaBlend = GetBlendOption(AlphaSourceBlend, true);
            desc.DestinationAlphaBlend = GetBlendOption(AlphaDestinationBlend, true);

            desc.RenderTargetWriteMask = GetColorWriteMask(ColorWriteChannels);
        }

        static private SharpDX.Direct3D11.BlendOperation GetBlendOperation(BlendFunction blend)
        {
            switch (blend)
            {
                case BlendFunction.Add:
                    return SharpDX.Direct3D11.BlendOperation.Add;

                case BlendFunction.Max:
                    return SharpDX.Direct3D11.BlendOperation.Maximum;

                case BlendFunction.Min:
                    return SharpDX.Direct3D11.BlendOperation.Minimum;

                case BlendFunction.ReverseSubtract:
                    return SharpDX.Direct3D11.BlendOperation.ReverseSubtract;

                case BlendFunction.Subtract:
                    return SharpDX.Direct3D11.BlendOperation.Subtract;

                default:
                    throw new ArgumentException("Invalid blend function!");
            }
        }

        static private SharpDX.Direct3D11.BlendOption GetBlendOption(Blend blend, bool alpha)
        {
            switch (blend)
            {
                case Blend.BlendFactor:
                    return SharpDX.Direct3D11.BlendOption.BlendFactor;

                case Blend.DestinationAlpha:
                    return SharpDX.Direct3D11.BlendOption.DestinationAlpha;

                case Blend.DestinationColor:
                    return alpha ? SharpDX.Direct3D11.BlendOption.DestinationAlpha : SharpDX.Direct3D11.BlendOption.DestinationColor;

                case Blend.InverseBlendFactor:
                    return SharpDX.Direct3D11.BlendOption.InverseBlendFactor;

                case Blend.InverseDestinationAlpha:
                    return SharpDX.Direct3D11.BlendOption.InverseDestinationAlpha;

                case Blend.InverseDestinationColor:
                    return alpha ? SharpDX.Direct3D11.BlendOption.InverseDestinationAlpha : SharpDX.Direct3D11.BlendOption.InverseDestinationColor;

                case Blend.InverseSourceAlpha:
                    return SharpDX.Direct3D11.BlendOption.InverseSourceAlpha;

                case Blend.InverseSourceColor:
                    return alpha ? SharpDX.Direct3D11.BlendOption.InverseSourceAlpha : SharpDX.Direct3D11.BlendOption.InverseSourceColor;

                case Blend.One:
                    return SharpDX.Direct3D11.BlendOption.One;

                case Blend.SourceAlpha:
                    return SharpDX.Direct3D11.BlendOption.SourceAlpha;

                case Blend.SourceAlphaSaturation:
                    return SharpDX.Direct3D11.BlendOption.SourceAlphaSaturate;

                case Blend.SourceColor:
                    return alpha ? SharpDX.Direct3D11.BlendOption.SourceAlpha : SharpDX.Direct3D11.BlendOption.SourceColor;

                case Blend.Zero:
                    return SharpDX.Direct3D11.BlendOption.Zero;

                default:
                    throw new ArgumentException("Invalid blend!");
            }
        }

        static private SharpDX.Direct3D11.ColorWriteMaskFlags GetColorWriteMask(ColorWriteChannels mask)
        {
            return  ((mask & ColorWriteChannels.Red) != 0 ? SharpDX.Direct3D11.ColorWriteMaskFlags.Red : 0) |
                    ((mask & ColorWriteChannels.Green) != 0 ? SharpDX.Direct3D11.ColorWriteMaskFlags.Green : 0) |
                    ((mask & ColorWriteChannels.Blue) != 0 ? SharpDX.Direct3D11.ColorWriteMaskFlags.Blue : 0) |
                    ((mask & ColorWriteChannels.Alpha) != 0 ? SharpDX.Direct3D11.ColorWriteMaskFlags.Alpha : 0);
        }
#endif

	}
}

