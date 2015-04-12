// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace MonoGame.Tools.Pipeline
{
    public class History
    {
        private const string ProjectHistoryPath = "ProjectHistory.txt";

        private IsolatedStorageFile _isoStore;
        private List<string> _projectHistory;

        public static History Default { get; private set; }

        public string StartupProject { get; set; }

        static History()
        {
            Default = new History();
        }
        
        public History()
        {
            _projectHistory = new List<string>();
            _isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);            
        }

        /// <summary>
        /// If the project already exists in history, it will be moved to the end.
        /// </summary>
        public void AddProjectHistory(string file)
        {
            var cleanFile = file.Trim();
            _projectHistory.Remove(cleanFile);
            _projectHistory.Add(cleanFile);
        }
        
        public void RemoveProjectHistory(string file)
        {
            var cleanFile = file.Trim();
            _projectHistory.Remove(cleanFile);
        }

        public IEnumerable<string> ProjectHistory
        {
            get
            {
                return _projectHistory;
            }
        }        

        public void Clear()
        {
            _projectHistory.Clear();
            StartupProject = null;
            Save();
        }

        public void Save()
        {
            var mode = FileMode.CreateNew;
			if (_isoStore.FileExists (ProjectHistoryPath)) 
				mode = FileMode.Truncate;

            using (var isoStream = new IsolatedStorageFileStream(ProjectHistoryPath, mode, _isoStore))
            {
                using (var writer = new StreamWriter(isoStream))
                {
                    writer.WriteLine(StartupProject);

                    foreach (var file in _projectHistory)
                        writer.WriteLine(file);
                }
            }
        }

        public void Load()
		{
            if (_isoStore.FileExists(ProjectHistoryPath))
            {
                using (var isoStream = new IsolatedStorageFileStream(ProjectHistoryPath, FileMode.Open, _isoStore))
                {
                    using (var reader = new StreamReader(isoStream))
                    {
                        var line = reader.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            line = line.Trim();
                            if (File.Exists(line))
                                StartupProject = line;
                        }

                        while (!reader.EndOfStream)
                        {
                            line = reader.ReadLine();
                            if (string.IsNullOrEmpty(line))
                                continue;

                            line = line.Trim();
                            if (!File.Exists(line))
                                continue;

                            AddProjectHistory(line);
                        }
                    }
                }
            }
        }
    }
}
