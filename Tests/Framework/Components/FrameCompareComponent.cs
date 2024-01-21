// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NUnit.Framework;

// TODO: It's likely that a more sophisticated approach will be required for
//       comparing images.  In particular, comparing pixel deltas would give
//       quite a high score to some images that humans would rate as extremely
//       different to each other.  For example, with PixelDeltaComparer, a
//       totally white background vs a totally white background with a one-pixel
//       diagonal line from top-left to bottom-right would be scored in the
//       (very) high 90s for percent similarity, while no human would rate it
//       so.  Several passes may be desirable, in fact, to test for color
//       differences, contrast differences, color-contrast differences, edge
//       differences, etc.

namespace MonoGame.Tests.Components {
	/// <summary>
	/// Defines behavior needed for frames to be scheduled for capture and
	/// then manipulated and released.
	/// </summary>
	interface IFrameCaptureSource {
		/// <summary>
		/// Schedules a frame capture for the next available Draw cycle.
		/// </summary>
		void ScheduleFrameCapture ();

		/// <summary>
		/// Gets the captured frame from the last scheduled capture.
		/// </summary>
		/// <returns></returns>
		Texture2D GetCapturedFrame ();

		/// <summary>
		/// Notifies the <see cref="IFrameCaptureSource"/> implementation that
		/// a called has finished using the texture returned by
		/// <see cref="GetCapturedFrame"/>.
		/// </summary>
		/// <param name="frame"></param>
		void ReleaseCapturedFrame (Texture2D frame);
	}

	/// <summary>
	/// Defines methods for comparing two visual frames.
	/// </summary>
	interface IFrameComparer {
		/// <summary>
		/// Compares two frames and returns a similarity value from 0.0f
		/// to 1.0f.
		/// </summary>
        /// <param name="image">The image to compare.</param>
        /// <param name="referenceImage">A ground truth image to compare against.</param>
		/// <returns>A floating point value from 0.0f to 1.0f that
		/// represents the similarity of the two frames, according to
		/// this IFrameComparer implementation.</returns>
        float Compare(FramePixelData image, FramePixelData referenceImage);
	}

	class FrameCompareComponent : DrawableGameComponent, IEnumerable<IFrameComparer> {
		private static class Errors {
			public const string AtLeastOneFrameComparerRequired =
				"At least one IFrameComparer must be added before capturing and comparing frames";
		}

		private enum RunState {
			Idle,
			DidScheduleFrameCapture,
			DidCaptureFrame
		}

		private IFrameCaptureSource _frameSource;
		private string _fileNameFormat;
		private string _referenceImageDirectory;

		private readonly List<Tuple<IFrameComparer, float>> _frameComparers = new List<Tuple<IFrameComparer, float>> ();
		private readonly List<FrameComparisonResult> _results = new List<FrameComparisonResult> ();
		private readonly object _resultsSync = new object ();

		private Thread _workThread;
		private readonly object _workThreadSync = new object ();

		public FrameCompareComponent (
			Game game, Predicate<FrameInfo> captureWhen,
			string fileNameFormat, string referenceImageDirectory, string outputDirectory)
			: base (game)
		{
			if (fileNameFormat == null)
				throw new ArgumentNullException ("fileNameFormat");
			if (referenceImageDirectory == null)
				throw new ArgumentNullException ("compareSourceDirectory");

			CaptureWhen = captureWhen;
			_fileNameFormat = fileNameFormat;
			_referenceImageDirectory = referenceImageDirectory;
			OutputDirectory = outputDirectory;
		}

		public static FrameCompareComponent CreateDefault (
			Game game, Predicate<FrameInfo> captureWhen = null, int maxFrameNumber = 99)
		{
			var folderName = TestContext.CurrentContext.GetTestFolderName ();
			var fileNameFormat = TestContext.CurrentContext.GetTestFrameFileNameFormat (maxFrameNumber);

			return new FrameCompareComponent (
				game,
				captureWhen: captureWhen,
				fileNameFormat: fileNameFormat,
				referenceImageDirectory: Paths.ReferenceImage (folderName),
				outputDirectory: Paths.CapturedFrame (folderName))
				{
					{ new PixelDeltaFrameComparer(), 1 }
				};
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (_workThread != null) {
					try {
						_workThread.Abort ();
					} catch (ThreadStateException) { }
					_workThread = null;
				}
			}
			base.Dispose (disposing);
		}

