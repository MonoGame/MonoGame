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
    public class WindowConfiguration
    {
        [Serializable]
        public class WindowConfigurationSettings
        {
            public int X = 0;
            public int Y = 0;
            public int Width = 751;
            public int Height = 557;
            public int SeparatorPosition = 179;

            public override string ToString ()
            {
                return string.Format ("[WindowConfigurationSettings] x:{0}, y:{1}, width:{2}, height:{3}, sepeator:{4}", X, Y, Width, Height, SeparatorPosition);
            }
        }


        private const string WindowConfigurationPath = "WindowConfiguration.txt";

        private IsolatedStorageFile _isoStore;

        public static WindowConfiguration Default { get; private set; }
        public WindowConfigurationSettings _settings;


        static WindowConfiguration()
        {
            Default = new WindowConfiguration();
        }
        
        public WindowConfiguration()
        {
            _settings = new WindowConfigurationSettings();
            _isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);            
        }

        public WindowConfigurationSettings Settings
        {
            get
            {
                return _settings;
            }
        }        


        public void Save()
        {
            var mode = FileMode.CreateNew;
			if (_isoStore.FileExists (WindowConfigurationPath)) 
				mode = FileMode.Truncate;

            using (var isoStream = new IsolatedStorageFileStream(WindowConfigurationPath, mode, _isoStore))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(isoStream, Settings);
            }
        }

        public void Load()
		{
            if (_isoStore.FileExists(WindowConfigurationPath))
            {
                using (var isoStream = new IsolatedStorageFileStream(WindowConfigurationPath, FileMode.Open, _isoStore))
                {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    _settings = (WindowConfigurationSettings)binaryFormatter.Deserialize(isoStream);
                }
            }
        }
    }
}
