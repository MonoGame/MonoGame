// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Xna.Framework
{
    partial class GraphicsDeviceManager
    {
        [CLSCompliant(false)] 
        public SwapChainPanel SwapChainPanel { get; set; }

        partial void PlatformPreparePresentationParameters(PresentationParameters presentationParameters)
        {

            // The graphics device can use a XAML panel or a window
            // to created the default swapchain target.
            if (SwapChainPanel != null)
            {
                presentationParameters.DeviceWindowHandle = IntPtr.Zero;
                presentationParameters.SwapChainPanel = this.SwapChainPanel;
           }
            else
            {
                presentationParameters.DeviceWindowHandle = _game.Window.Handle;
                presentationParameters.SwapChainPanel = null;
            }
        }
    }
}
