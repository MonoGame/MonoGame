// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MonoGame.Tests {
	struct FrameInfo {
		public int UpdateNumber;
		public int DrawNumber;
		public TimeSpan ElapsedGameTime;
		public TimeSpan TotalGameTime;
		public bool IsRunningSlowly;
	    public GameTime GameTime;

		public void AdvanceUpdate (GameTime gameTime)
		{
			UpdateNumber++;
			UpdateGameTime (gameTime);
		}

		public void AdvanceDraw (GameTime gameTime)
		{
			DrawNumber++;
			UpdateGameTime (gameTime);
		}

		public void Reset ()
		{
			UpdateNumber = 0;
			DrawNumber = 0;
			ElapsedGameTime = TimeSpan.Zero;
			TotalGameTime = TimeSpan.Zero;
			IsRunningSlowly = false;
		}

		public void UpdateGameTime (GameTime gameTime)
		{
		    GameTime = gameTime;
			ElapsedGameTime = gameTime.ElapsedGameTime;
			TotalGameTime = gameTime.TotalGameTime;
			IsRunningSlowly = gameTime.IsRunningSlowly;
		}
	}

	interface IFrameInfoSource {
		FrameInfo FrameInfo { get; }
	}

	class FrameInfoEventArgs : EventArgs {
		public FrameInfoEventArgs (FrameInfo frameInfo)
		{
			_frameInfo = frameInfo;
		}

		private readonly FrameInfo _frameInfo;
		public FrameInfo FrameInfo { get { return _frameInfo; } }
	}
}
