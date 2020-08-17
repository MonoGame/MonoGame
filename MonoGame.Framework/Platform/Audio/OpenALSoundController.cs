using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using MonoGame.OpenAL;
using MonoGame.OpenGL;

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
        internal static void CheckError(string message = "", params object[] args)
        {
            ALError error;
            if ((error = AL.GetError()) != ALError.NoError)
            {
                if (args != null && args.Length > 0)
                    message = String.Format(message, args);
                
                throw new InvalidOperationException(message + " (Reason: " + AL.GetErrorString(error) + ")");
            }
        }

        public static bool IsStereoFormat(ALFormat format)
        {
            return (format == ALFormat.Stereo8
                || format == ALFormat.Stereo16
                || format == ALFormat.StereoFloat32
                || format == ALFormat.StereoIma4
                || format == ALFormat.StereoMSAdpcm);
        }
    }

    internal static class AlcHelper
    {
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        internal static void CheckError(string message = "", params object[] args)
        {
            AlcError error;
            if ((error = Alc.GetError()) != AlcError.NoError)
            {
                if (args != null && args.Length > 0)
                    message = String.Format(message, args);

                throw new InvalidOperationException(message + " (Reason: " + error.ToString() + ")");
            }
        }
    }

    internal sealed class OpenALSoundController : IDisposable
    {
        private static OpenALSoundController _instance = null;
        private static EffectsExtension _efx = null;
        private IntPtr _device;
        private IntPtr _context;
        IntPtr NullContext = IntPtr.Zero;
        private int[] allSourcesArray;
#if DESKTOPGL || ANGLE

        // MacOS & Linux shares a limit of 256.
        internal const int MAX_NUMBER_OF_SOURCES = 256;

#elif IOS

        // Reference: http://stackoverflow.com/questions/3894044/maximum-number-of-openal-sound-buffers-on-iphone
        internal const int MAX_NUMBER_OF_SOURCES = 32;

#elif ANDROID

        // Set to the same as OpenAL on iOS
        internal const int MAX_NUMBER_OF_SOURCES = 32;

#endif
#if ANDROID
        private const int DEFAULT_FREQUENCY = 48000;
        private const int DEFAULT_UPDATE_SIZE = 512;
        private const int DEFAULT_UPDATE_BUFFER_COUNT = 2;
#elif DESKTOPGL
        private static OggStreamer _oggstreamer;
#endif
        private List<int> availableSourcesCollection;
        private List<int> inUseSourcesCollection;
        bool _isDisposed;
        public bool SupportsIma4 { get; private set; }
        public bool SupportsAdpcm { get; private set; }
        public bool SupportsEfx { get; private set; }
        public bool SupportsIeee { get; private set; }

        /// <summary>
        /// Sets up the hardware resources used by the controller.
        /// </summary>
		private OpenALSoundController()
        {
            if (AL.NativeLibrary == IntPtr.Zero)
                throw new DllNotFoundException("Couldn't initialize OpenAL because the native binaries couldn't be found.");

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
            if (Efx.IsInitialized)
            {
                Filter = Efx.GenFilter();
            }
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
            try
            {
                _device = Alc.OpenDevice(string.Empty);
                EffectsExtension.device = _device;
            }
            catch (Exception ex)
            {
                throw new NoAudioHardwareException("OpenAL device could not be initialized.", ex);
            }

            AlcHelper.CheckError("Could not open OpenAL device");

            if (_device != IntPtr.Zero)
            {
#if ANDROID
                // Attach activity event handlers so we can pause and resume all playing sounds
                MonoGameAndroidGameView.OnPauseGameThread += Activity_Paused;
                MonoGameAndroidGameView.OnResumeGameThread += Activity_Resumed;

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
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
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
                AVAudioSession.SharedInstance().Init();

                // NOTE: Do not override AVAudioSessionCategory set by the game developer:
                //       see https://github.com/MonoGame/MonoGame/issues/6595

                EventHandler<AVAudioSessionInterruptionEventArgs> handler = delegate(object sender, AVAudioSessionInterruptionEventArgs e) {
                    switch (e.InterruptionType)
                    {
                        case AVAudioSessionInterruptionType.Began:
                            AVAudioSession.SharedInstance().SetActive(false);
                            Alc.MakeContextCurrent(IntPtr.Zero);
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

                // Activate the instance or else the interruption handler will not be called.
                AVAudioSession.SharedInstance().SetActive(true);

                int[] attribute = new int[0];
#else
                int[] attribute = new int[0];
#endif

                _context = Alc.CreateContext(_device, attribute);
#if DESKTOPGL
                _oggstreamer = new OggStreamer();
#endif

                AlcHelper.CheckError("Could not create OpenAL context");

                if (_context != NullContext)
                {
                    Alc.MakeContextCurrent(_context);
                    AlcHelper.CheckError("Could not make OpenAL context current");
                    SupportsIma4 = AL.IsExtensionPresent("AL_EXT_IMA4");
                    SupportsAdpcm = AL.IsExtensionPresent("AL_SOFT_MSADPCM");
                    SupportsEfx = AL.IsExtensionPresent("AL_EXT_EFX");
                    SupportsIeee = AL.IsExtensionPresent("AL_EXT_float32");
                    return true;
                }
            }
            return false;
        }

        public static void EnsureInitialized()
        {
            if (_instance == null)
            {
                try
                {
                    _instance = new OpenALSoundController();
                }
                catch (DllNotFoundException)
                {
                    throw;
                }
                catch (NoAudioHardwareException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw (new NoAudioHardwareException("Failed to init OpenALSoundController", ex));
                }
            }
        }


        public static OpenALSoundController Instance
        {
			get
            {
                if (_instance == null)
                    throw new NoAudioHardwareException("OpenAL context has failed to initialize. Call SoundEffect.Initialize() before sound operation to get more specific errors.");
				return _instance;
			}
		}

        public static EffectsExtension Efx
        {
            get
            {
                if (_efx == null)
                    _efx = new EffectsExtension();
                return _efx;
            }
        }

        public int Filter
        {
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

                    if (Filter != 0 && Efx.IsInitialized)
                        Efx.DeleteFilter(Filter);

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
            AL.Source(sourceId, ALSourcei.Buffer, 0);
            ALHelper.CheckError("Failed to free source from buffers.");

            lock (availableSourcesCollection)
            {
                if (inUseSourcesCollection.Remove(sourceId))
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
        void Activity_Paused(object sender, EventArgs e)
        {
            // Pause all currently playing sounds by pausing the mixer
            Alc.DevicePause(_device);
        }

        void Activity_Resumed(object sender, EventArgs e)
        {
            // Resume all sounds that were playing when the activity was paused
            Alc.DeviceResume(_device);
        }
#endif
    }
}
