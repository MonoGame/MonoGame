// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics.Effect
{
    internal class BasicEffectTest : GraphicsDeviceTestFixtureBase
    {
        private BasicEffect _effect;
        private VertexBuffer _texCubeVB;
        private VertexBuffer _colorCubeVB;
        private VertexBuffer _texColorCubeVB;

        private VertexBuffer _normalTexCubeVB;
        private VertexBuffer _normalColorCubeVB;
        private VertexBuffer _normalTexColorCubeVB;

        private IndexBuffer _ib;
        private Texture2D _whiteLinesTex;

        private float _cubeRotX, _cubeRotY;

        public void LoadContent(GraphicsDevice device, ContentManager content)
        {
            _effect = new BasicEffect(device);
            _colorCubeVB = BoxHelpers.GetColorUnitCube(device);
            _texCubeVB = BoxHelpers.GetTextureUnitCube(device);
            _texColorCubeVB = BoxHelpers.GetColorTextureUnitCube(device);

            _normalColorCubeVB = BoxHelpers.GetNormalColorUnitCube(device);
            _normalTexCubeVB = BoxHelpers.GetNormalTextureUnitCube(device);
            _normalTexColorCubeVB = BoxHelpers.GetNormalColorTextureUnitCube(device);

            _ib = BoxHelpers.GetCubeIndexBuffer(device);

            _whiteLinesTex = content.Load<Texture2D>(Paths.Texture("grid-512"));

            _cubeRotX = _cubeRotY = 0.0f;
        }

        public void UnloadContent()
        {
            _texCubeVB.Dispose();
            _texCubeVB = null;
            _colorCubeVB.Dispose();
            _colorCubeVB = null;
            _texColorCubeVB.Dispose();
            _texColorCubeVB = null;
            _ib.Dispose();
            _ib = null;
            _effect.Dispose();
            _effect = null;
        }

        [Test]
        public void DrawTest()
        {
            LoadContent(gd, content);
            _cubeRotX = 40;
            _cubeRotY = 25;

            // we're fitting a lot in one test so it's alright to go full hd here
            PrepareFrameCapture(width: 1920, height: 1080);

            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

            // Setup some state.
            gd.BlendState = BlendState.Opaque;
            gd.RasterizerState = RasterizerState.CullCounterClockwise;
            gd.DepthStencilState = DepthStencilState.Default;
            gd.Indices = _ib;

            // Set some common basic effect settings.
            _effect.FogStart = 10.75f;
            _effect.FogEnd = 11.5f;
            _effect.FogColor = Color.Black.ToVector3();
            _effect.Texture = _whiteLinesTex;
            _effect.DiffuseColor = Color.White.ToVector3();
            _effect.EnableDefaultLighting();

            // Setup the camera.
            _effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, gd.Viewport.AspectRatio,
                0.1f, 300);
            _effect.View = Matrix.CreateRotationY(MathHelper.ToRadians(0))*
                           Matrix.CreateRotationX(MathHelper.ToRadians(0))*
                           Matrix.CreateTranslation(0, 0, -11);

            var world = Matrix.CreateRotationX(MathHelper.ToRadians(_cubeRotX))*
                        Matrix.CreateRotationY(MathHelper.ToRadians(_cubeRotY));

            // Draw BasicEffect.
            gd.SetVertexBuffer(_colorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-7, 3, 0);
            _effect.VertexColorEnabled = _effect.TextureEnabled = _effect.LightingEnabled = false;
            _effect.FogEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_NoFog.
            gd.SetVertexBuffer(_colorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-5, 3, 0);
            _effect.FogEnabled = _effect.VertexColorEnabled = _effect.TextureEnabled = _effect.LightingEnabled = false;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_VertexColor.
            gd.SetVertexBuffer(_colorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-3, 3, 0);
            _effect.TextureEnabled = _effect.LightingEnabled = false;
            _effect.FogEnabled = _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_VertexColor_NoFog.
            gd.SetVertexBuffer(_colorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-1, 3, 0);
            _effect.FogEnabled = _effect.TextureEnabled = _effect.LightingEnabled = false;
            _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_Texture.
            gd.SetVertexBuffer(_texCubeVB);
            _effect.World = world*Matrix.CreateTranslation(1, 3, 0);
            _effect.VertexColorEnabled = _effect.LightingEnabled = false;
            _effect.FogEnabled = _effect.TextureEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_Texture_NoFog.
            gd.SetVertexBuffer(_texCubeVB);
            _effect.World = world*Matrix.CreateTranslation(3, 3, 0);
            _effect.FogEnabled = _effect.VertexColorEnabled = _effect.LightingEnabled = false;
            _effect.TextureEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_Texture_VertexColor.
            gd.SetVertexBuffer(_texColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(5, 3, 0);
            _effect.LightingEnabled = false;
            _effect.TextureEnabled = _effect.FogEnabled = _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_Texture_VertexColor_NoFog.
            gd.SetVertexBuffer(_texColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(7, 3, 0);
            _effect.FogEnabled = _effect.LightingEnabled = false;
            _effect.TextureEnabled = _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);


            // One vertex lighting row.
            _effect.LightingEnabled = true;
            _effect.PreferPerPixelLighting = false;
            _effect.DirectionalLight0.Enabled = true;
            _effect.DirectionalLight1.Enabled = true;
            _effect.DirectionalLight2.Enabled = true;

            // BasicEffect_VertexLighting
            gd.SetVertexBuffer(_normalColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-7, 1, 0);
            _effect.FogEnabled = true;
            _effect.VertexColorEnabled = _effect.TextureEnabled = false;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_VertexLighting_NoFog.
            gd.SetVertexBuffer(_normalColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-5, 1, 0);
            _effect.FogEnabled = _effect.VertexColorEnabled = _effect.TextureEnabled = false;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_VertexLighting_VertexColor.
            gd.SetVertexBuffer(_normalColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-3, 1, 0);
            _effect.FogEnabled = _effect.VertexColorEnabled = true;
            _effect.TextureEnabled = false;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_VertexLighting_VertexColor_NoFog.
            gd.SetVertexBuffer(_normalColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-1, 1, 0);
            _effect.FogEnabled = _effect.TextureEnabled = false;
            _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_VertexLighting_Texture.
            gd.SetVertexBuffer(_normalTexCubeVB);
            _effect.World = world*Matrix.CreateTranslation(1, 1, 0);
            _effect.VertexColorEnabled = false;
            _effect.FogEnabled = _effect.TextureEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_VertexLighting_Texture_NoFog.
            gd.SetVertexBuffer(_normalTexCubeVB);
            _effect.World = world*Matrix.CreateTranslation(3, 1, 0);
            _effect.FogEnabled = _effect.VertexColorEnabled = false;
            _effect.TextureEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_VertexLighting_Texture_VertexColor.
            gd.SetVertexBuffer(_normalTexColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(5, 1, 0);
            _effect.TextureEnabled = _effect.FogEnabled = _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_VertexLighting_Texture_VertexColor_NoFog.
            gd.SetVertexBuffer(_normalTexColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(7, 1, 0);
            _effect.FogEnabled = false;
            _effect.TextureEnabled = _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);


            // One light row.
            _effect.LightingEnabled = true;
            _effect.PreferPerPixelLighting = false;
            _effect.DirectionalLight0.Enabled = true;
            _effect.DirectionalLight1.Enabled = false;
            _effect.DirectionalLight2.Enabled = false;

            // BasicEffect_OneLight
            gd.SetVertexBuffer(_normalColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-7, -1, 0);
            _effect.FogEnabled = true;
            _effect.VertexColorEnabled = _effect.TextureEnabled = false;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_OneLight_NoFog.
            gd.SetVertexBuffer(_normalColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-5, -1, 0);
            _effect.FogEnabled = _effect.VertexColorEnabled = _effect.TextureEnabled = false;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_OneLight_VertexColor.
            gd.SetVertexBuffer(_normalColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-3, -1, 0);
            _effect.FogEnabled = _effect.VertexColorEnabled = true;
            _effect.TextureEnabled = false;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_OneLight_VertexColor_NoFog.
            gd.SetVertexBuffer(_normalColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-1, -1, 0);
            _effect.FogEnabled = _effect.TextureEnabled = false;
            _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_OneLight_Texture.
            gd.SetVertexBuffer(_normalTexCubeVB);
            _effect.World = world*Matrix.CreateTranslation(1, -1, 0);
            _effect.VertexColorEnabled = false;
            _effect.FogEnabled = _effect.TextureEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_OneLight_Texture_NoFog.
            gd.SetVertexBuffer(_normalTexCubeVB);
            _effect.World = world*Matrix.CreateTranslation(3, -1, 0);
            _effect.FogEnabled = _effect.VertexColorEnabled = false;
            _effect.TextureEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_OneLight_Texture_VertexColor.
            gd.SetVertexBuffer(_normalTexColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(5, -1, 0);
            _effect.TextureEnabled = _effect.FogEnabled = _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_OneLight_Texture_VertexColor_NoFog.
            gd.SetVertexBuffer(_normalTexColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(7, -1, 0);
            _effect.FogEnabled = false;
            _effect.TextureEnabled = _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Pixel lighting row.
            _effect.LightingEnabled = true;
            _effect.PreferPerPixelLighting = true;
            _effect.DirectionalLight0.Enabled = true;
            _effect.DirectionalLight1.Enabled = true;
            _effect.DirectionalLight2.Enabled = true;

            // BasicEffect_PixelLighting
            gd.SetVertexBuffer(_normalColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-7, -3, 0);
            _effect.FogEnabled = true;
            _effect.VertexColorEnabled = _effect.TextureEnabled = false;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_PixelLighting_NoFog.
            gd.SetVertexBuffer(_normalColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-5, -3, 0);
            _effect.FogEnabled = _effect.VertexColorEnabled = _effect.TextureEnabled = false;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_PixelLighting_VertexColor.
            gd.SetVertexBuffer(_normalColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-3, -3, 0);
            _effect.FogEnabled = _effect.VertexColorEnabled = true;
            _effect.TextureEnabled = false;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_PixelLighting_VertexColor_NoFog.
            gd.SetVertexBuffer(_normalColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(-1, -3, 0);
            _effect.FogEnabled = _effect.TextureEnabled = false;
            _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_PixelLighting_Texture.
            gd.SetVertexBuffer(_normalTexCubeVB);
            _effect.World = world*Matrix.CreateTranslation(1, -3, 0);
            _effect.VertexColorEnabled = false;
            _effect.FogEnabled = _effect.TextureEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_PixelLighting_Texture_NoFog.
            gd.SetVertexBuffer(_normalTexCubeVB);
            _effect.World = world*Matrix.CreateTranslation(3, -3, 0);
            _effect.FogEnabled = _effect.VertexColorEnabled = false;
            _effect.TextureEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_PixelLighting_Texture_VertexColor.
            gd.SetVertexBuffer(_normalTexColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(5, -3, 0);
            _effect.TextureEnabled = _effect.FogEnabled = _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            // Draw BasicEffect_PixelLighting_Texture_VertexColor_NoFog.
            gd.SetVertexBuffer(_normalTexColorCubeVB);
            _effect.World = world*Matrix.CreateTranslation(7, -3, 0);
            _effect.FogEnabled = false;
            _effect.TextureEnabled = _effect.VertexColorEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);

            CheckFrames();

            UnloadContent();
        }
    }
}