// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Globalization;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the enum value to the output. Usually 32 bit, but can be other sizes if T is not integer.
    /// </summary>
    /// <typeparam name="T">The enum type to write.</typeparam>
    [ContentTypeWriter]
    class EnumWriter<T> : BuiltInContentWriter<T>
    {
        private Type _underlyingType = null!;
        private ContentTypeWriter _underlyingTypeWriter = null!;

        /// <inheritdoc/>
        internal override void OnAddedToContentWriter(ContentWriter output)
        {
            base.OnAddedToContentWriter(output);
            _underlyingType = Enum.GetUnderlyingType(typeof(T));
            _underlyingTypeWriter = output.GetTypeWriter(_underlyingType);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform) => $"Microsoft.Xna.Framework.Content.EnumReader`1[[{GetRuntimeType(targetPlatform)}]]";

        protected override void Write(ContentWriter output, T value) => output.WriteRawObject(Convert.ChangeType(value, _underlyingType, CultureInfo.InvariantCulture), _underlyingTypeWriter);
    }
}
