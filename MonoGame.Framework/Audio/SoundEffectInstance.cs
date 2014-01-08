#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
#endregion License

#region Using Statements
using System;
#if DIRECTX
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using SharpDX.Multimedia;
#else
using System.IO;
#endif
#endregion Statements

namespace Microsoft.Xna.Framework.Audio
{
	public sealed class SoundEffectInstance : IDisposable
	{
		private bool isDisposed = false;
#if !DIRECTX
        private SoundState soundState = SoundState.Stopped;
#endif
#if ANDROID
        private int _streamId = -1;
#endif

#if DIRECTX        
        private SourceVoice _voice { get; set; }
        private SoundEffect _effect { get; set; }

        private bool _paused;
        private bool _loop;
#else
        private Sound _sound;
		internal Sound Sound 
		{ 
			get
			{
				return _sound;
			} 
			
			set
			{
				_sound = value;
			} 
		}
#endif

#if DIRECTX
        internal SoundEffectInstance(SoundEffect effect, SourceVoice voice)
        {
            _effect = effect;
            _voice = voice;
        }
#else
        internal SoundEffectInstance(){}

        /* Creates a standalone SoundEffectInstance from given wavedata. */
        internal SoundEffectInstance(byte[] buffer, int sampleRate, int channels)
        {
            // buffer should contain 16-bit PCM wave data
            short bitsPerSample = 16;

            using (var mStream = new MemoryStream(44+buffer.Length))
            using (var writer = new BinaryWriter(mStream))
            {
                writer.Write("RIFF".ToCharArray()); //chunk id
                writer.Write((int)(36 + buffer.Length)); //chunk size
                writer.Write("WAVE".ToCharArray()); //RIFF type

                writer.Write("fmt ".ToCharArray()); //chunk id
                writer.Write((int)16); //format header size
                writer.Write((short)1); //format (PCM)
                writer.Write((short)channels);
                writer.Write((int)sampleRate);
                short blockAlign = (short)((bitsPerSample / 8) * (int)channels);
                writer.Write((int)(sampleRate * blockAlign)); //byte rate
                writer.Write((short)blockAlign);
                writer.Write((short)bitsPerSample);

                writer.Write("data".ToCharArray()); //chunk id
                writer.Write((int)buffer.Length); //data size

                writer.Write(buffer);

                _sound = new Sound(mStream.ToArray(), 1.0f, false);
                _sound.Rate = sampleRate;
            }
        }
#endif

        public void Dispose()
        {
#if DIRECTX
            if (_voice != null)
            {
                _voice.DestroyVoice();
                _voice.Dispose();
                _voice = null;
            }
		    _effect = null;
#elif ANDROID
            if (_streamId >= 0)
                _sound.Stop(_streamId);
#else
            // When disposing a SoundEffectInstance, the Sound should
            // just be stopped as it will likely be reused later
            _sound.Stop();
#endif
			isDisposed = true;
		}
		
		public void Apply3D (AudioListener listener, AudioEmitter emitter)
        {
#if DIRECTX	
            // If we have no voice then nothing to do.
            if (_voice == null)
                return;

            // Convert from XNA Emitter to a SharpDX Emitter
            var e = emitter.ToEmitter();
            e.CurveDistanceScaler = SoundEffect.DistanceScale;
            e.DopplerScaler = SoundEffect.DopplerScale;
            e.ChannelCount = _effect._format.Channels;

            // Convert from XNA Listener to a SharpDX Listener
            var l = listener.ToListener();                        
            
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
#endif
        }
		
		public void Apply3D (AudioListener[] listeners,AudioEmitter emitter)
		{
            foreach ( var l in listeners )
                Apply3D(l, emitter);            
		}		
		
		public void Pause ()
        {
#if DIRECTX         
            if (_voice != null)
                _voice.Stop();
            _paused = true;
#else
            if ( _sound != null )
			{
#if ANDROID
				_sound.Pause(_streamId);
#else
				_sound.Pause();
#endif
                soundState = SoundState.Paused;
			}
#endif
		}
		
