﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using SharpDX.Multimedia;

namespace Microsoft.Xna.Framework.Audio
{
    public partial class SoundEffectInstance : IDisposable
    {
        private static float[] _defaultChannelAzimuths = new float[] { 0f, 0f };

        internal SourceVoice _voice;
        internal WaveFormat _format;

        private SharpDX.XAudio2.Fx.Reverb _reverb;

        private static readonly float[] _panMatrix = new float[8];

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
            if (_voice == null)
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

        private SharpDX.X3DAudio.Emitter _dxEmitter;
        private SharpDX.X3DAudio.Listener _dxListener;

        private SharpDX.X3DAudio.Emitter ToDXEmitter(AudioEmitter emitter)
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
            //  Therefore, we need to negate the z component of any position/velocity values, and negate any forward vectors.

            forward *= -1.0f;
            pos.Z *= -1.0f;
            vel.Z *= -1.0f;

            if (_dxEmitter == null)
                _dxEmitter = new Emitter();

#if WINDOWS_UAP
            _dxEmitter.Position = new SharpDX.Mathematics.Interop.RawVector3 { X = pos.X, Y = pos.Y, Z = pos.Z };
            _dxEmitter.Velocity =  new SharpDX.Mathematics.Interop.RawVector3 { X = vel.X, Y = vel.Y, Z = vel.Z };
            _dxEmitter.OrientFront = new SharpDX.Mathematics.Interop.RawVector3 { X = forward.X, Y = forward.Y, Z = forward.Z };
            _dxEmitter.OrientTop = new SharpDX.Mathematics.Interop.RawVector3 { X = up.X, Y = up.Y, Z = up.Z };
            
#else
            _dxEmitter.Position = new SharpDX.Vector3(pos.X, pos.Y, pos.Z);
            _dxEmitter.Velocity = new SharpDX.Vector3(vel.X, vel.Y, vel.Z);
            _dxEmitter.OrientFront = new SharpDX.Vector3(forward.X, forward.Y, forward.Z);
            _dxEmitter.OrientTop = new SharpDX.Vector3(up.X, up.Y, up.Z);
            _dxEmitter.DopplerScaler = emitter.DopplerScale;
#endif
            return _dxEmitter;
        }

        private SharpDX.X3DAudio.Listener ToDXListener(AudioListener listener)
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
            //  Therefore, we need to negate the z component of any position/velocity values, and negate any forward vectors.

            forward *= -1.0f;
            pos.Z *= -1.0f;
            vel.Z *= -1.0f;

