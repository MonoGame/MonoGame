// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Identifiers for the target platform.
    /// </summary>
    [TypeConverter(typeof(TargetPlatformTypeConverter))]
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
        /// Google Chrome Native Client
        /// (MonoGame)
        /// </summary>
        NativeClient,

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
        /// Sony PlayStation5
        /// </summary>
        PlayStation5,

        /// <summary>
        /// Xbox One
        /// </summary>
        XboxOne,

        /// <summary>
        /// Nintendo Switch
        /// </summary>
        Switch,

        /// <summary>
        /// Google Stadia
        /// </summary>
        Stadia,

        /// <summary>
        /// WebAssembly and Bridge.NET
        /// </summary>
        Web
    }


    /// <summary>
    /// Deserialize legacy Platforms from .MGCB files.
    /// </summary>
    internal class TargetPlatformTypeConverter : EnumConverter
    {
        public TargetPlatformTypeConverter(Type type) : base(type)
        {
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {   
            try
            {
                return base.ConvertFrom(context, culture, value);
            }
            catch (FormatException)
            { 
                // convert legacy Platforms
                if (value.Equals("Linux") || value.Equals("WindowsGL"))
                    return TargetPlatform.DesktopGL;
                else
                    throw;
            }
        }
    }
}
