// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MonoGame.Tools.Pipeline
{    
    /// <summary>
    /// Custom converter for the Processor property of a ContentItem.
    /// </summary>
    public class ImporterConverter : TypeConverter
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
            return PipelineTypes.ImportersStandardValuesCollection;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof (string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                var str = value as string;

                foreach (var i in PipelineTypes.Importers)
                {
                    if (i.DisplayName.Equals(str))
                    {
                        return i;
                    }
                }
                
                if (string.IsNullOrEmpty(str))
                    return PipelineTypes.NullImporter;
                else
                    return PipelineTypes.MissingImporter;
            }

            return base.ConvertFrom(context, culture, value);
        }


        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var importer = (ImporterTypeDescription)value;// contentItem.Importer;
            //System.Diagnostics.Debug.Assert(importer == value);

            if (destinationType == typeof (string))
            {
                if (importer == PipelineTypes.MissingImporter)
                {
                    var contentItem = (ContentItem)context.Instance;
                    return string.Format("[missing] {0}", contentItem.ImporterName ?? "[null]");
                }

                return ((ImporterTypeDescription)value).DisplayName;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {            
            return false;
        }
    }
}
