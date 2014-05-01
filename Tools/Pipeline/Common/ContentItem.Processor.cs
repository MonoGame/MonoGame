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
            // True means that values returned by GetStandardValues is exclusive (contains all possible valid values).
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var importer = ((ContentItem)context.Instance).Importer;
            var processors = new List<ProcessorTypeDescription>();
            foreach (var p in PipelineTypes.Processors)
            {
                if (p.InputType == importer.OutputType)
                {
                    processors.Add(p);
                }
            }
            return new StandardValuesCollection(processors);
        }

        // Overrides the CanConvertFrom method of TypeConverter.
        // The ITypeDescriptorContext interface provides the context for the
        // conversion. Typically, this interface is used at design time to 
        // provide information about the design-time container.
        public override bool CanConvertFrom(ITypeDescriptorContext context,
                                            Type sourceType)
        {
            if (sourceType == typeof (string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }


        public override object ConvertFrom(ITypeDescriptorContext context,
                                           CultureInfo culture,
                                           object value)
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


        public override object ConvertTo(ITypeDescriptorContext context,
                                         CultureInfo culture,
                                         object value,
                                         Type destinationType)
        {
            if (destinationType == typeof (string))
            {
                return ((ProcessorTypeDescription)value).DisplayName;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            // Create the property collection and filter
            var props = new PropertyDescriptorCollection(null);

            // Emit regular properties.
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(value, attributes, true))
            {
                props.Add(prop);
            }
            
            var processor = value as ProcessorTypeDescription;
            var contentItem = context.Instance as ContentItem;

            foreach (var p in processor.Properties)
            {
                var desc = new OpaqueDataDictionaryElementPropertyDescriptor(p.Name, p.Type, typeof(ProcessorTypeDescription), contentItem.ProcessorParams);
                props.Add(desc);                
            }            

            return props;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            var contentItem = context.Instance as ContentItem;
            if (contentItem.Processor.Properties.Count() > 0)
                return true;
            
            return false;
        }
    }
}
