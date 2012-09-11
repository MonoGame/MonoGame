#if OPENGL

using System;
using System.Collections.Generic;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif PSS
using Sce.PlayStation.Core.Graphics;
#elif WINRT

#else
using OpenTK.Graphics.ES20;
#if IPHONE || ANDROID
using ActiveUniformType = OpenTK.Graphics.ES20.All;
using ShaderType = OpenTK.Graphics.ES20.All;
using ProgramParameter = OpenTK.Graphics.ES20.All;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// This class is used to Cache the links between Vertex/Pixel Shaders and Constant Buffers.
    /// It will be responsible for linking the programs under OpenGL if they have not been linked
    /// before. If an existing link exists it will be resused.
    /// </summary>
    internal class ShaderProgramCache
    {
        private static readonly Dictionary<int, int> _programCache = new Dictionary<int, int>();

        internal static int? GetProgram(Shader vertexShader, Shader pixelShader)//, ConstantBuffer constantBuffer)
        {
            if (vertexShader == null)
                throw new ArgumentNullException("vertexShader");

            if (pixelShader == null)
                throw new ArgumentNullException("pixelShader");

            //
            var key = vertexShader.HashKey | pixelShader.HashKey;// +constantBuffer.HashKey;
            if (!_programCache.ContainsKey(key))
            {
                // the key does not exist so we need to link the programs
                Link(vertexShader, pixelShader);    
            }

            return _programCache[key];
        }        

        private static void Link(Shader vertexShader, Shader pixelShader)
        {
            // TODO: Shouldn't we be calling GL.DeleteProgram() somewhere?

            // NOTE: No need to worry about background threads here
            // as this is only called at draw time when we're in the
            // main drawing thread.

            var program = GL.CreateProgram();

            GL.AttachShader(program, vertexShader.ShaderHandle);
            GL.AttachShader(program, pixelShader.ShaderHandle);

            vertexShader.OnLink(program);
            pixelShader.OnLink(program);

            GL.LinkProgram(program);

            var linked = 0;

#if GLES
    		GL.GetProgram(_shaderProgram, ProgramParameter.LinkStatus, ref linked);
#else
            GL.GetProgram(program, ProgramParameter.LinkStatus, out linked);
#endif
            if (linked == 0)
            {
#if !GLES
                var log = GL.GetProgramInfoLog(program);
                Console.WriteLine(log);
#endif
                throw new InvalidOperationException("Unable to link effect program");
            }

            _programCache.Add(vertexShader.HashKey | pixelShader.HashKey, program);             
        }

    }
}

#endif // OPENGL