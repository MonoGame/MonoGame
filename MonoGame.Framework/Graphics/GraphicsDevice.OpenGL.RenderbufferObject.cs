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
using FramebufferAttachment = OpenTK.Graphics.ES20.All;
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
                int id;
                GL.GenRenderbuffers(1, out id);
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
