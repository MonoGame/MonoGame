using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
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

	public sealed class OpenALSoundController : IDisposable
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
        private bool _bSoundAvailable = false;
        private Exception _SoundInitException; // Here to bubble back up to the developer
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
                return;
            }
            // We have hardware here and it is ready
            _bSoundAvailable = true;

			allSourcesArray = new int[MAX_NUMBER_OF_SOURCES];
            ALHelper.CheckError("Failed to generate sources.");
            AL.GenSources(allSourcesArray);
            ALHelper.CheckError("Failed to generate sources.");
            checkAlError();
            int numErr = 0;
            for(int i=0;i<allSourcesArray.Length;++i)
            {
                for(int k=0;k<10;++k)
                {
                    AL.Source(allSourcesArray[i], ALSource3f.Position, 0, 0, 0);
                }
                checkAlError();
                for (int kk = 0; kk < 20; ++kk)
                {
                    OpenTK.Vector3 posIn = new OpenTK.Vector3(0, 0, 0);
                    AL.Listener(OpenTK.Audio.OpenAL.ALListener3f.Position, ref posIn);
                    float x = 0.0f, y = 0.0f, z = 0.0f;
                    checkAlError();
                    AL.GetListener(ALListener3f.Position, out x, out y, out z);
                    if ((Math.Abs(x) - posIn.X) > 0.01f || (Math.Abs(y) - posIn.Y) > 0.01f || (Math.Abs(z) - posIn.Z) > 0.01f)
                    {
                        byte[] bx = BitConverter.GetBytes(x);
                        byte[] by = BitConverter.GetBytes(y);
                        byte[] bz = BitConverter.GetBytes(z);
                        numErr++;
                    }
                }
                checkAlError();
                Microsoft.Xna.Framework.Audio.SoundEffectInstance.checkSourceAndListener(allSourcesArray[i]);
            }


            Filter = 0;
#if SUPPORTS_EFX
            if (Efx.IsInitialized) {
                Filter = Efx.GenFilter ();
            }
