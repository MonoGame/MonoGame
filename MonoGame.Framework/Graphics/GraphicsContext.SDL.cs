// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsContext : IGraphicsContext, IDisposable
    {
        private IntPtr _context;
        private IntPtr _winHandle;

        // Keeps track of last applied state to avoid redundant OpenGL calls
        internal BlendState _lastBlendState = new BlendState();

        public int SwapInterval
        {
            get
            {
                return Sdl.GL.GetSwapInterval();
            }
            set
            {
                Sdl.GL.SetSwapInterval(value);
            }
        }

        public bool IsDisposed
        {
            get { return _disposed; }
        }

        public GraphicsContext(GraphicsDevice device, IWindowInfo info)
        {
            Initialize(device);
            
            SetWindowHandle(info);
            _context = Sdl.GL.CreateContext(_winHandle);

            // GL entry points must be loaded after the GL context creation, otherwise some Windows drivers will return only GL 1.3 compatible functions
            try
            {
                OpenGL.GL.LoadEntryPoints();
            }
            catch (EntryPointNotFoundException)
            {
                throw new PlatformNotSupportedException(
                    "MonoGame requires OpenGL 3.0 compatible drivers, or either ARB_framebuffer_object or EXT_framebuffer_object extensions. " +
                    "Try updating your graphics drivers.");
            }
        }


        private void SetWindowHandle(IWindowInfo info)
        {
            if (info == null)
                _winHandle = IntPtr.Zero;
            else
                _winHandle = info.Handle;
        }

        private void PlatformApplyBlend(bool force = false)
        {
            _actualBlendState.PlatformApplyState(_device, force);
            ApplyBlendFactor(force);
        }

        private void ApplyBlendFactor(bool force)
        {
            if (force || BlendFactor != _lastBlendState.BlendFactor)
            {
                GL.BlendColor(
                    this.BlendFactor.R/255.0f,
                    this.BlendFactor.G/255.0f,
                    this.BlendFactor.B/255.0f,
                    this.BlendFactor.A/255.0f);
                GraphicsExtensions.CheckGLError();
                _lastBlendState.BlendFactor = this.BlendFactor;
            }
        }

        
        #region Implement IGraphicsContext
        public void MakeCurrent(IWindowInfo info)
        {
            if (_disposed)
                return;
            
            SetWindowHandle(info);
            Sdl.GL.MakeCurrent(_winHandle, _context);
        }

        public void SwapBuffers()
        {
            if (_disposed)
                return;
            
            Sdl.GL.SwapWindow(_winHandle);
        }

        #endregion
        
        #region Implement IDisposable
        private void PlatformDispose(bool disposing)
        {            
            if (disposing)
            {
                // Release managed objects
                // ...
            }

            // Release native objects
            Sdl.GL.DeleteContext(_context);
        }
        #endregion
    }
}
