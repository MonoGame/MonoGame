// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using SharpDX.Multimedia;
using SharpDX.Mathematics.Interop;

namespace Microsoft.Xna.Framework.Audio
{
    public partial class SoundEffectInstance : IDisposable
    {
        private static float[] _defaultChannelAzimuths = new float[] { 0f, 0f };

        internal SourceVoice _voice;
        internal WaveFormat _format;

        private SharpDX.XAudio2.Fx.Reverb _reverb;

        private static readonly float[] _outputMatrix = new float[16];

        private float _reverbMix;

        private bool _paused;
        private bool _loop;

        private void PlatformInitialize(byte[] buffer, int sampleRate, int channels)
        {
            throw new NotImplementedException();
        }

        private void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
            // If we have no voice then nothing to do.
            if (_voice == null || SoundEffect.MasterVoice == null)
                return;

            // Convert from XNA Emitter to a SharpDX Emitter
            var e = ToDXEmitter(emitter);
            e.CurveDistanceScaler = SoundEffect.DistanceScale;
            e.DopplerScaler = SoundEffect.DopplerScale;
            e.ChannelCount = _effect._format.Channels;
            
            //stereo channel
            if (e.ChannelCount > 1)
            {
                e.ChannelRadius = 0;
                e.ChannelAzimuths = _defaultChannelAzimuths;
             }

            // Convert from XNA Listener to a SharpDX Listener
            var l = ToDXListener(listener);

            // Number of channels in the sound being played.
            // Not actually sure if XNA supported 3D attenuation of sterio sounds, but X3DAudio does.
            var srcChannelCount = _effect._format.Channels;

            // Number of output channels.
            var dstChannelCount = SoundEffect.MasterVoice.VoiceDetails.InputChannelCount;

            // XNA supports distance attenuation and doppler.            
            var dpsSettings = SoundEffect.Device3D.Calculate(l, e, CalculateFlags.Matrix | CalculateFlags.Doppler, srcChannelCount, dstChannelCount);

            // Apply Volume settings (from distance attenuation) ...
            _voice.SetOutputMatrix(SoundEffect.MasterVoice, srcChannelCount, dstChannelCount, dpsSettings.MatrixCoefficients, 0);

            // Apply Pitch settings (from doppler) ...
            _voice.SetFrequencyRatio(dpsSettings.DopplerFactor);
        }

        private Emitter _dxEmitter;
        private Listener _dxListener;

        private Emitter ToDXEmitter(AudioEmitter emitter)
        {
            // Pulling out Vector properties for efficiency.
            var pos = emitter.Position;
            var vel = emitter.Velocity;
            var forward = emitter.Forward;
            var up = emitter.Up;

            // From MSDN:
            //  X3DAudio uses a left-handed Cartesian coordinate system, 
            //  with values on the x-axis increasing from left to right, on the y-axis from bottom to top, 
            //  and on the z-axis from near to far. 
            //  Azimuths are measured clockwise from a given reference direction. 
            //
            // From MSDN:
            //  The XNA Framework uses a right-handed coordinate system, 
            //  with the positive z-axis pointing toward the observer when the positive x-axis is pointing to the right, 
            //  and the positive y-axis is pointing up. 
            //
            // Programmer Notes:         
            //  According to this description the z-axis (forward vector) is inverted between these two coordinate systems.
            //  Therefore, we need to negate the z component of any position/directions/velocity values.

            forward.Z *= -1.0f;
            up.Z *= -1.0f;
            pos.Z *= -1.0f;
            vel.Z *= -1.0f;

            if (_dxEmitter == null)
                _dxEmitter = new Emitter();

            _dxEmitter.Position = new RawVector3(pos.X, pos.Y, pos.Z);
            _dxEmitter.Velocity = new RawVector3(vel.X, vel.Y, vel.Z);
            _dxEmitter.OrientFront = new RawVector3(forward.X, forward.Y, forward.Z);
            _dxEmitter.OrientTop = new RawVector3(up.X, up.Y, up.Z);
            _dxEmitter.DopplerScaler = emitter.DopplerScale;
            return _dxEmitter;
        }

