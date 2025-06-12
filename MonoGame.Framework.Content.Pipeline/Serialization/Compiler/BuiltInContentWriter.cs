// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Base class for the built-in content type writers where the content type is the same as the runtime type.
    /// </summary>
    /// <typeparam name="T">The content type being written.</typeparam>
    class BuiltInContentWriter<T> : ContentTypeWriter<T>
    {
        private List<ContentTypeWriter> _genericTypes;

        /// <inheritdoc/>
        internal override void OnAddedToContentWriter(ContentWriter output)
        {
            base.OnAddedToContentWriter(output);

            if (TargetType.IsGenericType)
            {
                _genericTypes = new List<ContentTypeWriter>();
                var arguments = TargetType.GetGenericArguments();
                foreach (var arg in arguments)
                    _genericTypes.Add(output.GetTypeWriter(arg));
            }
        }

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
            var readerClassName = this.GetType().Name.Replace("Writer", "Reader");

            // Add generic arguments if they exist.
            if (_genericTypes != null)
            {
                readerClassName += "[";
                foreach (var argWriter in _genericTypes)
                {
                    readerClassName += "[";
                    readerClassName += argWriter.GetRuntimeType(targetPlatform);
                    readerClassName += "]";
                    // Important: Do not add a space char after the comma because 
                    // this will not work with Type.GetType in Xamarin.Android!
                    readerClassName += ",";
                }
                readerClassName = readerClassName.TrimEnd(',', ' ');
                readerClassName += "]";
            }

            // From looking at XNA-produced XNBs, it appears built-in
            // type readers don't need assembly qualification.
            var readerNamespace = typeof(ContentTypeReader).Namespace;
            return readerNamespace + "." + readerClassName;
        }
    }
}
