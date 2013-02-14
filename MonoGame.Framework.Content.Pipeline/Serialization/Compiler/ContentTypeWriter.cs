// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    public abstract class ContentTypeWriter
    {
        public readonly Type _targetType;

        protected ContentTypeWriter(Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException();

            _targetType = targetType;
        }
        
        public virtual bool CanDeserializeIntoExistingObject
        {
            get
            {
                throw new NotImplementedException();
            }

        }

        public Type TargetType { get { return _targetType; } }

        private int _typeVersion;
        public virtual int TypeVersion { get { return _typeVersion; } }

        public abstract string GetRuntimeReader(TargetPlatform targetPlatform);

        public virtual string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return String.Empty;
        }

        protected virtual void Initialize(ContentCompiler compiler)
        {

        }

        protected internal virtual bool ShouldCompressContent(TargetPlatform targetPlatform, object value)
        {
            throw new NotImplementedException();
        }

        protected internal abstract void Write(ContentWriter output, object value);
    }

     public abstract class ContentTypeWriter<T> : ContentTypeWriter
     {
         protected ContentTypeWriter() : base(typeof(T))
         {

         }

         protected internal override void Write(ContentWriter output, object value)
         {

         }

         protected internal abstract void Write(ContentWriter output, T value);
     }
}
