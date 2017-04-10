using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Utilities;

#if MONOMAC && PLATFORM_MACOS_LEGACY
using MonoMac.OpenAL;
#endif
#if MONOMAC && !PLATFORM_MACOS_LEGACY
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;
#endif

#if GLES
using OpenTK.Audio.OpenAL;
using OpenTK;
#endif

#if DESKTOPGL
using OpenAL;
#endif
using OpenGL;

#if ANDROID
using System.Globalization;
using Android.Content.PM;
using Android.Content;
using Android.Media;
#endif

#if IOS
using AudioToolbox;
using AVFoundation;
#endif

namespace Microsoft.Xna.Framework.Audio
{
    internal static class ALHelper
    {
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        public static void CheckError(string message = "", params object[] args)
        {
            ALError error;
            if ((error = AL.GetError()) != ALError.NoError)
            {
                if (args != null && args.Length > 0)
                    message = String.Format(message, args);
                
                throw new InvalidOperationException(message + " (Reason: " + AL.GetErrorString(error) + ")");
            }
        }
    }

	internal sealed class OpenALSoundController : IDisposable
    {
        private static OpenALSoundController _instance = null;
#if SUPPORTS_EFX
        private static EffectsExtension _efx = null;
#endif
        private IntPtr _device;
#if !DESKTOPGL
        ContextHandle _context;
        ContextHandle NullContext = ContextHandle.Zero;
#else
        private IntPtr _context;
        IntPtr NullContext = IntPtr.Zero;
#endif
        //int outputSource;
        //int[] buffers;
        private AlcError _lastOpenALError;
        private int[] allSourcesArray;
#if DESKTOPGL || ANGLE

        // MacOS & Linux shares a limit of 256.
        internal const int MAX_NUMBER_OF_SOURCES = 256;

#elif MONOMAC

        // Reference: http://stackoverflow.com/questions/3894044/maximum-number-of-openal-sound-buffers-on-iphone
        internal const int MAX_NUMBER_OF_SOURCES = 256;

#elif IOS

        // Reference: http://stackoverflow.com/questions/3894044/maximum-number-of-openal-sound-buffers-on-iphone
        internal const int MAX_NUMBER_OF_SOURCES = 32;

#elif ANDROID

        // Set to the same as OpenAL on iOS
        internal const int MAX_NUMBER_OF_SOURCES = 32;

#endif
#if MONOMAC || IOS
        private const double PREFERRED_MIX_RATE = 44100;
#elif ANDROID
        private const int DEFAULT_FREQUENCY = 48000;
        private const int DEFAULT_UPDATE_SIZE = 512;
        private const int DEFAULT_UPDATE_BUFFER_COUNT = 2;
#elif DESKTOPGL
        private static OggStreamer _oggstreamer;
#endif
        private List<int> availableSourcesCollection;
        private List<int> inUseSourcesCollection;
        bool _isDisposed;
        public bool SupportsADPCM = false;

