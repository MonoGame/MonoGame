// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
using EnableCap = OpenTK.Graphics.ES20.All;
using BlendEquationMode = OpenTK.Graphics.ES20.All;
using BlendingFactorSrc = OpenTK.Graphics.ES20.All;
using BlendingFactorDest = OpenTK.Graphics.ES20.All;
#endif
#elif PSM
using Sce.PlayStation.Core.Graphics;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class BlendState : GraphicsResource
	{
#if DIRECTX
        SharpDX.Direct3D11.BlendState _state;
#endif

        private readonly TargetBlendState[] _targetBlendState;

	    private Color _blendFactor;

	    private int _multiSampleMask;

	    private bool _independentBlendEnable;

        [Conditional("DEBUG")]
        private void AssertIfBound()
        {
            Debug.Assert(GraphicsDevice == null, "You cannot modify the blend state after it has been bound to the graphics device!");
        }

        /// <summary>
        /// Returns the target specific blend state.
        /// </summary>
        /// <param name="index">The 0 to 3 target blend state index.</param>
        /// <returns>A target blend state.</returns>
        public TargetBlendState this[int index]
        {
            get { return _targetBlendState[index]; }
        }

	    public BlendFunction AlphaBlendFunction
	    {
	        get { return _targetBlendState[0].AlphaBlendFunction; } 
            set
            {
                AssertIfBound();
                _targetBlendState[0].AlphaBlendFunction = value;
            }
	    }

		public Blend AlphaDestinationBlend
        {
            get { return _targetBlendState[0].AlphaDestinationBlend; }
            set
            {
                AssertIfBound();
                _targetBlendState[0].AlphaDestinationBlend = value;
            }
        }

		public Blend AlphaSourceBlend
        {
            get { return _targetBlendState[0].AlphaSourceBlend; }
            set
            {
                AssertIfBound();
                _targetBlendState[0].AlphaSourceBlend = value;
            }
        }

		public BlendFunction ColorBlendFunction
        {
            get { return _targetBlendState[0].ColorBlendFunction; }
            set
            {
                AssertIfBound();
                _targetBlendState[0].ColorBlendFunction = value;
            }
        }

		public Blend ColorDestinationBlend
        {
            get { return _targetBlendState[0].ColorDestinationBlend; }
            set
            {
                AssertIfBound();
                _targetBlendState[0].ColorDestinationBlend = value;
            }
        }

		public Blend ColorSourceBlend
        {
            get { return _targetBlendState[0].ColorSourceBlend; }
            set
            {
                AssertIfBound();
                _targetBlendState[0].ColorSourceBlend = value;
            }
        }

		public ColorWriteChannels ColorWriteChannels
        {
            get { return _targetBlendState[0].ColorWriteChannels; }
            set
            {
                AssertIfBound();
                _targetBlendState[0].ColorWriteChannels = value;
            }
        }

		public ColorWriteChannels ColorWriteChannels1
        {
            get { return _targetBlendState[1].ColorWriteChannels; }
            set
            {
                AssertIfBound();
                _targetBlendState[1].ColorWriteChannels = value;
            }
        }

		public ColorWriteChannels ColorWriteChannels2
        {
            get { return _targetBlendState[2].ColorWriteChannels; }
            set
            {
                AssertIfBound();
                _targetBlendState[2].ColorWriteChannels = value;
            }
        }

		public ColorWriteChannels ColorWriteChannels3
        {
            get { return _targetBlendState[3].ColorWriteChannels; }
            set
            {
                AssertIfBound();
                _targetBlendState[3].ColorWriteChannels = value;
            }
        }

	    public Color BlendFactor
	    {
	        get { return _blendFactor; }
            set
            {
                AssertIfBound();
                _blendFactor = value;
            }
	    }

        public int MultiSampleMask
        {
            get { return _multiSampleMask; }
            set
            {
                AssertIfBound();
                _multiSampleMask = value;
            }
        }

        /// <summary>
        /// Enables use of the per-target blend states.
        /// </summary>
        public bool IndependentBlendEnable
        {
            get { return _independentBlendEnable; }
            set
            {
                AssertIfBound();
                _independentBlendEnable = value;
            }
        }

		private static readonly Utilities.ObjectFactoryWithReset<BlendState> _additive;
        private static readonly Utilities.ObjectFactoryWithReset<BlendState> _alphaBlend;
        private static readonly Utilities.ObjectFactoryWithReset<BlendState> _nonPremultiplied;
        private static readonly Utilities.ObjectFactoryWithReset<BlendState> _opaque;

        public static BlendState Additive { get { return _additive.Value; } }
        public static BlendState AlphaBlend { get { return _alphaBlend.Value; } }
        public static BlendState NonPremultiplied { get { return _nonPremultiplied.Value; } }
        public static BlendState Opaque { get { return _opaque.Value; } }
        
        public BlendState() 
        {
            _targetBlendState = new TargetBlendState[4];
            _targetBlendState[0] = new TargetBlendState();
            _targetBlendState[1] = new TargetBlendState();
            _targetBlendState[2] = new TargetBlendState();
            _targetBlendState[3] = new TargetBlendState();

			_blendFactor = Color.White;
            _multiSampleMask = Int32.MaxValue;
            _independentBlendEnable = false;
        }
		
		static BlendState() 
        {
            _additive = new Utilities.ObjectFactoryWithReset<BlendState>(() => new BlendState
            {
                Name = "BlendState.Additive",
                ColorSourceBlend = Blend.SourceAlpha,
                AlphaSourceBlend = Blend.SourceAlpha,
                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One
            });
			
			_alphaBlend = new Utilities.ObjectFactoryWithReset<BlendState>(() => new BlendState()
            {
                Name = "BlendState.AlphaBlend",
				ColorSourceBlend = Blend.One,
				AlphaSourceBlend = Blend.One,
				ColorDestinationBlend = Blend.InverseSourceAlpha,
				AlphaDestinationBlend = Blend.InverseSourceAlpha
			});
			
			_nonPremultiplied = new Utilities.ObjectFactoryWithReset<BlendState>(() => new BlendState() 
            {
                Name = "BlendState.NonPremultiplied",
				ColorSourceBlend = Blend.SourceAlpha,
				AlphaSourceBlend = Blend.SourceAlpha,
				ColorDestinationBlend = Blend.InverseSourceAlpha,
				AlphaDestinationBlend = Blend.InverseSourceAlpha
			});
			
			_opaque = new Utilities.ObjectFactoryWithReset<BlendState>(() => new BlendState()
            {
                Name = "BlendState.Opaque",
				ColorSourceBlend = Blend.One,
				AlphaSourceBlend = Blend.One,			    
				ColorDestinationBlend = Blend.Zero,
				AlphaDestinationBlend = Blend.Zero
			});
		}

#if OPENGL
        internal void ApplyState(GraphicsDevice device)
        {
            var blendEnabled = !(this.ColorSourceBlend == Blend.One && 
                                 this.ColorDestinationBlend == Blend.Zero &&
                                 this.AlphaSourceBlend == Blend.One &&
                                 this.AlphaDestinationBlend == Blend.Zero);
            if (blendEnabled)
                GL.Enable(EnableCap.Blend);
            else
                GL.Disable(EnableCap.Blend);
            GraphicsExtensions.CheckGLError();

            GL.BlendColor(
                this.BlendFactor.R / 255.0f,      
                this.BlendFactor.G / 255.0f, 
                this.BlendFactor.B / 255.0f, 
                this.BlendFactor.A / 255.0f);
            GraphicsExtensions.CheckGLError();

            GL.BlendEquationSeparate(
                this.ColorBlendFunction.GetBlendEquationMode(),
                this.AlphaBlendFunction.GetBlendEquationMode());
            GraphicsExtensions.CheckGLError();

            GL.BlendFuncSeparate(
                this.ColorSourceBlend.GetBlendFactorSrc(), 
                this.ColorDestinationBlend.GetBlendFactorDest(), 
                this.AlphaSourceBlend.GetBlendFactorSrc(), 
                this.AlphaDestinationBlend.GetBlendFactorDest());
            GraphicsExtensions.CheckGLError();

            GL.ColorMask(
                (this.ColorWriteChannels & ColorWriteChannels.Red) != 0,
                (this.ColorWriteChannels & ColorWriteChannels.Green) != 0,
                (this.ColorWriteChannels & ColorWriteChannels.Blue) != 0,
                (this.ColorWriteChannels & ColorWriteChannels.Alpha) != 0);
            GraphicsExtensions.CheckGLError();
        }

#elif DIRECTX

        protected internal override void GraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _state);
            base.GraphicsDeviceResetting();
        }

        internal void ApplyState(GraphicsDevice device)
        {
            if (_state == null)
            {
                // We're now bound to a device... no one should
                // be changing the state of this object now!
                GraphicsDevice = device;

                // Build the description.
                var desc = new SharpDX.Direct3D11.BlendStateDescription();
                _targetBlendState[0].GetState(ref desc.RenderTarget[0]);
                _targetBlendState[1].GetState(ref desc.RenderTarget[1]);
                _targetBlendState[2].GetState(ref desc.RenderTarget[2]);
                _targetBlendState[3].GetState(ref desc.RenderTarget[3]);
                desc.IndependentBlendEnable = _independentBlendEnable;

                // This is a new DX11 feature we should consider 
                // exposing as part of the extended MonoGame API.
                desc.AlphaToCoverageEnable = false;

                // Create the state.
                _state = new SharpDX.Direct3D11.BlendState(GraphicsDevice._d3dDevice, desc);
            }

            Debug.Assert(GraphicsDevice == device, "The state was created for a different device!");

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // Apply the state!
            var blendFactor = new SharpDX.Color4(_blendFactor.R / 255.0f, _blendFactor.G / 255.0f, _blendFactor.B / 255.0f, _blendFactor.A / 255.0f);
            device._d3dContext.OutputMerger.SetBlendState(_state, blendFactor);
        }

        internal static void ResetStates()
        {
            _additive.Reset();
            _alphaBlend.Reset();
            _nonPremultiplied.Reset();
            _opaque.Reset();
        }

#endif // DIRECTX	
#if PSM
        internal void ApplyState(GraphicsDevice device)
        {
            device._graphics.Enable(EnableMode.Blend);     
            device._graphics.SetBlendFuncAlpha(PSSHelper.ToBlendFuncMode(device.BlendState.AlphaBlendFunction),
                                          PSSHelper.ToBlendFuncFactor(device.BlendState.AlphaSourceBlend),
                                          PSSHelper.ToBlendFuncFactor(device.BlendState.AlphaDestinationBlend));
            device._graphics.SetBlendFuncRgb(PSSHelper.ToBlendFuncMode(device.BlendState.ColorBlendFunction),
                                          PSSHelper.ToBlendFuncFactor(device.BlendState.ColorSourceBlend),
                                          PSSHelper.ToBlendFuncFactor(device.BlendState.ColorDestinationBlend));
            
        }
#endif

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
#if DIRECTX
                SharpDX.Utilities.Dispose(ref _state);
#endif
            }

            base.Dispose(disposing);
        }
	}
}

