// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace MonoGame.Tools.Pipeline
{
    /// <summary>
    /// Custom converter for the Processor property of a ContentItem.
    /// </summary>
    internal class ProcessorConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            // True means show a combobox.
            if (GetStandardValues(context).Count > 0)
                return true;
                        
            return false;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var contentItem = (ContentItem)context.Instance;
            var processors = new List<ProcessorTypeDescription>();

            if (contentItem.BuildAction == BuildAction.Copy)
            {
                // Copy items do not have processors.
                return new StandardValuesCollection(processors);
            }
            else
            {
                var importer = contentItem.Importer;

                // If the importer is invalid then we do not know its real outputtype
                // so just show all processors.
                if (importer == PipelineTypes.MissingImporter)
                {
                    return new StandardValuesCollection(PipelineTypes.Processors);
                }
                
                foreach (var p in PipelineTypes.Processors)
                {
                    if (importer.OutputType.IsAssignableFrom(p.InputType))
                    {
                        processors.Add(p);
                    }
                }

                return new StandardValuesCollection(processors);
            }            
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof (string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }


        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                foreach (var i in PipelineTypes.Processors)
                {
                    if (i.DisplayName.Equals(value))
                    {
                        return i;
                    }
                }
            }

            return base.ConvertFrom(context, culture, value);
        }


        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof (string))
            {
                var contentItem = context.Instance as ContentItem;
                var processor = (ProcessorTypeDescription)value;

                if (processor == PipelineTypes.MissingProcessor)
                    return string.Format("[missing] {0}", contentItem.ProcessorName);

                return ((ProcessorTypeDescription)value).DisplayName;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var props = new PropertyDescriptorCollection(null);

            var processor = value as ProcessorTypeDescription;
            var contentItem = context.Instance as ContentItem;

            if (value == PipelineTypes.MissingProcessor)
            {            
                props.Add(new ReadonlyPropertyDescriptor("Name", typeof (string), typeof (ProcessorTypeDescription), contentItem.ProcessorName));

                foreach (var p in contentItem.ProcessorParams)
                {
                    var desc = new OpaqueDataDictionaryElementPropertyDescriptor(p.Key,
                                                                                 p.Value.GetType(),
                                                                                 typeof (ProcessorTypeDescription),
                                                                                 contentItem.ProcessorParams);
                    

                    props.Add(desc);
                }
            }
            else
            {
                // Emit regular properties.
                foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(value, attributes, true))
                    props.Add(prop);

                // Emit processor parameters.
                foreach (var p in processor.Properties)
                {
                    var desc = new OpaqueDataDictionaryElementPropertyDescriptor(p.Name,
                                                                                 p.Type,
                                                                                 typeof (ProcessorTypeDescription),
                                                                                 contentItem.ProcessorParams);
                    props.Add(desc);
                }
            }

            return props;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            var contentItem = context.Instance as ContentItem;
            if (contentItem.BuildAction == BuildAction.Copy)
                return false;

            if (contentItem.Processor.Properties.Any())
                return true;

            if (contentItem.Processor == PipelineTypes.MissingProcessor)
                return true;
            
            return false;
        }
    }
}
