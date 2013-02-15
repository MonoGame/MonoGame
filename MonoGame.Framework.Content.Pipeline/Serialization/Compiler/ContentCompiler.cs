﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Reflection;

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
                        while ((baseType != null) && (baseType.GetGenericTypeDefinition() != contentTypeWriterType))
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
            ContentTypeWriter result = null;
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
                    result = (ContentTypeWriter)Activator.CreateInstance(chosen.MakeGenericType(concreteType));

                    // save it for next time.
                    typeWriterMap.Add(contentTypeWriterType, result.GetType());
                }
                catch (Exception)
                {
                    throw new InvalidContentException(String.Format("Could not find ContentTypeWriter for type '{0}'", type.Name));
                }
            }
            else
            {
                result = (ContentTypeWriter)Activator.CreateInstance(typeWriterType);
            }

            if (result != null)
            {
                MethodInfo dynMethod = result.GetType().GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(result, new object[] { this });
            }
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
}
