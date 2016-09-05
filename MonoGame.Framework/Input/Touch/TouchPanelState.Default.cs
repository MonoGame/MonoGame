// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input.Touch
{
    public partial class TouchPanelState
    {
        private void PlatformAddEvent(int id, TouchLocationState state, Vector2 position, bool isMouse)
        {
            AddEventInternal(id, state, position, isMouse);
        }

        private void PlatformProcessQueued()
        { 
        }
    }
}
