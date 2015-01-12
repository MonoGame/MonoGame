using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenAL;
#else
using OpenTK.Audio.OpenAL;
using OpenTK;
#endif

#if ANDROID
using System.Globalization;
using Android.Content.PM;
using Android.Content;
using Android.Media;
#endif

#if IOS
using AudioToolbox;
#endif

namespace Microsoft.Xna.Framework.Audio
{
	internal sealed class OpenALSoundController : IDisposable
	{
        private static OpenALSoundController _instance = null;
        private IntPtr _device;
        private ContextHandle _context;
		//int outputSource;
		//int[] buffers;
        private AlcError _lastOpenALError;
        private int[] allSourcesArray;
        private const int MAX_NUMBER_OF_SOURCES = 32;
#if MONOMAC || IOS
        private const double PREFERRED_MIX_RATE = 44100;
#endif
#if ANDROID
        private const int DEFAULT_FREQUENCY = 48000;
        private const int DEFAULT_UPDATE_SIZE = 512;
        private const int DEFAULT_UPDATE_BUFFER_COUNT = 2;
#endif
        private List<int> availableSourcesCollection;
        private List<OALSoundBuffer> inUseSourcesCollection;
        private List<OALSoundBuffer> playingSourcesCollection;
        private List<OALSoundBuffer> purgeMe;
        private bool _bSoundAvailable = false;
        private Exception _SoundInitException; // Here to bubble back up to the developer
        bool _isDisposed;

        /// <summary>
        /// Sets up the hardware resources used by the controller.
        /// </summary>
		private OpenALSoundController()
		{
            if (!OpenSoundController())
            {
                return;
            }
            // We have hardware here and it is ready
            _bSoundAvailable = true;

			allSourcesArray = new int[MAX_NUMBER_OF_SOURCES];
			AL.GenSources(allSourcesArray);

            availableSourcesCollection = new List<int>(allSourcesArray);
			inUseSourcesCollection = new List<OALSoundBuffer>();
			playingSourcesCollection = new List<OALSoundBuffer>();
            purgeMe = new List<OALSoundBuffer>();
		}

        ~OpenALSoundController()
        {
            Dispose(false);
        }

