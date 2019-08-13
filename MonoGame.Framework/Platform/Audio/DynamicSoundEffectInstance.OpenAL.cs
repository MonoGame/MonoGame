// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class DynamicSoundEffectInstance : SoundEffectInstance
    {
        private Queue<OALSoundBuffer> _queuedBuffers;
        private ALFormat _format;

        private void PlatformCreate()
        {
            _format = _channels == AudioChannels.Mono ? ALFormat.Mono16 : ALFormat.Stereo16;
            InitializeSound();

            SourceId = controller.ReserveSource();
            HasSourceId = true;

            _queuedBuffers = new Queue<OALSoundBuffer>();
        }

        private int PlatformGetPendingBufferCount()
        {
            return _queuedBuffers.Count;
        }

        private void PlatformPlay()
        {
            AL.GetError();

            // Ensure that the source is not looped (due to source recycling)
            AL.Source(SourceId, ALSourceb.Looping, false);
            ALHelper.CheckError("Failed to set source loop state.");

            AL.SourcePlay(SourceId);
            ALHelper.CheckError("Failed to play the source.");
        }

        private void PlatformPause()
        {
            AL.GetError();
            AL.SourcePause(SourceId);
            ALHelper.CheckError("Failed to pause the source.");
        }

        private void PlatformResume()
        {
            AL.GetError();
            AL.SourcePlay(SourceId);
            ALHelper.CheckError("Failed to play the source.");
        }

        private void PlatformStop()
        {
            AL.GetError();
            AL.SourceStop(SourceId);
            ALHelper.CheckError("Failed to stop the source.");

            // Remove all queued buffers
            AL.Source(SourceId, ALSourcei.Buffer, 0);
            while (_queuedBuffers.Count > 0)
            {
                var buffer = _queuedBuffers.Dequeue();
                buffer.Dispose();
            }
        }

        private unsafe void PlatformSubmitBuffer(byte[] buffer, int offset, int count)
        {
            fixed (byte* dataPtr = buffer)
            {
                AlSubmitBuffer(dataPtr, offset, count, _format);
            }
        }

        private unsafe void AlSubmitBuffer(byte* dataPtr, int offset, int count, ALFormat format)
        {
            // Get a buffer
            OALSoundBuffer oalBuffer = new OALSoundBuffer();

            // Bind the data
            oalBuffer.BindDataBuffer(dataPtr + offset, format, count, _sampleRate);

            // Queue the buffer
            AL.SourceQueueBuffer(SourceId, oalBuffer.OpenALDataBuffer);
            ALHelper.CheckError();
            _queuedBuffers.Enqueue(oalBuffer);

            // If the source has run out of buffers, restart it
            var sourceState = AL.GetSourceState(SourceId);
            if (_state == SoundState.Playing && sourceState == ALSourceState.Stopped)
            {
                AL.SourcePlay(SourceId);
                ALHelper.CheckError("Failed to resume source playback.");
            }
        }

        internal unsafe void SubmitFloatBuffer(float[] buffer, int sampleOffset, int sampleCount)
        {
            var format = _channels == AudioChannels.Mono ? ALFormat.MonoFloat32 : ALFormat.StereoFloat32;
            fixed (float* dataPtr = buffer)
            {
                AlSubmitBuffer((byte*) dataPtr, sampleOffset, sampleCount, format);
            }
        }

        private void PlatformDispose(bool disposing)
        {
            // Stop the source and bind null buffer so that it can be recycled
            AL.GetError();
            if (AL.IsSource(SourceId))
            {
                AL.SourceStop(SourceId);
                AL.Source(SourceId, ALSourcei.Buffer, 0);
                ALHelper.CheckError("Failed to stop the source.");
                controller.RecycleSource(SourceId);
            }
            
            if (disposing)
            {
                while (_queuedBuffers.Count > 0)
                {
                    var buffer = _queuedBuffers.Dequeue();
                    buffer.Dispose();
                }

                DynamicSoundEffectInstanceManager.RemoveInstance(this);
            }
        }

        private void PlatformUpdateQueue()
        {
            // Get the completed buffers
            AL.GetError();
            int numBuffers;
            AL.GetSource(SourceId, ALGetSourcei.BuffersProcessed, out numBuffers);
            ALHelper.CheckError("Failed to get processed buffer count.");

            // Unqueue them
            if (numBuffers > 0)
            {
                AL.SourceUnqueueBuffers(SourceId, numBuffers);
                ALHelper.CheckError("Failed to unqueue buffers.");
                for (int i = 0; i < numBuffers; i++)
                {
                    var buffer = _queuedBuffers.Dequeue();
                    buffer.Dispose();
                }
            }

            // Raise the event for each removed buffer, if needed
            for (int i = 0; i < numBuffers; i++)
                CheckBufferCount();
        }
    }
}
