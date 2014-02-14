#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;
using System.IO;

using Microsoft.Xna.Framework.Audio;

using SDL2;

namespace Microsoft.Xna.Framework.Media
{
	public sealed class Song : IEquatable<Song>, IDisposable
	{
		#region SDL_mixer Open/Close Routines

		// Has SDL_mixer already been opened?
		private static bool initialized = false;

		private static void initializeMixer()
		{
			if (!initialized)
			{
				SDL.SDL_InitSubSystem(SDL.SDL_INIT_AUDIO);
				SDL_mixer.Mix_OpenAudio(44100, SDL.AUDIO_S16SYS, 2, 1024);
				initialized = true;
			}
		}

		internal static void closeMixer()
		{
			if (initialized)
			{
				SDL_mixer.Mix_CloseAudio();
				initialized = false;
			}
		}

		#endregion

		#region Private Member Data

		private IntPtr INTERNAL_mixMusic;

		SDL_mixer.MusicFinishedDelegate musicFinishedDelegate;

		#endregion

		#region Internal Member Data

		internal delegate void FinishedPlayingHandler(object sender, EventArgs args);

		#endregion

		#region Public Properties

		// TODO: A real Vorbis stream would have this info.
		public TimeSpan Duration
		{
			get;
			private set;
		}

		// TODO: A real Vorbis stream would have this info.
		public TimeSpan Position
		{
			get
			{
				return new TimeSpan(0);
			}
		}

		public bool IsProtected
		{
			get
			{
				return false;
			}
		}

		public bool IsRated
		{
			get
			{
				return false;
			}
		}

		public string Name
		{
			get
			{
				return Path.GetFileNameWithoutExtension(FilePath);
			}
		}

		public int PlayCount
		{
			get;
			private set;
		}

		public int Rating
		{
			get
			{
				return 0;
			}
		}

		// TODO: Could be obtained with Vorbis metadata
		public int TrackNumber
		{
			get
			{
				return 0;
			}
		}

		public bool IsDisposed
		{
			get;
			private set;
		}

		#endregion

		#region Internal Properties

		internal string FilePath
		{
			get;
			private set;
		}

		internal float Volume
		{
			get
			{
				return SDL_mixer.Mix_VolumeMusic(-1) / 128.0f;
			}
			set
			{
				SDL_mixer.Mix_VolumeMusic((int) (value * 128));
			}
		}

		#endregion

		#region Constructors, Deconstructor, Dispose()

		internal Song(string fileName, int durationMS) : this(fileName)
		{
			Duration = TimeSpan.FromMilliseconds(durationMS);
		}

		internal Song(string fileName)
		{
			FilePath = fileName;
			initializeMixer();
			INTERNAL_mixMusic = SDL_mixer.Mix_LoadMUS(fileName);
			IsDisposed = false;
		}

		~Song()
		{
			SDL_mixer.Mix_HookMusicFinished(null);
			Dispose(true);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (INTERNAL_mixMusic != IntPtr.Zero)
				{
					SDL_mixer.Mix_FreeMusic(INTERNAL_mixMusic);
				}
			}
			IsDisposed = true;
		}

		#endregion

		#region Internal Playback Methods

		internal void Play()
		{
			if (INTERNAL_mixMusic == IntPtr.Zero)
			{
				return;
			}
			musicFinishedDelegate = OnFinishedPlaying;
			SDL_mixer.Mix_HookMusicFinished(musicFinishedDelegate);
			SDL_mixer.Mix_PlayMusic(INTERNAL_mixMusic, 0);
			PlayCount += 1;
		}

		internal void Resume()
		{
			SDL_mixer.Mix_ResumeMusic();
		}

		internal void Pause()
		{
			SDL_mixer.Mix_PauseMusic();
		}

		internal void Stop()
		{
			SDL_mixer.Mix_HookMusicFinished(null);
			SDL_mixer.Mix_HaltMusic();
			PlayCount = 0;
		}

		#endregion

		#region Internal Event Handler Methods

		/// <summary>
		/// Set the event handler for "Finished Playing". Done this way to prevent multiple bindings.
		/// </summary>
		internal void SetEventHandler(FinishedPlayingHandler handler)
		{
			// No-op
		}

		internal void OnFinishedPlaying()
		{
			MediaPlayer.OnSongFinishedPlaying(null, null);
		}

		#endregion

		#region Public Comparison Methods/Operators

		public bool Equals(Song song) 
		{
			return (((object) song) != null) && (Name == song.Name);
		}

		public override bool Equals(Object obj)
		{
			if (obj == null)
			{
				return false;
			}
			return Equals(obj as Song);
		}

		public static bool operator ==(Song song1, Song song2)
		{
			if (((object) song1) == null)
			{
				return ((object) song2) == null;
			}
			return song1.Equals(song2);
		}

		public static bool operator !=(Song song1, Song song2)
		{
			return !(song1 == song2);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion
	}
}
