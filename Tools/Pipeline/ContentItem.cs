using System;
using System.Collections.Generic;
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

        private ImporterTypeDescription _importer;
        private ProcessorTypeDescription _processor;
        private List<ProcessorTypeDescription.Property> _processorProperties;

        public void ResolveTypes()
        {
            Importer = PipelineTypes.FindImporter(ImporterName, System.IO.Path.GetExtension(SourceFile));
            Processor = PipelineTypes.FindProcessor(ProcessorName, _importer);

            foreach (var p in Processor.Properties)
            {
                if (!ProcessorParams.ContainsKey(p.Name))
                {
                    ProcessorParams[p.Name] = p.DefaultValue;
                }
                else
                {
                    ProcessorParams[p.Name] = TypeDescriptor.GetConverter(p.TypeName).ConvertFrom(ProcessorParams[p.Name]);
                }
            }
        }

        [TypeConverter(typeof (ImporterConverter))]
        public ImporterTypeDescription Importer
        {
            get { return _importer; }

            set
            {
                _importer = value;
                ImporterName = _importer.TypeName;
            }
        }

        [TypeConverter(typeof (ProcessorConverter))]
        public ProcessorTypeDescription Processor
        {
            get { return _processor; }

            set
            {
                _processor = value;
                ProcessorName = _importer.TypeName;
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
