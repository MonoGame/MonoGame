// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Tools.Pipeline
{
    /// <summary>
    /// Custom converter for the Processor property of a ContentItem.
    /// </summary>
    public class ProcessorConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            if (context.Instance is Array)
            {
                var array = context.Instance as Array;
                foreach (var obj in array)
                {
                    var item = obj as ContentItem;
                    if (item.BuildAction == BuildAction.Copy)
                        return false;
                }
            }
            else
            {
                var contentItem = (context.Instance as ContentItem);
                if (contentItem.BuildAction == BuildAction.Copy)
                    return false;
            }                
                        
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return PipelineTypes.ProcessorsStandardValuesCollection;            
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
                var processor = (ProcessorTypeDescription)value;

                if (processor == PipelineTypes.MissingProcessor)
                {
                    var contentItem = context.Instance as ContentItem;
                    return string.Format("[missing] {0}", contentItem.ProcessorName);
                }

                return ((ProcessorTypeDescription)value).DisplayName;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var props = new PropertyDescriptorCollection(null);

            var processor = value as ProcessorTypeDescription;

            if (context.Instance is Array)
            {
                var array = context.Instance as object[];

                foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(value, attributes, true))
                    props.Add(new MultiTargetPropertyDescriptor(prop.Name, prop.PropertyType, prop.ComponentType, prop, array));

                var paramArray = array.Select(e => ((ContentItem)e).ProcessorParams).ToArray();

                foreach (var p in processor.Properties)
                {
                    var prop = new OpaqueDataDictionaryElementPropertyDescriptor(p.Name,
                                                                                 p.Type,
                                                                                 null);
                    var prop2 = new MultiTargetPropertyDescriptor(prop.Name,
                                                             prop.PropertyType,
                                                             prop.ComponentType,
                                                             prop,
                                                             paramArray);
                    props.Add(prop2);
                }
            }
            else
            {
                var contentItem = context.Instance as ContentItem;

                if (value == PipelineTypes.MissingProcessor)
                {

                    props.Add(new ReadonlyPropertyDescriptor("Name", typeof (string), typeof (ProcessorTypeDescription), contentItem.ProcessorName));

                    foreach (var p in contentItem.ProcessorParams)
                    {
                        var desc = new OpaqueDataDictionaryElementPropertyDescriptor(p.Key,
                                                                                     p.Value.GetType(),
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
                                                                                     contentItem.ProcessorParams);

                        props.Add(desc);
                    }
                }
            }

            return props;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            if (!GetStandardValuesSupported(context))
                return false;

            if (context.Instance is Array)
            {
                var array = (context.Instance as Array);
                var first = array.GetValue(0) as ContentItem;

                if (!first.Processor.Properties.Any())
                    return false;

                if (first.Processor == PipelineTypes.MissingProcessor)
                    return false;

                for (var i = 1; i < array.Length; i++)
                {
                    var item = array.GetValue(i) as ContentItem;
                    if (item.Processor != first.Processor)
                        return false;

                    if (!item.Processor.Properties.Any())
                        return false;                    
                }
            }
            else
            {
                var item = context.Instance as ContentItem;
                if (item.BuildAction == BuildAction.Copy)
                    return false;

                if (!item.Processor.Properties.Any())
                    return false;          
            }
            
            return true;
        }
    }
}
