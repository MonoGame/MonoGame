#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2019 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed partial class Video
	{
		#region Public Properties

		public int Width
		{
			get;
			private set;
		}

		public int Height
		{
			get;
			private set;
		}

		public float FramesPerSecond
		{
			get;
			internal set;
		}

		public VideoSoundtrackType VideoSoundtrackType
		{
			get;
			private set;
		}

		// FIXME: This is hacked, look up "This is a part of the Duration hack!"
		public TimeSpan Duration
		{
			get;
			internal set;
		}

		#endregion

		#region Internal Properties

		internal GraphicsDevice GraphicsDevice
		{
			get;
			private set;
		}

		#endregion

		#region Internal Variables: Theorafile

		internal IntPtr theora;
		internal bool needsDurationHack;

		#endregion

		#region Internal Constructors

		internal Video(string fileName, GraphicsDevice device)
		{
			GraphicsDevice = device;

			Theorafile.tf_fopen(fileName, out theora);
			int width, height;
			double fps;
			Theorafile.tf_videoinfo(theora, out width, out height, out fps);
			Width = width;
			Height = height;
			FramesPerSecond = (float) fps;

			// FIXME: This is a part of the Duration hack!
			Duration = TimeSpan.MaxValue;
			needsDurationHack = true;
		}

		internal Video(
			string fileName,
			GraphicsDevice device,
			int durationMS,
			int width,
			int height,
			float framesPerSecond,
			VideoSoundtrackType soundtrackType
		) : this(fileName, device) {
			/* If you got here, you've still got the XNB file! Well done!
			 * Except if you're running FNA, you're not using the WMV anymore.
			 * But surely it's the same video, right...?
			 * Well, consider this a check more than anything. If this bothers
			 * you, just remove the XNB file and we'll read the OGV straight up.
			 * -flibit
			 */
			if (width != Width || height != Height)
			{
				throw new InvalidOperationException(
					"XNB/OGV width/height mismatch!" +
					" Width: " + Width.ToString() +
					" Height: " + Height.ToString()
				);
			}
			if (Math.Abs(FramesPerSecond - framesPerSecond) >= 1.0f)
			{
				throw new InvalidOperationException(
					"XNB/OGV framesPerSecond mismatch!" +
					" FPS: " + FramesPerSecond.ToString()
				);
			}

			// FIXME: Oh, hey! I wish we had this info in Theora!
			Duration = TimeSpan.FromMilliseconds(durationMS);
			needsDurationHack = false;

			VideoSoundtrackType = soundtrackType;
		}

		#endregion

		#region Destructor

		~Video()
		{
			if (theora != IntPtr.Zero)
			{
				Theorafile.tf_close(ref theora);
			}
		}

		#endregion
	}
}