        /// <summary>
        /// Sets up the hardware resources used by the controller.
        /// </summary>
		private OpenALSoundController()
        {
#if WINDOWS
            // On Windows, set the DLL search path for correct native binaries
            NativeHelper.InitDllDirectory();
#endif

            if (!OpenSoundController())
            {
                throw new NoAudioHardwareException("OpenAL device could not be initialized, see console output for details.");
            }

            if (Alc.IsExtensionPresent(_device, "ALC_EXT_CAPTURE"))
                Microphone.PopulateCaptureDevices();

            // We have hardware here and it is ready

			allSourcesArray = new int[MAX_NUMBER_OF_SOURCES];
			AL.GenSources(allSourcesArray);
            ALHelper.CheckError("Failed to generate sources.");
            Filter = 0;
#if SUPPORTS_EFX
            if (Efx.IsInitialized) {
                Filter = Efx.GenFilter ();
            }
#endif
            availableSourcesCollection = new List<int>(allSourcesArray);
			inUseSourcesCollection = new List<int>();
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
#if MONOMAC
			alcMacOSXMixerOutputRate(PREFERRED_MIX_RATE);
#endif

            try
            {
                _device = Alc.OpenDevice(string.Empty);
#if DESKTOPGL
                EffectsExtension.device = _device;
#endif
            }
            catch (DllNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new NoAudioHardwareException("OpenAL device could not be initialized.", ex);
            }

            CheckALError("Could not open AL device");

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
                EventHandler<AVAudioSessionInterruptionEventArgs> handler = delegate(object sender, AVAudioSessionInterruptionEventArgs e) {
                    switch (e.InterruptionType) {
                        case AVAudioSessionInterruptionType.Began:
                            AVAudioSession.SharedInstance().SetActive(false);
                            Alc.MakeContextCurrent(ContextHandle.Zero);
                            Alc.SuspendContext(_context);
                            break;
                        case AVAudioSessionInterruptionType.Ended:
                            AVAudioSession.SharedInstance().SetActive(true);
                            Alc.MakeContextCurrent(_context);
                            Alc.ProcessContext(_context);
                            break;
                    }
                };
                AVAudioSession.Notifications.ObserveInterruption(handler);

                int[] attribute = new int[0];
#else
                int[] attribute = new int[0];
#endif

                _context = Alc.CreateContext(_device, attribute);
#if DESKTOPGL
                _oggstreamer = new OggStreamer();
#endif

                CheckALError("Could not create AL context");

                if (_context != NullContext)
                {
                    Alc.MakeContextCurrent(_context);
                    CheckALError("Could not make AL context current");
                    SupportsADPCM = AL.IsExtensionPresent ("AL_SOFT_MSADPCM");
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
#if SUPPORTS_EFX
        public static EffectsExtension Efx {
            get {
                if (_efx == null)
                    _efx = new EffectsExtension ();
                return _efx;
            }
        }
#endif
        public int Filter {
            get; private set;
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
        /// is returned, then the operation message and the error code is thrown in an Exception.
        /// </summary>
        /// <param name="operation">the operation message</param>
		public void CheckALError (string operation)
		{
			_lastOpenALError = Alc.GetError (_device);

			if (_lastOpenALError == AlcError.NoError)
				return;			

            CleanUpOpenAL();

			string errorFmt = "OpenAL Error: {0}";

            throw new NoAudioHardwareException(String.Format("{0} - {1}",
                            operation,
                            //string.Format (errorFmt, Alc.GetString (_device, _lastOpenALError))));
                            string.Format(errorFmt, _lastOpenALError)));
		}

        /// <summary>
        /// Destroys the AL context and closes the device, when they exist.
        /// </summary>
        private void CleanUpOpenAL()
        {
            Alc.MakeContextCurrent(NullContext);

            if (_context != NullContext)
            {
                Alc.DestroyContext (_context);
                _context = NullContext;
            }
            if (_device != IntPtr.Zero)
            {
                Alc.CloseDevice (_device);
                _device = IntPtr.Zero;
            }
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
#if DESKTOPGL
                    if(_oggstreamer != null)
                        _oggstreamer.Dispose();
#endif
                    for (int i = 0; i < allSourcesArray.Length; i++)
                    {
                        AL.DeleteSource(allSourcesArray[i]);
                        ALHelper.CheckError("Failed to delete source.");
                    }
#if SUPPORTS_EFX
                    if (Filter != 0 && Efx.IsInitialized)
                        Efx.DeleteFilter (Filter);
#endif
                    Microphone.StopMicrophones();
                    CleanUpOpenAL();                    
                }
                _isDisposed = true;
            }
		}

        /// <summary>
        /// Reserves a sound buffer and return its identifier. If there are no available sources
        /// or the controller was not able to setup the hardware then an
        /// <see cref="InstancePlayLimitException"/> is thrown.
        /// </summary>
        /// <returns>The source number of the reserved sound buffer.</returns>
		public int ReserveSource()
		{
            int sourceNumber;

            lock (availableSourcesCollection)
            {                
                if (availableSourcesCollection.Count == 0)
                {
                    throw new InstancePlayLimitException();
                }

                sourceNumber = availableSourcesCollection.Last();
                inUseSourcesCollection.Add(sourceNumber);
                availableSourcesCollection.Remove(sourceNumber);
            }

            return sourceNumber;
		}

        public void RecycleSource(int sourceId)
		{
            lock (availableSourcesCollection)
            {
                inUseSourcesCollection.Remove(sourceId);
                availableSourcesCollection.Add(sourceId);
            }
		}

        public void FreeSource(SoundEffectInstance inst)
        {
            RecycleSource(inst.SourceId);
            inst.SourceId = 0;
            inst.HasSourceId = false;
            inst.SoundState = SoundState.Stopped;
		}

        public double SourceCurrentPosition (int sourceId)
		{
            int pos;
			AL.GetSource (sourceId, ALGetSourcei.SampleOffset, out pos);
            ALHelper.CheckError("Failed to set source offset.");
			return pos;
		}

#if ANDROID
        const string Lib = "openal32.dll";
        const CallingConvention Style = CallingConvention.Cdecl;

        [DllImport(Lib, EntryPoint = "alcDevicePauseSOFT", ExactSpelling = true, CallingConvention = Style)]
        unsafe static extern void alcDevicePauseSOFT(IntPtr device);

        [DllImport(Lib, EntryPoint = "alcDeviceResumeSOFT", ExactSpelling = true, CallingConvention = Style)]
        unsafe static extern void alcDeviceResumeSOFT(IntPtr device);

        void Activity_Paused(object sender, EventArgs e)
        {
            // Pause all currently playing sounds. The internal pause count in OALSoundBuffer
            // will take care of sounds that were already paused.
            //            lock (playingSourcesCollection)
            //            {
            //                foreach (var source in playingSourcesCollection)
            //                    source.Pause();
            //            }
            alcDevicePauseSOFT(_device);
        }

        void Activity_Resumed(object sender, EventArgs e)
        {
            // Resume all sounds that were playing when the activity was paused. The internal
            // pause count in OALSoundBuffer will take care of sounds that were previously paused.
            //            lock (playingSourcesCollection)
            //            {
            //                foreach (var source in playingSourcesCollection)
            //                    source.Resume();
            //            }
            alcDeviceResumeSOFT(_device);
        }
#endif

#if MONOMAC
		public const string OpenALLibrary = "/System/Library/Frameworks/OpenAL.framework/OpenAL";

		[DllImport(OpenALLibrary, EntryPoint = "alcMacOSXMixerOutputRate")]
		static extern void alcMacOSXMixerOutputRate (double rate); // caution
#endif
    }
}

