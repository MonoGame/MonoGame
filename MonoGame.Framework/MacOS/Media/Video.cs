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

using System;
using System.IO;

using MonoMac.ObjCRuntime;
using MonoMac.QTKit;
using MonoMac.Foundation;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media
{
    public sealed class Video : IDisposable
    {
		private string _fileName;
		private Color _backColor = Color.Black;
		private QTMovie mMovie;
		private QTMovieView mMovieView;
		
		internal Video(string FileName)
		{
			_fileName = FileName;
			Prepare();
		}
				
		public Color BackgroundColor
		{
			set
			{
				_backColor = value;
			}
			get
			{
				return _backColor;
			}
		}
		
		public string FileName
		{
			get 
			{
				return _fileName;
			}
		}
		
		internal static string Normalize(string FileName)
		{
			if (File.Exists(FileName))
				return FileName;
			
			// Check the file extension
			if (!string.IsNullOrEmpty(Path.GetExtension(FileName)))
			{
				return null;
			}
			
			// Concat the file name with valid extensions
			if (File.Exists(FileName+".mp4"))
				return FileName+".mp4";
			if (File.Exists(FileName+".mov"))
				return FileName+".mov";
			if (File.Exists(FileName+".avi"))
				return FileName+".avi";
			if (File.Exists(FileName+".m4v"))
				return FileName+".m4v";
			if (File.Exists(FileName+".3gp"))
				return FileName+".3gp";
			
			
			return null;
		}
		
		internal void Prepare()
		{
			NSError err = new NSError();
			
			mMovie = new QTMovie(_fileName, out err);
			if (mMovie != null)				
			{
				mMovieView = new QTMovieView();
				mMovieView.Movie = mMovie;
				
				mMovieView.IsControllerVisible = false;				
			}
			else
			{
				Console.WriteLine(err);
			}
		}
		
		internal QTMovieView MovieView
		{
			get
			{
				return mMovieView;
			}
		}
		
		internal float Volume
		{
			get
			{
				return mMovie.Volume;
			}
			set
			{
				// TODO When Xamarain fix the set Volume mMovie.Volume = value;
			}
		}
		
		internal TimeSpan Duration
		{
			get
			{
				return new TimeSpan( mMovie.Duration.TimeValue );
			}
			
		}
		
		internal TimeSpan CurrentPosition
		{
			get
			{
				return new TimeSpan( mMovie.CurrentTime.TimeValue );
			}
			
		}
		
		public void Dispose()
		{
			if (mMovieView != null)
			{
				mMovieView.Dispose();
				mMovieView = null;
			} 
			
			if (mMovie != null)
			{
				mMovie.Dispose();
				mMovie = null;
			} 
		}
    }
}