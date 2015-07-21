using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    /// <summary>
    /// Base class for test classes with floating point operations.
    /// </summary>
    class FPointBase
    {
        protected void Compare(float expected, float source)
        {
            Assert.That(expected, Is.EqualTo(source).Using(FloatComparer.Epsilon));
        }
    }
}