// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Provides an implementation for many of the ContentCompiler methods including compilation, state tracking for shared resources and creation of the header type manifest.
    /// </summary>
    /// <remarks>A new ContentWriter is constructed for each compilation operation.</remarks>
    public sealed class ContentWriter : BinaryWriter
    {
        TargetPlatform _targetPlatform;
        GraphicsProfile _targetProfile;

        /// <summary>
        /// Gets the content build target platform.
        /// </summary>
        public TargetPlatform TargetPlatform { get { return _targetPlatform; } }

        /// <summary>
        /// Gets or sets the target graphics profile.
        /// </summary>
        public GraphicsProfile TargetProfile { get { return _targetProfile; } }

        /// <summary>
        /// Releases the resources used by the IDisposable class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {

        }

        /// <summary>
        /// Writes the name of an external file to the output binary.
        /// </summary>
        /// <typeparam name="T">The type of reference.</typeparam>
        /// <param name="reference">External reference to a data file for the content item.</param>
        public void WriteExternalReference<T>(ExternalReference<T> reference)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a single object preceded by a type identifier to the output binary.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to write.</param>
        /// <remarks>This method can be called recursively with a null value.</remarks>
        public void WriteObject<T>(T value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a single object to the output binary, using the specified type hint and writer worker.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to write.</param>
        /// <param name="typeWriter">The content type writer.</param>
        /// <remarks>The type hint should be retrieved from the Initialize method of the ContentTypeWriter
        /// that is calling WriteObject, by calling GetTypeWriter and passing it the type of the field used
        /// to hold the value being serialized. If the hint type is a sealed value type (which cannot be
        /// null or hold a polymorphic object instance) this method skips writing the usual type identifier.</remarks>
        public void WriteObject<T>(T value, ContentTypeWriter typeWriter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a single object to the output binary as an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to write.</param>
        /// <remarks>If you specify a base class of the actual object value only data from this base type
        /// will be written. This method does not write any type identifier so it cannot support null or
        /// polymorphic values, and the reader must specify an identical type while loading the compiled data.</remarks>
        public void WriteRawObject<T>(T value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a single object to the output binary using the specified writer worker.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to write.</param>
        /// <param name="typeWriter">The writer worker. This should be looked up from the Initialize method
        /// of the ContentTypeWriter that is calling WriteRawObject, by calling GetTypeWriter.</param>
        /// <remarks>WriteRawObject does not write any type identifier, so it cannot support null or polymorphic
        /// values, and the reader must specify an identical type while loading the compiled data.</remarks>
        public void WriteRawObject<T>(T value, ContentTypeWriter typeWriter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a shared reference to the output binary and records the object to be serialized later.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The object to record.</param>
        public void WriteSharedResource<T>(T value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a Color value.
        /// </summary>
        /// <param name="value">Value of a color using Red, Green, Blue, and Alpha values to write.</param>
        public void Write(Color value)
        {
            Write(value.R);
            Write(value.G);
            Write(value.B);
            Write(value.A);
        }

        /// <summary>
        /// Writes a Matrix value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(Matrix value)
        {
            Write(value.M11);
            Write(value.M12);
            Write(value.M13);
            Write(value.M14);
            Write(value.M21);
            Write(value.M22);
            Write(value.M23);
            Write(value.M24);
            Write(value.M31);
            Write(value.M32);
            Write(value.M33);
            Write(value.M34);
            Write(value.M41);
            Write(value.M42);
            Write(value.M43);
            Write(value.M44);
        }

        /// <summary>
        /// Writes a Matrix value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(Quaternion value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }

        /// <summary>
        /// Writes a Vector2 value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(Vector2 value)
        {
            Write(value.X);
            Write(value.Y);
        }

        /// <summary>
        /// Writes a Vector3 value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(Vector3 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
        }

        /// <summary>
        /// Writes a Vector4 value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public void Write(Vector4 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }
    }
}
