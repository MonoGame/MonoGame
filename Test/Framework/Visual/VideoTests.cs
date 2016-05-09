// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using NUnit.Framework;
using System;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    class VideoTests : VisualTestFixtureBase
    {
        SpriteBatch _spriteBatch;
        Video _video;
        VideoPlayer _player;
        TimeSpan _start;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Game.LoadContentWith += (sender, e) =>
            {
                _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
                _video = Game.Content.Load<Video>(Paths.Video("SampleVideo_360x240_5s"));
                _player.Play(_video);
                _start = e.FrameInfo.TotalGameTime;
            };

            Game.UnloadContentWith += (sender, e) =>
            {
                _spriteBatch.Dispose();
                _spriteBatch = null;
                _player.Dispose();
                _player = null;
                _video = null;
            };

            Game.PreDrawWith += (sender, e) =>
            {
                Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            };
        }

        [Test]
        public void Plain()
        {
            Game.ExitCondition = x => _player.State == MediaState.Stopped;

            Game.InitializeWith += (sender, e) =>
            {
                Game.Window.Title = "Not Looped";
                _player = new VideoPlayer();
            };

            Game.UpdateWith += (sender, e) =>
            {
            };

            Game.DrawWith += (sender, e) =>
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(_player.GetTexture(), Vector2.Zero, Color.White);
                _spriteBatch.End();
            };

            Game.Run();
        }

        [Test]
        public void Looped()
        {
            Game.ExitCondition = x => (x.TotalGameTime - _start).TotalSeconds > 8.0;

            Game.InitializeWith += (sender, e) =>
            {
                Game.Window.Title = "Looped";
                _player = new VideoPlayer();
                _player.IsLooped = true;
            };

            Game.UpdateWith += (sender, e) =>
            {
                Assert.AreNotEqual(_player.State, MediaState.Stopped);
            };

            Game.DrawWith += (sender, e) =>
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(_player.GetTexture(), Vector2.Zero, Color.White);
                _spriteBatch.End();
            };

            Game.Run();
        }
    }
}
