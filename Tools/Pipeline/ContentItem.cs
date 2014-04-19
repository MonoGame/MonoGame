using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Tools.Pipeline
{
    internal class ContentItem : IProjectItem
    {
        public string SourceFile;
        public string ImporterName;
        public string ProcessorName;
        public OpaqueDataDictionary ProcessorParams;

        private Processor _processor;
        private Importer _importer;

        [TypeConverter(typeof(ImporterConverter))]
        public Importer Importer
        {
            get
            {
                if (_importer == null)
                {
                    _importer = new Importer();
                    _importer.Name = ImporterName;
                }

                return _importer;
            }

            set
            {
                _importer = value;
                ImporterName = _importer.Name;
            }
        }

        [TypeConverter(typeof(ProcessorConverter))]
        public Processor Processor
        {
            get
            {
                if (_processor == null)
                {
                    _processor = new Processor();
                    _processor.Data = ProcessorParams;
                    _processor.Name = ProcessorName;
                }

                return _processor;
            }

            set
            {
                _processor = value;
                _processor.Data = ProcessorParams;
                ProcessorName = _processor.Name;
            }
        }

        public string Label 
        { 
            get
            {
                return System.IO.Path.GetFileName(SourceFile);
            }
        }

        public string Path
        {
            get
            {
                return System.IO.Path.GetDirectoryName(SourceFile);
            }
        }

        public string Icon { get; private set; }
    }
}
