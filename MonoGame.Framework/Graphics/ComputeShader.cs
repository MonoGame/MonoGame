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
