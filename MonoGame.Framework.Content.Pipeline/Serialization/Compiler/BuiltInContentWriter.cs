// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Base class for the built-in content type writers where the content type is the same as the runtime type.
    /// </summary>
    /// <typeparam name="T">The content type being written.</typeparam>
    class BuiltInContentWriter<T> : ContentTypeWriter<T>
    {
        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected internal override void Write(ContentWriter output, T value)
        {
        }

        /// <summary>
        /// Gets the assembly qualified name of the runtime loader for this type.
        /// </summary>
        /// <param name="targetPlatform">Name of the platform.</param>
        /// <returns>Name of the runtime loader.</returns>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // Change "Writer" in this class name to "Reader" and use the runtime type namespace and assembly
            string readerClassName = this.GetType().Name.Replace("Writer", "Reader");

            // Add generic arguments if they exist
            var args = this.GetType().GetGenericArguments();
            if (args.Length > 0)
            {
                readerClassName += "[";
                for (int i = 0; i < args.Length; ++i)
                {
                    var arg = args[i];
                    readerClassName += "[";
                    readerClassName += arg.AssemblyQualifiedName;        
                    readerClassName += "]";
                    if (i < args.Length - 1)
                        readerClassName += ", ";
                }
                readerClassName += "]";
            }

            string readerNamespace = typeof(ContentTypeReader).Namespace;
            // From looking at XNA-produced XNBs, it appears built-in type readers don't need assembly qualification
            return readerNamespace + "." + readerClassName;
        }
    }
}
