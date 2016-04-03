// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    public class VertexTests
    {
#if !XNA
        [Test]
        public void TestVertexPosition()
        {
            Assert.That(VertexPosition.VertexDeclaration.VertexStride, Is.EqualTo(12));

            var vertexElements = VertexPosition.VertexDeclaration.GetVertexElements();
            Assert.That(vertexElements, Has.Length.EqualTo(1));
            Assert.That(vertexElements[0].Offset, Is.EqualTo(0));
            Assert.That(vertexElements[0].UsageIndex, Is.EqualTo(0));
            Assert.That(vertexElements[0].VertexElementFormat, Is.EqualTo(VertexElementFormat.Vector3));
            Assert.That(vertexElements[0].VertexElementUsage, Is.EqualTo(VertexElementUsage.Position));

            var vertex1 = new VertexPosition(Vector3.One);
            var vertex2 = new VertexPosition(Vector3.One);
            var vertex3 = new VertexPosition(Vector3.Zero);

            Assert.That(vertex1 == vertex2, Is.True);
            Assert.That(vertex1 != vertex2, Is.False);
            Assert.That(vertex1 == vertex3, Is.False);
            Assert.That(vertex1 != vertex3, Is.True);
            Assert.That(vertex1.Equals(vertex2), Is.True);
            Assert.That(vertex1.Equals(vertex3), Is.False);
        }
#endif

        [Test]
        public void TestVertexPositionColor()
        {
            Assert.That(VertexPositionColor.VertexDeclaration.VertexStride, Is.EqualTo(16));

            var vertexElements = VertexPositionColor.VertexDeclaration.GetVertexElements();
            Assert.That(vertexElements, Has.Length.EqualTo(2));
            Assert.That(vertexElements[0].Offset, Is.EqualTo(0));
            Assert.That(vertexElements[0].UsageIndex, Is.EqualTo(0));
            Assert.That(vertexElements[0].VertexElementFormat, Is.EqualTo(VertexElementFormat.Vector3));
            Assert.That(vertexElements[0].VertexElementUsage, Is.EqualTo(VertexElementUsage.Position));
            Assert.That(vertexElements[1].Offset, Is.EqualTo(12));
            Assert.That(vertexElements[1].UsageIndex, Is.EqualTo(0));
            Assert.That(vertexElements[1].VertexElementFormat, Is.EqualTo(VertexElementFormat.Color));
            Assert.That(vertexElements[1].VertexElementUsage, Is.EqualTo(VertexElementUsage.Color));

            var vertex1 = new VertexPositionColor(Vector3.One, Color.Blue);
            var vertex2 = new VertexPositionColor(Vector3.One, Color.Blue);
            var vertex3 = new VertexPositionColor(Vector3.One, Color.Red);
            var vertex4 = new VertexPositionColor(Vector3.Forward, Color.Blue);

            Assert.That(vertex1 == vertex2, Is.True);
            Assert.That(vertex1 != vertex2, Is.False);
            Assert.That(vertex1 == vertex3, Is.False);
            Assert.That(vertex1 != vertex3, Is.True);
            Assert.That(vertex1 == vertex4, Is.False);
            Assert.That(vertex1 != vertex4, Is.True);
            Assert.That(vertex1.Equals(vertex2), Is.True);
            Assert.That(vertex1.Equals(vertex3), Is.False);
            Assert.That(vertex1.Equals(vertex4), Is.False);
        }

        [Test]
        public void TestVertexPositionColorTexture()
        {
            Assert.That(VertexPositionColorTexture.VertexDeclaration.VertexStride, Is.EqualTo(24));

            var vertexElements = VertexPositionColorTexture.VertexDeclaration.GetVertexElements();
            Assert.That(vertexElements, Has.Length.EqualTo(3));
            Assert.That(vertexElements[0].Offset, Is.EqualTo(0));
            Assert.That(vertexElements[0].UsageIndex, Is.EqualTo(0));
            Assert.That(vertexElements[0].VertexElementFormat, Is.EqualTo(VertexElementFormat.Vector3));
            Assert.That(vertexElements[0].VertexElementUsage, Is.EqualTo(VertexElementUsage.Position));
            Assert.That(vertexElements[1].Offset, Is.EqualTo(12));
            Assert.That(vertexElements[1].UsageIndex, Is.EqualTo(0));
            Assert.That(vertexElements[1].VertexElementFormat, Is.EqualTo(VertexElementFormat.Color));
            Assert.That(vertexElements[1].VertexElementUsage, Is.EqualTo(VertexElementUsage.Color));
            Assert.That(vertexElements[2].Offset, Is.EqualTo(16));
            Assert.That(vertexElements[2].UsageIndex, Is.EqualTo(0));
            Assert.That(vertexElements[2].VertexElementFormat, Is.EqualTo(VertexElementFormat.Vector2));
            Assert.That(vertexElements[2].VertexElementUsage, Is.EqualTo(VertexElementUsage.TextureCoordinate));

            var vertex1 = new VertexPositionColorTexture(Vector3.One, Color.Blue, Vector2.One);
            var vertex2 = new VertexPositionColorTexture(Vector3.One, Color.Blue, Vector2.One);
            var vertex3 = new VertexPositionColorTexture(Vector3.One, Color.Red, Vector2.One);
            var vertex4 = new VertexPositionColorTexture(Vector3.Forward, Color.Blue, Vector2.One);
            var vertex5 = new VertexPositionColorTexture(Vector3.Forward, Color.Blue, Vector2.Zero);

            Assert.That(vertex1 == vertex2, Is.True);
            Assert.That(vertex1 != vertex2, Is.False);
            Assert.That(vertex1 == vertex3, Is.False);
            Assert.That(vertex1 != vertex3, Is.True);
            Assert.That(vertex1 == vertex4, Is.False);
            Assert.That(vertex1 != vertex4, Is.True);
            Assert.That(vertex4 == vertex5, Is.False);
            Assert.That(vertex4 != vertex5, Is.True);
            Assert.That(vertex1.Equals(vertex2), Is.True);
            Assert.That(vertex1.Equals(vertex3), Is.False);
            Assert.That(vertex1.Equals(vertex4), Is.False);
            Assert.That(vertex4.Equals(vertex5), Is.False);
        }

        [Test]
        public void TestVertexPositionNormalTexture()
        {
            Assert.That(VertexPositionNormalTexture.VertexDeclaration.VertexStride, Is.EqualTo(32));

            var vertexElements = VertexPositionNormalTexture.VertexDeclaration.GetVertexElements();
            Assert.That(vertexElements, Has.Length.EqualTo(3));
            Assert.That(vertexElements[0].Offset, Is.EqualTo(0));
            Assert.That(vertexElements[0].UsageIndex, Is.EqualTo(0));
            Assert.That(vertexElements[0].VertexElementFormat, Is.EqualTo(VertexElementFormat.Vector3));
            Assert.That(vertexElements[0].VertexElementUsage, Is.EqualTo(VertexElementUsage.Position));
            Assert.That(vertexElements[1].Offset, Is.EqualTo(12));
            Assert.That(vertexElements[1].UsageIndex, Is.EqualTo(0));
            Assert.That(vertexElements[1].VertexElementFormat, Is.EqualTo(VertexElementFormat.Vector3));
            Assert.That(vertexElements[1].VertexElementUsage, Is.EqualTo(VertexElementUsage.Normal));
            Assert.That(vertexElements[2].Offset, Is.EqualTo(24));
            Assert.That(vertexElements[2].UsageIndex, Is.EqualTo(0));
            Assert.That(vertexElements[2].VertexElementFormat, Is.EqualTo(VertexElementFormat.Vector2));
            Assert.That(vertexElements[2].VertexElementUsage, Is.EqualTo(VertexElementUsage.TextureCoordinate));

            var vertex1 = new VertexPositionNormalTexture(Vector3.One, Vector3.Forward, Vector2.One);
            var vertex2 = new VertexPositionNormalTexture(Vector3.One, Vector3.Forward, Vector2.One);
            var vertex3 = new VertexPositionNormalTexture(Vector3.One, Vector3.Backward, Vector2.One);
            var vertex4 = new VertexPositionNormalTexture(Vector3.Forward, Vector3.Backward, Vector2.One);
            var vertex5 = new VertexPositionNormalTexture(Vector3.Forward, Vector3.Backward, Vector2.Zero);

            Assert.That(vertex1 == vertex2, Is.True);
            Assert.That(vertex1 != vertex2, Is.False);
            Assert.That(vertex1 == vertex3, Is.False);
            Assert.That(vertex1 != vertex3, Is.True);
            Assert.That(vertex1 == vertex4, Is.False);
            Assert.That(vertex1 != vertex4, Is.True);
            Assert.That(vertex4 == vertex5, Is.False);
            Assert.That(vertex4 != vertex5, Is.True);
            Assert.That(vertex1.Equals(vertex2), Is.True);
            Assert.That(vertex1.Equals(vertex3), Is.False);
            Assert.That(vertex1.Equals(vertex4), Is.False);
            Assert.That(vertex4.Equals(vertex5), Is.False);
        }

        [Test]
        public void TestVertexPositionTexture()
        {
            Assert.That(VertexPositionTexture.VertexDeclaration.VertexStride, Is.EqualTo(20));

            var vertexElements = VertexPositionTexture.VertexDeclaration.GetVertexElements();
            Assert.That(vertexElements, Has.Length.EqualTo(2));
            Assert.That(vertexElements[0].Offset, Is.EqualTo(0));
            Assert.That(vertexElements[0].UsageIndex, Is.EqualTo(0));
            Assert.That(vertexElements[0].VertexElementFormat, Is.EqualTo(VertexElementFormat.Vector3));
            Assert.That(vertexElements[0].VertexElementUsage, Is.EqualTo(VertexElementUsage.Position));
            Assert.That(vertexElements[1].Offset, Is.EqualTo(12));
            Assert.That(vertexElements[1].UsageIndex, Is.EqualTo(0));
            Assert.That(vertexElements[1].VertexElementFormat, Is.EqualTo(VertexElementFormat.Vector2));
            Assert.That(vertexElements[1].VertexElementUsage, Is.EqualTo(VertexElementUsage.TextureCoordinate));

            var vertex1 = new VertexPositionTexture(Vector3.One, Vector2.One);
            var vertex2 = new VertexPositionTexture(Vector3.One, Vector2.One);
            var vertex3 = new VertexPositionTexture(Vector3.One, Vector2.Zero);
            var vertex4 = new VertexPositionTexture(Vector3.Zero, Vector2.Zero);

            Assert.That(vertex1 == vertex2, Is.True);
            Assert.That(vertex1 != vertex2, Is.False);
            Assert.That(vertex1 == vertex3, Is.False);
            Assert.That(vertex1 != vertex3, Is.True);
            Assert.That(vertex1 == vertex4, Is.False);
            Assert.That(vertex1 != vertex4, Is.True);
            Assert.That(vertex3 == vertex4, Is.False);
            Assert.That(vertex3 != vertex4, Is.True);
            Assert.That(vertex1.Equals(vertex2), Is.True);
            Assert.That(vertex1.Equals(vertex3), Is.False);
            Assert.That(vertex1.Equals(vertex4), Is.False);
            Assert.That(vertex3.Equals(vertex4), Is.False);
        }
    }
}