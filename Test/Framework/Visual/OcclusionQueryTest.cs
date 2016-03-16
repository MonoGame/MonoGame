// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    internal class OcclusionQueryTest : VisualTestFixtureBase
    {
        [Test]
        public void ConstructorsAndProperties()
        {
            Game.DrawWith += (sender, e) =>
            {
                Assert.Throws<ArgumentNullException>(() => new OcclusionQuery(null));

                var occlusionQuery = new OcclusionQuery(Game.GraphicsDevice);

                Assert.IsFalse(occlusionQuery.IsComplete);

                Assert.Throws<InvalidOperationException>(
                    () => { var n = occlusionQuery.PixelCount; },
                    "PixelCount throws when query not yet started.");
            };
            Game.Run();
        }

        [Test]
        public void MismatchedBeginEnd()
        {
            Game.DrawWith += (sender, e) =>
            {
                var occlusionQuery = new OcclusionQuery(Game.GraphicsDevice);

                Assert.Throws<InvalidOperationException>(() => occlusionQuery.End());

                occlusionQuery.Begin();
                Assert.Throws<InvalidOperationException>(() => occlusionQuery.Begin());
            };
            Game.Run();
        }

        [Test]
        public void QueryOccludedSprites()
        {
            SpriteBatch spriteBatch = null;
            Texture2D whiteTexture = null;
            OcclusionQuery occlusionQuery = null;

            int state = 0;
            int queryFrameCount = 0;

            Game.LoadContentWith += (sender, e) =>
            {
                spriteBatch = new SpriteBatch(Game.GraphicsDevice);
                whiteTexture = Game.Content.Load<Texture2D>(Paths.Texture("white-64"));
            };

            Game.DrawWith += (sender, e) =>
            {
                if (occlusionQuery == null)
                    occlusionQuery = new OcclusionQuery(Game.GraphicsDevice);

                Game.GraphicsDevice.Clear(Color.CornflowerBlue);

                // White rectangle at depth 0.
                spriteBatch.Begin(SpriteSortMode.Immediate, null, null, DepthStencilState.Default, null);
                spriteBatch.Draw(whiteTexture, new Rectangle(100, 100, 100, 100), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                spriteBatch.End();

                if (state == 0)
                {
                    // Make query with red rectangle, 50% occluded.
                    occlusionQuery.Begin();
                    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, DepthStencilState.Default, null);
                    spriteBatch.Draw(whiteTexture, new Rectangle(50, 100, 100, 100), null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 1);
                    spriteBatch.End();
                    occlusionQuery.End();
                    state = 1;
                    queryFrameCount = 0;
                }
                else if (state == 1)
                {
                    queryFrameCount++;

                    if (queryFrameCount > 5)
                        Assert.Fail("Occlusion query did not complete.");

                    if (occlusionQuery.IsComplete)
                    {
                        Assert.AreEqual(100 * 100 / 2, occlusionQuery.PixelCount);
                        state = 2;
                    }
                }
                else if (state == 2)
                {
                    // Same results as last frame.
                    Assert.IsTrue(occlusionQuery.IsComplete);
                    Assert.AreEqual(100 * 100 / 2, occlusionQuery.PixelCount);

                    // Reuse query a second time, 10% occlusion.
                    occlusionQuery.Begin();
                    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, DepthStencilState.Default, null);
                    spriteBatch.Draw(whiteTexture, new Rectangle(10, 100, 100, 100), null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 1);
                    spriteBatch.End();
                    occlusionQuery.End();
                    state = 3;
                    queryFrameCount = 0;
                }
                else if (state == 3)
                {
                    queryFrameCount++;

                    if (queryFrameCount > 5)
                        Assert.Fail("Occlusion query did not complete.");

                    if (occlusionQuery.IsComplete)
                    {
                        Assert.AreEqual(100 * 100 * 9 / 10, occlusionQuery.PixelCount);
                        state = 4;
                    }
                }
            };

            Game.Run(until: frameInfo => state == 4 || frameInfo.DrawNumber > 15);
        }
    }
}
