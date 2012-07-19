using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.VisualStudio.TextTemplating
{
    /// <summary>
    /// reproduces some basic functionality of the Visual Studio SDK NativeMethods class
    /// </summary>
    internal static class NativeMethods
    {
        public static bool Failed(int hResult)
        {
            return hResult < 0;
        }
    }
}
