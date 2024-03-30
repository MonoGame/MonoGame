// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.OpenGL
{
    internal class GraphicsContext : IGraphicsContext, IDisposable
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

		public bool IsCurrent
		{
			get { return true; }
		}

        public GraphicsContext(IWindowInfo info)
        {
            if (_disposed)
                return;
            
            SetWindowHandle(info);
#if DEBUG
            // create debug context, so we get better error messages (glDebugMessageCallback)
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextFlags, 1); // 1 = SDL_GL_CONTEXT_DEBUG_FLAG
#endif
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

            GraphicsDevice.DisposeContext(_context);
            _context = IntPtr.Zero;
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
