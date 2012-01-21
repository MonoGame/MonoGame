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
using System.Runtime.InteropServices;

using MonoTouch.AVFoundation;
using MonoTouch.AudioToolbox;
using MonoTouch.CoreMedia;
using MonoTouch.Foundation;

namespace Microsoft.Xna.Framework.Media
{
    public partial class VideoPlayer
    {
        private class StreamingAudioPlayer : IDisposable
        {
            private const int BufferSize = 32 * 1024;
            private const int BufferCount = 3;

            private OutputAudioQueue _audioQueue;

            private AVAssetReader _reader;
            private AudioStreamBasicDescription _audioFormat;
            private AVAssetReaderAudioMixOutput _audioOutput;

            private CMSampleBuffer _currentInputSampleBuffer;
            private CMBlockBuffer _currentInputBuffer;
            private int _currentInputBufferOffset;

            private IntPtr _intermediateBuffer;

            public StreamingAudioPlayer(AVAssetReader reader, AVAssetTrack[] audioTracks)
            {
                if (reader == null)
                    throw new ArgumentNullException("reader");
                if (audioTracks == null)
                    throw new ArgumentNullException("audioTracks");
                if (audioTracks.Length == 0)
                    throw new ArgumentException("audioTracks must not be empty");

                _reader = reader;
                _audioOutput = new AVAssetReaderAudioMixOutput(audioTracks, null);
                _reader.AddOutput(_audioOutput);

                _intermediateBuffer = Marshal.AllocHGlobal(BufferSize);
            }

            ~StreamingAudioPlayer()
            {
                Dispose(false);
            }

            private float _volume = 1f;
            public float Volume
            {
                get { return _volume; }
                set
                {
                    if (_volume != value)
                    {
                        _volume = value;
                        if (_audioQueue != null)
                            // FIXME: Translate iOS 0->1 = silence->unity to
                            //        XNA's 0->1 = -96dB->unity
                            _audioQueue.Volume = value;
                    }
                }
            }

            public void Pause()
            {
                _audioQueue.Pause();
            }

            public void Play()
            {
                InitializeAudioQueue();
                _audioQueue.Start();
            }

            public void Resume()
            {
                _audioQueue.Start();
            }

            private void InitializeAudioQueue()
            {
                AudioStreamBasicDescriptionExtensions.FillOutForLinearPCM(
                    ref _audioFormat,
                    44100, 2, 16, 16,
                    false, false, false);

                _audioQueue = new OutputAudioQueue(_audioFormat);
                _audioQueue.Volume = _volume;
                _audioQueue.OutputCompleted += AudioQueue_OutputCompleted;

                // Prime the output buffers with the first round of data.
                for (int index = 0; index < BufferCount; index++)
                {
                    IntPtr bufferPtr;
                    _audioQueue.AllocateBuffer(BufferSize, out bufferPtr);
                    FillOutputBuffer(bufferPtr);
                }
            }

            private void AudioQueue_OutputCompleted(object sender, OutputCompletedEventArgs e)
            {
                using (var pool = new NSAutoreleasePool())
                {
                    FillOutputBuffer(e.IntPtrBuffer);
                }
            }

            private void FillOutputBuffer(IntPtr bufferPtr)
            {
                // Fill the given output buffer by copying input data into the
                // intermediate buffer, then copying however much data was read
                // from the input to the output buffer.  This continues until
                // no more input data can be read or until the output buffer is
                // full.
                int bufferOffset = 0;
                int intermediateBytesFilled;
                do
                {
                    intermediateBytesFilled = FillIntermediateBuffer(BufferSize - bufferOffset);
                    AudioQueue.FillAudioData(bufferPtr, bufferOffset, _intermediateBuffer, 0, intermediateBytesFilled);
                    bufferOffset += intermediateBytesFilled;
                }
                while (intermediateBytesFilled > 0 && bufferOffset < BufferSize);

                // If no data was added to the buffer, then the source is
                // exhausted.  Stop the queue to prevent any further attempts
                // to play the non-existent audio.
                if (bufferOffset == 0)
                {
                    _audioQueue.Stop(true);
                    return;
                }

                _audioQueue.EnqueueBuffer(bufferPtr, bufferOffset, null);
            }

