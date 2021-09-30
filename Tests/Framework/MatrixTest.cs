using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class MatrixTest
    {
        [Test]
        public void Add()
        {
            Matrix test = new Matrix(1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16);
            Matrix expected = new Matrix(2,4,6,8,10,12,14,16,18,20,22,24,26,28,30,32);
            Matrix result;
            Matrix.Add(ref test,ref test, out result);
            Assert.AreEqual(expected, result);
            Assert.AreEqual(expected, Matrix.Add(test,test));
            Assert.AreEqual(expected, test+test);
        }

        /// <summary>
        /// Test method for ToString in Matrix class
        /// </summary>
        [Test]
        public void TestToString()
        {
            Matrix test = new Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
            string expected = "{M11:2 M12:4 M13:6 M14:8} {M21:10 M22:12 M23:14 M24:16} {M31:18 M32:20 M33:22 M34:24} {M41:26 M42:28 M43:30 M44:32}";
            Matrix result = Matrix.Add(test, test);
            Assert.AreEqual(expected, result.ToString());

            Matrix mat = Matrix.CreateFromAxisAngle(new Vector3(1,1,1), 0.1f);
            string expected2 = "{M11:1 M12:0.104829244 M13:-0.0948376 M14:0} {M21:-0.0948376 M22:1 M23:0.104829244 M24:0} {M31:0.104829244 M32:-0.0948376 M33:1 M34:0} {M41:0 M42:0 M43:0 M44:1}";
            Assert.AreEqual(expected2,mat.ToString());

            Matrix div = Matrix.Divide(test, 0.5f);
            string expected3 = "{M11:2 M12:4 M13:6 M14:8} {M21:10 M22:12 M23:14 M24:16} {M31:18 M32:20 M33:22 M34:24} {M41:26 M42:28 M43:30 M44:32}";
            Assert.AreEqual(expected3, div.ToString());

            Matrix secondTest = new Matrix();
            string expected4 = "{M11:0 M12:0 M13:0 M14:0} {M21:0 M22:0 M23:0 M24:0} {M31:0 M32:0 M33:0 M34:0} {M41:0 M42:0 M43:0 M44:0}";
            Assert.AreEqual(expected4, secondTest.ToString());
        }
    }
}
