using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Shell.Interop
{
    [ComConversionLoss]
    [Guid("6DDD8DC3-32B2-4BF1-A1E1-B6DA40526D1E")]
    [InterfaceType(1)]
    public interface IVsHierarchyEvents
    {
        int OnInvalidateIcon(IntPtr hicon);

        int OnInvalidateItems(uint itemidParent);

        int OnItemAdded(uint itemidParent, uint itemidSiblingPrev, uint itemidAdded);

        int OnItemDeleted(uint itemid);

        int OnItemsAppended(uint itemidParent);

        int OnPropertyChanged(uint itemid, int propid, uint flags);
    }
}
