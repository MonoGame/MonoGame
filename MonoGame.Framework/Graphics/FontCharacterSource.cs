// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Text;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Provides indexed character access over either a <see cref="string"/> or a <see cref="StringBuilder"/>.
/// </summary>
internal struct FontCharacterSource
{
    private readonly StringBuilder _builder;
    private readonly string _string;

    /// <summary>
    /// The number of characters in the wrapped text source.
    /// </summary>
    public readonly int Length;

    /// <summary>
    /// Gets the character at the specified index.
    /// </summary>
    /// <param name="index">The index of the character.</param>
    public char this[int index]
    {
        get
        {
            if (_string != null)
            {
                return _string[index];
            }

            return _builder[index];
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontCharacterSource"/> struct from a string.
    /// </summary>
    /// <param name="text">The string to wrap.</param>
    public FontCharacterSource(string text)
    {
        _string = text;
        _builder = null;
        Length = text.Length;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontCharacterSource"/> struct from a string builder.
    /// </summary>
    /// <param name="text"></param>
    public FontCharacterSource(StringBuilder text)
    {
        _string = null;
        _builder = text;
        Length = text.Length;
    }

}
