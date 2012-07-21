#region License
/*
 Microsoft Public License (Ms-PL)
 MonoGame - Copyright © 2012 The MonoGame Team
 
 All rights reserved.
 
 This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
 accept the license, do not use the software.
 
 1. Definitions
 The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
 U.S. copyright law.
 
 A "contribution" is the original software, or any additions or changes to the software.
 A "contributor" is any person that distributes its contribution under this license.
 "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 
 2. Grant of Rights
 (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 
 3. Conditions and Limitations
 (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
 your patent license from such contributor to the software ends automatically.
 (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
 notices that are present in the software.
 (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
 a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
 code form, you may only do so under a license that complies with this license.
 (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
 or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
 permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
 purpose and non-infringement.
 */
#endregion License

using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Represents a processor parameter. Processor parameters are automatically discovered by the content pipeline. Therefore, only custom processor developers should use this class directly.
    /// </summary>
    [SerializableAttribute]
    public sealed class ProcessorParameter
    {
        PropertyInfo propInfo;
        ReadOnlyCollection<string> enumValues;

        /// <summary>
        /// Default value of the processor parameter.
        /// </summary>
        public Object DefaultValue { get; set; }

        /// <summary>
        /// Description of the parameter, as specified by the [Description] attribute.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Name of the parameter displayed in the designer, as specified by the [DisplayName] attribute.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets a value indicating whether the parameter is an enumeration.
        /// </summary>
        public bool IsEnum
        {
            get
            {
                return enumValues != null;
            }
        }

        /// <summary>
        /// Available options for enumerated type parameters. For parameters of other types, this value is null.
        /// </summary>
        public ReadOnlyCollection<string> PossibleEnumValues
        {
            get
            {
                return enumValues;
            }
        }

        /// <summary>
        /// Name of the property, as defined in the C# code.
        /// </summary>
        public string PropertyName
        {
            get
            {
                return propInfo.Name;
            }
        }

        /// <summary>
        /// Type of the parameter.
        /// </summary>
        public string PropertyType
        {
            get
            {
                return propInfo.PropertyType.Name;
            }
        }

        /// <summary>
        /// Constructs a ProcessorParameter instance.
        /// </summary>
        /// <param name="propertyInfo">The info for the property.</param>
        internal ProcessorParameter(PropertyInfo propertyInfo)
        {
            propInfo = propertyInfo;
            if (propInfo.PropertyType.IsEnum)
                enumValues = new ReadOnlyCollection<string>(propInfo.PropertyType.GetEnumNames());
        }
    }
}
