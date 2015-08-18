// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Identifiers for the target platform.
    /// </summary>
    public enum TargetPlatform
    {
        /// <summary>
        /// All desktop versions of Windows using DirectX.
        /// </summary>
        Windows,

        /// <summary>
        /// Xbox 360 video game and entertainment system
        /// </summary>
        Xbox360,

        /// <summary>
        /// Windows Phone
        /// </summary>
        WindowsPhone,

        // MonoGame-specific platforms listed below

        /// <summary>
        /// Apple iOS-based devices (iPod Touch, iPhone, iPad)
        /// (MonoGame)
        /// </summary>
        iOS,

        /// <summary>
        /// Android-based devices
        /// (MonoGame)
        /// </summary>
        Android,

        /// <summary>
        /// All desktop versions using OpenGL.
        /// (MonoGame)
        /// </summary>
        DesktopGL,

        /// <summary>
        /// Apple Mac OSX-based devices (iMac, MacBook, MacBook Air, etc)
        /// (MonoGame)
        /// </summary>
        MacOSX,

        /// <summary>
        /// Windows Store App
        /// (MonoGame)
        /// </summary>
        WindowsStoreApp,

        /// <summary>
        /// Google Chrome Native Client
        /// (MonoGame)
        /// </summary>
        NativeClient,

        /// <summary>
        /// Sony PlayStation Mobile (PS Vita)
        /// (MonoGame)
        /// </summary>
        [Obsolete("PlayStation Mobile is no longer supported")]
        PlayStationMobile,

        /// <summary>
        /// Windows Phone 8
        /// (MonoGame)
        /// </summary>
        WindowsPhone8,

        /// <summary>
        /// Raspberry Pi
        /// (MonoGame)
        /// </summary>
        RaspberryPi,

        /// <summary>
        /// Sony PlayStation4
        /// </summary>
        PlayStation4,

        /// <summary>
        /// All desktop versions of Windows using OpenGL.
        /// (MonoGame)
        /// </summary>
        [Obsolete("This platform is obsolete, use DesktopGL instead")]
        WindowsGL = DesktopGL,

        /// <summary>
        /// Linux-based PCs
        /// (MonoGame)
        /// </summary>
        [Obsolete("This platform is obsolete, use DesktopGL instead")]
        Linux = DesktopGL
    }
}
