// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Provides methods for writing compiled binary format.
    /// </summary>
    public sealed class ContentCompiler
    {
        readonly Dictionary<Type, Type> typeWriterMap = new Dictionary<Type, Type>();

        /// <summary>
        /// Initializes a new instance of ContentCompiler.
        /// </summary>
        internal ContentCompiler()
        {
            GetTypeWriters();
        }

        /// <summary>
        /// Iterates through all loaded assemblies and finds the content type writers.
        /// </summary>
        void GetTypeWriters()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                Type[] exportedTypes;
                try
                {
                    exportedTypes = assembly.GetTypes();
                }
                catch (Exception)
                {
                    continue;
                }

                var contentTypeWriterType = typeof(ContentTypeWriter<>);
                foreach (var type in exportedTypes)
                {
                    if (type.IsAbstract)
                        continue;
                    if (Attribute.IsDefined(type, typeof(ContentTypeWriterAttribute)))
                    {
                        // Find the content type this writer implements
                        Type baseType = type.BaseType;
                        while ((baseType != null) && (baseType.Name != contentTypeWriterType.Name))
                            baseType = baseType.BaseType;
                        if (baseType != null)
                            typeWriterMap.Add(baseType, type);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the worker writer for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The worker writer.</returns>
        /// <remarks>This should be called from the ContentTypeWriter.Initialize method.</remarks>
        public ContentTypeWriter GetTypeWriter(Type type)
        {
            var contentTypeWriterType = typeof(ContentTypeWriter<>).MakeGenericType(type);
            Type typeWriterType;
            if (!typeWriterMap.TryGetValue(contentTypeWriterType, out typeWriterType))
            {
                var inputTypeDef = type.GetGenericTypeDefinition();

                Type chosen = null;
                foreach (var kvp in typeWriterMap)
                {
                    var args = kvp.Key.GetGenericArguments();

                    if (args.Length == 0)
                        continue;

                    if (!args[0].IsGenericType)
                        continue;

                    // Compare generic type definition
                    var keyTypeDef = args[0].GetGenericTypeDefinition();
                    if (inputTypeDef.Equals(keyTypeDef))
                    {
                        chosen = kvp.Value;
                        break;
                    }
                }

                try
                {
                    var concreteType = type.GetGenericArguments();
                    var output = (ContentTypeWriter)Activator.CreateInstance(chosen.MakeGenericType(concreteType));

                    MethodInfo dynMethod = output.GetType().GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
                    dynMethod.Invoke(output, new object[] { this });

                    // save it for next time.
                    typeWriterMap.Add(contentTypeWriterType, output.GetType());
                    return output;
                }
                catch (Exception)
                {
                    throw new InvalidContentException(String.Format("Could not find ContentTypeWriter for type '{0}'", type.Name));
                }                
            }
                
            var result = (ContentTypeWriter)Activator.CreateInstance(typeWriterType);
            return result;
        }

        /// <summary>
        /// Write the content to a XNB file.
        /// </summary>
        /// <param name="stream">The stream to write the XNB file to.</param>
        /// <param name="content">The content to write to the XNB file.</param>
        /// <param name="targetPlatform">The platform the XNB is intended for.</param>
        /// <param name="targetProfile">The graphics profile of the target.</param>
        /// <param name="compress">True if the content should be compressed.</param>
        /// <param name="rootDirectory">The root directory of the content.</param>
        /// <param name="referenceRelocationPath">The path of the XNB file, used to calculate relative paths for external references.</param>
        internal void Compile(Stream stream, object content, TargetPlatform targetPlatform, GraphicsProfile targetProfile, bool compressContent, string rootDirectory, string referenceRelocationPath)
        {
            using (var writer = new ContentWriter(this, stream, targetPlatform, targetProfile, compressContent, rootDirectory, referenceRelocationPath))
            {
                writer.WriteObject(content);
                writer.Flush();
            }
        }
    }





    public static class TypeHelpers
    {
        /// <summary>
        /// Gets the System.Type with the specified name, performing a case-sensitive search.
        /// </summary>
        /// <param name="typeName">The assembly-qualified name of the type to get. See System.Type.AssemblyQualifiedName.</param>
        /// <param name="throwOnError">Whether or not to throw an exception or return null if the type was not found.</param>
        /// <param name="ignoreCase">Whether or not to perform a case-insensitive search.</param>
        /// <returns>The System.Type with the specified name.</returns>
        /// <remarks>
        /// This method can load types from dynamically loaded assemblies as long as the referenced assembly 
        /// has already been loaded into the current AppDomain.
        /// </remarks>
        public static Type GetType(string typeName, bool throwOnError, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException("typeName");

            // handle the trivial case
            Type type;
            if ((type = Type.GetType(typeName, false, ignoreCase)) != null)
                return type;

            // otherwise, perform the recursive search
            try
            {
                return GetTypeFromRecursive(typeName, ignoreCase);
            }
            catch (Exception e)
            {
                if (throwOnError)
                    throw;
            }

            return null;
        }

        #region Private Static Helper Methods

        private static Type GetTypeFromRecursive(string typeName, bool ignoreCase)
        {
            int startIndex = typeName.IndexOf('[');
            int endIndex = typeName.LastIndexOf(']');

            if (startIndex == -1)
            {
                // try to load the non-generic type (e.g. System.Int32)
                return TypeHelpers.GetNonGenericType(typeName, ignoreCase);
            }
            else
            {
                // determine the cardinality of the generic type
                int cardinalityIndex = typeName.IndexOf('`', 0, startIndex);
                string cardinalityString = typeName.Substring(cardinalityIndex + 1, startIndex - cardinalityIndex - 1);
                int cardinality = int.Parse(cardinalityString);

                // get the FullName of the non-generic type (e.g. System.Collections.Generic.List`1)
                string fullName = typeName.Substring(0, startIndex);
                if (typeName.Length - endIndex - 1 > 0)
                    fullName += typeName.Substring(endIndex + 1, typeName.Length - endIndex - 1);

                // parse the child type arguments for this generic type (recursive)
                List<Type> list = new List<Type>();
                string typeArguments = typeName.Substring(startIndex + 1, endIndex - startIndex - 1);
                foreach (string item in EachAssemblyQualifiedName(typeArguments, cardinality))
                {
                    Type typeArgument = GetTypeFromRecursive(item, ignoreCase);
                    list.Add(typeArgument);
                }

                // construct the generic type definition
                return TypeHelpers.GetNonGenericType(fullName, ignoreCase).MakeGenericType(list.ToArray());
            }
        }

        private static IEnumerable<string> EachAssemblyQualifiedName(string s, int count)
        {
            Debug.Assert(count != 0);
            Debug.Assert(string.IsNullOrEmpty(s) == false);
            Debug.Assert(s.Length > 2);

            // e.g. "[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]"
            // e.g. "[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.DateTime, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]"
            // e.g. "[System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.DateTime, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]"

            int startIndex = 0;
            int bracketCount = 0;

            while (count > 0)
            {
                bracketCount = 0;

                for (int i = startIndex; i < s.Length; i++)
                {
                    switch (s[i])
                    {
                        case '[':
                            bracketCount++;
                            continue;

                        case ']':
                            if (--bracketCount == 0)
                            {
                                string item = s.Substring(startIndex + 1, i - startIndex - 1);
                                yield return item;
                                startIndex = i + 2;
                            }
                            break;

                        default:
                            continue;
                    }
                }

                if (bracketCount != 0)
                {
                    const string SR_Malformed = "The brackets are unbalanced in the string, '{0}'.";
                    throw new FormatException(string.Format(SR_Malformed, s));
                }

                count--;
            }
        }

        private static Type GetNonGenericType(string typeName, bool ignoreCase)
        {
            // assume the type information is not a dynamically loaded assembly
            Type type = Type.GetType(typeName, false, ignoreCase);
            if (type != null)
                return type;

            // otherwise, search the assemblies in the current AppDomain for the type
            int assemblyFullNameIndex = typeName.IndexOf(',');
            if (assemblyFullNameIndex != -1)
            {
                string assemblyFullName = typeName.Substring(assemblyFullNameIndex + 2, typeName.Length - assemblyFullNameIndex - 2);
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().FullName == assemblyFullName)
                    {
                        string fullName = typeName.Substring(0, assemblyFullNameIndex);
                        type = assembly.GetType(fullName, false, ignoreCase);
                        if (type != null)
                            return type;
                    }
                }
            }

            // no luck? blow up
            const string SR_TypeNotFound = "The type, '{0}', was not found.";
            throw new ArgumentException(string.Format(SR_TypeNotFound, typeName), "typeName");
        }

        #endregion
    }
}
