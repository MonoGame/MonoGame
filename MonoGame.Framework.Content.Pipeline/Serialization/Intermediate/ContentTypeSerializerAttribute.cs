// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.ObjectModel;
using System.Reflection;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

/// <summary>
/// Used to identify custom ContentTypeSerializer classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ContentTypeSerializerAttribute : Attribute
{
    private static ReadOnlyCollection<Type>? _types;
    private static readonly object _lock = new();

    internal static ReadOnlyCollection<Type> GetTypes()
    {
        lock (_lock)
        {
            if (_types == null)
            {
                var found = new List<Type>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            var attributes = type.GetCustomAttributes(typeof(ContentTypeSerializerAttribute), false);
                            if (attributes.Length > 0)
                                found.Add(type);
                        }
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        Console.WriteLine($"Warning: {ex.Message}");
                    }
                }

                _types = new ReadOnlyCollection<Type>(found);
            }
        }

        return _types;
    }
}
