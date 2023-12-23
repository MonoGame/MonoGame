// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Used by the platform code to control the graphics device.
    /// </summary>
    public interface IGraphicsDeviceManager
    {
        /// <summary>
        ///     Gets the <see cref="Graphics.GraphicsDevice" /> associated with the <see cref="IGraphicsDeviceManager" />.
        /// </summary>
        GraphicsDevice GraphicsDevice { get; }

        /// <summary>
        /// Raised when a new <see cref="Graphics.GraphicsDevice"/> has been created.
        /// </summary>
        event EventHandler<EventArgs> DeviceCreated;

        /// <summary>
        /// Raised when the <see cref="GraphicsDevice"/> is disposed.
        /// </summary>
        event EventHandler<EventArgs> DeviceDisposing;

        /// <summary>
        /// Raised when the <see cref="GraphicsDevice"/> has reset.
        /// </summary>
        /// <seealso cref="Microsoft.Xna.Framework.Graphics.GraphicsDevice.Reset"/>
        event EventHandler<EventArgs> DeviceReset;

        /// <summary>
        /// Raised before the <see cref="GraphicsDevice"/> is resetting.
        /// </summary>
        event EventHandler<EventArgs> DeviceResetting;

        /// <summary>
        ///     Gets or sets the graphics profile, which determines the graphics feature set.
        /// </summary>
        GraphicsProfile GraphicsProfile { get; set; }

        /// <summary>
        ///     Gets or sets a boolean indicating whether the window should be resizable.
        /// </summary>
        bool AllowResize { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether the device should start in full-screen mode.
        /// </summary>
        bool IsFullScreen { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the mouse cursor should be visible.
        /// </summary>
        bool IsMouseVisible { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to use fixed time steps.
        /// </summary>
        /// <value>true if using fixed time steps; false otherwise.</value>
        bool IsFixedTimeStep { get; set; }

        /// <summary>
        ///     Gets or sets the target time between calls to update when <see cref="IsFixedTimeStep"/> is true.
        /// </summary>
        TimeSpan TargetElapsedTime { get; set; }

        /// <summary>
        ///     Gets or sets the boolean which defines how window switches from windowed to fullscreen state.
        ///     "Hard" mode(true) is slow to switch, but more efficient for performance, while "soft" mode(false) is vice versa.
        ///     The default value is true.
        /// </summary>
        bool HardwareModeSwitch { get; set; }

        /// <summary>
        ///     Indicates if DX9 style pixel addressing or current standard pixel addressing should be used.
        /// </summary>
        bool PreferHalfPixelOffset { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether to enable a multisampled back buffer.
        /// </summary>
        bool PreferMultiSampling { get; set; }

        /// <summary>
        ///     Gets or sets the format of the back buffer.
        /// </summary>
        SurfaceFormat PreferredBackBufferFormat { get; set; }

        /// <summary>
        ///     Gets or sets the preferred back-buffer height.
        /// </summary>
        int PreferredBackBufferHeight { get; set; }

        /// <summary>
        ///     Gets or sets the preferred back-buffer width.
        /// </summary>
        int PreferredBackBufferWidth { get; set; }

        /// <summary>
        ///     Gets or sets the format of the depth stencil.
        /// </summary>
        DepthFormat PreferredDepthStencilFormat { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether to sync to the vertical trace (vsync) when presenting the back buffer.
        /// </summary>
        bool SynchronizeWithVerticalRetrace { get; set; }

        /// <summary>
        ///     Gets or sets the display orientations that are available if automatic rotation and scaling is enabled.
        /// </summary>
        DisplayOrientation SupportedOrientations { get; set; }

        /// <summary>
        ///     Applies any changes to device-related properties, changing the graphics device as necessary.
        /// </summary>
        void ApplyChanges();
    }
}

