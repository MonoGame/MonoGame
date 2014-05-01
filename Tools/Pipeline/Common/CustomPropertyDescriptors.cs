// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Tools.Pipeline
{
    /// <summary>
    /// PropertyDescriptor for a named item within an OpaqueDataDictionary.
    /// </summary>
    public class OpaqueDataDictionaryElementPropertyDescriptor : PropertyDescriptor
    {
        private readonly Type _propertyType;
        private readonly Type _componentType;
        private readonly string _propertyName;        
        private readonly OpaqueDataDictionary _data;

        public OpaqueDataDictionaryElementPropertyDescriptor(string propertyName, Type propertyType, Type componentType, OpaqueDataDictionary data)
            : base(propertyName, new Attribute[] { })
        {
            _propertyType = propertyType;
            _propertyName = propertyName;
            _componentType = componentType;
            _data = data;
        }

        public override object GetValue(object component)
        {
            if (!_data.ContainsKey(_propertyName))
                return string.Empty;

            return _data[_propertyName];
        }

        public override void SetValue(object component, object value)
        {
            _data[_propertyName] = value;
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return _componentType; } }        
        public override bool IsReadOnly { get { return false; } }
        public override Type PropertyType { get { return _propertyType; } }
        public override void ResetValue(object component) { SetValue(component, null); }        
        public override bool ShouldSerializeValue(object component) { return true; }
    }

    /// <summary>
    /// PropertyDescriptor which has a fixed value.
    /// </summary>
    public class ReadonlyPropertyDescriptor : PropertyDescriptor
    {
        private readonly Type _propertyType;
        private readonly Type _componentType;
        private readonly object _value;

        public ReadonlyPropertyDescriptor(string propertyName, Type propertyType, Type componentType, object value)
            : base(propertyName, new Attribute[] {})
        {
            _propertyType = propertyType;
            _componentType = componentType;
            _value = value;
        }

        public override object GetValue(object component)
        {
            return _value;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get { return _componentType; }
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        public override Type PropertyType
        {
            get { return _propertyType; }
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }

    /// <summary>
    /// PropertyDescriptor for a string representing an assembly reference within a list.
    /// </summary>
    public class ReferenceListElementPropertyDescriptor : PropertyDescriptor
    {
        private readonly Type _componentType;
        private readonly List<string> _list;
        private readonly int _index;

        public ReferenceListElementPropertyDescriptor(List<string> list, int index, Type componentType)
            : base(string.Format("Reference[{0}]", index), new Attribute[] { })
        {
            _componentType = componentType;
            _list = list;
            _index = index;
        }

        public override object GetValue(object component)
        {
            return _list[_index];
        }

        public override void SetValue(object component, object value)
        {
            if (string.IsNullOrEmpty((string)value))
            {
                _list[_index] = " ";
                //_list.RemoveAt(_index);
                //((PipelineProject)component).ContentItems[0].View.UpdateProperties(((PipelineProject)component));
            }
            else
                _list[_index] = (string)value;
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return _componentType; } }
        public override bool IsReadOnly { get { return false; } }
        public override Type PropertyType { get { return typeof(string); } }
        public override void ResetValue(object component) { SetValue(component, ""); }
        public override bool ShouldSerializeValue(object component) { return true; }
    }

    /// <summary>
    /// Custom converter for the References property of a PipelineProject.
    /// </summary>
    internal class AssemblyReferenceListConverter : TypeConverter
    {       
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var props = new PropertyDescriptorCollection(null);

            var list = ((PipelineProject)context.Instance).References;
            for (var i = 0; i < list.Count; i++)
            {
                props.Add(new ReferenceListElementPropertyDescriptor(list, i, typeof(PipelineProject)));
            }

            return props;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}