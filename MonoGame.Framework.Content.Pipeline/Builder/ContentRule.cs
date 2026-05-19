// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Content.Pipeline.Builder;

/// <summary>
/// A rule by which <see cref="ContentCollection"/> includes or excludes the files.
/// </summary>
public abstract class ContentRule
{
    /// <summary>
    /// A pattern passed from <see cref="ContentCollection"/> for using in <see cref="IsMatch"/> method. 
    /// </summary>
    public string Pattern { get; init; } = "";

    /// <summary>
    /// Used in <see cref="ContentCollection"/> to check if the passed filePath matches the current <see cref="Pattern"/>.
    /// </summary>
    /// <param name="filePath">Relative path to the content file.</param>
    /// <returns>Returns a boolean indicating if the rule has matched the file.</returns>
    public abstract bool IsMatch(string filePath);
}
