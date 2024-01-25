// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    internal class OcclusionQueryTest : GraphicsDeviceTestFixtureBase
    {

        [Test]
        public void ConstructorsAndProperties()
        {
            Assert.Throws<ArgumentNullException>(() => new OcclusionQuery(null));

            var occlusionQuery = new OcclusionQuery(gd);

            Assert.IsFalse(occlusionQuery.IsComplete);

            Assert.Throws<InvalidOperationException>(
                () => { var n = occlusionQuery.PixelCount; },
                "PixelCount throws when query not yet started.");

            occlusionQuery.Dispose();
        }

        [Test]
        public void MismatchedBeginEnd()
        {
            var occlusionQuery = new OcclusionQuery(gd);

            Assert.Throws<InvalidOperationException>(() => occlusionQuery.End());

            occlusionQuery.Begin();
            Assert.Throws<InvalidOperationException>(() => occlusionQuery.Begin());

            occlusionQuery.Dispose();
        }

        [Test]
        public void QueryOccludedSprites()
        {
            var spriteBatch = new SpriteBatch(gd);
            var whiteTexture = content.Load<Texture2D>(Paths.Texture("white-64"));

            var occlusionQuery = new OcclusionQuery(gd);

            var state = 0;
            var queryFrameCount = 0;

            Action<int> action = frameNr =>
            {
                gd.Clear(Color.CornflowerBlue);

                // White rectangle at depth 0.
                spriteBatch.Begin(SpriteSortMode.Immediate, null, null, DepthStencilState.Default, null);
                spriteBatch.Draw(whiteTexture, new Rectangle(100, 100, 100, 100), null, Color.White, 0, Vector2.Zero,
                    SpriteEffects.None, 0);
                spriteBatch.End();

                switch (state)
                {
                    case 0:
                        // Make query with red rectangle, 50% occluded.
                        occlusionQuery.Begin();
                        spriteBatch.Begin(SpriteSortMode.Immediate, null, null, DepthStencilState.Default, null);
                        spriteBatch.Draw(whiteTexture, new Rectangle(50, 100, 100, 100), null, Color.Red, 0, Vector2.Zero,
                            SpriteEffects.None, 1);
                        spriteBatch.End();
                        occlusionQuery.End();
                        state = 1;
                        queryFrameCount = 0;
                        break;
                    case 1:
                        queryFrameCount++;

                        if (queryFrameCount > 5)
                            Assert.Fail("Occlusion query did not complete.");

                        if (occlusionQuery.IsComplete)
                        {
                            Assert.AreEqual(100*100/2, occlusionQuery.PixelCount);
                            Console.WriteLine("First occlusionQuery completed in {0} frames", queryFrameCount);
                            state = 2;
                        }
                        break;
                    case 2:
                        // Same results as last frame.
                        Assert.IsTrue(occlusionQuery.IsComplete);
                        Assert.AreEqual(100*100/2, occlusionQuery.PixelCount);

                        // Reuse query a second time, 10% occlusion.
                        occlusionQuery.Begin();
                        spriteBatch.Begin(SpriteSortMode.Immediate, null, null, DepthStencilState.Default, null);
                        spriteBatch.Draw(whiteTexture, new Rectangle(10, 100, 100, 100), null, Color.Red, 0, Vector2.Zero,
                            SpriteEffects.None, 1);
                        spriteBatch.End();
                        occlusionQuery.End();
                        state = 3;
                        queryFrameCount = 0;
                        break;
                    case 3:
                        queryFrameCount++;

                        if (queryFrameCount > 5)
                            Assert.Fail("Occlusion query did not complete.");

                        if (occlusionQuery.IsComplete)
                        {
                            Assert.AreEqual(100*100*9/10, occlusionQuery.PixelCount);
                            Console.WriteLine("Second occlusionQuery completed in {0} frames", queryFrameCount);
                            state = 4;
                        }
                        break;
                }
            };

            Predicate<int> exitCondition = frame => state == 4 || frame > 15;
            
            DoGameLoop(action, exitCondition);

            spriteBatch.Dispose();
            whiteTexture.Dispose();
            occlusionQuery.Dispose();
        }
    }
}
