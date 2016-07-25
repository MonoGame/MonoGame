// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class GraphicsAdapter : IDisposable
    {
        /// <summary>
        /// Defines the driver type for graphics adapter. Usable only on DirectX platforms for now.
        /// </summary>
        public enum DriverType
        {
            /// <summary>
            /// Hardware device been used for rendering. Maximum speed and performance.
            /// </summary>
            Hardware,
            /// <summary>
            /// Emulates the hardware device on CPU. Slowly, only for testing.
            /// </summary>
            Reference,
            /// <summary>
            /// Useful when <see cref="DriverType.Hardware"/> acceleration does not work.
            /// </summary>
            FastSoftware
        }

        private static readonly ReadOnlyCollection<GraphicsAdapter> _adapters;

        private DisplayModeCollection _supportedDisplayModes;

        private DisplayMode _currentDisplayMode;

        static GraphicsAdapter()
        {
            // NOTE: An adapter is a monitor+device combination, so we expect
            // at lease one adapter per connected monitor.
            PlatformInitializeAdapters(out _adapters);

            // The first adapter is considered the default.
            _adapters[0].IsDefaultAdapter = true;
        }

        public static GraphicsAdapter DefaultAdapter
        {
            get { return _adapters[0]; }
        }
        
        public static ReadOnlyCollection<GraphicsAdapter> Adapters 
        {
            get  { return _adapters; }
        }

        /// <summary>
        /// Used to request creation of the reference graphics device, 
        /// or the default hardware accelerated device (when set to false).
        /// </summary>
        /// <remarks>
        /// This only works on DirectX platforms where a reference graphics
        /// device is available and must be defined before the graphics device
        /// is created. It defaults to false.
        /// </remarks>
        public static bool UseReferenceDevice
        {
            get { return UseDriverType == DriverType.Reference; }
            set { UseDriverType = value ? DriverType.Reference : DriverType.Hardware; }
        }

        /// <summary>
        /// Used to request creation of a specific kind of driver.
        /// </summary>
        /// <remarks>
        /// These values only work on DirectX platforms and must be defined before the graphics device
        /// is created. <see cref="DriverType.Hardware"/> by default.
        /// </remarks>
        public static DriverType UseDriverType { get; set; }

        public string Description { get; private set; }

        public int DeviceId { get; private set; }

        public string DeviceName { get; private set; }

        public int VendorId { get; private set; }

        public bool IsDefaultAdapter { get; private set; }

        public IntPtr MonitorHandle { get; private set; }

        public int Revision { get; private set; }

        public int SubSystemId { get; private set; }
       
        public DisplayModeCollection SupportedDisplayModes
        {
            get { return _supportedDisplayModes; }
        }

        public DisplayMode CurrentDisplayMode
        {
            get { return _currentDisplayMode; }
        }

        /// <summary>
        /// Returns true if the <see cref="GraphicsAdapter.CurrentDisplayMode"/> is widescreen.
        /// </summary>
        /// <remarks>
        /// Common widescreen modes include 16:9, 16:10 and 2:1.
        /// </remarks>
        public bool IsWideScreen
        {
            get
            {
                // Seems like XNA treats aspect ratios above 16:10 as wide screen.
                const float minWideScreenAspect = 16.0f / 10.0f;
                return CurrentDisplayMode.AspectRatio >= minWideScreenAspect;
            }
        }

        /*
		public bool QueryRenderTargetFormat(
			GraphicsProfile graphicsProfile,
			SurfaceFormat format,
			DepthFormat depthFormat,
			int multiSampleCount,
			out SurfaceFormat selectedFormat,
			out DepthFormat selectedDepthFormat,
			out int selectedMultiSampleCount)
		{
			throw new NotImplementedException();
		}
        */

        public void Dispose()
        {
            // We don't keep any resources, so we have
            // nothing to do... just here for XNA compatibility.
        }
    }
}
