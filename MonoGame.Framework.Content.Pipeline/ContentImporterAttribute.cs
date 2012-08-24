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

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides properties that identify and provide metadata about the importer, such as supported file extensions and caching information.
    /// Importers are required to initialize this attribute.
    /// </summary>
    public class ContentImporterAttribute : Attribute
    {
        List<string> extensions = new List<string>();

        /// <summary>
        /// Gets and sets the caching of the content during importation.
        /// </summary>
        public bool CacheImportedData { get; set; }

        /// <summary>
        /// Gets or sets the name of the default processor for content read by this importer.
        /// </summary>
        public string DefaultProcessor { get; set; }

        /// <summary>
        /// Gets or sets the string representing the importer in a user interface. This name is not used by the content pipeline and should not be passed to the BuildAssets task (a custom MSBuild task used by XNA Game Studio). It is used for display purposes only.
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// Gets the supported file name extensions of the importer.
        /// </summary>
        public IEnumerable<string> FileExtensions { get { return extensions; } }

        /// <summary>
        /// Initializes a new instance of ContentImporterAttribute and sets the file name extension supported by the importer.
        /// </summary>
        /// <param name="fileExtension">The list of file name extensions supported by the importer. Prefix each extension with a '.'.</param>
        public ContentImporterAttribute(
            string fileExtension
            )
        {
            extensions.Add(fileExtension);
        }

        /// <summary>
        /// Initializes a new instance of ContentImporterAttribute and sets the file name extensions supported by the importer.
        /// </summary>
        /// <param name="fileExtensions">The list of file name extensions supported by the importer. Prefix each extension with a '.'.</param>
        public ContentImporterAttribute(
            params string[] fileExtensions
            )
        {
            extensions.AddRange(fileExtensions);
        }
    }
}
