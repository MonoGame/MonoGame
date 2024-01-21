// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

using MonoGame.InteractiveTests.TestUI;
using System.Drawing;

namespace MonoGame.InteractiveTests {
	[InteractiveTest("Guide", Categories.GamerServices)]
	public class GuideTestGame : Game {
		public GuideTestGame ()
		{
			var graphics = new GraphicsDeviceManager(this);
			graphics.SupportedOrientations =
				DisplayOrientation.Portrait |
				DisplayOrientation.LandscapeLeft |
				DisplayOrientation.LandscapeRight;

			Components.Add (new GamerServicesComponent (this));

			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			base.Initialize();
			TouchPanel.EnabledGestures = GestureType.DoubleTap | GestureType.Tap;
		}

		private SpriteFont _font;
		protected override void LoadContent()
		{
			base.LoadContent();

			_font = Content.Load<SpriteFont> (@"Fonts\Default");
			InitializeGui();
		}

		private Universe _universe;
		private Label _labelEndShowKeyboardInput;
		private Label _labelShowKeyboardInputCallback;
		private void InitializeGui()
		{
			_universe = new Universe (Content)
			{
				AutoHandleInput = true
			};
			Components.Add (new UniverseComponent (this, _universe));

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

			_labelEndShowKeyboardInput = new Label
			{
				Frame = new RectangleF (20, 60, 320, 20),
				Font = _font,
				TextColor = Color.White
			};

			_labelShowKeyboardInputCallback = new Label
			{
				Frame = new RectangleF (
					_labelEndShowKeyboardInput.Frame.Left,
					_labelEndShowKeyboardInput.Frame.Bottom + 10,
					_labelEndShowKeyboardInput.Frame.Width,
					_labelEndShowKeyboardInput.Frame.Height),
				Font = _font,
				TextColor = Color.White
			};

			var buttonShowKeyboardInput = new Button
			{
				BackgroundColor = Color.Lavender,
				Content = new Label
				{
					Font = _font,
					Text = "Show Keyboard Input",
					TextColor = Color.Black
				},
				Location = new PointF(20, 200)
			};

			buttonShowKeyboardInput.Content.SizeToFit ();
			buttonShowKeyboardInput.SizeToFit ();
			buttonShowKeyboardInput.Tapped += (sender, e) => {
				TestShowKeyboardInput (
					"Some normal title",
					"And a perfectly ordinary description",
					"the default");
			};

			var buttonShowKeyboardInputLong = new Button
			{
				BackgroundColor = Color.Lavender,
				Content = new Label
				{
					Font = _font,
					Text = "Show Keyboard Input (long)",
					TextColor = Color.Black
				},
				Location = new PointF(
					buttonShowKeyboardInput.Frame.Left,
					buttonShowKeyboardInput.Frame.Bottom + 10)
			};

			buttonShowKeyboardInputLong.Content.SizeToFit ();
			buttonShowKeyboardInputLong.SizeToFit ();
			buttonShowKeyboardInputLong.Tapped += (sender, e) => {
				TestShowKeyboardInput (
					"This is the title that never ends, yes it goes on and on my friends.  One " +
					"day some people started writing it, etc",
					"And here is a super-duper description that rambles on a bit about, you " +
					"know, whatever.  And then finally ends over here at about this point.",
					"surprisingly terse default");
			};

			_universe.Add (exitButton);
			_universe.Add (_labelEndShowKeyboardInput);
			_universe.Add (_labelShowKeyboardInputCallback);
			_universe.Add (buttonShowKeyboardInput);
			_universe.Add (buttonShowKeyboardInputLong);
		}

		private void TestShowKeyboardInput (string title, string description, string defaultText)
		{
			var result = Guide.BeginShowKeyboardInput(
				PlayerIndex.One,
				title,
				description,
				defaultText,
				Guide_ShowKeyboardInputCallback, null);

			_labelEndShowKeyboardInput.Text =
				"EndShow: " + (Guide.EndShowKeyboardInput (result) ?? "<null>");
		}

		private void Guide_ShowKeyboardInputCallback(IAsyncResult result)
		{
			_labelShowKeyboardInputCallback.Text =
				"CallBack: " + (Guide.EndShowKeyboardInput (result) ?? "<null>");
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear (Color.Indigo);
			base.Draw(gameTime);
		}
	}
}

