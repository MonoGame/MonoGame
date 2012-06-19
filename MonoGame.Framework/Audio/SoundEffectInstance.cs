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
#if WINRT
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using SharpDX.Multimedia;
#endif
#endregion Statements

namespace Microsoft.Xna.Framework.Audio
{
	public sealed class SoundEffectInstance : IDisposable
	{
		private bool isDisposed = false;
		private SoundState soundState = SoundState.Stopped;

#if WINRT        
        internal SourceVoice _voice { get; set; }
        internal SoundEffect _effect { get; set; }

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

        internal SoundEffectInstance()
		{			
		}
		
		public void Dispose()
		{
#if WINRT
            _voice.DestroyVoice();
            _voice.Dispose();
            _voice = null;
            _effect = null;
#else
			_sound.Dispose();
#endif
			isDisposed = true;
		}
		
		public void Apply3D (AudioListener listener, AudioEmitter emitter)
		{
#if WINRT		
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
#if WINRT            
            _voice.Stop();
            _paused = true;
#else
            if ( _sound != null )
			{
				_sound.Pause();
				soundState = SoundState.Paused;
			}
#endif
		}
		
		public void Play ()
		{
#if WINRT              
            // Choose the correct buffer depending on if we are looped.            
            var buffer = _loop ? _effect._loopedBuffer : _effect._buffer;

            if (_voice.State.BuffersQueued > 0)
            {
                _voice.Stop();
                _voice.FlushSourceBuffers();
            }

            _voice.SubmitSourceBuffer(buffer, null);
            _voice.Start();

            _paused = false;
#else
			if ( _sound != null )
			{
				if (soundState == SoundState.Paused)
					_sound.Resume();
				else
					_sound.Play();
				soundState = SoundState.Playing;
			}
#endif
		}
		
		public void Resume()
		{
#if WINRT
            _voice.Start();
            _paused = false;
#else
#endif
		}
		
		public void Stop()
		{
#if WINRT
            _voice.Stop(0);
            _voice.FlushSourceBuffers();
            _paused = false;
#else
#endif
        }

        public void Stop(bool immediate)
        {
#if WINRT            
            _voice.Stop( immediate ? 0 : (int)PlayFlags.Tails );
            _paused = false;
#else
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
#if WINRT
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
#if WINRT
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
		        
#if WINRT
        private float _pan;
        private static float[] _panMatrix;
#endif

		public float Pan 
		{ 
			get
			{
#if WINRT                
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
#if WINRT                
                // According to XNA documentation:
                // "Panning, ranging from -1.0f (full left) to 1.0f (full right). 0.0f is centered."
                _pan = MathHelper.Clamp(value, -1.0f, 1.0f);
                
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
                                
                switch (SoundEffect.MasterVoice.ChannelMask)
                {
                    case ((int)Speakers.Stereo):
                    case ((int)Speakers.TwoPointOne):
                    case ((int)Speakers.Surround):
                        _panMatrix[0] = lVal;
                        _panMatrix[1] = rVal;
                        break;

                    case ((int)Speakers.Quad):
                        _panMatrix[0] = _panMatrix[2] = lVal;
                        _panMatrix[1] = _panMatrix[3] = rVal;
                        break;

                    case ((int)Speakers.FourPointOne):
                        _panMatrix[0] = _panMatrix[3] = lVal;
                        _panMatrix[1] = _panMatrix[4] = rVal;
                        break;

                    case ((int)Speakers.FivePointOne):
                    case ((int)Speakers.SevenPointOne):
                    case ((int)Speakers.FivePointOneSurround):
                        _panMatrix[0] = _panMatrix[4] = lVal;
                        _panMatrix[1] = _panMatrix[5] = rVal;
                        break;

                    case ((int)Speakers.SevenPointOneSurround):
                        _panMatrix[0] = _panMatrix[4] = _panMatrix[6] = lVal;
                        _panMatrix[1] = _panMatrix[5] = _panMatrix[7] = rVal;
                        break;

                    case ((int)Speakers.Mono):
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
#if WINRT
                    return XAudio2.FrequencyRatioToSemitones(_voice.FrequencyRatio) * 12.0f;
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
                    // According to XNA documentation a value of 1.0 adjusts pitch upwards by an octave,
                    // therefore the scale of this Pitch property is 1.0f Pitch = 12 semitones;                    
#if WINRT
                    _voice.SetFrequencyRatio(XAudio2.SemitonesToFrequencyRatio(value * 12.0f));                    
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
#if WINRT           
                // If no buffers queued the sound is stopped.
                if (_voice.State.BuffersQueued == 0)
                {
                    return SoundState.Stopped;
                }
                
                // Because XAudio2 does not actually provide if a SourceVoice is Started / Stopped
                // we have to save the "paused" state ourself.
                if (_paused)
                    return SoundState.Paused;

                return SoundState.Playing;                                
#else
				if (_sound != null && soundState == SoundState.Playing && !_sound.Playing) {
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
#if WINRT
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
#if WINRT
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
