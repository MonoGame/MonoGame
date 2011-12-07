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
#endregion Statements

namespace Microsoft.Xna.Framework.Audio
{
	public sealed class SoundEffectInstance : IDisposable
	{
		private bool isDisposed = false;
		private SoundState soundState = SoundState.Stopped;
		
		public SoundEffectInstance ()
		{
			
		}
		
		public void Dispose()
		{
			_sound.Dispose();
			isDisposed = true;
		}
		
		public void Apply3D (AudioListener listener, AudioEmitter emitter)
		{
			throw new NotImplementedException();
		}
		
		public void Apply3D (AudioListener[] listeners,AudioEmitter emitter)
		{
			throw new NotImplementedException();
		}		
		
		public void Pause ()
		{
            if ( _sound != null )
			{
				_sound.Pause();
				soundState = SoundState.Paused;
			}
		}
		
		public void Play ()
		{
			if ( _sound != null )
			{
				if (soundState == SoundState.Paused)
					_sound.Resume();
				else
					_sound.Play();
				soundState = SoundState.Playing;
			}
		}
		
		public void Resume ()
		{
			Play();
		}
		
		public void Stop ()
		{
			if ( _sound != null )
			{
				_sound.Stop();
				soundState = SoundState.Stopped;
			}
		}
		
		public void Stop (bool immediate)
		{
			Stop();
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
				if ( _sound != null )
				{
					return _sound.Looping;
				}
				else
				{
					return false;
				}
			}
			
			set
			{
				if ( _sound != null )
				{
					if ( _sound.Looping != value )
					{
						_sound.Looping = value;
					}
				}
			}
		}
		
		public float Pan 
		{ 
			get
			{
                if ( _sound != null )
				{
					return _sound.Pan;
				}
				else
				{
					return 0.0f;
				}
			}
			
			set
			{
                if ( _sound != null )
				{
					if ( _sound.Pan != value )
					{
						_sound.Pan = value;
					}
				}
			}
		}
		
		public float Pitch         
		{             
	            get
	            {
					if ( _sound != null)
				    {
	                   return _sound.Rate;
				    }
				    return 0.0f;
	            }
	            set
	            {
				    if ( _sound != null && _sound.Rate != value)
				    {
	                   _sound.Rate = value;
				    } 
	            }        
		 }
		
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
		
		public SoundState State 
		{ 
			get
			{
				if (_sound != null && soundState == SoundState.Playing && !_sound.Playing) {
					soundState = SoundState.Stopped;
				}
				return soundState;
			} 
		}
		
		public float Volume
		{ 
			get
			{
				if (_sound != null)
				{
					return _sound.Volume;
				}
				else
				{
					return 0.0f;
				}
			}
			
			set
			{
				if ( _sound != null )
				{
					if ( _sound.Volume != value )
					{
						_sound.Volume = value;
					}
				}
			}
		}	
		
		
	}
}
