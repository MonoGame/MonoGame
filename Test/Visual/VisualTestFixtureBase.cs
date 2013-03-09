#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using NUnit.Framework;

using MonoGame.Tests.Components;

namespace MonoGame.Tests.Visual {
	class VisualTestFixtureBase {
		private VisualTestGame _game;
		protected VisualTestGame Game { get { return _game; } }

		[SetUp]
		public virtual void SetUp ()
		{
			Paths.SetStandardWorkingDirectory();
			_game = new VisualTestGame ();
			_game.ExitCondition = x => x.DrawNumber > 1;
		}

		[TearDown]
		public virtual void TearDown ()
		{
			_game.Dispose ();
			_game = null;
		}

		/// <summary>
		/// Provides a quick and easy way to run a single frame visual
		/// test with the default comparison and diff-writing options.
		/// Tests that need more control over the run-diff-assert
		/// process should manually call:
		/// 
		/// <code>
		/// Game.Components.Add ($FrameCompareComponent$);
		/// Game.Run ();
		/// WriteFrameDiffs ();
		/// AssertFrameComparisonPassed ();
		/// </code>
		/// </summary>
		/// <param name="similarity">The similarity to the reference
		/// image required for a frame to be considered passing.</param>
		/// <param name="writeDiffs">A value indicating whether visual
		/// diffs should be written for this test.</param>
		protected void RunSingleFrameTest (
			float similarity = Constants.StandardRequiredSimilarity,
			bool writeDiffs = true)
		{
			RunMultiFrameTest (1, 1, similarity, writeDiffs);
		}

		/// <summary>
		/// Provides a quick and easy way to run a multi-frame visual
		/// test using the default comparison and diff-writing options.
		/// Tests that need more control over the run-diff-assert
		/// process should manually call:
		/// 
		/// <code>
		/// Game.Components.Add ($FrameCompareComponent$);
		/// Game.Run ();
		/// WriteFrameDiffs ();
		/// AssertFrameComparisonPassed ();
		/// </code>
		/// </summary>
		/// <param name="captureCount">The total number of frames to
		/// capture.</param>
		/// <param name="captureStride">How often to capture.  A value
		/// of 1 captures every frame, 2 captures every other, etc.
		/// </param>
		/// <param name="similarity">The similarity to the reference
		/// image required for a frame to be considered passing.</param>
		/// <param name="writeDiffs">A value indicating whether visual
		/// diffs should be written for this test.</param>
		protected void RunMultiFrameTest (
			int captureCount,
			int captureStride = 1,
			float similarity = Constants.StandardRequiredSimilarity,
			bool writeDiffs = true)
		{
			if (captureCount < 1)
				throw new ArgumentOutOfRangeException (
					"captureCount", "captureCount must be positive");
			if (captureStride < 1)
				throw new ArgumentOutOfRangeException (
					"captureStride", "captureStride must be positive");

			if (!Game.Components.Any (x => x is FrameCompareComponent))
				Game.Components.Add (FrameCompareComponent.CreateDefault (
					Game,
					captureWhen: x => x.DrawNumber % captureStride == 0,
					maxFrameNumber: captureCount * captureStride));

			Game.Run (until: x => x.DrawNumber > captureCount * captureStride);
			if (writeDiffs)
				WriteFrameDiffs ();
			AssertFrameComparisonPassed (similarity: similarity, expectedCount: captureCount);
		}

		protected void AssertFrameComparisonPassed (
			float similarity = Constants.StandardRequiredSimilarity,
			int expectedCount = 1)
		{
			var folderName = TestContext.CurrentContext.GetTestFolderName ();
			bool found = false;
			foreach (var frameCompareComponent in Game.Components.OfType<FrameCompareComponent> ()) {
				AssertFrameComparisonPassed (frameCompareComponent.Results, similarity, expectedCount);
				found = true;
			}

			if (!found)
				Assert.Fail ("No FrameCompareComponents were found.");
		}