        /// <summary>
        /// Open the sound device, sets up an audio context, and makes the new context
        /// the current context. Note that this method will stop the playback of
        /// music that was running prior to the game start. If any error occurs, then
        /// the state of the controller is reset.
        /// </summary>
        /// <returns>True if the sound controller was setup, and false if not.</returns>
        private bool OpenSoundController()
        {
#if MONOMAC || IOS
			alcMacOSXMixerOutputRate(PREFERRED_MIX_RATE);
#endif
            try
            {
                _device = Alc.OpenDevice(string.Empty);
            }
            catch (Exception ex)
            {
                _SoundInitException = ex;
                return (false);
            }
            if (CheckALError("Could not open AL device"))
            {
                return(false);
            }
            if (_device != IntPtr.Zero)
            {
#if ANDROID
                // Attach activity event handlers so we can pause and resume all playing sounds
                AndroidGameActivity.Paused += Activity_Paused;
                AndroidGameActivity.Resumed += Activity_Resumed;

                // Query the device for the ideal frequency and update buffer size so
                // we can get the low latency sound path.

                /*
                The recommended sequence is:

                Check for feature "android.hardware.audio.low_latency" using code such as this:
                import android.content.pm.PackageManager;
                ...
                PackageManager pm = getContext().getPackageManager();
                boolean claimsFeature = pm.hasSystemFeature(PackageManager.FEATURE_AUDIO_LOW_LATENCY);
                Check for API level 17 or higher, to confirm use of android.media.AudioManager.getProperty().
                Get the native or optimal output sample rate and buffer size for this device's primary output stream, using code such as this:
                import android.media.AudioManager;
                ...
                AudioManager am = (AudioManager) getSystemService(Context.AUDIO_SERVICE);
                String sampleRate = am.getProperty(AudioManager.PROPERTY_OUTPUT_SAMPLE_RATE));
                String framesPerBuffer = am.getProperty(AudioManager.PROPERTY_OUTPUT_FRAMES_PER_BUFFER));
                Note that sampleRate and framesPerBuffer are Strings. First check for null and then convert to int using Integer.parseInt().
                Now use OpenSL ES to create an AudioPlayer with PCM buffer queue data locator.

                See http://stackoverflow.com/questions/14842803/low-latency-audio-playback-on-android
                */

                int frequency = DEFAULT_FREQUENCY;
                int updateSize = DEFAULT_UPDATE_SIZE;
                int updateBuffers = DEFAULT_UPDATE_BUFFER_COUNT;
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.JellyBeanMr1)
                {
                    Android.Util.Log.Debug("OAL", Game.Activity.PackageManager.HasSystemFeature(PackageManager.FeatureAudioLowLatency) ? "Supports low latency audio playback." : "Does not support low latency audio playback.");

                    var audioManager = Game.Activity.GetSystemService(Context.AudioService) as AudioManager;
                    if (audioManager != null)
                    {
                        var result = audioManager.GetProperty(AudioManager.PropertyOutputSampleRate);
                        if (!string.IsNullOrEmpty(result))
                            frequency = int.Parse(result, CultureInfo.InvariantCulture);
                        result = audioManager.GetProperty(AudioManager.PropertyOutputFramesPerBuffer);
                        if (!string.IsNullOrEmpty(result))
                            updateSize = int.Parse(result, CultureInfo.InvariantCulture);
                    }

                    // If 4.4 or higher, then we don't need to double buffer on the application side.
                    // See http://stackoverflow.com/a/15006327
                    // Use the explicit value rather than a constant as the 4.2 SDK (the build SDK) does not define a constant for 4.4.
                    if ((int)Android.OS.Build.VERSION.SdkInt >= 19)
                    {
                        updateBuffers = 1;
                    }
                }
                else
                {
                    Android.Util.Log.Debug("OAL", "Android 4.2 or higher required for low latency audio playback.");
                }
                Android.Util.Log.Debug("OAL", "Using sample rate " + frequency + "Hz and " + updateBuffers + " buffers of " + updateSize + " frames.");

                // These are missing and non-standard ALC constants
                const int AlcFrequency = 0x1007;
                const int AlcUpdateSize = 0x1014;
                const int AlcUpdateBuffers = 0x1015;

                int[] attribute = new[]
                {
                    AlcFrequency, frequency,
                    AlcUpdateSize, updateSize,
                    AlcUpdateBuffers, updateBuffers,
                    0
                };
#elif IOS
                AudioSession.Initialize();

                AudioSession.Interrupted += (sender, e) => {
                    AudioSession.SetActive(false);
                    Alc.MakeContextCurrent(ContextHandle.Zero);
                    Alc.SuspendContext(_context);
                };
                AudioSession.Resumed += (sender, e) => {
                    AudioSession.SetActive(true);
                    Alc.MakeContextCurrent(_context);
                    Alc.ProcessContext(_context);
                };

                int[] attribute = new int[0];
#else
                int[] attribute = new int[0];
#endif
                _context = Alc.CreateContext(_device, attribute);
                if (CheckALError("Could not create AL context"))
                {
                    CleanUpOpenAL();
                    return(false);
                }

                if (_context != ContextHandle.Zero)
                {
                    Alc.MakeContextCurrent(_context);
                    if (CheckALError("Could not make AL context current"))
                    {
                        CleanUpOpenAL();
                        return(false);
                    }
                    return (true);
                }
            }
            return (false);
        }

		public static OpenALSoundController GetInstance {
			get {
				if (_instance == null)
					_instance = new OpenALSoundController();
				return _instance;
			}
		}

        public static void DestroyInstance()
        {
            if (_instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }
        }


        /// <summary>
        /// Checks the error state of the OpenAL driver. If a value that is not AlcError.NoError
        /// is returned, then the operation message and the error code is output to the console.
        /// </summary>
        /// <param name="operation">the operation message</param>
        /// <returns>true if an error occurs, and false if not.</returns>
		public bool CheckALError (string operation)
		{
			_lastOpenALError = Alc.GetError (_device);

			if (_lastOpenALError == AlcError.NoError) {
				return(false);
			}

			string errorFmt = "OpenAL Error: {0}";
			Console.WriteLine (String.Format ("{0} - {1}",
							operation,
							//string.Format (errorFmt, Alc.GetString (_device, _lastOpenALError))));
							string.Format (errorFmt, _lastOpenALError)));
            return (true);
		}

