// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Tasks
{
    /// <summary>
    /// Provides methods and properties for importing and processing game assets into a binary format.
    /// </summary>
    public class BuildContent : Task
    {
        /// <summary>
        /// The format specifier for the named event used to cancel the build.
        /// </summary>
        public const string CancelEventNameFormat = "TODO";

        /// <summary>
        /// Gets or sets the content build configuration name.
        /// </summary>
        /// <value>Name of the configuration.</value>
        public string BuildConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the content compression flag.
        /// </summary>
        /// <value>If true, all content types that permit compression will be compressed when built (the default). If false, no content will be compressed.</value>
        public bool CompressContent { get; set; }

        /// <summary>
        /// Gets or sets the directory for storing temporary build files.
        /// </summary>
        /// <value>Directory containing the intermediate build files.</value>
        public string IntermediateDirectory { get; set; }

        /// <summary>
        /// Gets all file names produced by the build, regardless of any incremental optimizations. This list can be used as input for a subsequent pack file generator task.
        /// </summary>
        /// <value>Array of file names produced by the content build.</value>
        [OutputAttribute]
        public ITaskItem[] IntermediateFiles { get; internal set; }

        /// <summary>
        /// Gets or sets the base reference path used when reporting errors during the content build process.
        /// </summary>
        /// <value>Current name of the base directory or the value to be set.</value>
        public string LoggerRootDirectory { get; set; }

        /// <summary>
        /// Gets all file names produced by the build, regardless of any incremental optimizations. This list can be used as input for a subsequent pack file generator task.
        /// </summary>
        /// <value>Array of file names produced by the content build.</value>
        [OutputAttribute]
        public ITaskItem[] OutputContentFiles { get; internal set; }

        /// <summary>
        /// Gets or sets the output directory for the final build results.
        /// </summary>
        /// <value>Output directory for final build result files</value>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the names of assemblies that provide Importer<T> or ContentProcessor<T> components for use by the build.
        /// </summary>
        /// <value>Current pipeline assemblies or the values to be set.</value>
        [RequiredAttribute]
        public ITaskItem[] PipelineAssemblies { get; set; }

        /// <summary>
        /// Gets or sets the dependencies of the pipeline assemblies.
        /// </summary>
        /// <value>Array of content build dependencies.</value>
        public ITaskItem[] PipelineAssemblyDependencies { get; set; }

        /// <summary>
        /// Gets or sets the force rebuild flag.
        /// </summary>
        /// <value>Current value of the force rebuild flag.
        ///
        /// If true, all content is rebuilt (even when incremental checks indicate everything is up to date). The default value is false.</value>
        public bool RebuildAll { get; set; }

        /// <summary>
        /// Gets the list of file names modified by an incremental rebuild. This list is suitable for passing to a subsequent incremental deploy or reload notification task.
        /// </summary>
        /// <value>Array of file names modified by an incremental rebuild.</value>
        [OutputAttribute]
        public ITaskItem[] RebuiltContentFiles { get; internal set; }

        /// <summary>
        /// Gets or sets the base path for the entire content build process.
        /// </summary>
        /// <value>Base path of the content build process.</value>
        public string RootDirectory { get; set; }

        /// <summary>
        /// Gets or sets the source asset files to be built.
        /// </summary>
        /// <value>Current source asset files to be built or the values to be set.</value>
        [RequiredAttribute]
        public ITaskItem[] SourceAssets { get; set; }

        /// <summary>
        /// Gets or sets the content build target platform.
        /// This should be one of the values of the TargetPlatform Enumeration enumeration.
        /// </summary>
        /// <value>Target of the content build. For a list of possible values, see TargetPlatform Enumeration.</value>
        [RequiredAttribute]
        public string TargetPlatform { get; set; }

        /// <summary>
        /// Gets or sets the target graphics profile.
        /// </summary>
        /// <value>Target graphics profile of the content build. For a list of possible values, see GraphicsProfile Enumeration.</value>
        [RequiredAttribute]
        public string TargetProfile { get; set; }

        /// <summary>
        /// Initializes a new instance of BuildContent.
        /// </summary>
        public BuildContent()
        {

        }

        /// <summary>
        /// Executes the related build task.
        /// </summary>
        /// <returns>true if the task completed successfully; false otherwise.</returns>
        public override bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
