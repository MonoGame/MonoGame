// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// Defines the core behavior of content type readers used for reading a specific managed type from an .xnb binary
    /// format and provides a base for derived classes.
    /// </summary>
    public abstract class ContentTypeReader
    {
        private Type _targetType;

        /// <summary>
        /// Gets a value that indicates whether this content type read can deserialize into an object with the same
        /// type ad defined in the <see cref="TargetType"/> property.
        /// </summary>
        public virtual bool CanDeserializeIntoExistingObject
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the type handled by this reader component.
        /// </summary>
        public Type TargetType
        {
            get { return _targetType; }
        }

        /// <summary>
        /// Gets the format version number for this type.
        /// </summary>
        public virtual int TypeVersion
        {
            get { return 0; }   // The default version (unless overridden) is zero
        }

        /// <summary />
        protected ContentTypeReader(Type targetType)
        {
            _targetType = targetType;
        }

        /// <summary />
        protected internal virtual void Initialize(ContentTypeReaderManager manager)
        {
            // Do nothing. Are we supposed to add ourselves to the manager?
        }

        /// <summary />
        protected internal abstract object Read(ContentReader input, object existingInstance);
    }

    /// <summary>
    /// Defines the core behavior of content type readers used for reading a specific managed type from an .xnb binary
    /// format and provides a base for derived classes.  Derive from this class to add new data types to the content
    /// pipeline system.
    /// </summary>
    /// <typeparam name="T">The managed type to read.</typeparam>
    public abstract class ContentTypeReader<T> : ContentTypeReader
    {
        /// <summary />
        protected ContentTypeReader()
            : base(typeof(T))
        {
            // Nothing
        }

        /// <inheritdoc />
        protected internal override object Read(ContentReader input, object existingInstance)
        {
			// as per the documentation http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.content.contenttypereader.read.aspx
			// existingInstance
			// The object receiving the data, or null if a new instance of the object should be created.
			if (existingInstance == null)
            {
				return Read(input, default(T));
			} 
			return Read(input, (T)existingInstance);
        }

        /// <summary />
        protected internal abstract T Read(ContentReader input, T existingInstance);
    }
}
