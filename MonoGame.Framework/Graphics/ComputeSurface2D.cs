#if DIRECTX
    using SharpDX.Direct3D11;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public class ComputeSurface2D : ComputeSurface
    {
        public Texture2D Texture { get; private set; }

        public ComputeSurface2D(GraphicsDevice graphics,int width,int height,bool mipmap,SurfaceFormat surfaceFormat)
        {
            Texture = new Texture2D(graphics,width,height,mipmap,surfaceFormat,false,true);
#if DIRECTX
            _uav = new UnorderedAccessView(Texture.GraphicsDevice._d3dDevice,(Resource)Texture.Handle);
#else
           Texture.Dispose(); 
           throw new NotImplementedException();
#endif
        } 

        public override void Dispose()
        {
            Texture.Dispose();

            base.Dispose();
        }
    }
}
