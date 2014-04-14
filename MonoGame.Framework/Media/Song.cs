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

#if IOS
using MonoTouch.Foundation;
using MonoTouch.AVFoundation;
#elif WINDOWS_MEDIA_SESSION
using SharpDX;
using SharpDX.MediaFoundation;
#endif

namespace Microsoft.Xna.Framework.Media
{
    public sealed class Song : IEquatable<Song>, IDisposable
    {
#if IOS
		private AVAudioPlayer _sound;
#elif PSM
        private PSSuiteSong _sound;
#elif WINDOWS_MEDIA_SESSION
        private Topology _topology;
#elif !WINDOWS_MEDIA_ENGINE
		private SoundEffectInstance _sound;
#endif
		
		private string _name;
		private int _playCount = 0;
        private TimeSpan _duration = TimeSpan.Zero;
        bool disposed;

        internal Song(string fileName, int durationMS)
            : this(fileName)
        {
            _duration = TimeSpan.FromMilliseconds(durationMS);
        }

		internal Song(string fileName)
		{			
			_name = fileName;
			
#if IOS
			_sound = AVAudioPlayer.FromUrl(NSUrl.FromFilename(fileName));
			_sound.NumberOfLoops = 0;
            _sound.FinishedPlaying += OnFinishedPlaying;
#elif PSM
            _sound = new PSSuiteSong(_name);
#elif WINDOWS_MEDIA_SESSION 
            GetTopology();      
#elif !WINDOWS_MEDIA_ENGINE && !WINDOWS_PHONE
            _sound = new SoundEffect(_name).CreateInstance();
#endif
        }

        ~Song()
        {
            Dispose(false);
        }

        internal string FilePath
		{
			get { return _name; }
		}
		
		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
#if WINDOWS_MEDIA_SESSION

                    if (_topology != null)
                    {
                        _topology.Dispose();
                        _topology = null;
                    }

#elif !WINDOWS_MEDIA_ENGINE

                    if (_sound != null)
                    {
#if IOS
                       _sound.FinishedPlaying -= OnFinishedPlaying;
#endif
                        _sound.Dispose();
                        _sound = null;
                    }
#endif
                }

                disposed = true;
            }
        }
        
		public bool Equals(Song song)
        {
#if DIRECTX
            return song != null && song.FilePath == FilePath;
#else
			return ((object)song != null) && (Name == song.Name);
#endif
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		public override bool Equals(Object obj)
		{
			if(obj == null)
			{
				return false;
			}
			
			return Equals(obj as Song);  
		}
		
		public static bool operator ==(Song song1, Song song2)
		{
			if((object)song1 == null)
			{
				return (object)song2 == null;
			}

			return song1.Equals(song2);
		}
		
		public static bool operator !=(Song song1, Song song2)
		{
		  return ! (song1 == song2);
		}

#if WINDOWS_MEDIA_SESSION

        internal Topology GetTopology()
        {
            if (_topology == null)
            {
                MediaManagerState.CheckStartup();

                MediaFactory.CreateTopology(out _topology);

                SharpDX.MediaFoundation.MediaSource mediaSource;
                {
                    SourceResolver resolver;
                    MediaFactory.CreateSourceResolver(out resolver);

                    ObjectType otype;
                    ComObject source;
                    resolver.CreateObjectFromURL(FilePath, (int)SourceResolverFlags.MediaSource, null, out otype,
                                                 out source);
                    mediaSource = source.QueryInterface<SharpDX.MediaFoundation.MediaSource>();
                    resolver.Dispose();
                    source.Dispose();
                }

                PresentationDescriptor presDesc;
                mediaSource.CreatePresentationDescriptor(out presDesc);

                for (var i = 0; i < presDesc.StreamDescriptorCount; i++)
                {
                    Bool selected;
                    StreamDescriptor desc;
                    presDesc.GetStreamDescriptorByIndex(i, out selected, out desc);

                    if (selected)
                    {
                        TopologyNode sourceNode;
                        MediaFactory.CreateTopologyNode(TopologyType.SourceStreamNode, out sourceNode);

                        sourceNode.Set(TopologyNodeAttributeKeys.Source, mediaSource);
                        sourceNode.Set(TopologyNodeAttributeKeys.PresentationDescriptor, presDesc);
                        sourceNode.Set(TopologyNodeAttributeKeys.StreamDescriptor, desc);

                        TopologyNode outputNode;
                        MediaFactory.CreateTopologyNode(TopologyType.OutputNode, out outputNode);

                        var majorType = desc.MediaTypeHandler.MajorType;
                        if (majorType != MediaTypeGuids.Audio)
                            throw new NotSupportedException("The song contains video data!");

                        Activate activate;
                        MediaFactory.CreateAudioRendererActivate(out activate);
                        outputNode.Object = activate;

                        _topology.AddNode(sourceNode);
                        _topology.AddNode(outputNode);
                        sourceNode.ConnectOutput(0, outputNode, 0);

                        sourceNode.Dispose();
                        outputNode.Dispose();
                    }

                    desc.Dispose();
                }

                presDesc.Dispose();
                mediaSource.Dispose();
            }

            return _topology;
        }
            
#elif !WINDOWS_MEDIA_ENGINE

        internal delegate void FinishedPlayingHandler(object sender, EventArgs args);
		event FinishedPlayingHandler DonePlaying;

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

#if IOS
            // AVAudioPlayer sound.Stop() does not reset the playback position as XNA does.
            // Set Play's currentTime to 0 to ensure playback at the start.
            _sound.CurrentTime = 0.0;
#endif
			_sound.Play();

            _playCount++;
        }

		internal void Resume()
		{
			if (_sound == null)
				return;			
#if IOS

			_sound.Play();
#else
			_sound.Resume();
#endif
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

#endif // !DIRECTX

        public TimeSpan Duration
        {
            get
            {
                return _duration;
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
				return Path.GetFileNameWithoutExtension(_name);
            }
        }

        public int PlayCount
        {
            get
            {
				return _playCount;
            }
        }

        public int Rating
        {
            get
            {
				return 0;
            }
        }

        public int TrackNumber
        {
            get
            {
				return 0;
            }
        }
    }
}

