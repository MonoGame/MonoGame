// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Xml.Serialization;

namespace MonoGame.Tools.Pipeline
{
    public class PipelineSettings
    {
        private const string SettingsPath = "Settings.xml";
        private IsolatedStorageFile _isoStore;

        public static PipelineSettings Default { get; private set; }

        public List<string> ProjectHistory;
        public string StartupProject;
        public Microsoft.Xna.Framework.Point Size;
        public int HSeparator, VSeparator;
        public bool Maximized, FilterOutput, DebugMode;

        static PipelineSettings()
        {
            Default = new PipelineSettings();
        }
        
        public PipelineSettings()
        {
            ProjectHistory = new List<string>();
            _isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);            
        }

        /// <summary>
        /// If the project already exists in history, it will be moved to the end.
        /// </summary>
        public void AddProjectHistory(string file)
        {
            var cleanFile = file.Trim();
            ProjectHistory.Remove(cleanFile);
            ProjectHistory.Add(cleanFile);
        }
        
        public void RemoveProjectHistory(string file)
        {
            var cleanFile = file.Trim();
            ProjectHistory.Remove(cleanFile);
        }   

        public void Clear()
        {
            ProjectHistory.Clear();
            StartupProject = null;
            Save();
        }

        public void Save()
        {
            var mode = FileMode.CreateNew;
            if (_isoStore.FileExists (SettingsPath)) 
				mode = FileMode.Truncate;

            using (var isoStream = new IsolatedStorageFileStream(SettingsPath, mode, _isoStore))
            {
                using (var writer = new StreamWriter(isoStream))
                {
                    var serializer = new XmlSerializer(typeof(PipelineSettings));
                    serializer.Serialize(writer, this);
                }
            }
        }

        public void Load()
		{
            if (_isoStore.FileExists(SettingsPath))
            {
                using (var isoStream = new IsolatedStorageFileStream(SettingsPath, FileMode.Open, _isoStore))
                {
                    using (var reader = new StreamReader(isoStream))
                    {
                        var serializer = new XmlSerializer(typeof(PipelineSettings));
                        Default = (PipelineSettings)serializer.Deserialize(reader);
                    }
                }
            }
        }
    }
}
