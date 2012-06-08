using System.Runtime.InteropServices;
using System;

namespace Microsoft.VisualStudio.OLE.Interop
{
    [Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
    [InterfaceType(1)]
    public interface IServiceProvider
    {
        int QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject);
    }
}