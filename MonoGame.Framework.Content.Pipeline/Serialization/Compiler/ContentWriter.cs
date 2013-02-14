// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    public sealed class ContentWriter : BinaryWriter
    {
        private TargetPlatform _targetPlatform;
        public TargetPlatform TargetPlatform { get { return _targetPlatform; } }

        private GraphicsProfile _targetProfile;
        public GraphicsProfile TargetProfile { get { return _targetProfile; } }

        public void WriteExternalReference<T>(ExternalReference<T> reference)
        {
            throw new NotImplementedException();
        }

        public void WriteObject<T>(T value)
        {
            throw new NotImplementedException();
        }

        public void WriteObject<T>(T value, ContentTypeWriter typeWriter)
        {
            throw new NotImplementedException();
        }

        public void WriteRawObject<T>(T value)
        {
            throw new NotImplementedException();
        }

        public void WriteRawObject<T>(T value, ContentTypeWriter typeWriter)
        {
            throw new NotImplementedException();
        }

        public void WriteSharedResource<T>(T value)
        {
            throw new NotImplementedException();
        }

        public void Write(Color value)
        {
            Write(value.R);
            Write(value.G);
            Write(value.B);
            Write(value.A);
        }

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

        public void Write(Quaternion value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }

        public void Write(Vector2 value)
        {
            Write(value.X);
            Write(value.Y);
        }

        public void Write(Vector3 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
        }

        public void Write(Vector4 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }
    }
}