            private int FillIntermediateBuffer(int count)
            {
                int bytesFilled = 0;

                // Fill the intermediate buffer with up to count bytes.  The
                // intermediate buffer may not be aligned perfectly to incoming
                // samples (larger, smaller or shifted), so input buffers are
                // processed in a loop until they are exhausted or 'count' is
                // satisfied.
                CMBlockBuffer dataBuffer;
                int dataOffset;
                while (bytesFilled < count && TryGetCurrentInputBuffer(out dataBuffer, out dataOffset))
                {
                    int bytesToCopy = Math.Min((int)dataBuffer.DataLength - dataOffset, count - bytesFilled);
                    IntPtr intermediateBufferPosition = new IntPtr(_intermediateBuffer.ToInt64() + bytesFilled);
                    dataBuffer.CopyDataBytes((uint)dataOffset, (uint)bytesToCopy, intermediateBufferPosition);
                    MarkCurrentDataBufferReadTo(dataOffset + bytesToCopy);

                    bytesFilled += bytesToCopy;
                }

                return bytesFilled;
            }

            private bool TryGetCurrentInputBuffer(
                out CMBlockBuffer inputBuffer,
                out int inputOffset)
            {
                if (_currentInputBuffer == null && _reader.Status == AVAssetReaderStatus.Reading)
                {
                    try
                    {
                        _currentInputSampleBuffer = _audioOutput.CopyNextSampleBuffer();
                        _currentInputBuffer = _currentInputSampleBuffer.GetDataBuffer();
                        _currentInputBufferOffset = 0;
                    }
                    catch (MonoTouchException)
                    {
                        // HACK: Despite checking _reader.Status (and despite it
                        //       being 'Reading'), CopyNextSampleBuffer throws
                        //       NSInternalInconsistencyException at the end of
                        //       the stream.  From what the docs say, it should
                        //       return null, rather than throwing, but null is
                        //       never returned. Until the root cause can be
                        //       found, catching the exception and doing nothing
                        //       will have to suffice.
                    }
                }

                if (_currentInputBuffer == null)
                {
                    _currentInputSampleBuffer = null;
                    _currentInputBufferOffset = 0;
                }

                inputBuffer = _currentInputBuffer;
                inputOffset = _currentInputBufferOffset;

                return inputBuffer != null;
            }

            private void MarkCurrentDataBufferReadTo(int offset)
            {
                if (offset < _currentInputBuffer.DataLength)
                {
                    _currentInputBufferOffset = offset;
                    return;
                }

                _currentInputBuffer.Dispose();
                _currentInputBuffer = null;
                _currentInputSampleBuffer.Dispose();
                _currentInputSampleBuffer = null;
                _currentInputBufferOffset = 0;
            }

            #region IDisposable Members

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_audioOutput != null)
                    {
                        _audioOutput.Dispose();
                        _audioOutput = null;
                    }

                    if (_audioQueue != null)
                    {
                        _audioQueue.Dispose();
                        _audioQueue = null;
                    }

                    if (_currentInputBuffer != null)
                    {
                        _currentInputBuffer.Dispose();
                        _currentInputBuffer = null;
                    }

                    if (_currentInputSampleBuffer != null)
                    {
                        _currentInputSampleBuffer.Dispose();
                        _currentInputSampleBuffer = null;
                    }
                }

                Marshal.FreeHGlobal(_intermediateBuffer);
                _intermediateBuffer = IntPtr.Zero;
            }

            #endregion IDisposable Members
        }
    }

    static class AudioStreamBasicDescriptionExtensions
    {
        public static void FillOutForLinearPCM(
            ref AudioStreamBasicDescription absd,
            double sampleRate,
            int channelsPerFrame,
            int validBitsPerChannel,
            int totalBitsPerChannel,
            bool isFloat,
            bool isBigEndian,
            bool isNonInterleaved)
        {
            absd.SampleRate = sampleRate;
            absd.Format = AudioFormatType.LinearPCM;
            absd.FormatFlags = CalculateLPCMFlags(
                validBitsPerChannel,
                totalBitsPerChannel,
                isFloat,
                isBigEndian,
                isNonInterleaved);
            absd.BytesPerPacket = (isNonInterleaved ? 1 : channelsPerFrame) * (totalBitsPerChannel / 8);
            absd.FramesPerPacket = 1;
            absd.BytesPerFrame = (isNonInterleaved ? 1 : channelsPerFrame) * (totalBitsPerChannel / 8);
            absd.ChannelsPerFrame = channelsPerFrame;
            absd.BitsPerChannel = validBitsPerChannel;
        }

        static AudioFormatFlags CalculateLPCMFlags(
            int validBitsPerChannel,
            int totalBitsPerChannel,
            bool isFloat,
            bool isBigEndian,
            bool isNonInterleaved)
        {
            return
                (isFloat ? AudioFormatFlags.IsFloat : AudioFormatFlags.IsSignedInteger) |
                (isBigEndian ? AudioFormatFlags.IsBigEndian : 0) |
                ((validBitsPerChannel == totalBitsPerChannel) ? AudioFormatFlags.IsPacked : AudioFormatFlags.IsAlignedHigh) |
                (isNonInterleaved ? AudioFormatFlags.IsNonInterleaved : 0);
        }
    }
}
