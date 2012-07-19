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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Tasks
{
    /// <summary>
    /// An MSBuild task for deleting all the intermediate and output files that were created by a previous Content Pipeline build operation.
    /// </summary>
    public class CleanContent : Task
    {
        /// <summary>
        /// Gets or sets the content build configuration name.
        /// </summary>
        /// <value>Name of the configuration.</value>
        public string BuildConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the directory for storing temporary build files.
        /// </summary>
        /// <value>Directory containing the intermediate build files.</value>
        public string IntermediateDirectory { get; set; }

        /// <summary>
        /// Gets or sets the output directory for the final build results.
        /// </summary>
        /// <value>Output directory for final build result files</value>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the base path for the entire content build process.
        /// </summary>
        /// <value>Base path of the content build process.</value>
        public string RootDirectory { get; set; }

        /// <summary>
        /// Gets or sets the content build target platform.
        /// </summary>
        /// <value>The content build target platform.</value>
        [RequiredAttribute]
        public string TargetPlatform { get; set; }

        /// <summary>
        /// Gets or sets the target graphics profile.
        /// </summary>
        /// <value>Target graphics profile of the content build. For a list of possible values, see GraphicsProfile Enumeration.</value>
        [RequiredAttribute]
        public string TargetProfile { get; set; }

        /// <summary>
        /// Instantiates a new instance of this MSBuild task for deleting all the intermediate and output files that were created by a previous Content Pipeline build operation.
        /// </summary>
        public CleanContent()
        {

        }

        /// <summary>
        /// Removes all intermediate and output files that were created by a previous Content Pipeline build operation.
        /// </summary>
        /// <returns>true if errors were logged; false otherwise.</returns>
        public override bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
