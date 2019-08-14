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
		#region Internal Variables: Theorafile

		internal IntPtr theora;
		internal bool needsDurationHack;

		#endregion

		#region Internal Constructors

		partial void PlatformInitialize()
		{
			// MG Note: we double check that the metadata in the XNB matches
			//          what Theorafile reads from the actual video file

			Theorafile.tf_fopen(FileName, out theora);
			int width, height;
			double fps;
			Theorafile.tf_videoinfo(theora, out width, out height, out fps);

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
			if (Math.Abs(fps - FramesPerSecond) >= 1.0f)
			{
				throw new InvalidOperationException(
					"XNB/OGV framesPerSecond mismatch!" +
					" FPS: " + FramesPerSecond.ToString()
				);
			}

			needsDurationHack = false;
		}

		#endregion

		#region Destructor

		private void PlatformDispose(bool disposing)
		{
			if (theora != IntPtr.Zero)
			{
				Theorafile.tf_close(ref theora);
			}
		}

		#endregion
	}
}
