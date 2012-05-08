#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.IO;

#if ANDROID
using System.Linq;
#endif

#if WINRT
using Windows.Storage;
#endif

namespace Microsoft.Xna.Framework.Content
{
    public abstract class ContentTypeReader
    {
        #region Private Member Variables

        private Type targetType;
#if ANDROID
		// Keep this static so we only call Game.Activity.Assets.List() once
		// No need to call it for each file if the list will never change
		static string[] files = null;
#endif

        #endregion Private Member Variables


        #region Public Properties

        public Type TargetType
        {
            get { return this.targetType; }
        }

        public virtual int TypeVersion
        {
            get { return 0; }   // The default version (unless overridden) is zero
        }

        #endregion Public Properties


        #region Protected Constructors

        protected ContentTypeReader(Type targetType)
        {
            this.targetType = targetType;
        }

        #endregion Protected Constructors


        #region Protected Methods

        protected internal virtual void Initialize(ContentTypeReaderManager manager)
        {
            // Do nothing. Are we supposed to add ourselves to the manager?
        }

        protected internal abstract object Read(ContentReader input, object existingInstance);

        #endregion Protected Methods

        #region Internal Static Helper Methods
#if ANDROID
        internal static string Normalize(string fileName, string[] extensions)
        {
            int index = fileName.LastIndexOf(Path.DirectorySeparatorChar);
            string path = string.Empty;
            string file = fileName;
            if (index >= 0)
            {
                file = fileName.Substring(index + 1, fileName.Length - index - 1);
                path = fileName.Substring(0, index);
            }

			// Only read the assets file list once
			if (files == null)
				files = Game.Activity.Assets.List(path);

            if (files.Any(s => s == file))
                return fileName;

            // Check the file extension
            if (!string.IsNullOrEmpty(Path.GetExtension(fileName)))
            {
                return null;
            }

			// FirstOrDefault returns null as the default if the file is not found. This crashed Path.Combine so check
			// for it first.
			string file2 = files.FirstOrDefault(s => extensions.Any(ext => s.ToLower() == (file.ToLower() + ext)));
			if (String.IsNullOrEmpty(file2))
				return null;
            return Path.Combine(path, file2);
        }
#else
		public static string Normalize(string fileName, string[] extensions)
		{
#if WINRT
            if (MetroHelper.AppDataFileExists(fileName))
                return fileName;
#else
            if (File.Exists(fileName))
				return fileName;
#endif
			// Check the file extension
			if (!string.IsNullOrEmpty(Path.GetExtension(fileName)))
				return null;
			
            foreach (string ext in extensions)
            {
			    // Concat the file name with valid extensions
                string fileNamePlusExt = fileName + ext;

#if WINRT
                if (MetroHelper.AppDataFileExists(fileNamePlusExt))
                    return fileNamePlusExt;
#else
			    if (File.Exists(fileNamePlusExt))
				    return fileNamePlusExt;
#endif
            }
			
			return null;
		}
#endif
        #endregion
    }

    public abstract class ContentTypeReader<T> : ContentTypeReader
    {
        #region Protected Constructors

        protected ContentTypeReader()
            : base(typeof(T))
        {
            // Nothing
        }

        #endregion Protected Constructors


        #region Protected Methods

        protected internal override object Read(ContentReader input, object existingInstance)
        {
			// as per the documentation http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.content.contenttypereader.read.aspx
			// existingInstance
			// The object receiving the data, or null if a new instance of the object should be created.
			if (existingInstance == null) {
				return this.Read (input, default(T));
			} 
			else {
				return this.Read (input, (T)existingInstance);
			}

		//return Read(input, (T)existingInstance);
        }

        protected internal abstract T Read(ContentReader input, T existingInstance);

        #endregion Protected Methods
    }
}