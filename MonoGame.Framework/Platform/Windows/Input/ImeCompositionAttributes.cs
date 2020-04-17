namespace Microsoft.Xna.Framework.Windows.Input
{
    /// <summary>
    /// Winforms IME composition character attributes
    /// </summary>
    internal enum ImeCompositionAttributes
    {
        /// <summary>
        /// Character being entered by the user.
        /// The IME has yet to convert this character.
        /// </summary>
        Input = 0x00,
        /// <summary>
        /// Character selected by the user and then converted by the IME.
        /// </summary>
        TargetConverted = 0x01,
        /// <summary>
        /// Character that the IME has already converted.
        /// </summary>
        Converted = 0x02,
        /// <summary>
        /// Character being converted. The user has selected this character
        /// but the IME has not yet converted it.
        /// </summary>
        TargetNotConverted = 0x03,
        /// <summary>
        /// An error character that the IME cannot convert. For example,
        /// the IME cannot put together some consonants.
        /// </summary>
        InputError = 0x04,
        /// <summary>
        /// Characters that the IME will no longer convert.
        /// </summary>
        FixedConverted = 0x05,
    }

}
