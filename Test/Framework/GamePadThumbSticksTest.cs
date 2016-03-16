using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    class GamePadThumbSticksTest
    {
        [TestCaseSource("PublicConstructorClampsValuesTestCases")]
        public void PublicConstructorClampsValues(Vector2 left, Vector2 right, Vector2 expectedLeft, Vector2 expectedRight)
        {
            var gamePadThumbSticks = new GamePadThumbSticks(left, right);
            Assert.That(gamePadThumbSticks.Left, Is.EqualTo(expectedLeft).Using(Vector2Comparer.Epsilon));
            Assert.That(gamePadThumbSticks.Right, Is.EqualTo(expectedRight).Using(Vector2Comparer.Epsilon));
        }

        private static IEnumerable<TestCaseData> PublicConstructorClampsValuesTestCases
        {
            get
            {
                yield return new TestCaseData(
                    Vector2.Zero, Vector2.Zero,
                    Vector2.Zero, Vector2.Zero);

                yield return new TestCaseData(
                    Vector2.One, Vector2.One,
                    Vector2.One, Vector2.One);

                yield return new TestCaseData(
                    -Vector2.One, -Vector2.One,
                    -Vector2.One, -Vector2.One);

                yield return new TestCaseData(
                    new Vector2(-2.7f, 5.6f), new Vector2(3.5f, -4.8f),
                    new Vector2(-1f, 1f), new Vector2(1f, -1f));

                yield return new TestCaseData(
                    new Vector2(34f, 65f), new Vector2(-33f, -17f),
                    new Vector2(1f, 1f), new Vector2(-1f, -1f));
            }
        }
    }
}