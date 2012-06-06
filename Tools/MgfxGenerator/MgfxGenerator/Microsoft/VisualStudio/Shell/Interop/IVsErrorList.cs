using System.Runtime.InteropServices;
namespace Microsoft.VisualStudio.Shell.Interop
{
    [InterfaceType(1)]
    [Guid("D824A797-62D2-4F60-98F8-4423624BC1BF")]
    public interface IVsErrorList
    {
        int BringToFront();

        int ForceShowErrors();
    }
}