            if (_dxListener == null)
                _dxListener = new Listener();

#if WINDOWS_UAP
            _dxListener.Position = new SharpDX.Mathematics.Interop.RawVector3 { X = pos.X, Y = pos.Y, Z = pos.Z };
            _dxListener.Velocity = new SharpDX.Mathematics.Interop.RawVector3 { X = vel.X, Y = vel.Y, Z = vel.Z };
            _dxListener.OrientFront = new SharpDX.Mathematics.Interop.RawVector3 { X = forward.X, Y = forward.Y, Z = forward.Z };
            _dxListener.OrientTop = new SharpDX.Mathematics.Interop.RawVector3 { X = up.X, Y = up.Y, Z = up.Z };
#else
            _dxListener.Position = new SharpDX.Vector3(pos.X, pos.Y, pos.Z);
            _dxListener.Velocity = new SharpDX.Vector3(vel.X, vel.Y, vel.Z);
            _dxListener.OrientFront = new SharpDX.Vector3(forward.X, forward.Y, forward.Z);
            _dxListener.OrientTop = new SharpDX.Vector3(up.X, up.Y, up.Z);
#endif
            return _dxListener;
        }

        private void PlatformPause()
        {
            if (_voice != null)
                _voice.Stop();
            _paused = true;
        }

        private void PlatformPlay()
        {
            if (_voice != null)
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
            if (_voice != null)
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
            if (_voice != null)
            {
                if (immediate)
                {
                    _voice.Stop(0);
                    _voice.FlushSourceBuffers();
                }
                else
                    _voice.Stop((int)PlayFlags.Tails);
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
            if (_voice == null)
                return;

            UpdateOutputMatrix();
        }

        private void UpdateOutputMatrix()
        {
            var srcChannelCount = _voice.VoiceDetails.InputChannelCount;
            var dstChannelCount = SoundEffect.MasterVoice.VoiceDetails.InputChannelCount;

            // Set the pan on the correct channels based on the reverb mix.
            if (!(_reverbMix > 0.0f))
                _voice.SetOutputMatrix(srcChannelCount, dstChannelCount, CalculatePanMatrix(_pan, 1.0f));
            else
            {
                _voice.SetOutputMatrix(SoundEffect.ReverbVoice, srcChannelCount, dstChannelCount, CalculatePanMatrix(_pan, _reverbMix));
                _voice.SetOutputMatrix(SoundEffect.MasterVoice, srcChannelCount, dstChannelCount, CalculatePanMatrix(_pan, 1.0f - Math.Min(_reverbMix, 1.0f)));
            }
        }

        private static float[] CalculatePanMatrix(float pan, float scale)
        {
            // From X3DAudio documentation:
            /*
                For submix and mastering voices, and for source voices without a channel mask or a channel mask of 0, 
                   XAudio2 assumes default speaker positions according to the following table. 

                Channels

                Implicit Channel Positions

                1 Always maps to FrontLeft and FrontRight at full scale in both speakers (special case for mono sounds) 
                2 FrontLeft, FrontRight (basic stereo configuration) 
                3 FrontLeft, FrontRight, LowFrequency (2.1 configuration) 
                4 FrontLeft, FrontRight, BackLeft, BackRight (quadraphonic) 
                5 FrontLeft, FrontRight, FrontCenter, SideLeft, SideRight (5.0 configuration) 
                6 FrontLeft, FrontRight, FrontCenter, LowFrequency, SideLeft, SideRight (5.1 configuration) (see the following remarks) 
                7 FrontLeft, FrontRight, FrontCenter, LowFrequency, SideLeft, SideRight, BackCenter (6.1 configuration) 
                8 FrontLeft, FrontRight, FrontCenter, LowFrequency, BackLeft, BackRight, SideLeft, SideRight (7.1 configuration) 
                9 or more No implicit positions (one-to-one mapping)                      
             */

            // Clear all the channels.
            var panMatrix = _panMatrix;
            Array.Clear(panMatrix, 0, panMatrix.Length);

            // Notes:
            //
            // Since XNA does not appear to expose any 'master' voice channel mask / speaker configuration,
            // I assume the mappings listed above should be used.
            //
            // Assuming it is correct to pan all channels which have a left/right component.

            var lVal = (1.0f - pan) * scale;
            var rVal = (1.0f + pan) * scale;

            switch (SoundEffect.Speakers)
            {
                case Speakers.Stereo:
                case Speakers.TwoPointOne:
                case Speakers.Surround:
                    panMatrix[0] = lVal;
                    panMatrix[1] = rVal;
                    break;

                case Speakers.Quad:
                    panMatrix[0] = panMatrix[2] = lVal;
                    panMatrix[1] = panMatrix[3] = rVal;
                    break;

                case Speakers.FourPointOne:
                    panMatrix[0] = panMatrix[3] = lVal;
                    panMatrix[1] = panMatrix[4] = rVal;
                    break;

                case Speakers.FivePointOne:
                case Speakers.SevenPointOne:
                case Speakers.FivePointOneSurround:
                    panMatrix[0] = panMatrix[4] = lVal;
                    panMatrix[1] = panMatrix[5] = rVal;
                    break;

                case Speakers.SevenPointOneSurround:
                    panMatrix[0] = panMatrix[4] = panMatrix[6] = lVal;
                    panMatrix[1] = panMatrix[5] = panMatrix[7] = rVal;
                    break;

                case Speakers.Mono:
                default:
                    // don't do any panning here   
                    break;
            }

            return panMatrix;
        }

        private void PlatformSetPitch(float value)
        {
            _pitch = value;

            if (_voice == null)
                return;

            // NOTE: This is copy of what XAudio2.SemitonesToFrequencyRatio() does
            // which avoids the native call and is actually more accurate.
             var pitch = (float)Math.Pow(2.0, value);
             _voice.SetFrequencyRatio(pitch);
        }

        private SoundState PlatformGetState()
        {
            // If no voice or no buffers queued the sound is stopped.
            if (_voice == null || _voice.State.BuffersQueued == 0)
                return SoundState.Stopped;

            // Because XAudio2 does not actually provide if a SourceVoice is Started / Stopped
            // we have to save the "paused" state ourself.
            if (_paused)
                return SoundState.Paused;

            return SoundState.Playing;
        }

        private void PlatformSetVolume(float value)
        {
            if (_voice != null)
                _voice.SetVolume(value, XAudio2.CommitNow);
        }

        internal void PlatformSetReverbMix(float mix)
        {
            // At least for XACT we can't go over 2x the volume on the mix.
            _reverbMix = MathHelper.Clamp(mix, 0, 2);

            // If we have no voice then nothing more to do.
            if (_voice == null)
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
            if (_voice == null)
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
            if (_voice == null)
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

                if (_voice != null)
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
