using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


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

        private static Dictionary<int, int> ProgramCache = new Dictionary<int, int>();

        internal static int? GetProgram(Shader vertexShader, Shader pixelShader, ConstantBuffer constantBuffer)
        {
            //
            int key = vertexShader.HashKey + pixelShader.HashKey + constantBuffer.HashKey;
            if (!ProgramCache.ContainsKey(key))
            {
                // the key does not exist so we need to link the programs
                Link(vertexShader, pixelShader, constantBuffer);    
            }            
            return ProgramCache[key];
        }

        internal static int Hash(byte[] data)
        {
            // generate a hash for this constant buffer            
            unchecked
            {
                const int p = 16777619;
                int hash = (int)2166136261;

                for (int i = 0; i < data.Length; i++)
                    hash = (hash ^ data[i]) * p;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }

        private static void Link(Shader _vertexShader, Shader _pixelShader, ConstantBuffer _constantBuffer)
        {
            #if OPENGL
            Threading.BlockOnUIThread(() =>
            {
                // TODO: Shouldn't we be calling GL.DeleteProgram() somewhere?

                // TODO: We could cache the various program combinations 
                // of vertex/pixel shaders and share them across effects.

                int _shaderProgram = GL.CreateProgram();

                GL.AttachShader(_shaderProgram, _vertexShader.ShaderHandle);
                GL.AttachShader(_shaderProgram, _pixelShader.ShaderHandle);

                _vertexShader.OnLink(_shaderProgram);
                _pixelShader.OnLink(_shaderProgram);
                GL.LinkProgram(_shaderProgram);

                var linked = 0;

#if GLES
    			GL.GetProgram(_shaderProgram, ProgramParameter.LinkStatus, ref linked);
#else
                GL.GetProgram(_shaderProgram, ProgramParameter.LinkStatus, out linked);
#endif
                if (linked == 0)
                {
#if !GLES
                    string log = GL.GetProgramInfoLog(_shaderProgram);
                    Console.WriteLine(log);
#endif
                    throw new InvalidOperationException("Unable to link effect program");
                }

                ProgramCache.Add(_vertexShader.HashKey + _pixelShader.HashKey + _constantBuffer.HashKey, _shaderProgram); 
            });
#endif
            
        }

    }
}