		public Predicate<FrameInfo> CaptureWhen { get; set; }

		private RunState _state = RunState.Idle;
		private RunState State
		{
			get { return _state; }
			set {
				if (_state != value) {
					// Debugging only
					//Console.WriteLine ("State: {0}->{1}", _state, value);
					_state = value;
				}
			}
		}

		public string OutputDirectory { get; set; }

		public void Add (IFrameComparer comparer, float weight)
		{
			if (comparer == null)
				throw new ArgumentNullException ("comparer");
			if (weight < 0)
				throw new ArgumentOutOfRangeException ("weight", "weight must not be negative");

			_frameComparers.Add (Tuple.Create (comparer, weight));
		}

		public bool Remove (IFrameComparer comparer)
		{
			for (int i = 0; i < _frameComparers.Count; ++i) {
				if (object.Equals (_frameComparers [i], comparer)) {
					_frameComparers.RemoveAt (i);
					return true;
				}
			}
			return false;
		}

		public IEnumerable<FrameComparisonResult> Results
		{
			get
			{
				// Signal the end of the work items, then wait
				// for processing to complete.
				_workItems.Add (null);
				lock (_resultsSync)
					return _results;
			}
		}

		public override void Initialize ()
		{
			base.Initialize ();
			_frameSource = Game.Services.RequireService<IFrameCaptureSource> ();
		}

		public override void Update (GameTime gameTime)
		{
			var frameInfo = Game.Services.RequireService<IFrameInfoSource> ().FrameInfo;

			if (State == RunState.DidCaptureFrame)
				ProcessCapturedFrame ();

			if (State == RunState.Idle && (CaptureWhen == null || CaptureWhen (frameInfo)))
				ScheduleFrameCapture ();
		}

		public override void Draw (GameTime gameTime)
		{
			switch (State) {
			case RunState.DidScheduleFrameCapture:
				// By this point, IFrameSource is processing the
				// capture request, and will have finished by
				// the next call to Update.
				State = RunState.DidCaptureFrame;
				break;
			}
		}

		private void ScheduleFrameCapture ()
		{
			_frameSource.ScheduleFrameCapture ();
			State = RunState.DidScheduleFrameCapture;
		}

		private void ProcessCapturedFrame ()
		{
			var frame = _frameSource.GetCapturedFrame ();
			try {
				if (_frameComparers.Count == 0)
					throw new InvalidOperationException (Errors.AtLeastOneFrameComparerRequired);

				lock (_workThreadSync) {
					if (_workThread == null) {
						_workThread = new Thread (CompareAndWriteWorker);
						_workThread.Priority = ThreadPriority.Lowest;
						_workThread.IsBackground = true;
						_workThread.Start ();
					}
				}

				var frameInfo = Game.Services.RequireService<IFrameInfoSource> ().FrameInfo;

				var fileName = string.Format (_fileNameFormat, frameInfo.DrawNumber);

				string frameOutputPath = null;
				if (OutputDirectory != null)
					frameOutputPath = Path.Combine (OutputDirectory ?? ".", fileName);
				var referenceImagePath = Path.Combine (_referenceImageDirectory, fileName);

				var textureData = GetTextureData (frame);

				_workItems.Add (new WorkItem (
					frameInfo, textureData, frame.Width, frame.Height,
					frameOutputPath, referenceImagePath,
					_frameComparers.ToArray()));
			} finally {
				_frameSource.ReleaseCapturedFrame (frame);
				State = RunState.Idle;
			}
		}

