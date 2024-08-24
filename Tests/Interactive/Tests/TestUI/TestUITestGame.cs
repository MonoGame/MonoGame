// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace MonoGame.InteractiveTests.TestUI
{
    /// <summary>
    /// A meta test that tests <see cref="Universe"/> UI components such as <see cref="Button"/>,
    /// <see cref="Label"/> etc.
    /// </summary>
    [InteractiveTest("Test UI Test", Categories.Meta)]
    public class TestUITestGame : TestGame
    {
        protected override void InitializeGui()
        {
            base.InitializeGui();

            var label = new Label
            {
                BackgroundColor = Color.Indigo,
                Font = _font,
                Location = new Point(20, 200),
                Text = "fantastic",
                TextColor = Color.White
            };

            label.SizeToFit();

            var button1 = new Button
            {
                BackgroundColor = Color.OrangeRed,
                Content = new Label
                {
                    Font = _font,
                    Text = "Button 1",
                    TextColor = Color.White
                },
                Location = new Point(label.Frame.Left, 60)
            };

            button1.Content.SizeToFit();
            button1.SizeToFit();

            button1.Tapped += (sender, e) => { label.Text = ("button 1!"); };

            var button2 = new Button
            {
                BackgroundColor = Color.Goldenrod,
                Content = new Label
                {
                    Font = _font,
                    Text = "Button 2",
                    TextColor = Color.White
                },
                Location = new Point(button1.Frame.Left, button1.Frame.Bottom)
            };

            button2.Content.SizeToFit();
            button2.SizeToFit();

            button2.Tapped += (sender, e) => { label.Text = ("button 2!"); };

            _universe.Add(button1);
            _universe.Add(button2);
            _universe.Add(label);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.DarkBlue);
            base.Draw(gameTime);
        }
    }
}
