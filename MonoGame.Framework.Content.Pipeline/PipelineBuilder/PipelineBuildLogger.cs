// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Globalization;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder;

/// <inheritdoc cref="ContentBuildLogger">ContentBuildLogger</inheritdoc>
public class PipelineBuildLogger : ContentBuildLogger
{
    /// <inheritdoc/>
    [Obsolete($"{nameof(LogMessage)} is deprecated, please use Log instead.")]
    public override void LogMessage(string message, params object[] messageArgs)
        => Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, message, messageArgs));

    /// <inheritdoc/>
    [Obsolete($"{nameof(LogImportantMessage)} is deprecated, please use Log instead.")]
    public override void LogImportantMessage(string message, params object[] messageArgs)
        => Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, message, messageArgs));

    /// <inheritdoc/>
    [Obsolete($"{nameof(LogWarning)} is deprecated, please use Log instead.")]
    public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
        => Trace.WriteLine($"{GetCurrentFilename(contentIdentity)}: {string.Format(CultureInfo.InvariantCulture, message, messageArgs)}");
}
