// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace OpenGL
{
    public class GraphicsContext : IGraphicsContext, IDisposable
    {
        private IntPtr _context;
        private IntPtr _winHandle;
        private bool _disposed;

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

        public GraphicsContext(IWindowInfo info)
        {
            if (_disposed)
                return;
            
            SetWindowHandle(info);
            _context = Sdl.GL.CreateContext(_winHandle);
        }

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

        public void Dispose()
        {
            if (_disposed)
                return;
            
            Sdl.GL.DeleteContext(_context);
            _disposed = true;
        }

        private void SetWindowHandle(IWindowInfo info)
        {
            if (info == null)
                _winHandle = IntPtr.Zero;
            else
                _winHandle = info.Handle;
        }
    }
}
