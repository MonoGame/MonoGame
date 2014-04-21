using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Tools.Pipeline
{
    internal class Processor
    {
        public string Name;
        public OpaqueDataDictionary Data;
    }

    /// <summary>
    /// Custom converter for the Processor property of a ContentItem.
    /// </summary>
    internal class ProcessorConverter : ExpandableObjectConverter
    {
        // JCF: Temporary hard coded values for testing purposes.
        //      This should be populated with real processors from loaded assemblies.
        private static readonly Processor[] _processorTypes = new Processor[]
            {
                new Processor() {Name = "Sound Processor"},
                new Processor() {Name = "Texture Processor"},
            };

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //true means show a combobox
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            //true will limit to list. false will show the list, 
            //but allow free-form entry
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(_processorTypes);
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
                foreach (var i in _processorTypes)
                {
                    if (i.Name.Equals(value))
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
                return ((Processor)value).Name;
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

            // Emit items in the OpaqueDataDictionary.
            var processor = value as Processor;
            foreach (var item in processor.Data)
            {
                var desc = new OpaqueDataDictionaryElementPropertyDescriptor(item.Key, typeof (string), typeof (Processor), processor.Data);
                props.Add(desc);
            }

            return props;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
