using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGame.Tools.Pipeline
{
    public static class Extensions
    {
        public static bool NullOrEmpty(this IList list)
        {
            if (list == null || list.Count == 0)
                return true;

            return false;
        }
    }
}
