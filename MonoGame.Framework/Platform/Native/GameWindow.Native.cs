// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework;

internal class NativeGameWindow : GameWindow
{
    public override bool AllowUserResizing { get; set; }
    public override Rectangle ClientBounds { get; }
    public override DisplayOrientation CurrentOrientation { get; }
    public override IntPtr Handle { get; }
    public override string ScreenDeviceName { get; }

    public override void BeginScreenDeviceChange(bool willBeFullScreen)
    {

    }

    public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
    {

    }

    protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
    {

    }

    protected override void SetTitle(string title)
    {

    }
}
