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

#region VideoPlayer Graphics Define
#if SDL2
#define VIDEOPLAYER_OPENGL
#endif
#endregion

using System;
using System.IO;
using System.Threading;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media
{
    public sealed class Video : IDisposable
    {
        #region Private Variables: Video Implementation
        private string _fileName;
        private Color _backColor = Color.Black;
        #endregion
        
        #region Internal Variables: TheoraPlay
        internal IntPtr theoraDecoder;
        internal IntPtr videoStream;
        internal IntPtr audioStream;
        #endregion
        
        #region Internal Properties
        internal bool IsDisposed
        {
            get;
            private set;
        }
        #endregion
     
        #region Public Properties
        public int Width
        {
            get;
            private set;
        }
        
        public int Height
        {
            get;
            private set;
        }
         
        public string FileName
        {
            get 
            {
                return _fileName;
            }
        }
        
        private float INTERNAL_fps = 0.0f;
        public float FramesPerSecond
        {
            get
            {
                return INTERNAL_fps;
            }
            internal set
            {
                INTERNAL_fps = value;
            }
        }
        
        // FIXME: This is hacked, look up "This is a part of the Duration hack!"
        public TimeSpan Duration
        {
            get;
            internal set;
        }
        #endregion
        
        #region Internal Video Constructor
        internal Video(string FileName)
        {
            // Check out the file...
            _fileName = Normalize(FileName);
            if (_fileName == null)
            {
                throw new Exception("File " + FileName + " does not exist!");
            }
            
            // Set everything to NULL. Yes, this actually matters later.
            theoraDecoder = IntPtr.Zero;
            videoStream = IntPtr.Zero;
            audioStream = IntPtr.Zero;
            
            // Initialize the decoder nice and early...
            IsDisposed = true;
            Initialize();
            
            // FIXME: This is a part of the Duration hack!
            Duration = TimeSpan.MaxValue;
        }
        #endregion
        
        #region File name normalizer
        internal static string Normalize(string FileName)
        {
            if (File.Exists(FileName))
            {
                return FileName;
            }
            
            // Check the file extension
            if (!string.IsNullOrEmpty(Path.GetExtension(FileName)))
            {
                return null;
            }
            
            // Concat the file name with valid extensions
            if (File.Exists(FileName + ".ogv"))
            {
                return FileName + ".ogv";
            }
            if (File.Exists(FileName + ".ogg"))
            {
                return FileName + ".ogg";
            }
            
            return null;
        }
        #endregion
        
        #region Internal TheoraPlay Initialization
        internal void Initialize()
        {
            if (!IsDisposed)
            {
                Dispose(); // We need to start from the beginning, don't we? :P
            }
            
            // Initialize the decoder.
            theoraDecoder = TheoraPlay.THEORAPLAY_startDecodeFile(
                _fileName,
                150, // Arbitrarily 5 seconds in a 30fps movie.
#if VIDEOPLAYER_OPENGL
                TheoraPlay.THEORAPLAY_VideoFormat.THEORAPLAY_VIDFMT_IYUV
#else
                // Use the TheoraPlay software converter.
                TheoraPlay.THEORAPLAY_VideoFormat.THEORAPLAY_VIDFMT_RGBA
#endif
            );
            
            // Wait until the decoder is ready.
            while (TheoraPlay.THEORAPLAY_isInitialized(theoraDecoder) == 0)
            {
                Thread.Sleep(10);
            }
            
            // Initialize the audio stream pointer and get our first packet.
            if (TheoraPlay.THEORAPLAY_hasAudioStream(theoraDecoder) != 0)
            {
                while (audioStream == IntPtr.Zero)
                {
                    audioStream = TheoraPlay.THEORAPLAY_getAudio(theoraDecoder);
                    Thread.Sleep(10);
                }
            }
            
            // Initialize the video stream pointer and get our first frame.
            if (TheoraPlay.THEORAPLAY_hasVideoStream(theoraDecoder) != 0)
            {
                while (videoStream == IntPtr.Zero)
                {
                    videoStream = TheoraPlay.THEORAPLAY_getVideo(theoraDecoder);
                    Thread.Sleep(10);
                }
                
                TheoraPlay.THEORAPLAY_VideoFrame frame = TheoraPlay.getVideoFrame(videoStream);
                
                // We get the FramesPerSecond from the first frame.
                FramesPerSecond = (float) frame.fps;
                Width = (int) frame.width;
                Height = (int) frame.height;
            }
            
            IsDisposed = false;
        }
        #endregion
        
        #region Disposal Method
        public void Dispose()
        {
            // Stop and unassign the decoder.
            if (theoraDecoder != IntPtr.Zero)
            {
                TheoraPlay.THEORAPLAY_stopDecode(theoraDecoder);
                theoraDecoder = IntPtr.Zero;
            }
            
            // Free and unassign the video stream.
            if (videoStream != IntPtr.Zero)
            {
                TheoraPlay.THEORAPLAY_freeVideo(videoStream);
                videoStream = IntPtr.Zero;
            }
            
            // Free and unassign the audio stream.
            if (audioStream != IntPtr.Zero)
            {
                TheoraPlay.THEORAPLAY_freeAudio(audioStream);
                audioStream = IntPtr.Zero;
            }
            
            IsDisposed = true;
        }
        #endregion
    }
}
