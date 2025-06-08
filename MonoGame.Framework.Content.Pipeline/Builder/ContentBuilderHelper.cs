// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MonoGame.Framework.Content.Pipeline.Builder;

sealed class ColorConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Color);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        var color = new Color();
        var split = scalar.Value.Split(",");

        color.R = byte.Parse(split[0]);
        color.G = byte.Parse(split[1]);
        color.B = byte.Parse(split[2]);
        color.A = byte.Parse(split[3]);

        return color;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var color = (Color)(value ?? new Color());
        emitter.Emit(new Scalar($"{color.R},{color.G},{color.B},{color.A}"));
    }
}

class ContentBuilderHelper
{
    private struct ImporterInfo
    {
        public ContentImporterAttribute? attribute;
        public Type type;
    };

    private struct ProcessorInfo
    {
        public ContentProcessorAttribute? attribute;
        public Type type;
    };
    private static readonly List<ImporterInfo> _importers = [];
    private static readonly List<ProcessorInfo> _processors = [];

    static ContentBuilderHelper()
    {
        var serilizer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new ColorConverter())
            .EnablePrivateConstructors()
            .DisableAliases();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new ColorConverter())
            .EnablePrivateConstructors()
            .IgnoreFields()
            .IgnoreUnmatchedProperties();

        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] exportedTypes = a.GetTypes();
            foreach (var t in exportedTypes)
            {
                if (t.IsAbstract)
                    continue;

                if (t.GetInterface(@"IContentImporter") != null)
                {
                    serilizer.WithTagMapping("!" + t.ToString(), t);
                    deserializer.WithTagMapping("!" + t.ToString(), t);

                    _importers.Add(new ImporterInfo
                    {
                        attribute = GetImporterAttribute(t),
                        type = t
                    });
                }
                else if (t.GetInterface(@"IContentProcessor") != null)
                {
                    serilizer.WithTagMapping("!" + t.ToString(), t);
                    deserializer.WithTagMapping("!" + t.ToString(), t);

                    _processors.Add(new ProcessorInfo
                    {
                        attribute = GetProcessorAttribute(t),
                        type = t
                    });
                }
            }
        }

        Serializer = serilizer.Build();
        Deserializer = deserializer.Build();
    }

    public static readonly ISerializer Serializer;

    public static readonly IDeserializer Deserializer;

    public static bool ArePropsEqual(object? obj1, object? obj2)
    {
        if (obj1 == obj2) // same refs
        {
            return true;
        }

        if (obj1 == null || obj2 == null || obj1.GetType() != obj2.GetType()) // null + type check
        {
            return false;
        }

        if (obj1.GetType().IsPrimitive || obj1 is string) // if primitive or string, Equals is enough
        {
            return obj1.Equals(obj2);
        }

        if (obj1 is IList list1 && obj2 is IList list2) // if list, go through each entry to check if they match
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                if (!ArePropsEqual(list1[i], list2[i]))
                {
                    return false;
                }
            }
        }
        else // deal with complex objects by checking each property
        {
            foreach (var prop in obj1.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanRead && !ArePropsEqual(prop.GetValue(obj1), prop.GetValue(obj2)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static ContentImporterAttribute GetImporterAttribute(Type t)
    {
        var attributes = t.GetCustomAttributes(typeof(ContentImporterAttribute), false);
        for (int i = 0; i < attributes.Length; i++)
        {
            if (attributes[i] is ContentImporterAttribute attribute)
            {
                return attribute;
            }
        }

        return new ContentImporterAttribute(".*")
        {
            DefaultProcessor = "",
            DisplayName = t.Name
        };
    }

    public static ContentProcessorAttribute GetProcessorAttribute(Type t)
    {
        var attributes = t.GetCustomAttributes(typeof(ContentProcessorAttribute), false);
        for (int i = 0; i < attributes.Length; i++)
        {
            if (attributes[i] is ContentProcessorAttribute attribute)
            {
                return attribute;
            }
        }

        return new ContentProcessorAttribute()
        {
            DisplayName = t.Name
        };
    }

    public static bool GetImporter(string relativePath, IContentImporter? inImporter, out IContentImporter outImporter)
    {
        if (inImporter != null)
        {
            outImporter = inImporter;
            return true;
        }

        foreach (var info in _importers)
        {
            string fileExtension = Path.GetExtension(relativePath);
            if (info.attribute?.FileExtensions.Any(e => e.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase)) ?? false)
            {
                outImporter = (IContentImporter)Activator.CreateInstance(info.type)!;
                return true;
            }
        }

        outImporter = null!;
        return false;
    }

    public static bool GetProcessor(IContentImporter inImporter, IContentProcessor? inProcessor, out IContentProcessor outProcessor)
    {
        if (inProcessor != null)
        {
            outProcessor = inProcessor;
            return true;
        }

        var attribute = GetImporterAttribute(inImporter.GetType());

        foreach (var processor in _processors)
        {
            if (processor.type.Name == attribute.DefaultProcessor)
            {
                outProcessor = (IContentProcessor)Activator.CreateInstance(processor.type)!;
                return true;
            }
        }

        outProcessor = null!;
        return false;
    }
}
