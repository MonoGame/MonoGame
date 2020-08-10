using SharpDX.MediaFoundation;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Media
{
    internal class VideoSampleGrabber : SharpDX.CallbackBase, SampleGrabberSinkCallback
    {
        internal byte[] TextureData { get; private set; }

        public void OnProcessSample(Guid guidMajorMediaType, int dwSampleFlags, long llSampleTime, long llSampleDuration, IntPtr sampleBufferRef, int dwSampleSize)
        {
            if (TextureData == null || TextureData.Length != dwSampleSize)
                TextureData = new byte[dwSampleSize];

            Marshal.Copy(sampleBufferRef, TextureData, 0, dwSampleSize);
        }

        public void OnSetPresentationClock(PresentationClock presentationClockRef)
        {

        }

        public void OnShutdown()
        {

        }

        public void OnClockPause(long systemTime)
        {

        }

        public void OnClockRestart(long systemTime)
        {

        }

        public void OnClockSetRate(long systemTime, float flRate)
        {

        }

        public void OnClockStart(long systemTime, long llClockStartOffset)
        {

        }

        public void OnClockStop(long hnsSystemTime)
        {

        }
    }
}
