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
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reporting informational messages or warnings from content importers and processors.
    /// Do not use this class to report errors. Instead, report errors by throwing a PipelineException or InvalidContentException.
    /// </summary>
    public abstract class ContentBuildLogger
    {
        Stack<string> filenames = new Stack<string>();

        /// <summary>
        /// Gets or sets the base reference path used when reporting errors during the content build process.
        /// </summary>
        public string LoggerRootDirectory { get; set; }

        /// <summary>
        /// Initializes a new instance of ContentBuildLogger.
        /// </summary>
        protected ContentBuildLogger ()
        {
        }

        /// <summary>
        /// Returns the relative path to the filename from the root directory.
        /// </summary>
        /// <param name="filename">The target filename.</param>
        /// <param name="rootDirectory">The root directory. If not specified, the current directory is used.</param>
        /// <returns>The relative path.</returns>
        string GetRelativePath(string filename, string rootDirectory)
        {
            rootDirectory = Path.GetFullPath(string.IsNullOrEmpty(rootDirectory) ? "." : rootDirectory);
            filename = Path.GetFullPath(filename);
            if (filename.StartsWith(rootDirectory))
                return filename.Substring(rootDirectory.Length);
            return filename;
        }

        /// <summary>
        /// Gets the filename currently being processed, for use in warning and error messages.
        /// </summary>
        /// <param name="contentIdentity">Identity of a content item. If specified, GetCurrentFilename uses this value to refine the search. If no value is specified, the current PushFile state is used.</param>
        /// <returns>Name of the file being processed.</returns>
        protected string GetCurrentFilename(
            ContentIdentity contentIdentity
            )
        {
            if ((contentIdentity != null) && !string.IsNullOrEmpty(contentIdentity.SourceFilename))
                return GetRelativePath(contentIdentity.SourceFilename, LoggerRootDirectory);
            if (filenames.Count > 0)
                return GetRelativePath(filenames.Peek(), LoggerRootDirectory);
            return null;
        }

        /// <summary>
        /// Outputs a high-priority status message from a content importer or processor.
        /// </summary>
        /// <param name="message">Message being reported.</param>
        /// <param name="messageArgs">Arguments for the reported message.</param>
        public abstract void LogImportantMessage(
            string message,
            params Object[] messageArgs
            );

        /// <summary>
        /// Outputs a low priority status message from a content importer or processor.
        /// </summary>
        /// <param name="message">Message being reported.</param>
        /// <param name="messageArgs">Arguments for the reported message.</param>
        public abstract void LogMessage(
            string message,
            params Object[] messageArgs
            );

        /// <summary>
        /// Outputs a warning message from a content importer or processor.
        /// </summary>
        /// <param name="helpLink">Link to an existing online help topic containing related information.</param>
        /// <param name="contentIdentity">Identity of the content item that generated the message.</param>
        /// <param name="message">Message being reported.</param>
        /// <param name="messageArgs">Arguments for the reported message.</param>
        public abstract void LogWarning(
            string helpLink,
            ContentIdentity contentIdentity,
            string message,
            params Object[] messageArgs
            );

        /// <summary>
        /// Outputs a message indicating that a content asset has completed processing.
        /// </summary>
        public void PopFile()
        {
            filenames.Pop();
        }

        /// <summary>
        /// Outputs a message indicating that a content asset has begun processing.
        /// All logger warnings or error exceptions from this time forward to the next PopFile call refer to this file.
        /// </summary>
        /// <param name="filename">Name of the file containing future messages.</param>
        public void PushFile(string filename)
        {
            filenames.Push(filename);
        }
    }
}
