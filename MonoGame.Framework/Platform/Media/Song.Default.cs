// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        private SoundEffectInstance _sound;

        private void PlatformInitialize(string fileName)
        {

#if MONOMAC || (WINDOWS && OPENGL) || WEB

            using (var s = File.OpenRead(_name))
            {
                var soundEffect = SoundEffect.FromStream(s);
                _sound = soundEffect.CreateInstance();
            }
#endif
        }

        private void PlatformDispose(bool disposing)
        {
            if (_sound == null)
                return;

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

		internal void Play(TimeSpan? startPosition)
		{	
			if (startPosition.HasValue)
				throw new Exception("startPosition not implemented on this Platform"); //Should be possible to implement in OpenAL
			if ( _sound == null )
				return;

            PlatformPlay();

            _playCount++;
        }

        private void PlatformPlay()
        {
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
            _sound.Resume();
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

        private Album PlatformGetAlbum()
        {
            return null;
        }

        private Artist PlatformGetArtist()
        {
            return null;
        }

        private Genre PlatformGetGenre()
        {
            return null;
        }

        private TimeSpan PlatformGetDuration()
        {
            return _duration;
        }

        private bool PlatformIsProtected()
        {
            return false;
        }

        private bool PlatformIsRated()
        {
            return false;
        }

        private string PlatformGetName()
        {
            return Path.GetFileNameWithoutExtension(_name);
        }

        private int PlatformGetPlayCount()
        {
            return _playCount;
        }

        private int PlatformGetRating()
        {
            return 0;
        }

        private int PlatformGetTrackNumber()
        {
            return 0;
        }
    }
}