        private Listener ToDXListener(AudioListener listener)
        {
            // Pulling out Vector properties for efficiency.
            var pos = listener.Position;
            var vel = listener.Velocity;
            var forward = listener.Forward;
            var up = listener.Up;

            // From MSDN:
            //  X3DAudio uses a left-handed Cartesian coordinate system, 
            //  with values on the x-axis increasing from left to right, on the y-axis from bottom to top, 
            //  and on the z-axis from near to far. 
            //  Azimuths are measured clockwise from a given reference direction. 
            //
            // From MSDN:
            //  The XNA Framework uses a right-handed coordinate system, 
            //  with the positive z-axis pointing toward the observer when the positive x-axis is pointing to the right, 
            //  and the positive y-axis is pointing up. 
            //
            // Programmer Notes:         
            //  According to this description the z-axis (forward vector) is inverted between these two coordinate systems.
            //  Therefore, we need to negate the z component of any position/directions/velocity values.

            forward.Z *= -1.0f;
            up.Z *= -1.0f;
            pos.Z *= -1.0f;
            vel.Z *= -1.0f;

            if (_dxListener == null)
                _dxListener = new Listener();

            _dxListener.Position = new RawVector3 { X = pos.X, Y = pos.Y, Z = pos.Z };
            _dxListener.Velocity = new RawVector3 { X = vel.X, Y = vel.Y, Z = vel.Z };
            _dxListener.OrientFront = new RawVector3 { X = forward.X, Y = forward.Y, Z = forward.Z };
            _dxListener.OrientTop = new RawVector3 { X = up.X, Y = up.Y, Z = up.Z };
            return _dxListener;
        }

        private void PlatformPause()
        {
            if (_voice != null && SoundEffect.MasterVoice != null)
                _voice.Stop();
            _paused = true;
        }

        private void PlatformPlay()
        {
            if (_voice != null && SoundEffect.MasterVoice != null)
            {
                // Choose the correct buffer depending on if we are looped.            
                var buffer = _loop ? _effect._loopedBuffer : _effect._buffer;

                if (_voice.State.BuffersQueued > 0)
                {
                    _voice.Stop();
                    _voice.FlushSourceBuffers();
                }

                _voice.SubmitSourceBuffer(buffer, null);
                _voice.Start();
            }

            _paused = false;
        }

        private void PlatformResume()
        {
            if (_voice != null && SoundEffect.MasterVoice != null)
            {
                // Restart the sound if (and only if) it stopped playing
                if (!_loop)
                {
                    if (_voice.State.BuffersQueued == 0)
                    {
                        _voice.Stop();
                        _voice.FlushSourceBuffers();
                        _voice.SubmitSourceBuffer(_effect._buffer, null);
                    }
                }
                _voice.Start();
            }
            _paused = false;
        }

        private void PlatformStop(bool immediate)
        {
            if (_voice != null && SoundEffect.MasterVoice != null)
            {
                if (immediate)
                {
                    _voice.Stop(0);
                    _voice.FlushSourceBuffers();
                }
                else
                {
                    if (_loop)
                        _voice.ExitLoop();
                    else
                        _voice.Stop((int)PlayFlags.Tails);
                }
            }
            
            _paused = false;
        }

        private void PlatformSetIsLooped(bool value)
        {
            _loop = value;
        }

        private bool PlatformGetIsLooped()
        {
            return _loop;
        }

        private void PlatformSetPan(float value)
        {
            // According to XNA documentation:
            // "Panning, ranging from -1.0f (full left) to 1.0f (full right). 0.0f is centered."
            _pan = MathHelper.Clamp(value, -1.0f, 1.0f);

            // If we have no voice then nothing more to do.
            if (_voice == null || SoundEffect.MasterVoice == null)
                return;

            UpdateOutputMatrix();
        }

        internal void UpdateOutputMatrix()
        {
            var srcChannelCount = _voice.VoiceDetails.InputChannelCount;
            var dstChannelCount = SoundEffect.MasterVoice.VoiceDetails.InputChannelCount;

            // Set the pan on the correct channels based on the reverb mix.
            if (!(_reverbMix > 0.0f))
                _voice.SetOutputMatrix(srcChannelCount, dstChannelCount, CalculateOutputMatrix(_pan, 1.0f, srcChannelCount));
            else
            {
                _voice.SetOutputMatrix(SoundEffect.ReverbVoice, srcChannelCount, dstChannelCount, CalculateOutputMatrix(_pan, _reverbMix, srcChannelCount));
                _voice.SetOutputMatrix(SoundEffect.MasterVoice, srcChannelCount, dstChannelCount, CalculateOutputMatrix(_pan, 1.0f - Math.Min(_reverbMix, 1.0f), srcChannelCount));
            }
        }

