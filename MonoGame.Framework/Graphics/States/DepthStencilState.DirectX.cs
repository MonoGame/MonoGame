// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class DepthStencilState
    {
        private SharpDX.Direct3D11.DepthStencilState _state;

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
                var desc = new SharpDX.Direct3D11.DepthStencilStateDescription();

                desc.IsDepthEnabled = DepthBufferEnable;
                desc.DepthComparison = GetComparison(DepthBufferFunction);

                if (DepthBufferWriteEnable)
                    desc.DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask.All;
                else
                    desc.DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask.Zero;

                desc.IsStencilEnabled = StencilEnable;
                desc.StencilReadMask = (byte)StencilMask; // TODO: Should this instead grab the upper 8bits?
                desc.StencilWriteMask = (byte)StencilWriteMask;

                if (TwoSidedStencilMode)
                {
                    desc.BackFace.Comparison = GetComparison(CounterClockwiseStencilFunction);
                    desc.BackFace.DepthFailOperation = GetStencilOp(CounterClockwiseStencilDepthBufferFail);
                    desc.BackFace.FailOperation = GetStencilOp(CounterClockwiseStencilFail);
                    desc.BackFace.PassOperation = GetStencilOp(CounterClockwiseStencilPass);
                }
                else
                {   //use same settings as frontFace 
                    desc.BackFace.Comparison = GetComparison(StencilFunction);
                    desc.BackFace.DepthFailOperation = GetStencilOp(StencilDepthBufferFail);
                    desc.BackFace.FailOperation = GetStencilOp(StencilFail);
                    desc.BackFace.PassOperation = GetStencilOp(StencilPass);
                }

                desc.FrontFace.Comparison = GetComparison(StencilFunction);
                desc.FrontFace.DepthFailOperation = GetStencilOp(StencilDepthBufferFail);
                desc.FrontFace.FailOperation = GetStencilOp(StencilFail);
                desc.FrontFace.PassOperation = GetStencilOp(StencilPass);

                // Create the state.
                _state = new SharpDX.Direct3D11.DepthStencilState(GraphicsDevice._d3dDevice, desc);
            }

            Debug.Assert(GraphicsDevice == device, "The state was created for a different device!");

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // Apply the state!
            device._d3dContext.OutputMerger.SetDepthStencilState(_state, ReferenceStencil);
        }

        static private SharpDX.Direct3D11.Comparison GetComparison( CompareFunction compare)
        {
            switch (compare)
            {
                case CompareFunction.Always:
                    return SharpDX.Direct3D11.Comparison.Always;

                case CompareFunction.Equal:
                    return SharpDX.Direct3D11.Comparison.Equal;

                case CompareFunction.Greater:
                    return SharpDX.Direct3D11.Comparison.Greater;

                case CompareFunction.GreaterEqual:
                    return SharpDX.Direct3D11.Comparison.GreaterEqual;

                case CompareFunction.Less:
                    return SharpDX.Direct3D11.Comparison.Less;

                case CompareFunction.LessEqual:
                    return SharpDX.Direct3D11.Comparison.LessEqual;

                case CompareFunction.Never:
                    return SharpDX.Direct3D11.Comparison.Never;

                case CompareFunction.NotEqual:
                    return SharpDX.Direct3D11.Comparison.NotEqual;

                default:
                    throw new ArgumentException("Invalid comparison!");
            }
        }

        static private SharpDX.Direct3D11.StencilOperation GetStencilOp(StencilOperation op)
        {
            switch (op)
            {
                case StencilOperation.Decrement:
                    return SharpDX.Direct3D11.StencilOperation.Decrement;

                case StencilOperation.DecrementSaturation:
                    return SharpDX.Direct3D11.StencilOperation.DecrementAndClamp;

                case StencilOperation.Increment:
                    return SharpDX.Direct3D11.StencilOperation.Increment;

                case StencilOperation.IncrementSaturation:
                    return SharpDX.Direct3D11.StencilOperation.IncrementAndClamp;

                case StencilOperation.Invert:
                    return SharpDX.Direct3D11.StencilOperation.Invert;

                case StencilOperation.Keep:
                    return SharpDX.Direct3D11.StencilOperation.Keep;

                case StencilOperation.Replace:
                    return SharpDX.Direct3D11.StencilOperation.Replace;

                case StencilOperation.Zero:
                    return SharpDX.Direct3D11.StencilOperation.Zero;

                default:
                    throw new ArgumentException("Invalid stencil operation!");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                SharpDX.Utilities.Dispose(ref _state);
            base.Dispose(disposing);
        }
    }
}

