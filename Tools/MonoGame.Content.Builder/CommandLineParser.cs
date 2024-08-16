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

namespace MonoGame.Content.Builder
{
    /// <summary>
    /// Adapted from this generic command line argument parser:
    /// http://blogs.msdn.com/b/shawnhar/archive/2012/04/20/a-reusable-reflection-based-command-line-parser.aspx     
    /// </summary>
    public class MGBuildParser
    {
        public static MGBuildParser Instance;

        private readonly object _optionsObject;
        private readonly Queue<MemberInfo> _requiredOptions;
        private readonly Dictionary<string, MemberInfo> _optionalOptions;
        private readonly Dictionary<string, string> _flags;
        private readonly List<string> _requiredUsageHelp;

        public readonly Dictionary<string, string> _properties;

        public delegate void ErrorCallback(string msg, object[] args);
        public event ErrorCallback OnError;

        public MGBuildParser(object optionsObject)
        {
            Instance = this;

            _optionsObject = optionsObject;
            _requiredOptions = new Queue<MemberInfo>();
            _optionalOptions = new Dictionary<string, MemberInfo>();
            _requiredUsageHelp = new List<string>();

            _properties = new Dictionary<string, string>();

            // Reflect to find what commandline options are available...

            // Fields
            foreach (var field in optionsObject.GetType().GetFields())
            {
                var param = GetAttribute<CommandLineParameterAttribute>(field);
                if (param == null)
                    continue;

                CheckReservedPrefixes(param.Name);

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

            // Properties
            foreach (var property in optionsObject.GetType().GetProperties())
            {
                var param = GetAttribute<CommandLineParameterAttribute>(property);
                if (param == null)
                    continue;

                CheckReservedPrefixes(param.Name);

                if (param.Required)
                {
                    // Record a required option.
                    _requiredOptions.Enqueue(property);

                    _requiredUsageHelp.Add(string.Format("<{0}>", param.Name));
                }
                else
                {
                    // Record an optional option.
                    _optionalOptions.Add(param.Name.ToLowerInvariant(), property);
                }
            }

            // Methods
            foreach (var method in optionsObject.GetType().GetMethods())
            {
                var param = GetAttribute<CommandLineParameterAttribute>(method);
                if (param == null)
                    continue;

                CheckReservedPrefixes(param.Name);

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

            _flags = new Dictionary<string, string>();
            foreach(var pair in _optionalOptions)
            {
                var fi = GetAttribute<CommandLineParameterAttribute>(pair.Value);
                if(!string.IsNullOrEmpty(fi.Flag))
                    _flags.Add(fi.Flag, fi.Name);
            }
        }        

        public bool Parse(IEnumerable<string> args)
        {
            args = Preprocess(args);

            var showUsage = true;
            var success = true;
            foreach (var arg in args)
            {
                showUsage = false;

                if (!ParseFlags(arg))
                {
                    success = false;
                    break;
                }
            }

            var missingRequiredOption = _requiredOptions.FirstOrDefault(field => !IsList(field) || GetList(field).Count == 0);
            if (missingRequiredOption != null)
            {
                ShowError("Missing argument '{0}'", GetAttribute<CommandLineParameterAttribute>(missingRequiredOption).Name);
                return false;
            }

            if (showUsage)
                ShowError(null);

            return success;
        }

        private IEnumerable<string> Preprocess(IEnumerable<string> args)
        {
            var output = new List<string>();
            var ifstack = new Stack<IfCondition>();
            foreach (var arg in args)
                ParsePreprocessArg(arg, output, ifstack, false);

            return output.ToArray();
        }

        private void ParsePreprocessArg(string arg, List<string> output, Stack<IfCondition> ifstack, bool inResponseFile)
        {
            if (arg.StartsWith("$endif"))
            {
                ifstack.Pop();
                return;
            }

            if (ifstack.Count > 0)
            {
                foreach (var ifCondition in ifstack)
                {
                    var expected = ifCondition.Value;
                    string actual;
                    if (!_properties.TryGetValue(ifCondition.Key, out actual))
                        return;
                    if (expected != string.Empty && !expected.Equals(actual))
                        return;
                }
            }

            if (arg.StartsWith("$set "))
            {
                if (!inResponseFile)
                    throw new Exception("$set is invalid outside of a response file.");
                var words = arg.Substring(5).Split('=');
                var name = words[0].Trim();
                var value = words.Length > 1 ? words[1].Trim() : string.Empty;

                _properties[name] = value;
                return;
            }

            if (arg.StartsWith("$if "))
            {
                if (!inResponseFile)
                    throw new Exception("$if is invalid outside of a response file.");

                var words = arg.Substring(4).Split('=');
                var name = words[0].Trim();
                var value = words.Length > 1 ? words[1].Trim() : string.Empty;

                var condition = new IfCondition(name, value);
                ifstack.Push(condition);

                return;
            }

            if (arg.StartsWith("/define:") || arg.StartsWith("--define:"))
            {
                arg = arg.Substring(arg[0] == '/' ? 8 : 9);

                var words = arg.Split('=');
                var name = words[0];
                var value = words.Length > 1 ? words[1] : string.Empty;

                _properties[name] = value;

                return;
            }

            if (arg.StartsWith("/@") || arg.StartsWith("--@") || arg.StartsWith("-@") || (arg.EndsWith(".mgcb")))
            {
                var file = arg;
                if (file.StartsWith("/@") || file.StartsWith("-@"))
                    file = arg.Substring(3);
                if (file.StartsWith("--@"))
                    file = arg.Substring(4);

                file = Path.GetFullPath(file);

                if (!File.Exists(file))
                    throw new Exception(string.Format("File '{0}' does not exist.", file));

                var prevDir = Directory.GetCurrentDirectory();
                var dir = Path.GetDirectoryName(file);

                if (prevDir != dir)
                {
                    // make sure the working dir is changed both during preprocessing and during execution
                    Directory.SetCurrentDirectory(dir);
                    output.Add("/workingDir:" + dir);
                }

                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    ParsePreprocessArg(line, output, ifstack, true);
                }

                if (prevDir != dir)
                {
                    Directory.SetCurrentDirectory(prevDir);
                    output.Add("/workingDir:" + prevDir);
                }

                return;
            }

            output.Add(arg);
        }

        private bool ParseFlags(string arg)
        {
            // Filename detected, redo with a build command
            if (File.Exists(arg))
                return ParseFlags("/build=" + arg);

            // Only one flag
            if (arg.Length >= 3 &&
                (arg[0] == '-' || arg[0] == '/') &&
                (arg[2] == '=' || arg[2] == ':'))
            {
                string name;
                if (!_flags.TryGetValue(arg[1].ToString(), out name))
                {
                    ShowError("Unknown option '{0}'", arg[1].ToString());
                    return false;
                }

                ParseArgument("/" + name + arg.Substring(2));
                return true;
            }

            // Multiple flags
            if (arg.Length >= 2 &&
               ((arg[0] == '-' && arg[1] != '-') || arg[0] == '/') &&
               !arg.Contains(":") && !arg.Contains("=") &&
               !_optionalOptions.ContainsKey(arg.Substring(1)))
            {
                for (int i = 1; i < arg.Length; i++)
                {
                    string name;
                    if (!_flags.TryGetValue(arg[i].ToString(), out name))
                    {
                        ShowError("Unknown option '{0}'", arg[i].ToString());
                        break;
                    }

                    ParseArgument("/" + name);
                }

                return true;
            }

            // Not a flag, parse argument
            return ParseArgument(arg);
        }

        private bool ParseArgument(string arg)
        {
            if (arg.StartsWith("/") || arg.StartsWith("--"))
            {
                // After the first escaped argument we can no
                // longer read non-escaped arguments.
                if (_requiredOptions.Count > 0)
                    return false;

                // Parse an optional argument.
                char[] separators = { ':', '=' };

                var split = arg.Substring(arg.StartsWith("/") ? 1 : 2).Split(separators, 2, StringSplitOptions.None);

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

            if (_requiredOptions.Count > 0)
            {
                // Parse the next non escaped argument.
                var field = _requiredOptions.Peek();

                if (!IsList(field))
                    _requiredOptions.Dequeue();

                return SetOption(field, arg);
            }

            ShowError("Too many arguments");
            return false;
        }


        bool SetOption(MemberInfo member, string value)
        {
            try
            {
                if (IsList(member))
                {
                    // Append this value to a list of options.
                    GetList(member).Add(ChangeType(value, ListElementType(member)));
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
                    else if (member is FieldInfo)
                    {
                        var field = member as FieldInfo;
                        field.SetValue(_optionsObject, ChangeType(value, field.FieldType));
                    }
                    else 
                    {
                        var property = member as PropertyInfo;
                        property.SetValue(_optionsObject, ChangeType(value, property.PropertyType), null);
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

        static readonly string[] ReservedPrefixes = new[]
            {   
                "$",
                "/",                
                "#",
                "--",
                "-"
            };

        static void CheckReservedPrefixes(string str)
        {
            foreach (var i in ReservedPrefixes)
            {
                if (str.StartsWith(i))
                    throw new Exception(string.Format("'{0}' is a reserved prefix and cannot be used at the start of an argument name.", i));
            }
        }

        static object ChangeType(string value, Type type)
        {
            var converter = TypeDescriptor.GetConverter(type);

            return converter.ConvertFromInvariantString(value);
        }


        static bool IsList(MemberInfo member)
        {
            if (member is MethodInfo)
                return false;

            if (member is FieldInfo)
                return typeof(IList).IsAssignableFrom((member as FieldInfo).FieldType);
            
            return typeof(IList).IsAssignableFrom((member as PropertyInfo).PropertyType);
        }


        IList GetList(MemberInfo member)
        {
            if (member is PropertyInfo)
                return (IList)(member as PropertyInfo).GetValue(_optionsObject, null);

            if (member is FieldInfo)
                return (IList)(member as FieldInfo).GetValue(_optionsObject);

            throw new Exception();
        }


        static Type ListElementType(MemberInfo member)
        {
            if (member is FieldInfo)
            {
                var field = member as FieldInfo;
                var interfaces = from i in field.FieldType.GetInterfaces()
                                 where i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IEnumerable<>)
                                 select i;

                return interfaces.First().GetGenericArguments()[0];
            }

            if (member is PropertyInfo)
            {
                var property = member as PropertyInfo;
                var interfaces = from i in property.PropertyType.GetInterfaces()
                                 where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                                 select i;

                return interfaces.First().GetGenericArguments()[0];
            }

            throw new ArgumentException("Only FieldInfo and PropertyInfo are valid arguments.", "member");
        }

        public string Title { get; set; }

        bool IsWindows()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return true;
            }
            return false;
        }

        public void ShowError(string message, params object[] args)
        {
            if (!string.IsNullOrEmpty(message) && OnError != null)
            {
                OnError(message, args);
                return;
            }

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

            var defaultParamPrefix = IsWindows() ? "/" : "--";
            Console.Error.WriteLine("Usage: {0} {1}{2}", 
                name, 
                _requiredUsageHelp.Count > 0 ? string.Join(" ", _requiredUsageHelp) + " " : string.Empty, 
                _optionalOptions.Count > 0 ? "<Options>" : string.Empty);

            if (_optionalOptions.Count > 0)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Options:");

                var data = _optionalOptions.Values.ToList();
                data.Sort((x, y) => {
                    var px = GetAttribute<CommandLineParameterAttribute>(x);
                    var py = GetAttribute<CommandLineParameterAttribute>(y);

                    return px.Name.CompareTo(py.Name);
                });

                foreach(var d in data)
                {
                    var attr = GetAttribute<CommandLineParameterAttribute>(d);
                    var field = d as FieldInfo;
                    var prop = d as PropertyInfo;
                    var method = d as MethodInfo;
                    var hasValue = false;

                    if (field != null && field.FieldType != typeof (bool))
                        hasValue = true;
                    if (prop != null && prop.PropertyType != typeof (bool))
                        hasValue = true;
                    if (method != null && method.GetParameters().Length != 0)
                        hasValue = true;
                    
                    var s = "  ";

                    s += (!string.IsNullOrEmpty(attr.Flag)) ? (IsWindows() ? "/" : "-") + attr.Flag + "," : "   ";
                    s += " " + defaultParamPrefix + attr.Name;

                    if (hasValue)
                    {
                        if (IsWindows())
                            s += ":<" + attr.ValueName + ">";
                        else
                            s += "=" + attr.ValueName.Replace("=", ":").ToUpper();
                    }

                    s = s.PadRight(35, ' ');

                    // Wrap text description
                    var bw = Math.Max(60, Console.BufferWidth);
                    var desc = attr.Description.Split(' ');

                    foreach(var dw in desc)
                    {
                        if (s.Length + dw.Length >= bw)
                        {
                            Console.WriteLine(s);
                            s = string.Empty.PadRight(37, ' ');
                        }

                        s += " " + dw;
                    }

                    Console.WriteLine(s);
                }
            }
        }

        static T GetAttribute<T>(ICustomAttributeProvider provider) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
        }

        private struct IfCondition
        {
            public readonly string Key;
            public readonly string Value;

            public IfCondition(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }
    }

    // Used on an optionsObject field or method to rename the corresponding commandline option.
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class CommandLineParameterAttribute : Attribute
    {
        public CommandLineParameterAttribute()
        {
            ValueName = "value";
        }

        public string Name { get; set; }

        public string Flag { get; set; }

        public bool Required { get; set; }

        public string ValueName { get; set; }

        public string Description { get; set; }
    }
}
