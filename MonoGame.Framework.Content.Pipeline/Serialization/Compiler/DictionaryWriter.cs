// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the dictionary to the output.
    /// </summary>
    [ContentTypeWriter]
    class DictionaryWriter<K,V> : BuiltInContentWriter<Dictionary<K,V>>
    {
        ContentTypeWriter _keyWriter;
        ContentTypeWriter _valueWriter;

        protected override void Initialize(ContentCompiler compiler)
        {
            base.Initialize(compiler);

            _keyWriter = compiler.GetTypeWriter(typeof(K));
            _valueWriter = compiler.GetTypeWriter(typeof(V));
        }

        public override bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        protected internal override void Write(ContentWriter output, Dictionary<K,V> value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            output.Write(value.Count);
            foreach (var element in value)
            {
                output.WriteObject(element.Key, _keyWriter);
                output.WriteObject(element.Value, _valueWriter);
            }
        }
    }
}
