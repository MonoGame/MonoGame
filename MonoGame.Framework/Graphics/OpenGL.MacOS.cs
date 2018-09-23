// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using ObjCRuntime;
using AppKit;
using System.Security;
using MonoGame.Utilities;

namespace OpenGL
{
    public partial class GL
    {
        public static IntPtr Library = FuncLoader.LoadLibrary("/System/Library/Frameworks/OpenGL.framework/OpenGL");
        
        static partial void LoadPlatformEntryPoints()
		{
            BoundApi = RenderApi.GL;
        }

        private static T LoadFunction<T>(string function, bool throwIfNotFound = false)
        {
            return FuncLoader.LoadFunction<T>(Library, function, throwIfNotFound);
        }

        private static IGraphicsContext PlatformCreateContext (IWindowInfo info)
        {
            return new GraphicsContext ((MacWindowInfo)info);
        }
	}

    public class MacWindowInfo : IWindowInfo
    {
        public IntPtr Handle
        {
            get
            {
                return IntPtr.Zero;
            }
        }

        public int ColorSize { get; private set; }
        public int DepthSize { get; private set; }
        public int StencilSize { get; private set; }
        public int MultiSample { get; private set; }
         

        public MacWindowInfo(int colorSize, int depthSize, int stencils, int multipSample)
        {
            ColorSize = colorSize;
            DepthSize = depthSize;
            MultiSample = multipSample;
        }
    }

    public class GraphicsContext : IGraphicsContext
    {
        public GraphicsContext (MacWindowInfo info)
        {
            var attribs = new object[] {
                NSOpenGLPixelFormatAttribute.Accelerated,
                NSOpenGLPixelFormatAttribute.NoRecovery,
                NSOpenGLPixelFormatAttribute.DoubleBuffer,
                NSOpenGLPixelFormatAttribute.ColorSize, info.ColorSize,
                NSOpenGLPixelFormatAttribute.DepthSize, info.DepthSize,
                NSOpenGLPixelFormatAttribute.StencilSize , info.StencilSize,
                NSOpenGLPixelFormatAttribute.Multisample, info.MultiSample,
            };

            PixelFormat = new NSOpenGLPixelFormat(attribs);

            if (PixelFormat == null)
                Console.WriteLine("No OpenGL pixel format");
            
            Context = new NSOpenGLContext (PixelFormat, null);
            Context.MakeCurrentContext();
        }

        public bool IsCurrent {
            get {
                return NSOpenGLContext.CurrentContext == this.Context;
            }
        }

        public bool IsDisposed {
            get {
                return this.Context == null;
            }
        }

        public int SwapInterval {
            get {
                throw new NotImplementedException ();
            }

            set {
                throw new NotImplementedException ();
            }
        }

        public void Dispose ()
        {
            if (this.Context != null) {
                this.Context.Dispose ();
            }
            this.Context = null;
        }

        public void MakeCurrent (IWindowInfo info)
        {
            this.Context.MakeCurrentContext();
        }

        public void SwapBuffers ()
        {
            //if (!this.Context.PresentRenderBuffer (36161u)) {
            //    throw new InvalidOperationException ("EAGLContext.PresentRenderbuffer failed.");
            //}
        }

        internal NSOpenGLContext Context { get; private set; }

        internal NSOpenGLPixelFormat PixelFormat { get; private set; }
    }
}

