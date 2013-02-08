// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the array value to the output.
    /// </summary>
    [ContentTypeWriter]
    class ArrayWriter<T> : BuiltInContentWriter<T[]>
    {
        ContentTypeWriter elementWriter;

        /// <summary>
        /// Initialize the writer.
        /// </summary>
        /// <param name="compiler">Compiler instance calling this writer.</param>
        protected override void Initialize(ContentCompiler compiler)
        {
            base.Initialize(compiler);

            elementWriter = compiler.GetTypeWriter(typeof(T));
        }

        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected internal override void Write(ContentWriter output, T[] value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            output.Write(value.Length);
            foreach (var element in value)
                output.WriteObject(element, elementWriter);
        }

        /// <summary>
        /// Gets the assembly qualified name of the runtime loader for this type.
        /// </summary>
        /// <param name="targetPlatform">Name of the platform.</param>
        /// <returns>Name of the runtime loader.</returns>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // Change "Writer" in this class name to "Reader" and use the runtime type namespace and assembly
            string readerClassName = string.Format("{0}[[{1}]]", this.GetType().Name.Replace("Writer", "Reader"), typeof(T).AssemblyQualifiedName);
            string readerNamespace = typeof(ContentTypeReader).Namespace;
            string assemblyName = typeof(ContentTypeReader).Assembly.FullName;
            return readerNamespace + "." + readerClassName + ", " + assemblyName;
        }
    }
}
