// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Tools.Pipeline
{
    internal enum BuildAction
    {
        Build,
        Copy,
    }

    internal class ContentItem : IProjectItem
    {
        public IController Controller;

        public string SourceFile;
        public string ImporterName;
        public string ProcessorName;
        public OpaqueDataDictionary ProcessorParams;

        private ImporterTypeDescription _importer;
        private ProcessorTypeDescription _processor;
        private BuildAction _buildAction;

        #region IProjectItem

        public string Name 
        { 
            get
            {
                return System.IO.Path.GetFileName(SourceFile);
            }
        }

        public string Location
        {
            get
            {
                return System.IO.Path.GetDirectoryName(SourceFile);
            }
        }

        [Browsable(false)]
        public string Icon { get; set; }

        #endregion

        public BuildAction BuildAction
        {
            get { return _buildAction; }
            set
            {
                if (_buildAction == value)
                    return;

                _buildAction = value;

                if (Controller != null)
                    Controller.OnItemModified(this);
            }
        }

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

                if (Controller != null)
                    Controller.OnItemModified(this);
            }
        }

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

                if (Controller != null)
                    Controller.OnItemModified(this);

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
                _importer = PipelineTypes.FindImporter(ImporterName, System.IO.Path.GetExtension(SourceFile));
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

                            var converter = TypeDescriptor.GetConverter(p.Type);

                            // Should we throw an exception here?
                            // This property will actually not be editable in the property grid
                            // since we do not have a type converter for it.
                            if (converter.CanConvertFrom(srcType))
                            {
                                var dst = converter.ConvertFrom(src);
                                ProcessorParams[p.Name] = dst;
                            }
                        }
                    }
                }
            }
        }        

        public override string ToString()
        {
            return System.IO.Path.GetFileName(SourceFile);
        }
    }

    //internal class EditerContentItem
    //{
    //    private ContentItem _contentItem;
    //    private ImporterTypeDescription _importer;

    //    public EditerContentItem(ContentItem item)
    //    {
    //        _contentItem = item;
    //    }

    //    public string Name { get { return _contentItem.Label; } }

    //    public ImporterTypeDescription Importer
    //    {
    //        get
    //        {
    //            return _importer;
    //        }

    //        set
    //        {
    //            _importer = value;
    //        }
    //    }
    //}

    //internal class ContentItemTypeDescriptor : ICustomTypeDescriptor
    //{
    //    private readonly ContentItem _target;
    //    private readonly Type _targetType;
    //    private readonly PropertyDescriptorCollection _propCache;

    //    public ContentItemTypeDescriptor(ContentItem obj)
    //    {
    //        _target = obj;
    //        _targetType = obj.GetType();

    //        _propCache = new PropertyDescriptorCollection(null);
    //        foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(_target, null, true))
    //        {
    //            _propCache.Add(prop);
    //        }
    //    }

    //    object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
    //    {
    //        return _target;
    //    }

    //    AttributeCollection ICustomTypeDescriptor.GetAttributes()
    //    {
    //        return TypeDescriptor.GetAttributes(_target, true);
    //    }

    //    string ICustomTypeDescriptor.GetClassName()
    //    {
    //        return TypeDescriptor.GetClassName(_target, true);
    //    }

    //    public string GetComponentName()
    //    {
    //        return TypeDescriptor.GetComponentName(_target);
    //    }

    //    public TypeConverter GetConverter()
    //    {
    //        return TypeDescriptor.GetConverter(_target);
    //    }

    //    public EventDescriptor GetDefaultEvent()
    //    {
    //        return TypeDescriptor.GetDefaultEvent(_target);
    //    }

    //    public PropertyDescriptor GetDefaultProperty()
    //    {
    //        return TypeDescriptor.GetDefaultProperty(_target);
    //    }

    //    public object GetEditor(Type editorBaseType)
    //    {
    //        return TypeDescriptor.GetEditor(_target, editorBaseType);
    //    }

    //    public EventDescriptorCollection GetEvents()
    //    {
    //        return TypeDescriptor.GetEvents(_target);
    //    }

    //    public EventDescriptorCollection GetEvents(Attribute[] attributes)
    //    {
    //        return TypeDescriptor.GetEvents(_target, attributes);
    //    }       

    //    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
    //    {
    //        return ((ICustomTypeDescriptor)this).GetProperties(null);
    //    }

    //    public virtual PropertyDescriptorCollection GetProperties(Attribute[] attributes)
    //    {
    //        var results = new PropertyDescriptorCollection(null);
    //        results.Add( new PropertyDescriptor())
    //        foreach (var i in TypeDescriptor.GetProperties(_target, null, true))
    //        {
    //            _propCache.Add(prop);
    //        }

    //        // We currently ignore the attributes parameter b/c we don't need it and it complicates the implementation here.
    //        return _propCache;
    //    }
    //}
}
