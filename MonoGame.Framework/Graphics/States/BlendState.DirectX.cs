// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class BlendState
    {
        SharpDX.Direct3D11.BlendState _state;

        protected internal override void GraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _state);
            base.GraphicsDeviceResetting();
        }

        internal void PlatformApplyState(GraphicsDevice device)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                SharpDX.Utilities.Dispose(ref _state);
            base.Dispose(disposing);
        }
    }
}

