using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using System;

namespace Microsoft.VisualStudio.Shell.Interop
{
    [ComConversionLoss]
    [InterfaceType(1)]
    [Guid("59B2D1D0-5DB0-4F9F-9609-13F0168516D6")]
    public interface IVsHierarchy
    {
        int AdviseHierarchyEvents(IVsHierarchyEvents pEventSink, out uint pdwCookie);

        int Close();

        int GetCanonicalName(uint itemid, out string pbstrName);

        int GetGuidProperty(uint itemid, int propid, out Guid pguid);

        int GetNestedHierarchy(uint itemid, ref Guid iidHierarchyNested, out IntPtr ppHierarchyNested, out uint pitemidNested);

        int GetProperty(uint itemid, int propid, out object pvar);

        int GetSite(out Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP);

        int ParseCanonicalName(string pszName, out uint pitemid);

        int QueryClose(out int pfCanClose);

        int SetGuidProperty(uint itemid, int propid, ref Guid rguid);

        int SetProperty(uint itemid, int propid, object var);

        int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp);

        int UnadviseHierarchyEvents(uint dwCookie);

        int Unused0();

        int Unused1();

        int Unused2();

        int Unused3();

        int Unused4();
    }
}
