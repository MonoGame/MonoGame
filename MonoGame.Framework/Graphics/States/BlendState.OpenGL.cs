// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
#else
using OpenTK.Graphics.OpenGL;
#endif
#elif DESKTOPGL
using OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class BlendState
    {
        internal void PlatformApplyState(GraphicsDevice device, bool force = false)
        {
            var blendEnabled = !(this.ColorSourceBlend == Blend.One && 
                                 this.ColorDestinationBlend == Blend.Zero &&
                                 this.AlphaSourceBlend == Blend.One &&
                                 this.AlphaDestinationBlend == Blend.Zero);
            if (force || blendEnabled != device._lastBlendEnable)
            {
                if (blendEnabled)
                    GL.Enable(EnableCap.Blend);
                else
                    GL.Disable(EnableCap.Blend);
                GraphicsExtensions.CheckGLError();
                device._lastBlendEnable = blendEnabled;
            }

            if (force || 
                this.ColorBlendFunction != device.Context._lastBlendState.ColorBlendFunction || 
                this.AlphaBlendFunction != device.Context._lastBlendState.AlphaBlendFunction)
            {
                GL.BlendEquationSeparate(
                    this.ColorBlendFunction.GetBlendEquationMode(),
                    this.AlphaBlendFunction.GetBlendEquationMode());
                GraphicsExtensions.CheckGLError();
                device.Context._lastBlendState.ColorBlendFunction = this.ColorBlendFunction;
                device.Context._lastBlendState.AlphaBlendFunction = this.AlphaBlendFunction;
            }

            if (force ||
                this.ColorSourceBlend != device.Context._lastBlendState.ColorSourceBlend ||
                this.ColorDestinationBlend != device.Context._lastBlendState.ColorDestinationBlend ||
                this.AlphaSourceBlend != device.Context._lastBlendState.AlphaSourceBlend ||
                this.AlphaDestinationBlend != device.Context._lastBlendState.AlphaDestinationBlend)
            {
                GL.BlendFuncSeparate(
                    this.ColorSourceBlend.GetBlendFactorSrc(), 
                    this.ColorDestinationBlend.GetBlendFactorDest(), 
                    this.AlphaSourceBlend.GetBlendFactorSrc(), 
                    this.AlphaDestinationBlend.GetBlendFactorDest());
                GraphicsExtensions.CheckGLError();
                device.Context._lastBlendState.ColorSourceBlend = this.ColorSourceBlend;
                device.Context._lastBlendState.ColorDestinationBlend = this.ColorDestinationBlend;
                device.Context._lastBlendState.AlphaSourceBlend = this.AlphaSourceBlend;
                device.Context._lastBlendState.AlphaDestinationBlend = this.AlphaDestinationBlend;
            }

            if (force || this.ColorWriteChannels != device.Context._lastBlendState.ColorWriteChannels)
            {
                GL.ColorMask(
                    (this.ColorWriteChannels & ColorWriteChannels.Red) != 0,
                    (this.ColorWriteChannels & ColorWriteChannels.Green) != 0,
                    (this.ColorWriteChannels & ColorWriteChannels.Blue) != 0,
                    (this.ColorWriteChannels & ColorWriteChannels.Alpha) != 0);
                GraphicsExtensions.CheckGLError();
                device.Context._lastBlendState.ColorWriteChannels = this.ColorWriteChannels;
            }

            
        }
    }
}

