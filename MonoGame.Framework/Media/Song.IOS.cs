// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using MonoTouch.Foundation;
using MonoTouch.AVFoundation;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        private AVAudioPlayer _sound;

        private void PlatformInitialize(string fileName)
        {
            _sound = AVAudioPlayer.FromUrl(NSUrl.FromFilename(fileName));
			_sound.NumberOfLoops = 0;
            _sound.FinishedPlaying += OnFinishedPlaying;
        }

        private void PlatformDispose(bool disposing)
        {
            if (_sound == null)
                return;
                
            _sound.FinishedPlaying -= OnFinishedPlaying;
            _sound.Dispose();
            
            _sound = null;
        }

		internal void OnFinishedPlaying (object sender, EventArgs args)
		{
			if (DonePlaying == null)
				return;
			
			DonePlaying(sender, args);
		}

		/// <summary>
		/// Set the event handler for "Finished Playing". Done this way to prevent multiple bindings.
		/// </summary>
		internal void SetEventHandler(FinishedPlayingHandler handler)
		{
			if (DonePlaying != null)
				return;
			
			DonePlaying += handler;
		}

		internal void Play()
		{	
			if ( _sound == null )
				return;

            PlatformPlay();

            _playCount++;
        }

        private void PlatformPlay()
        {
            // AVAudioPlayer sound.Stop() does not reset the playback position as XNA does.
            // Set Play's currentTime to 0 to ensure playback at the start.
            _sound.CurrentTime = 0.0;
            
            _sound.Play();
        }

		internal void Resume()
		{
			if (_sound == null)
				return;

            PlatformResume();
		}

        private void PlatformResume()
        {
			_sound.Play();
        }
		
		internal void Pause()
		{			            
			if ( _sound == null )
				return;
			
			_sound.Pause();
        }
		
		internal void Stop()
		{
			if ( _sound == null )
				return;
			
			_sound.Stop();
			_playCount = 0;
		}

		internal float Volume
		{
			get
			{
				if (_sound != null)
					return _sound.Volume;
				else
					return 0.0f;
			}
			
			set
			{
				if ( _sound != null && _sound.Volume != value )
					_sound.Volume = value;
			}			
		}

		internal TimeSpan Position
        {
            get
            {
                // TODO: Implement
                return new TimeSpan(0);				
            }
        }
    }
}

