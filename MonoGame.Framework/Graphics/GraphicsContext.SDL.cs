﻿// MonoGame - Copyright (C) The MonoGame Team
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

            // GL entry points must be loaded after the GL context creation, otherwise some Windows drivers will return only GL 1.3 compatible functions
            try
            {
                OpenGL.GL.LoadEntryPoints();
            }
            catch (EntryPointNotFoundException)
            {
                throw new PlatformNotSupportedException(
                    "MonoGame requires OpenGL 3.0 compatible drivers, or either ARB_framebuffer_object or EXT_framebuffer_object extensions." +
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

        public virtual void Dispose()
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

        public static GraphicsContext CreateDummy()
        {
            return Dummy.Create();
        }

        private class Dummy : GraphicsContext
        {
            private Dummy(IntPtr windowHandle) : base(new WindowInfo(windowHandle))
            {
            }

            public static GraphicsContext Create()
            {
                var handle = Sdl.Window.Create(string.Empty, 0, 0, 1, 1,
                    Sdl.Window.State.OpenGL | Sdl.Window.State.Hidden);
                return new Dummy(handle);
            }

            public override void Dispose()
            {
                base.Dispose();
                Sdl.Window.Destroy(_winHandle);
            }
        }
    }
}
