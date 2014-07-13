// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the enum value to the output. Usually 32 bit, but can be other sizes if T is not integer.
    /// </summary>
    /// <typeparam name="T">The enum type to write.</typeparam>
    [ContentTypeWriter]
    class EnumWriter<T> : BuiltInContentWriter<T>
    {
        Type underlyingType;
        ContentTypeWriter elementWriter;

        /// <summary>
        /// Initialize the writer.
        /// </summary>
        /// <param name="compiler">Compiler instance calling this writer.</param>
        protected override void Initialize(ContentCompiler compiler)
        {
            base.Initialize(compiler);

            underlyingType = Enum.GetUnderlyingType(typeof(T));
            elementWriter = compiler.GetTypeWriter(underlyingType);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Microsoft.Xna.Framework.Content.EnumReader`1[[" + GetRuntimeType(targetPlatform) + "]]";
        }

        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected internal override void Write(ContentWriter output, T value)
        {
            elementWriter.Write(output, Convert.ChangeType(value, underlyingType));
        }
    }
}
