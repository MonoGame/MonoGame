// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using SharpDX;
using SharpDX.DXGI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Xna.Framework
{
    public class GenericSwapChainPanel
    {
        internal readonly Grid Panel;
        
        internal GenericSwapChainPanel(SwapChainPanel swapChainPanel)
        {
            Panel = swapChainPanel;
        }

        internal GenericSwapChainPanel(SwapChainBackgroundPanel swapChainBackgroundPanel)
        {
            Panel = swapChainBackgroundPanel;
        }
        
        internal void SetSwapChain(SwapChain1 _swapChain)
        {
            if (Panel is SwapChainBackgroundPanel)
            {
                using (var nativePanel = ComObject.As<SharpDX.DXGI.ISwapChainBackgroundPanelNative>(Panel))
                    nativePanel.SwapChain = _swapChain;
            }
            else
            {                
                using (var nativePanel = ComObject.As<SharpDX.DXGI.ISwapChainPanelNative>(Panel))
                    nativePanel.SwapChain = _swapChain;
            }
        }

        internal CoreIndependentInputSource CreateCoreIndependentInputSource(CoreInputDeviceTypes inputDevices)
        {   
            if (Panel is SwapChainBackgroundPanel)
                return ((SwapChainBackgroundPanel)Panel).CreateCoreIndependentInputSource(inputDevices);
            else
                return ((SwapChainPanel)Panel).CreateCoreIndependentInputSource(inputDevices);
        }
        
        public float CompositionScaleX
        {
            get
            {
                if (Panel is SwapChainBackgroundPanel)
                    return 1f;
                else
                    return ((SwapChainPanel)Panel).CompositionScaleX;
            }
        }
                
        public float CompositionScaleY
        {
            get
            {
                if (Panel is SwapChainBackgroundPanel)
                    return 1f;
                else
                    return ((SwapChainPanel)Panel).CompositionScaleY;
            }
        }

        public double ActualWidth
        {
            get
            {
                if (Panel is SwapChainBackgroundPanel)
                    return ((SwapChainBackgroundPanel)Panel).ActualWidth;
                else
                    return ((SwapChainPanel)Panel).ActualWidth;
            }
        }
        
        public double ActualHeight
        {
            get
            {
                if (Panel is SwapChainBackgroundPanel)
                    return ((SwapChainBackgroundPanel)Panel).ActualHeight;
                else
                    return ((SwapChainPanel)Panel).ActualHeight;
            }
        }
        
        [CLSCompliant(false)]
        public CoreDispatcher Dispatcher { get { return Panel.Dispatcher; } }
    }
}
