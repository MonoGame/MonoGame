using System;
#if DIRECTX
    using SharpDX.Direct3D11;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract class ComputeSurface : IDisposable
    {
#if DIRECTX
        internal UnorderedAccessView _uav;
#endif
        public virtual void Dispose()
        {
#if DIRECTX
            _uav.Dispose();
#else
            throw new NotImplementedException(); 
#endif
        }
    }
}
