// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
