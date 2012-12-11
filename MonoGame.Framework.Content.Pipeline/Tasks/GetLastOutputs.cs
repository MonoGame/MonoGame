// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Tasks
{
    /// <summary>
    /// Provides methods and properties for getting the names of all output content files from the content pipeline's cache file.
    /// </summary>
    public class GetLastOutputs : Task
    {
        /// <summary>
        /// Gets or sets the directory containing the cache file to be retrieved.
        /// </summary>
        /// <value>Path of the retrieved cache file.</value>
        [RequiredAttribute]
        public string IntermediateDirectory { get; set; }

        /// <summary>
        /// Gets the names of the output content files. This information may be out of date if a recent build was not completed. The collection is empty if there were no outputs or no cached information was found.
        /// </summary>
        /// <value>Collection of cache file names.</value>
        [OutputAttribute]
        public ITaskItem[] OutputContentFiles { get; internal set; }

        /// <summary>
        /// Creates a new instance of GetLastOutputs.
        /// </summary>
        public GetLastOutputs()
        {

        }

        /// <summary>
        /// Executes the related task using MSBuild.
        /// </summary>
        /// <returns>true if the task completed successfully; false otherwise.</returns>
        public override bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
