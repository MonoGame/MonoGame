// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
#if WINDOWS_STOREAPP || WINDOWS_UAP
using Windows.UI.Xaml.Controls;
#endif

namespace Microsoft.Xna.Framework
{
    partial class GraphicsDeviceManager
    {
#if WINDOWS_STOREAPP || WINDOWS_UAP
        public GenericSwapChainPanel SwapChainPanel { get; set; }
#endif

        partial void PlatformPreparePresentationParameters(PresentationParameters presentationParameters)
        {
#if WINDOWS_UAP

            presentationParameters.DeviceWindowHandle = IntPtr.Zero;
            presentationParameters.SwapChainPanel = this.SwapChainPanel;

#elif WINDOWS_STOREAPP

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
#endif
        }
    }
}
