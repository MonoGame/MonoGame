// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
	public class BlendState : GraphicsResource
	{
#if DIRECTX
        SharpDX.Direct3D11.BlendState _state;
#endif

        // TODO: We should be asserting if the state has
        // been changed after it has been bound to the device!

		public BlendFunction AlphaBlendFunction { get; set; }
		public Blend AlphaDestinationBlend { get; set; }
		public Blend AlphaSourceBlend { get; set; }
		public Color BlendFactor { get; set; }
		public BlendFunction ColorBlendFunction { get; set; }
		public Blend ColorDestinationBlend { get; set; }
		public Blend ColorSourceBlend { get; set; }
		public ColorWriteChannels ColorWriteChannels { get; set; }
		public ColorWriteChannels ColorWriteChannels1 { get; set; }
		public ColorWriteChannels ColorWriteChannels2 { get; set; }
		public ColorWriteChannels ColorWriteChannels3 { get; set; }
		public int MultiSampleMask { get; set; }

		public static readonly BlendState Additive;
		public static readonly BlendState AlphaBlend;
		public static readonly BlendState NonPremultiplied;
		public static readonly BlendState Opaque;
		
		public BlendState() 
        {
			AlphaBlendFunction = BlendFunction.Add;
			AlphaDestinationBlend = Blend.Zero;
			AlphaSourceBlend = Blend.One;
			BlendFactor = Color.White;
			ColorBlendFunction = BlendFunction.Add;
			ColorDestinationBlend = Blend.Zero;
			ColorSourceBlend = Blend.One;
			ColorWriteChannels = ColorWriteChannels.All;
			ColorWriteChannels1 = ColorWriteChannels.All;
			ColorWriteChannels2 = ColorWriteChannels.All;
			ColorWriteChannels3 = ColorWriteChannels.All;
			MultiSampleMask = Int32.MaxValue;
		}
		
		static BlendState() 
        {
			Additive = new BlendState() 
            {
				ColorSourceBlend = Blend.SourceAlpha,
				AlphaSourceBlend = Blend.SourceAlpha,
    			ColorDestinationBlend = Blend.One,	
				AlphaDestinationBlend = Blend.One
			};
			
			AlphaBlend = new BlendState()
            {
				ColorSourceBlend = Blend.One,
				AlphaSourceBlend = Blend.One,
				ColorDestinationBlend = Blend.InverseSourceAlpha,
				AlphaDestinationBlend = Blend.InverseSourceAlpha
			};
			
			NonPremultiplied = new BlendState() 
            {
				ColorSourceBlend = Blend.SourceAlpha,
				AlphaSourceBlend = Blend.SourceAlpha,
				ColorDestinationBlend = Blend.InverseSourceAlpha,
				AlphaDestinationBlend = Blend.InverseSourceAlpha
			};
			
			Opaque = new BlendState()
            {
				ColorSourceBlend = Blend.One,
				AlphaSourceBlend = Blend.One,			    
				ColorDestinationBlend = Blend.Zero,
				AlphaDestinationBlend = Blend.Zero
			};
		}

        public override string ToString ()
        {
            string blendStateName;

            if(this == BlendState.Additive)
                blendStateName = "Additive";
            else if (this == BlendState.AlphaBlend)
                blendStateName = "AlphaBlend";
            else if (this == BlendState.NonPremultiplied)
                blendStateName = "NonPremultiplied";
            else
                blendStateName = "Opaque";


            return string.Format("{0}.{1}", base.ToString(), blendStateName);
        }


#if DIRECTX

        internal void ApplyState(GraphicsDevice device)
        {
            if (_state == null)
            {
                // We're now bound to a device... no one should
                // be changing the state of this object now!
                graphicsDevice = device;

                // Build the description.
                var desc = new SharpDX.Direct3D11.BlendStateDescription();

                var targetDesc = new SharpDX.Direct3D11.RenderTargetBlendDescription();

                // We're blending if we're not in the opaque state.
                targetDesc.IsBlendEnabled = !(  ColorSourceBlend == Opaque.ColorSourceBlend &&
                                                ColorDestinationBlend == Opaque.ColorDestinationBlend &&
                                                AlphaSourceBlend == Opaque.AlphaSourceBlend &&
                                                AlphaDestinationBlend == Opaque.AlphaDestinationBlend);

                targetDesc.BlendOperation = GetBlendOperation(ColorBlendFunction);
                targetDesc.SourceBlend = GetBlendOption(ColorSourceBlend);
                targetDesc.DestinationBlend = GetBlendOption(ColorDestinationBlend);

                targetDesc.AlphaBlendOperation = GetBlendOperation(AlphaBlendFunction);
                targetDesc.SourceAlphaBlend = GetBlendOption(AlphaSourceBlend);
                targetDesc.DestinationAlphaBlend = GetBlendOption(AlphaDestinationBlend);

                // Set the first 4 targets to the same settings.
                desc.RenderTarget[0] = targetDesc;
                desc.RenderTarget[1] = targetDesc;
                desc.RenderTarget[2] = targetDesc;
                desc.RenderTarget[3] = targetDesc;
 
                // Set the color write controls per-target.
                desc.RenderTarget[0].RenderTargetWriteMask = GetColorWriteMask(ColorWriteChannels);
                desc.RenderTarget[1].RenderTargetWriteMask = GetColorWriteMask(ColorWriteChannels1);
                desc.RenderTarget[2].RenderTargetWriteMask = GetColorWriteMask(ColorWriteChannels2);
                desc.RenderTarget[3].RenderTargetWriteMask = GetColorWriteMask(ColorWriteChannels3);

                // These are new DX11 features we should consider exposing
                // as part of the extended MonoGame API.
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = false;

                // Create the state.
                _state = new SharpDX.Direct3D11.BlendState(graphicsDevice._d3dDevice, ref desc);
            }

            Debug.Assert(graphicsDevice == device, "The state was created for a different device!");

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // Apply the state!
            var d3dContext = device._d3dContext;
            d3dContext.OutputMerger.BlendFactor = new SharpDX.Color4(BlendFactor.R / 255.0f, BlendFactor.G / 255.0f, BlendFactor.B / 255.0f, BlendFactor.A / 255.0f);
            d3dContext.OutputMerger.BlendState = _state;
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
                    throw new NotImplementedException("Invalid blend function!");
            }
        }

        static private SharpDX.Direct3D11.BlendOption GetBlendOption(Blend blend)
        {
            switch (blend)
            {
                case Blend.BlendFactor:
                    return SharpDX.Direct3D11.BlendOption.BlendFactor;

                case Blend.DestinationAlpha:
                    return SharpDX.Direct3D11.BlendOption.DestinationAlpha;

                case Blend.DestinationColor:
                    return SharpDX.Direct3D11.BlendOption.DestinationColor;

                case Blend.InverseBlendFactor:
                    return SharpDX.Direct3D11.BlendOption.InverseBlendFactor;

                case Blend.InverseDestinationAlpha:
                    return SharpDX.Direct3D11.BlendOption.InverseDestinationAlpha;

                case Blend.InverseDestinationColor:
                    return SharpDX.Direct3D11.BlendOption.InverseDestinationColor;

                case Blend.InverseSourceAlpha:
                    return SharpDX.Direct3D11.BlendOption.InverseSourceAlpha;

                case Blend.InverseSourceColor:
                    return SharpDX.Direct3D11.BlendOption.InverseSourceColor;

                case Blend.One:
                    return SharpDX.Direct3D11.BlendOption.One;

                case Blend.SourceAlpha:
                    return SharpDX.Direct3D11.BlendOption.SourceAlpha;

                case Blend.SourceAlphaSaturation:
                    return SharpDX.Direct3D11.BlendOption.SourceAlphaSaturate;

                case Blend.SourceColor:
                    return SharpDX.Direct3D11.BlendOption.SourceColor;

                case Blend.Zero:
                    return SharpDX.Direct3D11.BlendOption.Zero;                    

                default:
                    throw new NotImplementedException("Invalid blend!");
            }
        }

        static private SharpDX.Direct3D11.ColorWriteMaskFlags GetColorWriteMask(ColorWriteChannels mask)
        {
            return  ((mask & ColorWriteChannels.Red) != 0 ? SharpDX.Direct3D11.ColorWriteMaskFlags.Red : 0) |
                    ((mask & ColorWriteChannels.Green) != 0 ? SharpDX.Direct3D11.ColorWriteMaskFlags.Green : 0) |
                    ((mask & ColorWriteChannels.Blue) != 0 ? SharpDX.Direct3D11.ColorWriteMaskFlags.Blue : 0) |
                    ((mask & ColorWriteChannels.Alpha) != 0 ? SharpDX.Direct3D11.ColorWriteMaskFlags.Alpha : 0);
        }

#endif // DIRECTX		
	}
}

