// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.Tests.Graphics
{
    /// <summary>
    /// Verifies that a vertex-stage texture fetch (VTF) keeps working across frame
    /// boundaries.
    /// </summary>
    [TestFixture]
    internal class VertexTextureBindingPersistenceTest : GraphicsDeviceTestFixtureBase
    {
        private const int Frames = 6;

        private Effect _effect;
        private Texture2D _texture;
        private RenderTarget2D _renderTarget;
        private SpriteBatch _spriteBatch;

        // A simple user-indexed quad
        private static readonly VertexPosition[] _quadVertices =
        {
            new VertexPosition(new Vector3(0, 0, 0)),
            new VertexPosition(new Vector3(1, 0, 0)),
            new VertexPosition(new Vector3(0, 1, 0)),
            new VertexPosition(new Vector3(1, 1, 0)),
        };

        private void SetUpVertexTextureFetch()
        {
            // A 1x1 texture with a single float value, which every lookup should return
            _texture = new Texture2D(gd, 1, 1, false, SurfaceFormat.Single);
            _texture.SetData(new[] { 1f });

            // The effect's vertex shader computes clip = mul((x, fetched, y, 1), M).
            // This matrix maps the [0,1] quad to fullscreen and adds 4*(fetched - 1)
            // to clip X: fetched == 1 covers the whole viewport, fetched == 0 puts the
            // quad entirely off the left edge.
            var matrix = new Matrix(
                2f, 0f, 0f, 0f,   // clipX += 2x
                4f, 0f, 0f, 0f,   // clipX += 4*fetched
                0f, 2f, 0f, 0f,   // clipY += 2y
                -5f, -1f, 0.5f, 1f);

            // Use the existing VertexTextureEffect with a unit height map size
            _effect = AssetTestUtility.LoadEffect(content, "VertexTextureEffect");
            _effect.Parameters["WorldViewProj"].SetValue(matrix);
            _effect.Parameters["HeightMapTexture"].SetValue(_texture);
            _effect.Parameters["HeightMapSize"].SetValue(1f);

            _renderTarget = new RenderTarget2D(gd, gd.Viewport.Width, gd.Viewport.Height);
            _spriteBatch = new SpriteBatch(gd);
        }

        [TearDown]
        public override void TearDown()
        {
            _effect?.Dispose();
            _texture?.Dispose();
            _renderTarget?.Dispose();
            _spriteBatch?.Dispose();
            base.TearDown();
        }

        private void DrawQuad()
        {
            gd.RasterizerState = RasterizerState.CullNone;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, _quadVertices, 0, 2);
        }

        private int CountDrawnPixels(Color[] pixels) => pixels.Count(p => p != ClearColor);

        private int DrawFrameToBackBuffer()
        {
            gd.Clear(ClearColor);
            DrawQuad();

            var pixels = new Color[gd.Viewport.Width * gd.Viewport.Height];
            gd.GetBackBufferData(pixels);
            return CountDrawnPixels(pixels);
        }

        private int DrawFrameToRenderTarget(bool blitToBackBufferAfter)
        {
            gd.SetRenderTarget(_renderTarget);
            gd.Clear(ClearColor);
            DrawQuad();
            gd.SetRenderTarget(null);

            // Get data from the target
            var pixels = new Color[_renderTarget.Width * _renderTarget.Height];
            _renderTarget.GetData(pixels);

            if (blitToBackBufferAfter)
            {
                // Draw the render target to the backbuffer with SpriteBatch,
                // which was poisoning the vertex texture binding in a fixed bug
                gd.Clear(Color.Black);
                _spriteBatch.Begin();
                _spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
                _spriteBatch.End();
            }

            return CountDrawnPixels(pixels);
        }

        private void AssertEveryFrameDrew(IList<int> perFrameDrawnPixels)
        {
            // Require that all the pixels in the viewport have something rendered to them because the quad should fill the viewport area
            var required = gd.Viewport.Width * gd.Viewport.Height;
            var log = string.Join(", ", perFrameDrawnPixels.Select((n, i) => $"f{i}={n}"));

            for (var i = 0; i < perFrameDrawnPixels.Count; i++)
            {
                Assert.That(perFrameDrawnPixels[i], Is.EqualTo(required),
                    $"Vertex texture fetch read 0 on frame {i} (quad landed off-screen). Per-frame drawn-pixel counts: {log}. This means the vertex-stage texture binding was not re-sent to the native device after a frame boundary wiped it."
                    );
            }
        }

        [Test]
#if DESKTOPGL
        [Ignore("Vertex Textures are not implemented for OpenGL")]
#endif
        public void VertexTextureSurvivesFrameBoundaries_BackBuffer()
        {
            SetUpVertexTextureFetch();

            var counts = new List<int>();
            for (var frame = 0; frame < Frames; frame++)
            {
                counts.Add(DrawFrameToBackBuffer());
                gd.Present();
            }

            AssertEveryFrameDrew(counts);
        }

        [Test]
#if DESKTOPGL
        [Ignore("Vertex Textures are not implemented for OpenGL")]
#endif
        public void VertexTextureSurvivesFrameBoundaries_RenderTarget()
        {
            SetUpVertexTextureFetch();

            var counts = new List<int>();
            for (var frame = 0; frame < Frames; frame++)
            {
                counts.Add(DrawFrameToRenderTarget(blitToBackBufferAfter: false));
                gd.Present();
            }

            AssertEveryFrameDrew(counts);
        }

        [Test]
#if DESKTOPGL
        [Ignore("Vertex Textures are not implemented for OpenGL")]
#endif
        public void VertexTextureSurvivesRenderTargetAndSpriteBatchHistory()
        {
            SetUpVertexTextureFetch();

            // Back buffer, then render-target frames composited with SpriteBatch, then
            // back buffer again: the sequence that triggers the bug these tests were written to detect regression of.
            var counts = new List<int>();
            for (var frame = 0; frame < Frames; frame++)
            {
                var useRenderTarget = frame == 2 || frame == 3;
                counts.Add(useRenderTarget
                    ? DrawFrameToRenderTarget(blitToBackBufferAfter: true)
                    : DrawFrameToBackBuffer());
                gd.Present();
            }

            AssertEveryFrameDrew(counts);
        }
        
        [Test]
#if DESKTOPGL
        [Ignore("Vertex Textures are not implemented for OpenGL")]
#endif
        public void VertexTextureWorksWhenRebindingEveryFrame()
        {
            SetUpVertexTextureFetch();

            var counts = new List<int>();
            for (var frame = 0; frame < Frames; frame++)
            {
                gd.VertexTextures[0] = null;
                gd.VertexSamplerStates[0] = (frame & 1) == 0
                    ? SamplerState.PointClamp
                    : SamplerState.PointWrap;
                counts.Add(DrawFrameToBackBuffer());
                gd.Present();
            }

            AssertEveryFrameDrew(counts);
        }
    }
}
