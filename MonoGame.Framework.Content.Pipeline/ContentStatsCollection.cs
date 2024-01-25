// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// A collection of content building statistics for use in diagnosing content issues.
    /// </summary>
    public class ContentStatsCollection
    {
        private static readonly string _header = "Source File,Dest File,Processor Type,Content Type,Source File Size,Dest File Size,Build Seconds";
        private static readonly Regex _split = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

        private readonly object _locker = new object();
        private readonly Dictionary<string, ContentStats> _statsBySource = new Dictionary<string, ContentStats>(1024);

        public static readonly string Extension = ".mgstats";

        /// <summary>
        /// Optionally used for copying stats that were stored in another collection.
        /// </summary>
        public ContentStatsCollection PreviousStats { get; set; }

        /// <summary>
        ///  The internal content statistics dictionary.
        /// </summary>
        public IReadOnlyDictionary<string, ContentStats> Stats
        {
            get { return _statsBySource; }
        }

        /// <summary>
        ///  Get the content statistics for a source file and returns true if found.
        /// </summary>
        public bool TryGetStats(string sourceFile, out ContentStats stats)
        {
            lock (_locker)
            {
                if (!_statsBySource.TryGetValue(sourceFile, out stats))
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Clears all the content statistics.
        /// </summary>
        public void Reset()
        {
            lock (_locker)
                _statsBySource.Clear();
        }

        /// <summary>
        /// Store content building stats for a source file.
        /// </summary>
        /// <param name="sourceFile">The absolute path to the source asset file.</param>
        /// <param name="destFile">The absolute path to the destination content file.</param>
        /// <param name="processorType">The type name of the content processor.</param>
        /// <param name="contentType">The content type object.</param>
        /// <param name="buildSeconds">The build time in seconds.</param>
        public void RecordStats(string sourceFile, string destFile, string processorType, Type contentType, float buildSeconds)
        {
            var sourceSize = new FileInfo(sourceFile).Length;
            var destSize = new FileInfo(destFile).Length;

            lock (_locker)
            {
                ContentStats stats;
                _statsBySource.TryGetValue(sourceFile, out stats);

                stats.SourceFile = sourceFile;
                stats.DestFile = destFile;

                stats.SourceFileSize = sourceSize;
                stats.DestFileSize = destSize;

                stats.ContentType = GetFriendlyTypeName(contentType);
                stats.ProcessorType = processorType;

                stats.BuildSeconds = buildSeconds;

                _statsBySource[stats.SourceFile] = stats;
            }
        }

        /// <summary>
        /// Copy content building stats to the current collection from the PreviousStats.
        /// </summary>
        /// <param name="sourceFile">The absolute path to the source asset file.</param>
        public void CopyPreviousStats(string sourceFile)
        {
            if (PreviousStats == null)
                return;

            lock (_locker)
            {
                if (_statsBySource.ContainsKey(sourceFile))
                    return;

                ContentStats stats;
                if (PreviousStats.TryGetStats(sourceFile, out stats))
                    _statsBySource[stats.SourceFile] = stats;
            }
        }

        private static string GetFriendlyTypeName(Type type)
        {
            if (type == null)
                return "";
            if (type == typeof(int))
                return "int";
            else if (type == typeof(short))
                return "short";
            else if (type == typeof(byte))
                return "byte";
            else if (type == typeof(bool))
                return "bool";
            else if (type == typeof(long))
                return "long";
            else if (type == typeof(float))
                return "float";
            else if (type == typeof(double))
                return "double";
            else if (type == typeof(decimal))
                return "decimal";
            else if (type == typeof(string))
                return "string";
            else if (type.IsArray)
                return GetFriendlyTypeName(type.GetElementType()) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
            else if (type.IsGenericType)
                return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => GetFriendlyTypeName(x)).ToArray()) + ">";
            else
                return type.Name;
        }

        /// <summary>
        /// Load the content statistics from a folder.
        /// </summary>
        /// <param name="outputPath">The folder where the .mgstats file can be found.</param>
        /// <returns>Returns the content statistics or an empty collection.</returns>
        public static ContentStatsCollection Read(string outputPath)
        {
            var collection = new ContentStatsCollection();

            var filePath = Path.Combine(outputPath, Extension);
            try
            {
                var lines = File.ReadAllLines(filePath);

                // The first line is the CSV header... if it doesn't match then
                // assume the data is invalid or changed formats.
                if (lines[0] != _header)
                    return collection;

                for (var i = 1; i < lines.Length; i++)
                {
                    var columns = _split.Split(lines[i]);
                    if (columns.Length != 7)
                        continue;

                    ContentStats stats;
                    stats.SourceFile = columns[0].Trim('"');
                    stats.DestFile = columns[1].Trim('"');
                    stats.ProcessorType = columns[2].Trim('"');
                    stats.ContentType = columns[3].Trim('"');
                    stats.SourceFileSize = long.Parse(columns[4]);
                    stats.DestFileSize = long.Parse(columns[5]);
                    stats.BuildSeconds = float.Parse(columns[6]);

                    if (!collection._statsBySource.ContainsKey(stats.SourceFile))
                        collection._statsBySource.Add(stats.SourceFile, stats);
                }
            }
            catch (Exception ex)
            {
                // Assume the file didn't exist or was incorrectly
                // formatted... either way we start from fresh data.
                collection.Reset();
            }

            return collection;
        }

        /// <summary>
        /// Write the content statistics to a folder with the .mgstats file name.
        /// </summary>
        /// <param name="outputPath">The folder to write the .mgstats file.</param>
        public void Write(string outputPath)
        {
            // ensure the output folder exists
            Directory.CreateDirectory(outputPath);
            var filePath = Path.Combine(outputPath, Extension);
            using (var textWriter = new StreamWriter(filePath, false, new UTF8Encoding(false)))
            {
                // Sort the items alphabetically to ensure a consistent output
                // and better mergability of the resulting file.
                var contentStats = _statsBySource.Values.OrderBy(c => c.SourceFile, StringComparer.InvariantCulture).ToList();

                textWriter.WriteLine(_header);
                foreach (var stats in contentStats)
                    textWriter.WriteLine("\"{0}\",\"{1}\",\"{2}\",\"{3}\",{4},{5},{6}", stats.SourceFile, stats.DestFile, stats.ProcessorType, stats.ContentType, stats.SourceFileSize, stats.DestFileSize, stats.BuildSeconds);
            }
        }

        /// <summary>
        /// Merge in statistics from PreviousStats that do not exist in this collection.
        /// </summary>
        public void MergePreviousStats()
        {
            if (PreviousStats == null)
                return;

            foreach (var stats in PreviousStats._statsBySource.Values)
            {
                if (!_statsBySource.ContainsKey(stats.SourceFile))
                    _statsBySource.Add(stats.SourceFile, stats);
            }
        }
    }
}
