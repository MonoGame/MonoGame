// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        private void PlatformInitialize(string fileName)
        {
            throw new NotImplementedException();
        }

        private void PlatformDispose(bool disposing)
        {
            throw new NotImplementedException();
        }

		internal void OnFinishedPlaying (object sender, EventArgs args)
		{
			throw new NotImplementedException();
		}

		internal void SetEventHandler(FinishedPlayingHandler handler)
		{
			throw new NotImplementedException();
		}

		internal void Play(TimeSpan? startPosition)
		{	
			throw new NotImplementedException();
        }

        private void PlatformPlay()
        {
            throw new NotImplementedException();
        }

		internal void Resume()
		{
			throw new NotImplementedException();
		}

        private void PlatformResume()
        {
            throw new NotImplementedException();
        }
		
		internal void Pause()
		{			            
			throw new NotImplementedException();
        }
		
		internal void Stop()
		{
			throw new NotImplementedException();
		}

		internal float Volume
		{
			get
			{
				throw new NotImplementedException();
			}
			
			set
			{
				throw new NotImplementedException();
			}			
		}

		internal TimeSpan Position
        {
            get
            {
                throw new NotImplementedException();				
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        private Album PlatformGetAlbum()
        {
            throw new NotImplementedException();
        }

        private Artist PlatformGetArtist()
        {
            throw new NotImplementedException();
        }

        private Genre PlatformGetGenre()
        {
            throw new NotImplementedException();
        }

        private bool PlatformIsProtected()
        {
            throw new NotImplementedException();
        }

        private bool PlatformIsRated()
        {
            throw new NotImplementedException();
        }

        private string PlatformGetName()
        {
            throw new NotImplementedException();
        }

        private int PlatformGetPlayCount()
        {
            throw new NotImplementedException();
        }

        private int PlatformGetRating()
        {
            throw new NotImplementedException();
        }

        private int PlatformGetTrackNumber()
        {
            throw new NotImplementedException();
        }
    }
}