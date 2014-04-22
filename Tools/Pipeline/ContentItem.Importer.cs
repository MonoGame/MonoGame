using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Tools.Pipeline
{    
    /// <summary>
    /// Custom converter for the Processor property of a ContentItem.
    /// </summary>
    internal class ImporterConverter : TypeConverter
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
            return new StandardValuesCollection(PipelineTypes.Importers);
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
                var contentItem = (context.Instance as ContentItem);
                return PipelineTypes.FindImporter(value as string, Path.GetExtension(contentItem.SourceFile));                
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
                return ((ImporterTypeDescription)value).DisplayName;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        //public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        //{
        //    return TypeDescriptor.GetProperties(value, attributes, true);
        //}

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {            
            return false;
        }
    }
}
