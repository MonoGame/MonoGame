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
