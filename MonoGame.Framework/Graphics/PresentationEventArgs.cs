// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class PresentationEventArgs : EventArgs
    {
        public PresentationParameters PresentationParameters { get; private set; }

        public PresentationEventArgs(PresentationParameters presentationParameters)
        {
            PresentationParameters = presentationParameters;
        }
    }
}
