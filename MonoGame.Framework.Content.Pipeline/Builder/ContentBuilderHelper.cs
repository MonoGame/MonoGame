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
using MonoGame.Framework.Content.Pipeline.Builder.Server;

namespace MonoGame.Framework.Content.Pipeline.Builder;

class ContentBuilderHelper
{
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

    record ImporterInfo
    {
        public required ContentImporterAttribute? Attribute { get; init; }
        public required Type Type { get; init; }
    }

    record ProcessorInfo
    {
        public required ContentProcessorAttribute? Attribute { get; init; }
        public required Type Type { get; init; }
    }

    record ServerPropertyInfo
    {
        public required ContentServerParameterAttribute Attribute { get; init; }
        public required PropertyInfo PropertyInfo { get; init; }
    }

    private static readonly List<ImporterInfo> _importers = [];
    private static readonly List<ProcessorInfo> _processors = [];
    private static readonly Dictionary<Type, List<ServerPropertyInfo>> _serverOptions = [];

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
            foreach (var t in a.GetTypes())
            {
                if (t.IsAbstract || t.IsInterface)
                    continue;

                if (t.GetInterface(nameof(IContentImporter)) != null)
                {
                    serilizer.WithTagMapping("!" + t.ToString(), t);
                    deserializer.WithTagMapping("!" + t.ToString(), t);

                    _importers.Add(new ImporterInfo
                    {
                        Attribute = GetImporterAttribute(t),
                        Type = t
                    });
                }
                else if (t.GetInterface(nameof(IContentProcessor)) != null)
                {
                    serilizer.WithTagMapping("!" + t.ToString(), t);
                    deserializer.WithTagMapping("!" + t.ToString(), t);

                    _processors.Add(new ProcessorInfo
                    {
                        Attribute = GetProcessorAttribute(t),
                        Type = t
                    });
                }
                else if (t.IsSubclassOf(typeof(ContentServer)))
                {
                    var props = new List<ServerPropertyInfo>();
                    foreach (var propInfo in t.GetProperties())
                    {
                        if (!propInfo.CanRead || !propInfo.CanWrite)
                        {
                            continue;
                        }

                        var attributes = propInfo.GetCustomAttributes(typeof(ContentServerParameterAttribute), false);
                        if (attributes.Length == 0)
                        {
                            continue;
                        }

                        props.Add(new ServerPropertyInfo
                        {
                            Attribute = (ContentServerParameterAttribute)attributes[0],
                            PropertyInfo = propInfo
                        });
                    }
                    _serverOptions[t] = props;
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
            if (info.Attribute?.FileExtensions.Any(e => e.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase)) ?? false)
            {
                outImporter = (IContentImporter)Activator.CreateInstance(info.Type)!;
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
            if (processor.Type.Name == attribute.DefaultProcessor)
            {
                outProcessor = (IContentProcessor)Activator.CreateInstance(processor.Type)!;
                return true;
            }
        }

        outProcessor = null!;
        return false;
    }

    public static IEnumerable<Type> GetServerTypes()
    {
        foreach (var pair in _serverOptions)
        {
            yield return pair.Key;
        }
    }

    public static IEnumerable<(ContentServerParameterAttribute attribute, PropertyInfo propertyInfo)> GetServerProperties(Type serverType)
    {
        if (_serverOptions.TryGetValue(serverType, out var ret))
        {
            foreach (var serverPropertyInfo in ret)
            {
                yield return (serverPropertyInfo.Attribute, serverPropertyInfo.PropertyInfo);
            }
        }
    }
}
