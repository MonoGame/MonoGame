#region License
/*
 Microsoft Public License (Ms-PL)
 MonoGame - Copyright © 2012 The MonoGame Team
 
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
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides a base class for all video objects.
    /// </summary>
    public class VideoContent : ContentItem, IDisposable
    {
        bool disposed;
        int bitsPerSecond;
        TimeSpan duration;
        float framesPerSecond;
        int height;
        int width;

        /// <summary>
        /// Gets the bit rate for this video.
        /// </summary>
        public int BitsPerSecond { get { return bitsPerSecond; } }

        /// <summary>
        /// Gets the duration of this video.
        /// </summary>
        public TimeSpan Duration { get { return duration; } }

        /// <summary>
        /// Gets or sets the file name for this video.
        /// </summary>
        [ContentSerializerAttribute]
        public string Filename { get; set; }

        /// <summary>
        /// Gets the frame rate for this video.
        /// </summary>
        public float FramesPerSecond { get { return framesPerSecond; } }

        /// <summary>
        /// Gets the height of this video.
        /// </summary>
        public int Height { get { return height; } }

        /// <summary>
        /// Gets or sets the type of soundtrack accompanying the video.
        /// </summary>
        [ContentSerializerAttribute]
        public VideoSoundtrackType VideoSoundtrackType { get; set; }

        /// <summary>
        /// Gets the width of this video.
        /// </summary>
        public int Width { get { return width; } }

        /// <summary>
        /// Initializes a new copy of the VideoContent class for the specified video file.
        /// </summary>
        /// <param name="filename">The file name of the video to import.</param>
        public VideoContent(
            string filename
            )
        {
            Filename = filename;
            // TODO: Open video and fill in properties
            // ...
        }

        ~VideoContent()
        {
            Dispose(false);
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TODO: Free managed resources here
                    // ...
                }
                // TODO: Free unmanaged resources here
                // ...
                disposed = true;
            }
        }
    }
}
