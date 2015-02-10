// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class BlendState
    {
        private static bool _currentBlendEnable = false;
        private static BlendState _currentBlendState = new BlendState();

        internal void PlatformApplyState(GraphicsDevice device, bool force = false)
        {
            var blendEnabled = !(this.ColorSourceBlend == Blend.One && 
                                 this.ColorDestinationBlend == Blend.Zero &&
                                 this.AlphaSourceBlend == Blend.One &&
                                 this.AlphaDestinationBlend == Blend.Zero);
            if (force || blendEnabled != _currentBlendEnable)
            {
                if (blendEnabled)
                    GL.Enable(EnableCap.Blend);
                else
                    GL.Disable(EnableCap.Blend);
                GraphicsExtensions.CheckGLError();
                _currentBlendEnable = blendEnabled;
            }

            if (force || this.BlendFactor != _currentBlendState.BlendFactor)
            {
                GL.BlendColor(
                    this.BlendFactor.R / 255.0f,      
                    this.BlendFactor.G / 255.0f, 
                    this.BlendFactor.B / 255.0f, 
                    this.BlendFactor.A / 255.0f);
                GraphicsExtensions.CheckGLError();
                _currentBlendState.BlendFactor = this.BlendFactor;
            }

            if (force || 
                this.ColorBlendFunction != _currentBlendState.ColorBlendFunction || 
                this.AlphaBlendFunction != _currentBlendState.AlphaBlendFunction)
            {
                GL.BlendEquationSeparate(
                    this.ColorBlendFunction.GetBlendEquationMode(),
                    this.AlphaBlendFunction.GetBlendEquationMode());
                GraphicsExtensions.CheckGLError();
                _currentBlendState.ColorBlendFunction = this.ColorBlendFunction;
                _currentBlendState.AlphaBlendFunction = this.AlphaBlendFunction;
            }

            if (force ||
                this.ColorSourceBlend != _currentBlendState.ColorSourceBlend ||
                this.ColorDestinationBlend != _currentBlendState.ColorDestinationBlend ||
                this.AlphaSourceBlend != _currentBlendState.AlphaSourceBlend ||
                this.AlphaDestinationBlend != _currentBlendState.AlphaDestinationBlend)
            {
                GL.BlendFuncSeparate(
                    this.ColorSourceBlend.GetBlendFactorSrc(), 
                    this.ColorDestinationBlend.GetBlendFactorDest(), 
                    this.AlphaSourceBlend.GetBlendFactorSrc(), 
                    this.AlphaDestinationBlend.GetBlendFactorDest());
                GraphicsExtensions.CheckGLError();
                _currentBlendState.ColorSourceBlend = this.ColorSourceBlend;
                _currentBlendState.ColorDestinationBlend = this.ColorDestinationBlend;
                _currentBlendState.AlphaSourceBlend = this.AlphaSourceBlend;
                _currentBlendState.AlphaDestinationBlend = this.AlphaDestinationBlend;
            }

            if (force || this.ColorWriteChannels != _currentBlendState.ColorWriteChannels)
            {
                GL.ColorMask(
                    (this.ColorWriteChannels & ColorWriteChannels.Red) != 0,
                    (this.ColorWriteChannels & ColorWriteChannels.Green) != 0,
                    (this.ColorWriteChannels & ColorWriteChannels.Blue) != 0,
                    (this.ColorWriteChannels & ColorWriteChannels.Alpha) != 0);
                GraphicsExtensions.CheckGLError();
                _currentBlendState.ColorWriteChannels = this.ColorWriteChannels;
            }

            
        }
    }
}

