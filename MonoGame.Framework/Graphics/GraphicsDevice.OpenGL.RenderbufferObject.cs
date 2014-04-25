// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if MONOMAC
using MonoMac.OpenGL;
#endif

#if (WINDOWS || LINUX) && !GLES
using OpenTK.Graphics.OpenGL;
#endif

#if GLES
using OpenTK.Graphics.ES20;
using RenderbufferStorage = OpenTK.Graphics.ES20.All;
using RenderbufferTarget = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    // ARB_framebuffer_object implementation
    partial class GraphicsDevice
    {
        internal class RenderbufferObject
        {
            public virtual void Bind(RenderbufferTarget target, int id)
            {
                GL.BindRenderbuffer(target, id);
                GraphicsExtensions.CheckGLError();
            }

            public virtual void Delete(int id)
            {
                GL.DeleteRenderbuffers(1, ref id);
                GraphicsExtensions.CheckGLError();
            }

            public virtual int Generate()
            {
                int id = 0;
#if IOS || ANDROID
                GL.GenRenderbuffers(1, ref id);
#else
                GL.GenRenderbuffers(1, out id);
#endif
                GraphicsExtensions.CheckGLError();
                return id;
            }

            public virtual void Storage(RenderbufferTarget target, RenderbufferStorage internalFormat, int width, int height)
            {
                GL.RenderbufferStorage(target, internalFormat, width, height);
                GraphicsExtensions.CheckGLError();
            }
        }

#if !(GLES || MONOMAC)
        // EXT_framebuffer_object implementation
        internal sealed class RenderbufferObjectEXT : RenderbufferObject
        {
            public override void Bind(RenderbufferTarget target, int id)
            {
                GL.Ext.BindRenderbuffer(target, id);
                GraphicsExtensions.CheckGLError();
            }

            public override void Delete(int id)
            {
                GL.Ext.DeleteRenderbuffers(1, ref id);
                GraphicsExtensions.CheckGLError();
            }

            public override int Generate()
            {
                int id;
                GL.Ext.GenRenderbuffers(1, out id);
                GraphicsExtensions.CheckGLError();
                return id;
            }

            public override void Storage(RenderbufferTarget target, RenderbufferStorage internalFormat, int width, int height)
            {
                GL.Ext.RenderbufferStorage(target, internalFormat, width, height);
                GraphicsExtensions.CheckGLError();
            }
        }
#endif
    }
}
