// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.ComponentModel;
using System.Globalization;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Builder.Convertors;

namespace MonoGame.Tools.Pipeline
{
    public enum BuildAction
    {
        Build,
        Copy,
    }

    public class ContentItem : IProjectItem
    {
        public IContentItemObserver Observer;
        
        public string ImporterName;
        public string ProcessorName;
        public OpaqueDataDictionary ProcessorParams;

        private ImporterTypeDescription _importer;
        private ProcessorTypeDescription _processor;
        private BuildAction _buildAction;

        #region IProjectItem

        [Browsable(false)]
        public string OriginalPath { get; set; }

        [Browsable(false)]
        public string DestinationPath { get; set; }

        [Category("Common")]
        [Description("The file name of this item.")]
        public string Name 
        { 
            get
            {
                return System.IO.Path.GetFileName(OriginalPath);
            }
        }

        [Category("Common")]
        [Description("The folder where this item is located.")]
        public string Location
        {
            get
            {
                return System.IO.Path.GetDirectoryName(OriginalPath);
            }
        }

        #endregion

        [Category("Settings")]
        [DisplayName("Build Action")]
        [Description("The way to process this content item.")]
        public BuildAction BuildAction
        {
            get { return _buildAction; }
            set
            {
                if (_buildAction == value)
                    return;

                _buildAction = value;

                if (Observer != null)
                    Observer.OnItemModified(this);
            }
        }

        [Category("Settings")]
        [Description("The importer used to load the content file.")]
        [TypeConverter(typeof(ImporterConverter))]
        public ImporterTypeDescription Importer
        {
            get { return _importer; }

            set
            {
                if (_importer == value)
                    return;

                _importer = value;
                ImporterName = _importer.TypeName;                

                // Validate that our processor can accept input content of the type output by the new importer.
                if ((_processor == null || _processor.InputType != _importer.OutputType) && _processor != PipelineTypes.MissingProcessor)
                {
                    // If it cannot, set the default processor.
                    Processor = PipelineTypes.FindProcessor(_importer.DefaultProcessor, _importer);
                }

                if (Observer != null)
                    Observer.OnItemModified(this);
            }
        }

        [Category("Settings")]
        [Description("The processor used to transform the content for runtime use.")]
        [TypeConverter(typeof(ProcessorConverter))]
        public ProcessorTypeDescription Processor
        {
            get { return _processor; }

            set
            {
                if (_processor == value)
                    return;
                
                _processor = value;
                ProcessorName = _processor.TypeName;
                
                // When the processor changes reset our parameters
                // to the default for the processor type.
                ProcessorParams.Clear();
                foreach (var p in _processor.Properties)
                {
                    ProcessorParams.Add(p.Name, p.DefaultValue);
                }

                if (Observer != null)
                    Observer.OnItemModified(this);

                // Note:
                // There is no need to validate that the new processor can accept input
                // of the type output by our importer, because that should be handled by
                // only showing valid processors in the drop-down (eg, within ProcessConverter).
            }
        }

        public void ResolveTypes()
        {
            if (BuildAction == BuildAction.Copy)
            {
                // Copy items do not have importers or processors.
                _importer = PipelineTypes.NullImporter;
                _processor = PipelineTypes.NullProcessor;
            }
            else
            {
                _importer = PipelineTypes.FindImporter(ImporterName, System.IO.Path.GetExtension(OriginalPath));
                if (_importer != null && (string.IsNullOrEmpty(ImporterName) || ImporterName != _importer.TypeName))
                    ImporterName = _importer.TypeName;

                if (_importer == null)
                    _importer = PipelineTypes.MissingImporter;

                _processor = PipelineTypes.FindProcessor(ProcessorName, _importer);
                if (_processor != null && (string.IsNullOrEmpty(ProcessorName) || ProcessorName != _processor.TypeName))
                    ProcessorName = _processor.TypeName;

                if (_processor == null)
                    _processor = PipelineTypes.MissingProcessor;

                // ProcessorParams get deserialized as strings
                // this code converts them to object(s) of their actual type
                // so that the correct editor appears within the property grid.
                foreach (var p in _processor.Properties)
                {
                    if (!ProcessorParams.ContainsKey(p.Name))
                    {
                        ProcessorParams[p.Name] = p.DefaultValue;
                    }
                    else
                    {
                        var src = ProcessorParams[p.Name];
                        if (src != null)
                        {
                            var srcType = src.GetType();

                            var converter = PipelineTypes.FindConverter(p.Type);

                            // Should we throw an exception here?
                            // This property will actually not be editable in the property grid
                            // since we do not have a type converter for it.
                            if (converter.CanConvertFrom(srcType))
                            {
                                var dst = converter.ConvertFrom(null, CultureInfo.InvariantCulture, src);
                                ProcessorParams[p.Name] = dst;
                            }
                        }
                    }
                }
            }
        }        

        public override string ToString()
        {
            return System.IO.Path.GetFileName(OriginalPath);
        }
    }
}