        internal static float[] CalculateOutputMatrix(float pan, float scale, int inputChannels)
        {
            // XNA only ever outputs to the front left/right speakers (channels 0 and 1)
            // Assumes there are at least 2 speaker channels to output to

            // Clear all the channels.
            var outputMatrix = _outputMatrix;
            Array.Clear(outputMatrix, 0, outputMatrix.Length);

            if (inputChannels == 1) // Mono source
            {
                // Left/Right output levels:
                //   Pan -1.0: L = 1.0, R = 0.0
                //   Pan  0.0: L = 1.0, R = 1.0
                //   Pan +1.0: L = 0.0, R = 1.0
                outputMatrix[0] = (pan > 0f) ? ((1f - pan) * scale) : scale; // Front-left output
                outputMatrix[1] = (pan < 0f) ? ((1f + pan) * scale) : scale; // Front-right output
            }
            else if (inputChannels == 2) // Stereo source
            {
                // Left/Right input (Li/Ri) mix for Left/Right outputs (Lo/Ro):
                //   Pan -1.0: Lo = 0.5Li + 0.5Ri, Ro = 0.0Li + 0.0Ri
                //   Pan  0.0: Lo = 1.0Li + 0.0Ri, Ro = 0.0Li + 1.0Ri
                //   Pan +1.0: Lo = 0.0Li + 0.0Ri, Ro = 0.5Li + 0.5Ri
                if (pan <= 0f)
                {
                    outputMatrix[0] = (1f + pan * 0.5f) * scale; // Front-left output, Left input
                    outputMatrix[1] = (-pan * 0.5f) * scale; // Front-left output, Right input
                    outputMatrix[2] = 0f; // Front-right output, Left input
                    outputMatrix[3] = (1f + pan) * scale; // Front-right output, Right input
                }
                else
                {
                    outputMatrix[0] = (1f - pan) * scale; // Front-left output, Left input
                    outputMatrix[1] = 0f; // Front-left output, Right input
                    outputMatrix[2] = (pan * 0.5f) * scale; // Front-right output, Left input
                    outputMatrix[3] = (1f - pan * 0.5f) * scale; // Front-right output, Right input
                }
            }

            return outputMatrix;
        }

        private void PlatformSetPitch(float value)
        {
            _pitch = value;

            if (_voice == null || SoundEffect.MasterVoice == null)
                return;

            // NOTE: This is copy of what XAudio2.SemitonesToFrequencyRatio() does
            // which avoids the native call and is actually more accurate.
             var pitch = MathF.Pow(2.0f, value);
             _voice.SetFrequencyRatio(pitch);
        }

        private SoundState PlatformGetState()
        {
            // If no voice or no buffers queued the sound is stopped.
            if (_voice == null || SoundEffect.MasterVoice == null || _voice.State.BuffersQueued == 0)
                return SoundState.Stopped;

            // Because XAudio2 does not actually provide if a SourceVoice is Started / Stopped
            // we have to save the "paused" state ourself.
            if (_paused)
                return SoundState.Paused;

            return SoundState.Playing;
        }

        private void PlatformSetVolume(float value)
        {
            if (_voice != null && SoundEffect.MasterVoice != null)
                _voice.SetVolume(value, XAudio2.CommitNow);
        }

        internal void PlatformSetReverbMix(float mix)
        {
            // At least for XACT we can't go over 2x the volume on the mix.
            _reverbMix = MathHelper.Clamp(mix, 0, 2);

            // If we have no voice then nothing more to do.
            if (_voice == null || SoundEffect.MasterVoice == null)
                return;

            if (!(_reverbMix > 0.0f))
                _voice.SetOutputVoices(new VoiceSendDescriptor(SoundEffect.MasterVoice));
            else
            {
                _voice.SetOutputVoices( new VoiceSendDescriptor(SoundEffect.ReverbVoice), 
                                        new VoiceSendDescriptor(SoundEffect.MasterVoice));
            }

            UpdateOutputMatrix();
        }

        internal void PlatformSetFilter(FilterMode mode, float filterQ, float frequency)
        {
            if (_voice == null || SoundEffect.MasterVoice == null)
                return;

            var filter = new FilterParameters 
            {
                Frequency = XAudio2.CutoffFrequencyToRadians(frequency, _voice.VoiceDetails.InputSampleRate), 
                OneOverQ = 1.0f / filterQ, 
                Type = (FilterType)mode 
            };
            _voice.SetFilterParameters(filter);
        }

        internal void PlatformClearFilter()
        {
            if (_voice == null || SoundEffect.MasterVoice == null)
                return;

            var filter = new FilterParameters { Frequency = 1.0f, OneOverQ = 1.0f, Type = FilterType.LowPassFilter };
            _voice.SetFilterParameters(filter);            
        }

        private void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
                if (_reverb != null)
                    _reverb.Dispose();

                if (_voice != null && SoundEffect.MasterVoice != null)
                {
                    _voice.DestroyVoice();
                    _voice.Dispose();
                }
            }
            _voice = null;
            _effect = null;
            _reverb = null;
        }
    }
}