        /// <summary>
        /// Destroys the AL context and closes the device, when they exist.
        /// </summary>
		private void CleanUpOpenAL()
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
            _bSoundAvailable = false;
		}

        /// <summary>
        /// Dispose of the OpenALSoundCOntroller.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the OpenALSoundCOntroller.
        /// </summary>
        /// <param name="disposing">If true, the managed resources are to be disposed.</param>
		void Dispose(bool disposing)
		{
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_bSoundAvailable)
                        CleanUpOpenAL();
                }
                _isDisposed = true;
            }
		}

        /// <summary>
        /// Reserves the given sound buffer. If there are no available sources then false is
        /// returned, otherwise true will be returned and the sound buffer can be played. If
        /// the controller was not able to setup the hardware, then false will be returned.
        /// </summary>
        /// <param name="soundBuffer">The sound buffer you want to play</param>
        /// <returns>True if the buffer can be played, and false if not.</returns>
		public bool ReserveSource (OALSoundBuffer soundBuffer)
		{
            if (!CheckInitState())
            {
                return(false);
            }
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
            if (!CheckInitState())
            {
                return;
            }
            inUseSourcesCollection.Remove(soundBuffer);
			availableSourcesCollection.Add (soundBuffer.SourceId);
			soundBuffer.RecycleSoundBuffer();
		}

		public void PlaySound (OALSoundBuffer soundBuffer)
        {
            if (!CheckInitState())
            {
                return;
            }
            lock (playingSourcesCollection)
            {
                playingSourcesCollection.Add (soundBuffer);
            }
			AL.SourcePlay (soundBuffer.SourceId);
		}

		public void StopSound (OALSoundBuffer soundBuffer)
        {
            if (!CheckInitState())
            {
                return;
            }
            AL.SourceStop(soundBuffer.SourceId);

            AL.Source (soundBuffer.SourceId, ALSourcei.Buffer, 0);
            lock (playingSourcesCollection) {
                playingSourcesCollection.Remove (soundBuffer);
            }
            RecycleSource (soundBuffer);
		}

		public void PauseSound (OALSoundBuffer soundBuffer)
		{
            if (!CheckInitState())
            {
                return;
            }
            soundBuffer.Pause();
		}

        public void ResumeSound(OALSoundBuffer soundBuffer)
        {
            if (!CheckInitState())
            {
                return;
            }
            soundBuffer.Resume();
        }

		public bool IsState (OALSoundBuffer soundBuffer, int state)
		{
            if (!CheckInitState())
            {
                return (false);
            }
            int sourceState;

			AL.GetSource (soundBuffer.SourceId, ALGetSourcei.SourceState, out sourceState);

			if (state == sourceState) {
				return true;
			}

			return false;
		}

        /// <summary>
        /// Checks if the AL controller was initialized properly. If there was an
        /// exception thrown during the OpenAL init, then that exception is thrown
        /// inside of NoAudioHardwareException.
        /// </summary>
        /// <returns>True if the controller was initialized, false if not.</returns>
        private bool CheckInitState()
        {
            if (!_bSoundAvailable)
            {
                if (_SoundInitException != null)
                {
                    Exception e = _SoundInitException;
                    _SoundInitException = null;
                    throw new NoAudioHardwareException("No audio hardware available.", e);
                }
                return (false);
            }
            return (true);
        }

        public double SourceCurrentPosition (int sourceId)
		{
            if (!CheckInitState())
            {
                return(0.0);
            }
            int pos;
			AL.GetSource (sourceId, ALGetSourcei.SampleOffset, out pos);
			return pos;
		}

        /// <summary>
        /// Called repeatedly, this method cleans up the state of the management lists. This method
        /// will also lock on the playingSourcesCollection. Sources that are stopped will be recycled
        /// using the RecycleSource method.
        /// </summary>
		public void Update()
        {
            if (!_bSoundAvailable)
            {
                //OK to ignore this here because the game can run without sound.
                 return;
            }

            ALSourceState state;
            lock (playingSourcesCollection)
            {
                for (int i = playingSourcesCollection.Count - 1; i >= 0; --i)
                {
                    var soundBuffer = playingSourcesCollection[i];
                    state = AL.GetSourceState(soundBuffer.SourceId);
                    if (state == ALSourceState.Stopped)
                    {
                        purgeMe.Add(soundBuffer);
                        playingSourcesCollection.RemoveAt(i);
                    }
                }
            }
            foreach (var soundBuffer in purgeMe)
            {
                AL.Source(soundBuffer.SourceId, ALSourcei.Buffer, 0);
                RecycleSource(soundBuffer);
            }
            purgeMe.Clear();
        }

#if ANDROID
        void Activity_Paused(object sender, EventArgs e)
        {
            // Pause all currently playing sounds. The internal pause count in OALSoundBuffer
            // will take care of sounds that were already paused.
            lock (playingSourcesCollection)
            {
                foreach (var source in playingSourcesCollection)
                    source.Pause();
            }
        }

        void Activity_Resumed(object sender, EventArgs e)
        {
            // Resume all sounds that were playing when the activity was paused. The internal
            // pause count in OALSoundBuffer will take care of sounds that were previously paused.
            lock (playingSourcesCollection)
            {
                foreach (var source in playingSourcesCollection)
                    source.Resume();
            }
        }
#endif

#if MONOMAC || IOS
		public const string OpenALLibrary = "/System/Library/Frameworks/OpenAL.framework/OpenAL";

		[DllImport(OpenALLibrary, EntryPoint = "alcMacOSXMixerOutputRate")]
		static extern void alcMacOSXMixerOutputRate (double rate); // caution
#endif

	}
}

