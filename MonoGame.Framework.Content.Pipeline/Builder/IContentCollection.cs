// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Content.Pipeline.Builder;

/// <summary>
/// An interface for a collection of data on how the content should be handled.
/// </summary>
public interface IContentCollection
{
    /// <summary>
    /// Gets information about how the ocntent should be handled for the passed relative filepath.
    /// </summary>
    /// <param name="filePath">Relative path to the content file.</param>
    /// <param name="contentInfo"><see cref="ContentInfo"/> describing the desired content handling.</param>
    /// <returns><c>true</c> if the content should be handled, <c>false</c> otherwise.</returns>
    bool GetContentInfo(string filePath, ref ContentInfo? contentInfo);
}