		public void Play ()
        {
            if (State == SoundState.Playing)
                return;
#if DIRECTX
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
#else
            if ( _sound != null )
			{
#if ANDROID
				if (soundState == SoundState.Paused)
					_sound.Resume(_streamId);
				else
					_streamId = _sound.Play();
#else
				if (soundState == SoundState.Paused)
					_sound.Resume();
				else
					_sound.Play();
#endif
				soundState = SoundState.Playing;
			}
#endif
		}

		/// <summary>
		/// Tries to play the sound, returns true if successful
		/// </summary>
		/// <returns></returns>
		internal bool TryPlay()
		{
			Play();
#if ANDROID
			return _streamId != 0;
#else
			return true;
#endif
		}

		public void Resume()
        {
#if DIRECTX
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
#else
            if ( _sound != null )
			{
				if (soundState == SoundState.Paused)
				{
#if ANDROID
					_sound.Resume(_streamId);
#else
                    _sound.Resume();
#endif
                }
				soundState = SoundState.Playing;
 			}
#endif
		}
		
		public void Stop()
        {
#if DIRECTX
            if (_voice != null)
            {
                _voice.Stop(0);
                _voice.FlushSourceBuffers();
            }

		    _paused = false;
#else
            if ( _sound != null )
			{
#if ANDROID
				_sound.Stop(_streamId);
				_streamId = -1;
#else
                _sound.Stop();
#endif
                soundState = SoundState.Stopped;
			}
#endif
        }

        public void Stop(bool immediate)
        {
#if DIRECTX            
            if (_voice != null)
                _voice.Stop(immediate ? 0 : (int)PlayFlags.Tails);

            _paused = false;
#else
            if ( _sound != null )
			{
#if ANDROID
                _sound.Stop(_streamId);
                _streamId = -1;
#else
                _sound.Stop();
#endif
				soundState = SoundState.Stopped;
			}
#endif
        }		
		
		public bool IsDisposed 
		{ 
			get
			{
				return isDisposed;
			}
		}
		
		public bool IsLooped 
		{ 
			get
            {
#if DIRECTX
                return _loop;
#else
                if ( _sound != null )
				{
					return _sound.Looping;
				}
				else
				{
					return false;
				}
#endif
			}
			
			set
            {
#if DIRECTX
                _loop = value;
#else
                if ( _sound != null )
				{
					if ( _sound.Looping != value )
					{
						_sound.Looping = value;
					}
				}
#endif
			}
		}

#if DIRECTX
        private float _pan;
        private static float[] _panMatrix;
#endif

        public float Pan 
		{ 
			get
            {
#if DIRECTX                
                return _pan;
#else
                if ( _sound != null )
				{
					return _sound.Pan;
				}
				else
				{
					return 0.0f;
				}
#endif
			}
			
			set
            {
#if DIRECTX       
                // According to XNA documentation:
                // "Panning, ranging from -1.0f (full left) to 1.0f (full right). 0.0f is centered."
                _pan = MathHelper.Clamp(value, -1.0f, 1.0f);

                // If we have no voice then nothing more to do.
                if (_voice == null)
                    return;
                
                var srcChannelCount = _effect._format.Channels;
                var dstChannelCount = SoundEffect.MasterVoice.VoiceDetails.InputChannelCount;
                
                if ( _panMatrix == null || _panMatrix.Length < dstChannelCount )
                    _panMatrix = new float[Math.Max(dstChannelCount,8)];                

                // Default to full volume for all channels/destinations   
                for (var i = 0; i < _panMatrix.Length; i++)
                    _panMatrix[i] = 1.0f;

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

                // Notes:
                //
                // Since XNA does not appear to expose any 'master' voice channel mask / speaker configuration,
                // I assume the mappings listed above should be used.
                //
                // Assuming it is correct to pan all channels which have a left/right component.

                var lVal = 1.0f - _pan;
                var rVal = 1.0f + _pan;
                                
                switch (SoundEffect.Speakers)
                {
                    case Speakers.Stereo:
                    case Speakers.TwoPointOne:
                    case Speakers.Surround:
                        _panMatrix[0] = lVal;
                        _panMatrix[1] = rVal;
                        break;

                    case Speakers.Quad:
                        _panMatrix[0] = _panMatrix[2] = lVal;
                        _panMatrix[1] = _panMatrix[3] = rVal;
                        break;

                    case Speakers.FourPointOne:
                        _panMatrix[0] = _panMatrix[3] = lVal;
                        _panMatrix[1] = _panMatrix[4] = rVal;
                        break;

                    case Speakers.FivePointOne:
                    case Speakers.SevenPointOne:
                    case Speakers.FivePointOneSurround:
                        _panMatrix[0] = _panMatrix[4] = lVal;
                        _panMatrix[1] = _panMatrix[5] = rVal;
                        break;

                    case Speakers.SevenPointOneSurround:
                        _panMatrix[0] = _panMatrix[4] = _panMatrix[6] = lVal;
                        _panMatrix[1] = _panMatrix[5] = _panMatrix[7] = rVal;
                        break;

                    case Speakers.Mono:
                    default:
                        // don't do any panning here   
                        break;
                }

                _voice.SetOutputMatrix(srcChannelCount, dstChannelCount, _panMatrix);

#else
                if ( _sound != null )
				{
					if ( _sound.Pan != value )
					{
						_sound.Pan = value;
					}
				}
#endif
            }
		}
		
