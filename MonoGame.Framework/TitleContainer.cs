// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Threading.Tasks;
using MonoGame.Framework.Content;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Provides functionality for opening a stream in the title storage area.
    /// </summary>
    public static partial class TitleContainer
    {
        static partial void PlatformInit();

        static TitleContainer()
        {
            Location = string.Empty;
            PlatformInit();
        }

        /// <summary>
        /// Set this to a valid <see cref="IContentProvider"/> for the <see cref="TitleContainer"/> to check any
        /// content requests by it.
        /// </summary>
        public static IContentProvider ContentProvider { get; set; }

        internal static string Location { get; private set; }

        /// <summary>
        /// Returns an open stream to an existing file in the title storage area.
        /// </summary>
        /// <param name="name">The filepath relative to the title storage area.</param>
        /// <returns>An open stream if a file is found.</returns>
        /// <exception cref="ArgumentNullException">If the name is null or invalid.</exception>
        /// <exception cref="FileNotFoundException">If a file is not found.</exception>
        public static Stream OpenStream(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            // We do not accept absolute paths here.
            if (Path.IsPathRooted(name))
                throw new ArgumentException("Invalid filename. TitleContainer.OpenStream requires a relative path.", name);

            if (ContentProvider != null)
            {
                var task = Task.Run(() => ContentProvider.FetchContent(name));
                task.Wait();
                var response = task.Result;
                if (!response)
                {
                    throw new Exception($"Content client failed to get a valid response for content: {name}");
                }
            }

            // Normalize the file path.
            var safeName = NormalizeRelativePath(name);

            // Call the platform code to open the stream. Any errors at this point should result in a file not found.
            Stream stream = null;
            try
            {
                if (ContentProvider != null)
                {
                    stream = ContentProvider.OpenReadStream(safeName);
                }

                stream ??= PlatformOpenStream(safeName);

                if (stream == null)
                {
                    throw FileNotFoundException(name, null);
                }
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileNotFoundException(name, ex);
            }

            return stream;
        }

        /// <summary>
        /// Faster version of <see cref="OpenStream"/> as it doesn't rely on exceptions in error cases.
        /// </summary>
        internal static Stream OpenStreamNoException(string name)
        {
            if (string.IsNullOrEmpty(name) || Path.IsPathRooted(name))
            {
                return null;
            }

            var safeName = NormalizeRelativePath(name);
            try
            {
                return PlatformOpenStream(safeName);
            }
            catch { }

            return null;
        }

        private static Exception FileNotFoundException(string name, Exception inner)
        {
            return new FileNotFoundException("Error loading \"" + name + "\". File not found.", inner);
        }

        private static string NormalizeRelativePath(string name)
        {
            var uri = new Uri("file:///" + FileHelpers.UrlEncode(name));
            return uri.LocalPath[1..].Replace(FileHelpers.NotSeparator, FileHelpers.Separator);
        }
    }
}

