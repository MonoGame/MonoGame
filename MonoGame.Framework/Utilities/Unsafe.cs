using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Xna.Framework.Internal {
    public static class UnsafeExtensions {
        public static IntPtr AddressWithOffset (this GCHandle handle, int offsetInBytes) {
            var address = handle.AddrOfPinnedObject();
            address += offsetInBytes;
            return address;
        }
    }
}