		public float Pitch         
		{             
	            get
            {
#if DIRECTX
                    if (_voice == null)
                        return 0.0f;

                    // NOTE: This is copy of what XAudio2.FrequencyRatioToSemitones() does
                    // which avoids the native call and is actually more accurate.
                    var pitch = 39.86313713864835 * Math.Log10(_voice.FrequencyRatio);

                    // Convert from semitones to octaves.
                    pitch /= 12.0;

                    return (float)pitch;
#else
                if ( _sound != null)
				    {
	                   return _sound.Rate;
				    }
				    return 0.0f;
#endif
	            }
	            set
            {
#if DIRECTX
                    if (_voice == null)
                        return;

                    // NOTE: This is copy of what XAudio2.SemitonesToFrequencyRatio() does
                    // which avoids the native call and is actually more accurate.
                    var ratio = Math.Pow(2.0, value);
                    _voice.SetFrequencyRatio((float)ratio);                  
#else
                if ( _sound != null && _sound.Rate != value)
				    {
	                   _sound.Rate = value;
				    } 
#endif
	            }        
		 }				
		
		public SoundState State 
		{ 
			get
            {
#if DIRECTX           
                // If no voice or no buffers queued the sound is stopped.
                if (_voice == null || _voice.State.BuffersQueued == 0)
                    return SoundState.Stopped;
                
                // Because XAudio2 does not actually provide if a SourceVoice is Started / Stopped
                // we have to save the "paused" state ourself.
                if (_paused)
                    return SoundState.Paused;

                return SoundState.Playing;                                
#elif ANDROID
                // Android SoundPool can't tell us when a sound is finished playing.
                // TODO: Remove this code when OpenAL for Android is implemented
                if (_sound != null && IsLooped)
                {
                    // Looping sounds use our stored state
                    return soundState;
                }
                else
                {
                    // Non looping sounds always return Stopped
                    return SoundState.Stopped;
                }
#else
                if (_sound != null && soundState == SoundState.Playing && !_sound.Playing) 
                {
                    soundState = SoundState.Stopped;
                }

                return soundState;
#endif
			} 
		}
		
		public float Volume
		{ 
			get
            {
#if DIRECTX
                if (_voice == null)
                    return 0.0f;
                else
                    return _voice.Volume;
#else
                if (_sound != null)
				{
					return _sound.Volume;
				}
				else
				{
					return 0.0f;
				}
#endif
			}
			
			set
            {
#if DIRECTX
                if (_voice != null)
                    _voice.SetVolume(value, XAudio2.CommitNow);
#else
                if ( _sound != null )
				{
					if ( _sound.Volume != value )
					{
						_sound.Volume = value;
					}
				}
#endif
			}
		}	
		
		
	}
}
