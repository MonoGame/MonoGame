using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    public class PlaneTest
    {
        [Test]
        public void TransformByMatrix()
        {
            // Our test plane.
            var plane = Plane.Normalize(new Plane(new Vector3(0, 1, 1), 2.5f));

            // Our matrix.
            var matrix = Matrix.CreateRotationX(MathHelper.PiOver2);

            // Test transform.
            var expectedResult = new Plane(new Vector3(0, -0.7071068f, 0.7071067f), 1.767767f);
            Assert.That(Plane.Transform(plane, matrix), Is.EqualTo(expectedResult).Using(PlaneComparer.Epsilon));
        }

        [Test]
        public void TransformByRefMatrix()
        {
            // Our test plane.
            var plane = Plane.Normalize(new Plane(new Vector3(1, 0.8f, 0.5f), 2.5f));
            var originalPlane = plane;

            // Our matrix.
            var matrix = Matrix.CreateRotationX(MathHelper.PiOver2);
            var originalMatrix = matrix;

            // Test transform.
            Plane result;
            Plane.Transform(ref plane, ref matrix, out result);

            var expectedResult = new Plane(new Vector3(0.7273929f, -0.3636965f, 0.5819144f), 1.818482f);
            Assert.That(result, Is.EqualTo(expectedResult).Using(PlaneComparer.Epsilon));

            Assert.That(plane, Is.EqualTo(originalPlane));
            Assert.That(matrix, Is.EqualTo(originalMatrix));
        }

        [Test]
        public void TransformByQuaternion()
        {
            // Our test plane.
            var plane = Plane.Normalize(new Plane(new Vector3(0, 1, 1), 2.5f));

            // Our quaternion.
            var quaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.PiOver2);

            // Test transform.
            var expectedResult = new Plane(new Vector3(0, -0.7071068f, 0.7071067f), 1.767767f);
            Assert.That(Plane.Transform(plane, quaternion), Is.EqualTo(expectedResult).Using(PlaneComparer.Epsilon));
        }

        [Test]
        public void TransformByRefQuaternion()
        {
            // Our test plane.
            var plane = Plane.Normalize(new Plane(new Vector3(1, 0.8f, 0.5f), 2.5f));
            var originalPlane = plane;

            // Our quaternion.
            var quaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.PiOver2);
            var originalQuaternion = quaternion;

            // Test transform.
            Plane result;
            Plane.Transform(ref plane, ref quaternion, out result);

            var expectedResult = new Plane(new Vector3(0.7273929f, -0.3636965f, 0.5819144f), 1.818482f);
            Assert.That(result, Is.EqualTo(expectedResult).Using(PlaneComparer.Epsilon));

            Assert.That(plane, Is.EqualTo(originalPlane));
            Assert.That(quaternion, Is.EqualTo(originalQuaternion));
        }
    }
}