		protected static void AssertFrameComparisonPassed (
			IEnumerable<FrameComparisonResult> results, float similarity, int expectedCount)
		{
			var allResults = new List<FrameComparisonResult> ();
			var failedResults = new List<FrameComparisonResult> ();
			foreach (var result in results) {
				allResults.Add (result);
				if (result.Similarity < similarity)
					failedResults.Add (result);
			}

			if (allResults.Count != expectedCount)
				Assert.Fail (
					"Expected {0} frame comparison result(s), but found {1}",
					expectedCount, allResults.Count);

			WriteComparisonResultReport (allResults, similarity);

			if (failedResults.Count > 0) {
				Assert.Fail (
					"{0} of {1} frames failed the similarity test.",
					failedResults.Count, allResults.Count);
			}
		}

		private static void WriteComparisonResultReport (
			IEnumerable<FrameComparisonResult> results, float similarity)
		{
			Console.WriteLine ("Required similarity: {0:0.####}", similarity);
			foreach (var result in results)
				Console.WriteLine (
					"Similarity: {0:0.####}, Capture: {1}, Reference: {2}",
					result.Similarity, result.CapturedImagePath, result.ReferenceImagePath);
		}

		protected void WriteFrameDiffs ()
		{
			var folderName = TestContext.CurrentContext.GetTestFolderName ();
			bool found = false;
			foreach (var frameCompareComponent in Game.Components.OfType<FrameCompareComponent> ()) {
				WriteFrameDiffs (frameCompareComponent.Results, Paths.CapturedFrameDiff (folderName));
				found = true;
			}

			if (!found)
				Assert.Fail ("No FrameCompareComponents were found.");
		}

		protected static void WriteFrameDiffs (
			IEnumerable<FrameComparisonResult> results, string directory)
		{
			try {
				Directory.CreateDirectory (directory);
			} catch (IOException) {
			}

			foreach (var result in results) {

				string diffFileName = string.Format (
					"diff-{0}-{1}.png",
					Path.GetFileNameWithoutExtension (result.ReferenceImagePath),
					Path.GetFileNameWithoutExtension (result.CapturedImagePath));


				string diffOutputPath = Path.Combine (directory, diffFileName);

				var a = FramePixelData.FromFile (result.ReferenceImagePath);
				var b = FramePixelData.FromFile (result.CapturedImagePath);
				var diff = CreateDiff (a, b);
				Normalize (diff);
				diff.Save (diffOutputPath);
			}
		}

		private static FramePixelData CreateDiff (FramePixelData a, FramePixelData b)
		{
			int minWidth, maxWidth, minHeight, maxHeight;

			MathUtility.MinMax (a.Width, b.Width, out minWidth, out maxWidth);
			MathUtility.MinMax (a.Height, b.Height, out minHeight, out maxHeight);

			var diff = new FramePixelData (maxWidth, maxHeight);

			for (int y = 0; y < minHeight; ++y) {

				int indexA = y * a.Width;
				int indexB = y * b.Width;
				int indexDiff = y * diff.Width;

				for (int x = 0; x < minWidth; ++x) {
					// Ignore alpha.  If alpha diffs are
					// needed, a special strategy will have
					// to be devised, since XOR'ing two
					// opaque pixels will cause a totally
					// transparent pixel and hide any other
					// difference.
					diff.Data [indexDiff] = new Color (
						(byte) (a.Data [indexA].R ^ b.Data [indexB].R),
						(byte) (a.Data [indexA].G ^ b.Data [indexB].G),
						(byte) (a.Data [indexA].B ^ b.Data [indexB].B));

					indexA++;
					indexB++;
					indexDiff++;
				}
			}

			return diff;
		}

		private static void Normalize (FramePixelData frame)
		{
			Color max = new Color(0, 0, 0, 0);
			foreach (var pixel in frame.Data) {
				max.B = Math.Max (pixel.B, max.B);
				max.G = Math.Max (pixel.G, max.G);
				max.R = Math.Max (pixel.R, max.R);
				max.A = Math.Max (pixel.A, max.A);
			}

			if (max.B == 0) max.B = 255;
			if (max.G == 0) max.G = 255;
			if (max.R == 0) max.R = 255;
			if (max.A == 0) max.A = 255;

			for (int i = 0; i < frame.Data.Length; ++i) {
				Color pixel = frame.Data[i];


				pixel.B = (byte)(pixel.B * 255 / max.B);
				pixel.G = (byte)(pixel.G * 255 / max.G);
				pixel.R = (byte)(pixel.R * 255 / max.R);
				pixel.A = (byte)(pixel.A * 255 / max.A);

				frame.Data[i] = pixel;
			}
		}
	}
}
