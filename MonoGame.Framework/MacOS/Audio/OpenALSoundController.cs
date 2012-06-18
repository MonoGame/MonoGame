using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

#if IPHONE
using OpenTK.Audio.OpenAL;
using OpenTK;
using MonoTouch.AudioToolbox;
#elif MONOMAC
using MonoMac.OpenAL;
#endif

namespace Microsoft.Xna.Framework.Audio
{
	internal sealed class OpenALSoundController : IDisposable
	{
		static OpenALSoundController _instance = null;
		IntPtr _device;
		ContextHandle _context;
		//int outputSource;
		//int[] buffers;
		AlcError _lastOpenALError;
		int[] allSourcesArray;
		const int MAX_NUMBER_OF_SOURCES = 32;
		const double PREFERRED_MIX_RATE = 44100;
		HashSet<int> availableSourcesCollection;
		HashSet<OALSoundBuffer> inUseSourcesCollection;
		HashSet<OALSoundBuffer> playingSourcesCollection;

		private OpenALSoundController ()
		{
#if IPHONE
			AudioSession.Initialize();

			// NOTE: iOS 5.1 simulator throws an exception when setting the category
			// to SoloAmbientSound.  This could be removed if that bug gets fixed.
			try
			{
				if (AudioSession.OtherAudioIsPlaying)
					AudioSession.Category = AudioSessionCategory.AmbientSound;
				else
				{
					AudioSession.Category = AudioSessionCategory.SoloAmbientSound;
				}
			}
			catch (AudioSessionException) { }
#endif
			alcMacOSXMixerOutputRate(PREFERRED_MIX_RATE);
			_device = Alc.OpenDevice (string.Empty);
			CheckALError ("Could not open AL device");
			if (_device != IntPtr.Zero) {
				int[] attribute = new int[0];
				_context = Alc.CreateContext (_device, attribute);
				CheckALError ("Could not open AL context");

				if (_context != ContextHandle.Zero) {
					Alc.MakeContextCurrent (_context);
					CheckALError ("Could not make AL context current");
				}
			} else {
				return;
			}

			allSourcesArray = new int[MAX_NUMBER_OF_SOURCES];
			AL.GenSources (allSourcesArray);

			availableSourcesCollection = new HashSet<int> ();
			inUseSourcesCollection = new HashSet<OALSoundBuffer> ();
			playingSourcesCollection = new HashSet<OALSoundBuffer> ();


			for (int x=0; x < MAX_NUMBER_OF_SOURCES; x++) {
				availableSourcesCollection.Add (allSourcesArray [x]);
			}
#if IPHONE

			AudioSession.Interrupted += (sender, e) =>
			{
				AudioSession.SetActive(false);

				Alc.MakeContextCurrent(ContextHandle.Zero);
				Alc.SuspendContext(_context);
			};

			AudioSession.Resumed += (sender, e) =>
			{
				// That is, without this, the code wont work :(
				// It will fail on the next line of code
				// Maybe you could ask for an explanation
				// to someone at xamarin
				System.Threading.Thread.Sleep(100);
				
				AudioSession.SetActive(true);
				AudioSession.Category = AudioSessionCategory.SoloAmbientSound;

				Alc.MakeContextCurrent(_context);
				Alc.ProcessContext(_context);
			};
#endif
		}

		public static OpenALSoundController GetInstance {
			get {
				if (_instance == null)
					_instance = new OpenALSoundController ();
				return _instance;
			}

		}

		public void CheckALError (string operation)
		{
			_lastOpenALError = Alc.GetError (_device);

			if (_lastOpenALError == AlcError.NoError) {
				return;
			}

			string errorFmt = "OpenAL Error: {0}";
#if DEBUG			
			Console.WriteLine (String.Format ("{0} - {1}",
							operation,
							//string.Format (errorFmt, Alc.GetString (_device, _lastOpenALError))));
							string.Format (errorFmt, _lastOpenALError)));
#endif
		}

		private void CleanUpOpenAL ()
		{
			Alc.MakeContextCurrent (ContextHandle.Zero);
			if (_context != ContextHandle.Zero) {
				Alc.DestroyContext (_context);
				_context = ContextHandle.Zero;
			}
			if (_device != IntPtr.Zero) {
				Alc.CloseDevice (_device);
				_device = IntPtr.Zero;
			}
		}

		public void Dispose ()
		{
			CleanUpOpenAL ();
		}

		public bool ReserveSource (OALSoundBuffer soundBuffer)
		{
			int sourceNumber;
			if (availableSourcesCollection.Count == 0) {

				soundBuffer.SourceId = 0;
				return false;
			}
			

			sourceNumber = availableSourcesCollection.First ();
			soundBuffer.SourceId = sourceNumber;
			inUseSourcesCollection.Add (soundBuffer);

			availableSourcesCollection.Remove (sourceNumber);

			//sourceId = sourceNumber;
			return true;
		}

		public void RecycleSource (OALSoundBuffer soundBuffer)
		{
			inUseSourcesCollection.Remove (soundBuffer);
			availableSourcesCollection.Add (soundBuffer.SourceId);
			soundBuffer.RecycleSoundBuffer();
		}

		public void PlaySound (OALSoundBuffer soundBuffer)
		{
			playingSourcesCollection.Add (soundBuffer);
			AL.SourcePlay (soundBuffer.SourceId);
		}

		public void StopSound (OALSoundBuffer soundBuffer)
		{
			AL.SourceStop (soundBuffer.SourceId);

			AL.Source (soundBuffer.SourceId,ALSourcei.Buffer, 0);
			playingSourcesCollection.Remove (soundBuffer);
			RecycleSource (soundBuffer);
		}

		public void PauseSound (OALSoundBuffer soundBuffer)
		{
			AL.SourcePause (soundBuffer.SourceId);
		}

		public bool IsState (OALSoundBuffer soundBuffer, int state)
		{
			int sourceState;

			AL.GetSource (soundBuffer.SourceId, ALGetSourcei.SourceState, out sourceState);

			if (state == sourceState) {
				return true;
			}

			return false;
		}

		public double SourceCurrentPosition (int sourceId)
		{
			int pos;
			AL.GetSource (sourceId, ALGetSourcei.SampleOffset, out pos);
			return pos;
		}

		public void Update ()
		{
			HashSet<OALSoundBuffer> purgeMe = new HashSet<OALSoundBuffer> ();

			ALSourceState state;
			foreach (var soundBuffer in playingSourcesCollection) {

				state = AL.GetSourceState (soundBuffer.SourceId);

				if (state == ALSourceState.Stopped) {

					AL.Source (soundBuffer.SourceId, ALSourcei.Buffer, 0);
					purgeMe.Add (soundBuffer);
					//Console.WriteLine ("to be recycled: " + soundBuffer.SourceId);
				}

			}

			foreach (var soundBuffer in purgeMe) {

				playingSourcesCollection.Remove (soundBuffer);
				RecycleSource (soundBuffer);
			}
		}

		public const string OpenALLibrary = "/System/Library/Frameworks/OpenAL.framework/OpenAL";

		[DllImport(OpenALLibrary, EntryPoint = "alcMacOSXMixerOutputRate")]
		static extern void alcMacOSXMixerOutputRate (double rate); // caution


	}
}

