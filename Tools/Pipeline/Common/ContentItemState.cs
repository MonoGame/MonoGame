// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Tools.Pipeline
{
    /// <summary>
    /// Snapshot of a ContentItem's state, used for undo/redo.
    /// </summary>
    internal class ContentItemState
    {
        public BuildAction BuildAction;
        public string SourceFile;
        public string ImporterName;
        public string ProcessorName;
        public OpaqueDataDictionary ProcessorParams;

        /// <summary>
        /// Create a ContentItemState storing member values of the passed ContentItem.
        /// </summary>        
        public static ContentItemState Get(ContentItem item)
        {
            var state = new ContentItemState()
                {
                    BuildAction = item.BuildAction,
                    SourceFile = item.OriginalPath,
                    ImporterName = item.ImporterName,
                    ProcessorName = item.ProcessorName,
                    ProcessorParams = new OpaqueDataDictionary(),
                };

            foreach (var pair in item.ProcessorParams)
            {
                state.ProcessorParams[pair.Key] = pair.Value;
            }

            return state;
        }

        /// <summary>
        /// Set a ContentItem's member values from this state object.
        /// </summary>
        public void Apply(ContentItem item)
        {
            item.BuildAction = BuildAction;
            item.OriginalPath = SourceFile;
            item.ImporterName = ImporterName;
            item.ProcessorName = ProcessorName;
            item.ProcessorParams = new OpaqueDataDictionary();

            foreach (var pair in ProcessorParams)
            {
                item.ProcessorParams[pair.Key] = pair.Value;
            }
        }
    }    
}
