// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace MonoGame.InteractiveTests.TestUI {
	[InteractiveTest("Test UI Test", Categories.Meta)]
	public class TestUITestGame : Game {
		private SpriteFont _font;
		private Universe _universe;

		public TestUITestGame ()
		{
			new GraphicsDeviceManager (this);
			TouchPanel.EnabledGestures = GestureType.DoubleTap | GestureType.Tap;

			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			_font = Content.Load<SpriteFont> (@"Fonts\Default");

			_universe = new Universe(Content) {
				AutoHandleInput = true
			};
			Components.Add (new UniverseComponent(this, _universe));

			var exitButton = new Button
			{
				BackgroundColor = Color.Black,
				Content = new Label {
					Font = _font,
					Text = "Exit",
					TextColor = Color.White
				},
				Location = PointF.Empty
			};

			exitButton.Content.SizeToFit ();
			exitButton.SizeToFit ();
			exitButton.Tapped += (sender, e) => {
				Exit ();
			};

			var label = new Label {
				BackgroundColor = Color.Indigo,
				Font = _font,
				Location = new PointF(20, 200),
				Text = "fantastic",
				TextColor = Color.White
			};

			label.SizeToFit ();

			var button1 = new Button
			{
				BackgroundColor = Color.OrangeRed,
				Content = new Label {
					Font = _font,
					Text = "Button 1",
					TextColor = Color.White
				},
				Location = new PointF(label.Frame.Left, 60)
			};

			button1.Content.SizeToFit ();
			button1.SizeToFit ();

			button1.Tapped += (sender, e) => {
				label.Text = ("button 1!");
			};

			var button2 = new Button
			{
				BackgroundColor = Color.Goldenrod,
				Content = new Label {
					Font = _font,
					Text = "Button 2",
					TextColor = Color.White
				},
				Location = new PointF(button1.Frame.Left, button1.Frame.Bottom)
			};

			button2.Content.SizeToFit ();
			button2.SizeToFit ();

			button2.Tapped += (sender, e) => {
				label.Text = ("button 2!");
			};

			_universe.Add (exitButton);
			_universe.Add (button1);
			_universe.Add (button2);
			_universe.Add (label);
		}

		protected override void UnloadContent()
		{
			base.UnloadContent();

			_font = null;
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear (Color.DarkBlue);
			base.Draw(gameTime);
		}
	}
}

