// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Text.RegularExpressions;

namespace MonoGame.Framework.Content.Pipeline.Builder;

/// <summary>
/// A regex based rule by which <see cref="ContentCollection"/> includes or excludes the files.
/// </summary>
public class RegexRule : ContentRule
{
    /// <summary>
    /// Used in <see cref="ContentCollection"/> to check if the passed filePath matches the current Regex <see cref="ContentRule.Pattern"/>.
    /// </summary>
    /// <param name="filePath">Relative path to the content file.</param>
    /// <returns>Returns true if filePath matches the <see cref="ContentRule.Pattern"/>.</returns>
    public override bool IsMatch(string filePath) => Regex.IsMatch(filePath, Pattern);
}
