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

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Implements a scanner object containing the available importers and processors for an application. Designed for internal use only.
    /// </summary>
    public sealed class PipelineComponentScanner
    {
        List<string> errors = new List<string>();
        Dictionary<string, ContentImporterAttribute> importerAttributes = new Dictionary<string, ContentImporterAttribute>();
        Dictionary<string, ContentProcessorAttribute> processorAttributes = new Dictionary<string, ContentProcessorAttribute>();
        Dictionary<string, string> importerOutputTypes = new Dictionary<string, string>();
        Dictionary<string, string> processorInputTypes = new Dictionary<string, string>();
        Dictionary<string, string> processorOutputTypes = new Dictionary<string, string>();
        Dictionary<string, ProcessorParameterCollection> processorParameters = new Dictionary<string, ProcessorParameterCollection>();

        /// <summary>
        /// Gets the list of error messages produced by the last call to Update.
        /// </summary>
        public IList<string> Errors { get { return errors; } }

        /// <summary>
        /// Gets a dictionary that maps importer names to their associated metadata attributes.
        /// </summary>
        public IDictionary<string, ContentImporterAttribute> ImporterAttributes { get { return importerAttributes; } }

        /// <summary>
        /// Gets the names of all available importers.
        /// </summary>
        public IEnumerable<string> ImporterNames { get { return importerAttributes.Keys; } }

        /// <summary>
        /// Gets a dictionary that maps importer names to the fully qualified name of their return types.
        /// </summary>
        public IDictionary<string, string> ImporterOutputTypes { get { return importerOutputTypes; } }

        /// <summary>
        /// Gets a dictionary that maps processor names to their associated metadata attributes.
        /// </summary>
        public IDictionary<string, ContentProcessorAttribute> ProcessorAttributes { get { return processorAttributes; } }

        /// <summary>
        /// Gets a dictionary that maps processor names to the fully qualified name of supported input types.
        /// </summary>
        public IDictionary<string, string> ProcessorInputTypes { get { return processorInputTypes; } }

        /// <summary>
        /// Gets the names of all available processors.
        /// </summary>
        public IEnumerable<string> ProcessorNames { get { return processorAttributes.Keys; } }

        /// <summary>
        /// Gets a dictionary that maps processor names to the fully qualified name of their output types.
        /// </summary>
        public IDictionary<string, string> ProcessorOutputTypes { get { return processorOutputTypes; } }

        /// <summary>
        /// A collection of supported processor parameters.
        /// </summary>
        public IDictionary<string, ProcessorParameterCollection> ProcessorParameters { get { return processorParameters; } }

        /// <summary>
        /// Initializes a new instance of PipelineComponentScanner.
        /// </summary>
        public PipelineComponentScanner()
        {
        }

        /// <summary>
        /// Updates the scanner object with the latest available assembly states.
        /// </summary>
        /// <param name="pipelineAssemblies">Enumerated list of available assemblies.</param>
        /// <returns>true if an actual scan was required, indicating the collection contents may have changed. false if no assembly changes were detected since the previous call.</returns>
        public bool Update(
            IEnumerable<string> pipelineAssemblies
            )
        {
            // TODO: Implement me
            // ...
            return false;
        }

        /// <summary>
        /// Updates the scanner object with the latest available assembly states.
        /// </summary>
        /// <param name="pipelineAssemblies">Enumerated list of available assemblies.</param>
        /// <param name="pipelineAssemblyDependencies">Enumerated list of dependent assemblies.</param>
        /// <returns>true if an actual scan was required, indicating the collection contents may have changed. false if no assembly changes were detected since the previous call.</returns>
        public bool Update(
            IEnumerable<string> pipelineAssemblies,
            IEnumerable<string> pipelineAssemblyDependencies
            )
        {
            return Update(pipelineAssemblies, null);
        }
    }
}
