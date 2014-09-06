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
        static History()
        {
            Default = new History();
        }

        public History()
        {
            _projectHistory = new List<string>();
            _isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            LoadProjectHistory();
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

        public IList<string> ProjectHistory
        {
            get
            {
                return _projectHistory.AsReadOnly();
            }
        }

        public void LoadProjectHistory()
        {
            if (_isoStore.FileExists(ProjectHistoryPath))
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(ProjectHistoryPath, FileMode.Open, _isoStore))
                {
                    using (StreamReader reader = new StreamReader(isoStream))
                    {
                        var projects = reader.ReadToEnd().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries );
                        foreach (var file in projects)
                        {
                            AddProjectHistory(file);
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            _projectHistory.Clear();
            Save();
        }

        public void Save()
        {
            using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(ProjectHistoryPath, FileMode.Create, _isoStore))
            {
                using (StreamWriter writer = new StreamWriter(isoStream))
                {
                    foreach (var file in _projectHistory)
                    {
                        writer.WriteLine(file);
                    }
                }
            }
        }

    }
}
