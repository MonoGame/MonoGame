// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public class GraphicsDebugMessage
    {
        public string Message { get; set; }

        public string Severity { get; set; }

        public long Id { get; set; }

        public string IdName { get; set; }

        public string Category { get; set; }

        public string Source { get; set; }

        public string Type { get; set; }

        public IntPtr UserdataPointer { get; set; }
    }
}
