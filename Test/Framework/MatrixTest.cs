using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    /// <summary>
    /// Contains tests for Matrix struct.
    /// </summary>
    class MatrixTest
    {
        [Test]
        public void Constructors()
        {
            var expected = new Matrix
            {
                M11 = 1,
                M12 = 2,
                M13 = 3,
                M14 = 4,
                M21 = 5,
                M22 = 6,
                M23 = 7,
                M24 = 8,
                M31 = 9,
                M32 = 10,
                M33 = 11,
                M34 = 12,
                M41 = 13,
                M42 = 14,
                M43 = 15,
                M44 = 16
            };

            // Constructor 1

            Assert.AreEqual(expected, new Matrix(1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16));

#if !XNA

            // Constructor 2

            Assert.AreEqual(expected, new Matrix
            (
                new Vector4(1, 2, 3, 4),
                new Vector4(5, 6, 7, 8),
                new Vector4(9, 10, 11, 12),
                new Vector4(13, 14, 15, 16))
            );
#endif
        }

        [Test]
        public void Add()
        {
            var test = new Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
            var expected = new Matrix(2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32);

            // test Add method

            var result = Matrix.Add(test, test);
            Assert.AreEqual(expected, result);

            // test operator +

            Assert.AreEqual(expected, test + test);
        }
    }
}