// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Describes a range of consecutive characters.
/// </summary>
public struct CharacterRegion
{
    /// <summary>
    /// The default ASCII character region, U+0020 (space) through U+007E (tilde).
    /// </summary>
    public static CharacterRegion Default => new CharacterRegion(' ', '~');

    /// <summary>
    /// The first character in the region.
    /// </summary>
    public char Start;

    /// <summary>
    /// The last character in the region.
    /// </summary>
    public char End;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterRegion"/> struct.
    /// </summary>
    /// <param name="start">The first character in the region.</param>
    /// <param name="end">The last character in the region.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="start"/> is greater than <paramref name="end"/>.
    /// </exception>
    public CharacterRegion(char start, char end)
    {
        if (start > end)
        {
            throw new ArgumentException(
                $"The start character '{start}' must not be greater than the end character '{end}'");
        }

        Start = start;
        End = end;
    }

    /// <summary>
    /// Enumerates all characters in the region.
    /// </summary>
    /// <returns>An enumeration of characters from <see cref="Start"/> through <see cref="End"/>.</returns>
    public readonly IEnumerable<char> Characters()
    {
        for (char c = Start; c <= End; c++)
        {
            yield return c;
        }
    }
}
