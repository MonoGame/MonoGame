#region License
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2013 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
#endregion

using System; 

namespace Microsoft.Xna.Framework.Graphics
{
    public class ComputeShader : IDisposable
    {
        private GraphicsDevice _graphics;

#if DIRECTX
        private SharpDX.Direct3D11.ComputeShader _shader;
#endif
        public GraphicsDevice GraphicsDevice { get { return _graphics; } private set { _graphics = value; } }

        private ComputeShader(GraphicsDevice graphics,byte[] bytecode)
        {
            _graphics = graphics;
#if DIRECTX
            _shader = new SharpDX.Direct3D11.ComputeShader(GraphicsDevice._d3dDevice,bytecode);
#endif
        }

        public static ComputeShader CreateFromCompiledBytecode(GraphicsDevice graphics,byte[] bytecode)
        {
            return new ComputeShader(graphics,bytecode);
        }
         
        public void SetOutputSurface(ComputeSurface computeSurface)
        {
#if DIRECTX
            _graphics._d3dContext.ComputeShader.SetUnorderedAccessView(0, computeSurface._uav, -1);
#else
            throw new NotImplementedException();
#endif
        }
         
        public void SetOutputSurface(int startSlot,ComputeSurface computeSurface,int uavInitialCount = -1)
        {
#if DIRECTX
             _graphics._d3dContext.ComputeShader.SetUnorderedAccessView(startSlot,computeSurface._uav,uavInitialCount);
#else 
             throw new NotImplementedException();
#endif
        }

        public void Compute(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
        {
#if DIRECTX
            _graphics._d3dContext.ComputeShader.Set(_shader);
            _graphics._d3dContext.Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
#else
            throw new NotImplementedException();
#endif
        }


        public void Dispose()
        {
#if DIRECTX
            _shader.Dispose();
#else
            throw new NotImplementedException();
#endif
        }
    }
}
