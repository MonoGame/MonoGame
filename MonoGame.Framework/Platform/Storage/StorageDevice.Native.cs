using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Storage
{
    partial class StorageDevice
    {
        /// <summary>
        /// Gets the root directory path for storing application data, adapted to the operating system's conventions.
        /// </summary>
        /// <remarks>
        /// This property determines the appropriate storage location for application data based on the current operating system:
        /// <list type="bullet">
        /// <item>
        /// <term>Linux:</term>
        /// <description>
        /// Returns the value of the <c>XDG_DATA_HOME</c> environment variable if set; 
        /// otherwise, it defaults to <c>$HOME/.local/share</c>. If the home directory cannot be determined, 
        /// it falls back to the current working directory ("<c>.</c>").
        /// </description>
        /// </item>
        /// <item>
        /// <term>macOS:</term>
        /// <description>
        /// Returns the user's <c>Library/Application Support</c> directory. If the home directory is unavailable,
        /// it falls back to the current working directory ("<c>.</c>").
        /// </description>
        /// </item>
        /// <item>
        /// <term>Windows:</term>
        /// <description>
        /// Returns the path to the user's <c>LocalApplicationData</c> folder (e.g., <c>C:\Users\Username\AppData\Local</c>).
        /// </description>
        /// </item>
        /// </list>
        /// This approach ensures that application data is stored in a location consistent with each operating system's guidelines.
        /// </remarks>
        /// <returns>
        /// A string representing the root directory path for application data storage.
        /// </returns>
        internal static string StorageRoot
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string osConfigDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                    if (string.IsNullOrEmpty(osConfigDir))
                    {
                        string homeDir = Environment.GetEnvironmentVariable("HOME");
                        if (string.IsNullOrEmpty(homeDir))
                        {
                            return "."; // Fallback to current directory
                        }
                        osConfigDir = Path.Combine(homeDir, ".local", "share");
                    }
                    return osConfigDir;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    string osConfigDir = Environment.GetEnvironmentVariable("HOME");
                    if (string.IsNullOrEmpty(osConfigDir))
                    {
                        return "."; // Fallback to current directory
                    }
                    osConfigDir = Path.Combine(osConfigDir, "Library", "Application Support");
                    return osConfigDir;
                }
                else // Windows?
                {
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
            }
        }
    }
}