﻿// MonoGame - Copyright (C) The MonoGame Team
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


namespace MGCB
{
    // Reusable, reflection based helper for parsing commandline options.
    //
    // Original From Shawn Hargreaves Blog:
    // http://blogs.msdn.com/b/shawnhar/archive/2012/04/20/a-reusable-reflection-based-command-line-parser.aspx
    //
    
    public class CommandLineParser
    {
        readonly object _optionsObject;

        readonly Queue<MemberInfo> _requiredOptions = new Queue<MemberInfo>();
        readonly Dictionary<string, MemberInfo> _optionalOptions = new Dictionary<string, MemberInfo>();

        readonly List<string> _requiredUsageHelp = new List<string>();

        public CommandLineParser(object optionsObject)
        {
            _optionsObject = optionsObject;

            // Reflect to find what commandline options are available.
            foreach (var field in optionsObject.GetType().GetFields())
            {
                var param = GetAttribute<CommandLineParameterAttribute>(field);
                if (param == null)
                    continue;

                if (param.Required)
                {
                    // Record a required option.
                    _requiredOptions.Enqueue(field);

                    _requiredUsageHelp.Add(string.Format("<{0}>", param.Name));
                }
                else
                {
                    // Record an optional option.
                    _optionalOptions.Add(param.Name.ToLowerInvariant(), field);
                }
            }

            foreach (var method in optionsObject.GetType().GetMethods())
            {
                var param = GetAttribute<CommandLineParameterAttribute>(method);
                if (param == null)
                    continue;

                // Only accept methods that take less than 1 parameter.
                if (method.GetParameters().Length > 1)
                    throw new NotSupportedException("Methods must have one or zero parameters.");

                if (param.Required)
                {
                    // Record a required option.
                    _requiredOptions.Enqueue(method);

                    _requiredUsageHelp.Add(string.Format("<{0}>", param.Name));
                }
                else
                {
                    // Record an optional option.
                    _optionalOptions.Add(param.Name.ToLowerInvariant(), method);
                }
            }
        }


        public bool ParseCommandLine(string[] args)
        {
            var success = true;

            // Parse each argument in turn.
            foreach (var arg in args)
            {
                if (!ParseArgument(arg.Trim()))
                {
                    success = false;
                    break;
                }
            }

            // Make sure we got all the required options.
            var missingRequiredOption = _requiredOptions.FirstOrDefault(field => !IsList(field) || GetList(field).Count == 0);
            if (missingRequiredOption != null)
            {
                ShowError("Missing argument '{0}'", GetAttribute<CommandLineParameterAttribute>(missingRequiredOption).Name);
                return false;
            }

            return success;
        }


        bool ParseArgument(string arg)
        {
            if (arg.StartsWith("/"))
            {
                // After the first escaped argument we can no
                // longer read non-escaped arguments.
                if (_requiredOptions.Count > 0)
                    return false;

                // Parse an optional argument.
                char[] separators = { ':' };

                var split = arg.Substring(1).Split(separators, 2, StringSplitOptions.None);

                var name = split[0];
                var value = (split.Length > 1) ? split[1] : "true";

                MemberInfo member;

                if (!_optionalOptions.TryGetValue(name.ToLowerInvariant(), out member))
                {
                    ShowError("Unknown option '{0}'", name);
                    return false;
                }

                return SetOption(member, value);
            }
            else if ( _requiredOptions.Count > 0 )
            {
                // Parse the next non escaped argument.
                var field = _requiredOptions.Peek();

                if (!IsList(field))
                    _requiredOptions.Dequeue();

                return SetOption(field, arg);
            }
            else
            {
                ShowError("Too many arguments");
                return false;
            }
        }


        bool SetOption(MemberInfo member, string value)
        {
            try
            {
                if (IsList(member))
                {
                    // Append this value to a list of options.
                    GetList(member).Add(ChangeType(value, ListElementType(member as FieldInfo)));
                }
                else
                {
                    // Set the value of a single option.
                    if (member is MethodInfo)
                    {
                        var method = member as MethodInfo;
                        var parameters = method.GetParameters();
                        if (parameters.Length == 0)
                            method.Invoke(_optionsObject, null);
                        else
                            method.Invoke(_optionsObject, new[] { ChangeType(value, parameters[0].ParameterType) });
                    }
                    else
                    {
                        var field = member as FieldInfo;
                        field.SetValue(_optionsObject, ChangeType(value, field.FieldType));
                    }
                }

                return true;
            }
            catch
            {
                ShowError("Invalid value '{0}' for option '{1}'", value, GetAttribute<CommandLineParameterAttribute>(member).Name);
                return false;
            }
        }


        static object ChangeType(string value, Type type)
        {
            var converter = TypeDescriptor.GetConverter(type);

            return converter.ConvertFromInvariantString(value);
        }


        static bool IsList(MemberInfo field)
        {
            if (field is MethodInfo)
                return false;

            return typeof (IList).IsAssignableFrom((field as FieldInfo).FieldType);
        }


        IList GetList(MemberInfo field)
        {
            return (IList)(field as FieldInfo).GetValue(_optionsObject);
        }


        static Type ListElementType(FieldInfo field)
        {
            var interfaces = from i in field.FieldType.GetInterfaces()
                             where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                             select i;

            return interfaces.First().GetGenericArguments()[0];
        }

        public string Title { get; set; }

        public void ShowUsage()
        {
            ShowError(null);
        }

        public void ShowError(string message, params object[] args)
        {
            var name = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);

            if (!string.IsNullOrEmpty(Title))
            {
                Console.Error.WriteLine(Title);
                Console.Error.WriteLine();
            }

            if (!string.IsNullOrEmpty(message))
            {
                Console.Error.WriteLine(message, args);
                Console.Error.WriteLine();
            }

            Console.Error.WriteLine("Usage: {0} {1}{2}", 
                name, 
                string.Join(" ", _requiredUsageHelp), 
                _optionalOptions.Count > 0 ? " <Options>" : string.Empty);

            if (_optionalOptions.Count > 0)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Options:\n");

                foreach (var pair in _optionalOptions)
                {
                    var field = pair.Value as FieldInfo;
                    var method = pair.Value as MethodInfo;
                    var param = GetAttribute<CommandLineParameterAttribute>(pair.Value);

                    var hasValue = (field != null && field.FieldType != typeof (bool)) ||
                                   (method != null && method.GetParameters().Length != 0);

                    if (hasValue)
                        Console.Error.WriteLine("  /{0}:<{1}>\n    {2}\n", param.Name, param.ValueName, param.Description);
                    else
                        Console.Error.WriteLine("  /{0}\n    {1}\n", param.Name, param.Description);
                }
            }
        }


        static T GetAttribute<T>(ICustomAttributeProvider provider) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
        }
    }

    // Used on an optionsObject field or method to rename the corresponding commandline option.
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public sealed class CommandLineParameterAttribute : Attribute
    {
        public CommandLineParameterAttribute()
        {
            ValueName = "value";
        }

        public string Name { get; set; }

        public bool Required { get; set; }

        public string ValueName { get; set; }

        public string Description { get; set; }        
    }
}
