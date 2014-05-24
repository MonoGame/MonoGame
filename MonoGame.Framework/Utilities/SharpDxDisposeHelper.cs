using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Microsoft.Xna.Framework.Utilities
{
    public static class SharpDxDisposeHelper
    {
        public static void SafeDispose<T>(ref T comObject) where T : CppObject
        {
            // seems like SharpDX.Utilities.Dispose is not such safe dispose
            // -> can throw NullReferenceException if NativePointer is nullptr
            // -> example: .Dispose(_d3dContext)
            if (comObject != null && comObject.NativePointer != IntPtr.Zero)
                SharpDX.Utilities.Dispose(ref comObject);

            comObject = null;
        }
    }
}
