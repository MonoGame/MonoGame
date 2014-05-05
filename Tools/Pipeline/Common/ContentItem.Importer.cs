// MonoGame - Copyright (C) The MonoGame Team
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
    internal class ImporterConverter : TypeConverter
    {                
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
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
            var importers = new List<ImporterTypeDescription>();
            var contentItem = (context.Instance as ContentItem);
            
            if (contentItem.BuildAction == BuildAction.Copy)
            {
                // Copy items do not have importers.
                return new StandardValuesCollection(importers);
            }
            else if (!string.IsNullOrEmpty(contentItem.SourceFile))
            {
                // If the asset has a file extension then show only importers which accept it.
                var ext = Path.GetExtension(contentItem.SourceFile);
                if (!string.IsNullOrEmpty(ext))
                {
                    foreach (var i in PipelineTypes.Importers)
                    {
                        if (i.FileExtensions.Contains(ext))
                        {
                            importers.Add(i);
                        }
                    }

                    // If we didn't find any importers targeting this extensions, just show all of them.
                    if (importers.Count > 0)
                        return new StandardValuesCollection(importers);
                }
            }

            // Default case, show all importers.
            return new StandardValuesCollection(PipelineTypes.Importers);
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
            var contentItem = (ContentItem)context.Instance;
            var importer = (ImporterTypeDescription)value;// contentItem.Importer;
            //System.Diagnostics.Debug.Assert(importer == value);

            if (destinationType == typeof (string))
            {
                if (importer == PipelineTypes.MissingImporter)
                    return string.Format("[missing] {0}", contentItem.ImporterName ?? "[null]");

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
