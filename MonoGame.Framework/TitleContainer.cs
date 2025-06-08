// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Provides functionality for opening a stream in the title storage area.
    /// </summary>
    public static partial class TitleContainer
    {
        private const string CACHE_PATH = "MonoGameCache";
        private const string MODIFIED_TIMES_FILENAME = "ModifiedTimes.yaml";

        static partial void PlatformInit();

        static TitleContainer()
        {
            Client = new();
            Location = string.Empty;
            CheckContentServer = false;
            ContentServerAddress = "http://localhost:7771/";
            PlatformInit();
            ModifiedTimes = LoadModifiedTimes();
        }

        /// <summary>
        /// Determines if the assets will first be attempted to be loaded from a content server.
        /// </summary>
        /// <value><c>false</c> by default.</value>
        public static bool CheckContentServer { get; set; }

        /// <summary>
        /// The location of the content server for loading assets. To use it enable <see cref="CheckContentServer"/>.
        /// </summary>
        /// <value><c>http://localhost:7771/</c> by default.</value>
        public static string ContentServerAddress { get; set; }

        internal static string Location { get; private set; }

        private static Dictionary<string, long> ModifiedTimes { get; set; }

        private static HttpClient Client { get; set; }

        private static Dictionary<string, long> LoadModifiedTimes()
        {
            using var fileStream = PlatformOpenStream(MODIFIED_TIMES_FILENAME);
            if (fileStream == null)
            {
                return [];
            }

            var ret = new Dictionary<string, long>();
            using var reader = new StreamReader(fileStream);
            foreach (var line in reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var split = line.Trim().Split(": ");
                if (split.Length == 2)
                {
                    ret[split[0]] = long.Parse(split[1]);
                }
            }

            return ret;
        }

        private static void SaveModifiedTimes()
        {
            using var fileStream = PlatformOpenWriteStream(MODIFIED_TIMES_FILENAME);
            using var streamWritter = new StreamWriter(fileStream);
            foreach (var pair in ModifiedTimes)
            {
                streamWritter.WriteLine($"{pair.Key}: {pair.Value}");
            }
        }

        private static async Task<bool> CheckServerForContent(string relativePath)
        {
            if (!CheckContentServer)
            {
                return true;
            }

            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("Path", relativePath);
            if (ModifiedTimes.TryGetValue(relativePath, out long lastModifiedTime))
            {
                Client.DefaultRequestHeaders.Add("LastModifiedTime", lastModifiedTime.ToString());
            }

            try
            {
                using var stream = await Client.GetStreamAsync(ContentServerAddress);
                byte[] lastModifiedTimeBytes = new byte[sizeof(long)];
                var readBytes = stream.Read(lastModifiedTimeBytes, 0, lastModifiedTimeBytes.Length);
                if (readBytes != lastModifiedTimeBytes.Length)
                {
                    // if we just recieved a 0 that means the server does not have the file
                    // no error has occured while processing this request so return true
                    // and let the client try to find the file in its storage.
                    return readBytes == 1 && lastModifiedTimeBytes[0] == 0;
                }

                using var fileStream = PlatformOpenWriteStream(relativePath);
                stream.CopyTo(fileStream);

                ModifiedTimes[relativePath] = BitConverter.ToInt64(lastModifiedTimeBytes);
                SaveModifiedTimes();

                return true;
            }
            catch (HttpRequestException)
            {
                return true;
            }
            catch (SocketException)
            {
                return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Returns an open stream to an existing file in the title storage area.
        /// </summary>
        /// <param name="name">The filepath relative to the title storage area.</param>
        /// <returns>An open stream if file is found.</returns>
        /// <exception cref="ArgumentNullException">If name is null or invalid.</exception>
        /// <exception cref="FileNotFoundException">If file is not found.</exception>
        public static Stream OpenStream(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // We do not accept absolute paths here.
            if (Path.IsPathRooted(name))
                throw new ArgumentException("Invalid filename. TitleContainer.OpenStream requires a relative path.", name);

            if (CheckContentServer)
            {
                var task = Task.Run(() => CheckServerForContent(name));
                task.Wait();
                var response = task.Result;
                if (!response)
                {
                    throw new Exception($"Could not prepare content: {name}");
                }
            }

            // Normalize the file path.
            var safeName = NormalizeRelativePath(name);

            // Call the platform code to open the stream.  Any errors
            // at this point should result in a file not found.
            Stream stream;
            try
            {
                stream = PlatformOpenStream(safeName);
                if (stream == null)
                    throw FileNotFoundException(name, null);
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

            string safeName = NormalizeRelativePath(name);
            Stream stream;
            try
            {
                stream = PlatformOpenStream(safeName);

                return stream;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Exception FileNotFoundException(string name, Exception inner)
        {
            return new FileNotFoundException("Error loading \"" + name + "\". File not found.", inner);
        }

        internal static string NormalizeRelativePath(string name)
        {
            var uri = new Uri("file:///" + FileHelpers.UrlEncode(name));
            var path = uri.LocalPath;
            path = path.Substring(1);
            return path.Replace(FileHelpers.NotSeparator, FileHelpers.Separator);
        }
    }
}