		private BlockingCollection<WorkItem> _workItems =
			new BlockingCollection<WorkItem> (new ConcurrentQueue<WorkItem> ());
		private void CompareAndWriteWorker ()
		{
			// HACK: This should not be needed!
			Paths.SetStandardWorkingDirectory ();

			lock (_resultsSync) {
				while (true) {
					var workItem = _workItems.Take ();
					if (workItem == null)
						break;

					if (workItem.FrameOutputPath != null) {
						var directory = Path.GetDirectoryName (workItem.FrameOutputPath);
						if (!Directory.Exists (directory))
							Directory.CreateDirectory (directory);
					}

				    var framePixelData = new FramePixelData (
						workItem.TextureWidth, workItem.TextureHeight, workItem.TextureData);
					var comparePixelData = LoadOrCreateEmptyFramePixelData (workItem.ReferenceImagePath);

					var similarity = CompareFrames (
					    framePixelData,
					    comparePixelData,
					    workItem.FrameComparers);

					if (workItem.FrameOutputPath != null) {
						try {
							framePixelData.Save (workItem.FrameOutputPath, "Output");
						} catch (IOException) {
							// FIXME: Report this error somehow.
						}
					}

					_results.Add (new FrameComparisonResult (
						workItem.FrameInfo.DrawNumber, similarity,
						workItem.ReferenceImagePath, workItem.FrameOutputPath));
				}
			}

			lock (_workThreadSync) {
				_workThread = null;
			}
		}

		private static float CompareFrames (
			FramePixelData image, FramePixelData referenceImage,
			Tuple<IFrameComparer, float> [] frameComparers)
		{
			float sumOfWeights = 0;
			foreach (var item in frameComparers) {
				sumOfWeights += item.Item2;
			}

			float similarity = 0;
			foreach (var item in frameComparers) {
				var comparer = item.Item1;
				var weight = item.Item2;
                similarity += comparer.Compare(image, referenceImage) * weight / sumOfWeights;
			}
			return similarity;
		}

		private Color[] GetTextureData (Texture2D frame)
		{
			var data = new Color [frame.Width * frame.Height];
			frame.GetData (data);
			return data;
		}

		private static FramePixelData LoadOrCreateEmptyFramePixelData (string path)
		{
			try {
				return FramePixelData.FromFile (path);
			} catch (FileNotFoundException) {
				// TODO: It would be nice to communicate
				//       information about what went wrong, when
				//       things go wrong.
				return new FramePixelData (0, 0, new Color[0]);
			}
		}

		public IEnumerator<IFrameComparer> GetEnumerator ()
		{
			foreach (var item in _frameComparers)
				yield return item.Item1;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			foreach (var item in _frameComparers)
				yield return item.Item1;
		}

		private class WorkItem {
			public readonly FrameInfo FrameInfo;
			public readonly Color [] TextureData;
			public readonly int TextureWidth;
			public readonly int TextureHeight;
			public readonly string FrameOutputPath;
			public readonly string ReferenceImagePath;
			public readonly Tuple<IFrameComparer, float> [] FrameComparers;

			public WorkItem (
				FrameInfo frameInfo,
				Color [] textureData, int textureWidth, int textureHeight,
				string frameOutputPath, string referenceImagePath,
				Tuple<IFrameComparer, float> [] frameComparers)
			{
				FrameInfo = frameInfo;
				TextureData = textureData;
				TextureWidth = textureWidth;
				TextureHeight = textureHeight;
				FrameOutputPath = frameOutputPath;
				ReferenceImagePath = referenceImagePath;
				FrameComparers = frameComparers;
			}
		}
	}

	public struct FrameComparisonResult {

		public FrameComparisonResult (
			int drawNumber, float similarity, 
			string referenceImagePath, string capturedImagePath = null)
		{
			DrawNumber = drawNumber;
			Similarity = similarity;
			ReferenceImagePath = referenceImagePath;
			CapturedImagePath = capturedImagePath;
		}

		public int DrawNumber;
		public float Similarity;
		public string CapturedImagePath;
		public string ReferenceImagePath;
	}

	class ConstantComparer : IFrameComparer {
		private float _value;
		public ConstantComparer (float value)
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException ("value", "value must not be negative");
			_value = value;
		}

		public float Compare (FramePixelData a, FramePixelData b)
		{
			return _value;
		}
	}
}
