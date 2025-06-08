// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline;

/// <summary>
/// Specifies the importants of the message when logging the mssages from <see cref="ContentBuildLogger.Log(LogLevel, string)"/>.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Specifies that the message is used for debugging purposes only.
    /// </summary>
    Debug,

    /// <summary>
    /// Specifies that the message is just an informative message.
    /// </summary>
    Info,

    /// <summary>
    /// Specifies that the message is a warning message.
    /// </summary>
    Warning,

    /// <summary>
    /// Specifies that the message is an error message.
    /// </summary>
    Error
}
