using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Xna.Framework.Internal {
    public static class UnsafeExtensions {
        public static IntPtr AddressWithOffset (this GCHandle handle, int offsetInBytes) {
#if PSM
            // HACK: PSM doesn't support .NET 4
            var address = handle.AddrOfPinnedObject().ToInt64();
            // FIXME: Will this work if the Int64 form of the pointer is negative?
            address += offsetInBytes;
            return (IntPtr)address;
#else
            var address = handle.AddrOfPinnedObject();
            address += offsetInBytes;
            return address;
#endif
        }
    }
}
