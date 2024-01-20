// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;


namespace MonoGame.Effect
{
    // Reusable, reflection based helper for parsing commandline options.
    //
    // From Shawn Hargreaves Blog:
    // http://blogs.msdn.com/b/shawnhar/archive/2012/04/20/a-reusable-reflection-based-command-line-parser.aspx
    //
    public class CommandLineParser
    {
        object _optionsObject;

        Queue<FieldInfo> _requiredOptions = new Queue<FieldInfo>();
        Dictionary<string, FieldInfo> _optionalOptions = new Dictionary<string, FieldInfo>();

        List<string> _requiredUsageHelp = new List<string>();
        List<string> _optionalUsageHelp = new List<string>();


        // Constructor.
        public CommandLineParser(object optionsObject)
        {
            this._optionsObject = optionsObject;

            // Reflect to find what commandline options are available.
            foreach (var field in optionsObject.GetType().GetFields())
            {
                String description;
                var fieldName = GetOptionNameAndDescription(field, out description);

                if (GetAttribute<RequiredAttribute>(field) != null)
                {
                    // Record a required option.
                    _requiredOptions.Enqueue(field);

                    _requiredUsageHelp.Add(string.Format("<{0}> {1}", fieldName, description));
                }
                else
                {
                    // Record an optional option.
                    _optionalOptions.Add(fieldName.ToLowerInvariant(), field);

                    if (field.FieldType == typeof(bool))
                        _optionalUsageHelp.Add(string.Format("/{0} {1}", fieldName, description));
                    else
                        _optionalUsageHelp.Add(string.Format("/{0}:value {1}", fieldName, description));
                }
            }
        }


        public bool ParseCommandLine(string[] args)
        {
            // Parse each argument in turn.
            foreach (var arg in args)
            {
                if (!ParseArgument(arg.Trim()))
                    return false;
            }

            // Make sure we got all the required options.
            var missingRequiredOption = _requiredOptions.FirstOrDefault(field => !IsList(field) || GetList(field).Count == 0);

            if (missingRequiredOption != null)
            {
                ShowError("Missing argument '{0}'", GetOptionName(missingRequiredOption));
                return false;
            }

            return true;
        }


        bool ParseArgument(string arg)
        {
            if (_requiredOptions.Count > 0)
            {
                // Parse the next non escaped argument.
                var field = _requiredOptions.Peek();

                if (!IsList(field))
                    _requiredOptions.Dequeue();

                return SetOption(field, arg);
            }
            else if (arg.StartsWith("/"))
            {
                // After the first escaped argument we can no
                // longer read non-escaped arguments.
                _requiredOptions.Clear();

                // Parse an optional argument.
                char[] separators = { ':' };

                var split = arg.Substring(1).Split(separators, 2, StringSplitOptions.None);

                var name = split[0];
                var value = (split.Length > 1) ? split[1] : "true";

                FieldInfo field;

                if (!_optionalOptions.TryGetValue(name.ToLowerInvariant(), out field))
                {
                    ShowError("Unknown option '{0}'", name);
                    return false;
                }

                return SetOption(field, value);
            }
            
            ShowError("Too many arguments");
            return false;
        }


        bool SetOption(FieldInfo field, string value)
        {
            try
            {
                if (IsList(field))
                {
                    // Append this value to a list of options.
                    GetList(field).Add(ChangeType(value, ListElementType(field)));
                }
                else
                {
                    // Set the value of a single option.
                    field.SetValue(_optionsObject, ChangeType(value, field.FieldType));
                }

                return true;
            }
            catch
            {
                ShowError("Invalid value '{0}' for option '{1}'", value, GetOptionName(field));
                return false;
            }
        }


        static object ChangeType(string value, Type type)
        {
            var converter = TypeDescriptor.GetConverter(type);

            return converter.ConvertFromInvariantString(value);
        }


        static bool IsList(FieldInfo field)
        {
            return typeof(IList).IsAssignableFrom(field.FieldType);
        }


        IList GetList(FieldInfo field)
        {
            return (IList)field.GetValue(_optionsObject);
        }


        static Type ListElementType(FieldInfo field)
        {
            var interfaces = from i in field.FieldType.GetInterfaces()
                             where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                             select i;

            return interfaces.First().GetGenericArguments()[0];
        }


        static string GetOptionName(FieldInfo field)
        {
            var nameAttribute = GetAttribute<NameAttribute>(field);
            if (nameAttribute != null)
                return nameAttribute.Name;
            else
                return field.Name;
        }

        static string GetOptionNameAndDescription(FieldInfo field, out String description)
        {
            var nameAttribute = GetAttribute<NameAttribute>(field);

            if (nameAttribute != null)
            {
                description = nameAttribute.Description;
                return nameAttribute.Name;
            }
            else
            {
                description = null;
                return field.Name;
            }
        }

        public string Title { get; set; }

        void ShowError(string message, params object[] args)
        {
            var name = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);

            if (!string.IsNullOrEmpty(Title))
            {
                Console.Error.WriteLine(Title);
                Console.Error.WriteLine();
            }
            Console.Error.WriteLine(message, args);
            Console.Error.WriteLine();
            Console.Error.WriteLine("Usage: {0} {1}", name, string.Join(" ", _requiredUsageHelp));

            if (_optionalUsageHelp.Count > 0)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Options:");

                foreach (string optional in _optionalUsageHelp)
                    Console.Error.WriteLine("    {0}", optional);
            }
        }


        static T GetAttribute<T>(ICustomAttributeProvider provider) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
        }


        // Used on optionsObject fields to indicate which options are required.
        [AttributeUsage(AttributeTargets.Field)]
        public sealed class RequiredAttribute : Attribute
        {
        }

        // Used on an optionsObject field to rename the corresponding commandline option.
        [AttributeUsage(AttributeTargets.Field)]
        public class NameAttribute : Attribute
        {
            public NameAttribute(string name)
            {
                Name = name;
                Description = null;
            }

            public NameAttribute(string name, string description)
            {
                Name = name;
                Description = description;
            }

            public string Name { get; private set; }
            public string Description { get; protected set; }
        }

        [AttributeUsage(AttributeTargets.Field)]
        public sealed class ProfileNameAttribute : NameAttribute
        {
            public ProfileNameAttribute()
                : base("Profile")
            {
                var names = ShaderProfile.All.Select(p => p.Name);
                Description = "\t - Must be one of the following: " + string.Join(", ", names);                               
            }
        }
    }
}
