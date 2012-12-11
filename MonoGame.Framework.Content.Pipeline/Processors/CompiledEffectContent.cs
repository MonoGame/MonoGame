// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Represents a compiled Effect.
    /// </summary>
    public class CompiledEffectContent : ContentItem
    {
        byte[] effectCode;

        /// <summary>
        /// Creates a new instance of the CompiledEffectContent class
        /// </summary>
        /// <param name="effectCode">The compiled effect code.</param>
        public CompiledEffectContent(byte[] effectCode)
        {
            this.effectCode = effectCode;
        }

        /// <summary>
        /// Retrieves the compiled byte code for this shader.
        /// </summary>
        /// <returns>The compiled bytecode.</returns>
        public byte[] GetEffectCode()
        {
            return effectCode;
        }
    }
}