#endif
            availableSourcesCollection = new List<int>(allSourcesArray);
			inUseSourcesCollection = new List<int>();


            checkRopoErr();

        }

        public void checkRopoErr()
        {
            if(availableSourcesCollection!=null)
            {
                lock (availableSourcesCollection)
                {
                    for (int i = 0; i < availableSourcesCollection.Count; ++i)
                    {
                        Microsoft.Xna.Framework.Audio.SoundEffectInstance.checkSourceAndListener(availableSourcesCollection[i]);
                    }
                }
            }
          
            if(inUseSourcesCollection!=null)
            {
                lock (inUseSourcesCollection)
                {
                    for (int i = 0; i < inUseSourcesCollection.Count; ++i)
                    {
                        Microsoft.Xna.Framework.Audio.SoundEffectInstance.checkSourceAndListener(inUseSourcesCollection[i]);
                    }
                }
            }
           
        }

        ~OpenALSoundController()
        {
            checkRopoErr();
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
            checkRopoErr();
            try
            {
                _device = Alc.OpenDevice(string.Empty);
#if DESKTOPGL
                EffectsExtension.device = _device;
#endif
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
                checkRopoErr();
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
                checkRopoErr();
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
                checkRopoErr();
                if (CheckALError("Could not create AL context"))
                {
                    CleanUpOpenAL();
                    return(false);
                }

                if (_context != NullContext)
                {
                    Alc.MakeContextCurrent(_context);
                    if (CheckALError("Could not make AL context current"))
                    {
                        checkRopoErr();
                        CleanUpOpenAL();
                        return(false);
                    }
                    SupportsADPCM = AL.IsExtensionPresent ("AL_SOFT_MSADPCM");
                    checkRopoErr();

                    {
                        int numErr = 0;
                        for(int i=0;i<20;++i)
                        {
                            /* byte[] b = { 1,2,3,4 }; // 666.666f
                             byte[] b1 = { 5, 6, 7, 8 }; // 666.666f
                             byte[] b2 = { 9, 10, 11, 12 }; // 666.666f
                             float myFloat = System.BitConverter.ToSingle(b,0);
                             float[] f = { myFloat, myFloat, myFloat };
                             // c# does not seem to corrupt floats in its own memory space, try the 3i integer method to see if bytes get corrupted also when passing ints.
                             //AL.Listener(OpenTK.Audio.OpenAL.ALListenerfv.Position,ref f);
                             int ii= System.BitConverter.ToInt32(b,0);
                             int ival = 511;
                             byte[] bb = System.BitConverter.GetBytes(ival);
                             //AL.Listener(OpenTK.Audio.OpenAL.ALListener3f.Position, myFloat, myFloat, myFloat);
                             */
                            //AL.Listener3i(OpenTK.Audio.OpenAL.ALListener3i.Position,    System.BitConverter.ToInt32(b, 0), System.BitConverter.ToInt32(b1, 0), System.BitConverter.ToInt32(b2, 0));
                            //AL.Listener(OpenTK.Audio.OpenAL.ALListener3f.Position, f[0],f[1],f[2]);


                            /*  byte[] b = System.BitConverter.GetBytes(11);
                              byte[] b1 = System.BitConverter.GetBytes(2233);
                              byte[] b2 = System.BitConverter.GetBytes(88776655);

                              AL.Listener3i(OpenTK.Audio.OpenAL.ALListener3i.Position, System.BitConverter.ToInt32(b, 0), System.BitConverter.ToInt32(b1, 0), System.BitConverter.ToInt32(b2, 0));
                              int ii1 = System.BitConverter.ToInt32(b, 0);
                              int ii2 = System.BitConverter.ToInt32(b1, 0);
                              int ii3 = System.BitConverter.ToInt32(b2, 0);*/
                            float f1 = 10.0f;
                            float f2 = 666.666f;
                            float f3 = 12345.0f;
                            AL.ALVec3 v3 = new AL.ALVec3();
                            v3.x = f1;
                            v3.y = f2;
                            v3.z = f3;
                            byte[] b = System.BitConverter.GetBytes(f1);
                            byte[] b1 = System.BitConverter.GetBytes(f2);
                            byte[] b2 = System.BitConverter.GetBytes(f3);
                            // PASSING FLOATS BY VALUE FAILS (EVEN IF PASS STRUCT BY VALUE, NOT SURE IF PROBLEM IS IN STRUCT),
                            // BUT SEEMS THAT IF FLOATS PASSED BY VALUE IT FAILS.
                            // PASSING INDIVIDUAL FLOATS BY POINTERS (ref f1, ref2, ref f3) WORKS.
                            AL.Listener(OpenTK.Audio.OpenAL.ALListener3f.Position,    System.BitConverter.ToSingle(b, 0), System.BitConverter.ToSingle(b1, 0), System.BitConverter.ToSingle(b2, 0));
                            // AL.Listener3fVecVal(OpenTK.Audio.OpenAL.ALListener3f.Position, v3);
                            //AL.Listener3FFloatPtrIndividual(OpenTK.Audio.OpenAL.ALListener3f.Position, ref f1, ref f2, ref f3); // TODO: NEEDS FIXED KEYWORD?
                             float ff1 = System.BitConverter.ToSingle(b, 0);
                            float ff2 = System.BitConverter.ToSingle(b1, 0);
                            float ff3 = System.BitConverter.ToSingle(b2, 0);
                            float myFloat = System.BitConverter.ToSingle(b, 0);
                            OpenTK.Vector3 posIn = new OpenTK.Vector3(myFloat, myFloat, myFloat);

                        

                            float x = 0.0f, y = 0.0f, z = 0.0f;
                            AL.GetListener(ALListener3f.Position, out x, out y, out z);
                            if ((Math.Abs(x) - posIn.X)>0.01f || (Math.Abs(y) - posIn.Y) > 0.01f || (Math.Abs(z) - posIn.Z) > 0.01f)
                            {
                                byte[] bx=BitConverter.GetBytes(x);
                                byte[] by = BitConverter.GetBytes(y);
                                byte[] bz = BitConverter.GetBytes(z);
                                numErr++;
                            }
                        }
                        if(numErr>0)
                        {
                          //  throw new Exception("error: " + numErr);
                        }
                        /*float x = 0.0f, y = 0.0f, z = 0.0f;
                        AL.GetListener(ALListener3f.Position, out x, out y, out z);
                        if (Math.Abs(x) > 1000 || Math.Abs(y) > 1000 || Math.Abs(z) > 1000)
                        {
                            throw new Exception("fail: ");
                        }

                        Microsoft.Xna.Framework.Audio.OpenALSoundController.checkAlError();
                        OpenTK.Vector3 posIn = new OpenTK.Vector3(0, 0, 0);
                        AL.Listener(OpenTK.Audio.OpenAL.ALListener3f.Position, ref posIn);
                        Microsoft.Xna.Framework.Audio.OpenALSoundController.checkAlError();

                        OpenTK.Vector3 v = new OpenTK.Vector3(0, 0, 0); // TODO: THIS SO FLOATS NOT DOUBLES?
                        AL.GetListener(ALListener3f.Position, out v);
                        if (Math.Abs(v.X) > 1000 || Math.Abs(v.Y) > 1000 || Math.Abs(v.Z) > 1000)
                        {
                            throw new Exception("fail: ");
                        }
                        Microsoft.Xna.Framework.Audio.OpenALSoundController.checkAlError();*/
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
                _instance.checkRopoErr();

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
                _instance.checkRopoErr();
                _instance.Dispose();
                _instance = null;
            }
        }


        public static void checkAlError()
        {
            if(_instance!=null)
            {
                AlcError e2 = Alc.GetError(_instance._device);

                if (e2 != AlcError.NoError)
                {
                    Debug.WriteLine("ALError: " + e2);
                    throw new Exception("AL Error: " + e2.ToString());
                }
            }

            ALError e = AL.GetError();
            if (e != ALError.NoError)
            {
                Debug.WriteLine("ALError: " + e);
                throw new Exception("AL Error: " + e.ToString());
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
            checkRopoErr();

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
            checkRopoErr();
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

            _bSoundAvailable = false;
        }

        /// <summary>
        /// Dispose of the OpenALSoundCOntroller.
        /// </summary>
        public void Dispose()
        {
            checkRopoErr();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the OpenALSoundCOntroller.
        /// </summary>
        /// <param name="disposing">If true, the managed resources are to be disposed.</param>
		void Dispose(bool disposing)
		{
            checkRopoErr();
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_bSoundAvailable)
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
                        CleanUpOpenAL();
                    }
                }
                _isDisposed = true;
            }
            checkRopoErr();
        }

        /// <summary>
        /// Reserves a sound buffer and return its identifier. If there are no available sources
        /// or the controller was not able to setup the hardware then an
        /// <see cref="InstancePlayLimitException"/> is thrown.
        /// </summary>
        /// <returns>The source number of the reserved sound buffer.</returns>
		public int ReserveSource()
		{
            if (!CheckInitState())
            {
                throw new InstancePlayLimitException();
            }
            checkRopoErr();
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
                checkRopoErr();
            }

            checkRopoErr();

            return sourceNumber;
		}

        public void RecycleSource(int sourceId)
		{
            if (!CheckInitState())
            {
                return;
            }

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
            checkRopoErr();
        }

        /// <summary>
        /// Checks if the AL controller was initialized properly. If there was an
        /// exception thrown during the OpenAL init, then that exception is thrown
        /// inside of NoAudioHardwareException.
        /// </summary>
        /// <returns>True if the controller was initialized, false if not.</returns>
        internal bool CheckInitState()
        {
            checkRopoErr();
            if (!_bSoundAvailable)
            {
                if (_SoundInitException != null)
                {
                    Exception e = _SoundInitException;
                    _SoundInitException = null;
                    throw new NoAudioHardwareException("No audio hardware available.", e);
                }
                checkRopoErr();
                return (false);
            }
            checkRopoErr();
            return (true);
        }

        public double SourceCurrentPosition (int sourceId)
		{
            checkRopoErr();
            if (!CheckInitState())
            {
                return(0.0);
            }
            int pos;
			AL.GetSource (sourceId, ALGetSourcei.SampleOffset, out pos);
            ALHelper.CheckError("Failed to set source offset.");
            checkRopoErr();
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
            checkRopoErr();
            // Pause all currently playing sounds. The internal pause count in OALSoundBuffer
            // will take care of sounds that were already paused.
            //            lock (playingSourcesCollection)
            //            {
            //                foreach (var source in playingSourcesCollection)
            //                    source.Pause();
            //            }
            alcDevicePauseSOFT(_device);
            checkRopoErr();
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
            checkRopoErr();
            alcDeviceResumeSOFT(_device);
            checkRopoErr();
        }
#endif

#if MONOMAC
		public const string OpenALLibrary = "/System/Library/Frameworks/OpenAL.framework/OpenAL";

		[DllImport(OpenALLibrary, EntryPoint = "alcMacOSXMixerOutputRate")]
		static extern void alcMacOSXMixerOutputRate (double rate); // caution
#endif
    }
}

