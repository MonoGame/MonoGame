// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
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
            return Update(pipelineAssemblies, null);
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
            throw new NotImplementedException();
        }
    }
}
