using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using XnaPoint = Microsoft.Xna.Framework.Point;
using SysPoint = System.Drawing.Point;

namespace Microsoft.Xna.Framework
{
    internal static class PointConversionExtensions
    {
        public static XnaPoint ToPoint(this SysPoint point)
        {
            return new XnaPoint(point.X, point.Y);
        }

        public static SysPoint ToPoint(this XnaPoint point)
        {
            return new SysPoint(point.X, point.Y);
        }
    }
}
