﻿// MonoGame - Copyright (C) The MonoGame Team
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
}