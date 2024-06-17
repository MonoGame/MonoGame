// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;

#if OPENAL
using MonoGame.OpenAL;
#if IOS || MONOMAC
using AudioToolbox;
using AudioUnit;
using AVFoundation;
#endif
#endif

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Provides microphones capture features.  
    /// </summary>
    public sealed partial class Microphone
    {
        private IntPtr _captureDevice = IntPtr.Zero;

        internal void CheckALCError(string operation)
        {
            AlcError error = Alc.GetErrorForDevice(_captureDevice);

            if (error == AlcError.NoError)
                return;

            string errorFmt = "OpenAL Error: {0}";

            throw new NoMicrophoneConnectedException(String.Format("{0} - {1}",
                            operation,
                            string.Format(errorFmt, error)));
        }

        internal static void PopulateCaptureDevices()
        {
            // clear microphones
            if (_allMicrophones != null)
                _allMicrophones.Clear();
            else
                _allMicrophones = new List<Microphone>();

            _default = null;

            // default device
            string defaultDevice = Alc.GetString(IntPtr.Zero, AlcGetString.CaptureDefaultDeviceSpecifier);

#if true //DESKTOPGL
            // enumerating capture devices
            IntPtr deviceList = Alc.alcGetString(IntPtr.Zero, (int)AlcGetString.CaptureDeviceSpecifier);

            // Marshal native UTF-8 character array to .NET string
            // The native string is a null-char separated list of known capture device specifiers ending with an empty string

            while (true)
            {
                var deviceIdentifier = InteropHelpers.Utf8ToString(deviceList);

                if (string.IsNullOrEmpty(deviceIdentifier))
                    break;

                var microphone = new Microphone(deviceIdentifier);
                _allMicrophones.Add(microphone);
                if (deviceIdentifier == defaultDevice)
                    _default = microphone;

                // increase the offset, add one extra for the terminator
                deviceList += deviceIdentifier.Length + 1;
            }
#else
            // Xamarin platforms don't provide an handle to alGetString that allow to marshal string arrays
            // so we're basically only adding the default microphone
            Microphone microphone = new Microphone(defaultDevice);
            _allMicrophones.Add(microphone);
            _default = microphone;
#endif
        }

        internal void PlatformStart()
        {
            if (_state == MicrophoneState.Started)
                return;

            _captureDevice = Alc.CaptureOpenDevice(
                Name,
                (uint)_sampleRate,
                ALFormat.Mono16,
                GetSampleSizeInBytes(_bufferDuration));

            CheckALCError("Failed to open capture device.");

            if (_captureDevice != IntPtr.Zero)
            {
                Alc.CaptureStart(_captureDevice);
                CheckALCError("Failed to start capture.");

                _state = MicrophoneState.Started;
            }
            else
            {
                throw new NoMicrophoneConnectedException("Failed to open capture device.");
            }
        }

        internal void PlatformStop()
        {
            if (_state == MicrophoneState.Started)
            {
                Alc.CaptureStop(_captureDevice);
                CheckALCError("Failed to stop capture.");
                Alc.CaptureCloseDevice(_captureDevice);
                CheckALCError("Failed to close capture device.");
                _captureDevice = IntPtr.Zero;
            }
            _state = MicrophoneState.Stopped;
        }

        internal int GetQueuedSampleCount()
        {
            if (_state == MicrophoneState.Stopped || BufferReady == null)
                return 0;

            int[] values = new int[1];
            Alc.GetInteger(_captureDevice, AlcGetInteger.CaptureSamples, 1, values);

            CheckALCError("Failed to query capture samples.");

            return values[0];
        }

        internal void Update()
        {
            if (GetQueuedSampleCount() > 0)
            {
                BufferReady.Invoke(this, EventArgs.Empty);
            }
        }

        internal int PlatformGetData(byte[] buffer, int offset, int count)
        {
            int sampleCount = GetQueuedSampleCount();
            sampleCount = Math.Min(count / 2, sampleCount); // 16bit adjust

            if (sampleCount > 0)
            {
                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    Alc.CaptureSamples(_captureDevice, handle.AddrOfPinnedObject() + offset, sampleCount);
                    CheckALCError("Failed to capture samples.");
                }
                finally
                {
                    handle.Free();
                }

                return sampleCount * 2; // 16bit adjust
            }

            return 0;
        }
    }
}
