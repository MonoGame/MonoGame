#region File Description
//-----------------------------------------------------------------------------
// Settings.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Xml.Serialization;
#if IPHONE
using XnaTouch.Framework;
using XnaTouch.Framework.Graphics;
using XnaTouch.Framework.Storage;
using XnaTouch.Framework.GamerServices;
#else
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
#endif
#endregion

namespace Marblets
{
    /// <summary>
    /// The Setting class handles loading and saving of global application settings.
    /// The normal .Net classes (System.Configuration) for doing this are not available
    /// on the Xbox 360.
    /// </summary>
    public class Settings
    {
        private static string fileName;

        #region General App Settings

        /// <summary>
        /// The path to look for all media in
        /// </summary>
        public string MediaPath = @"Content\";

        /// <summary>
        /// The name of the window when running in windowed mode
        /// </summary>
        public string WindowTitle = "Marblets";

        #endregion

        #region MarbleColors

        /// <summary>
        /// Default marble colors to use
        /// </summary>
        public Color[] MarbleColors = new Color[] { new Color(255, 0, 0), 
                                                    new Color(40, 175, 255),
                                                    new Color(40, 255, 20),
                                                    new Color(255, 255, 0),
                                                    new Color(255, 20, 230) };

        //monochrome
        //public Color[] MarbleColors = new Color[] { new Color(255, 255, 255), 
        //                                            new Color(205, 205, 205), 
        //                                            new Color(155, 155, 155), 
        //                                            new Color(105, 105, 105), 
        //                                            new Color(55, 55, 55) };

        #endregion

        #region Load/Save code

        // Modified to work with the StorageDevice, rather than direct file access.

        /// <summary>
        /// Saves the current settings
        /// </summary>
        /// <param name="filename">The filename to save to</param>
        public void Save()
        {
            // TODO Guide.BeginShowStorageDeviceSelector(new AsyncCallback(SaveSettingsCallback),    null);
        }

        private static void SaveSettingsCallback(IAsyncResult result)
        {
            if ((result != null) && result.IsCompleted)
            {
                // TODO MarbletsGame.StorageDevice = Guide.EndShowStorageDeviceSelector(result);
            }

            if ((MarbletsGame.StorageDevice != null) &&
                MarbletsGame.StorageDevice.IsConnected)
            {
                using (StorageContainer storageContainer =
                    MarbletsGame.StorageDevice.OpenContainer("Marblets"))
                {
                    string settingsPath = Path.Combine(storageContainer.Path,
                        "settings.xml");

                    using (FileStream file = File.Create(settingsPath))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                        serializer.Serialize(file, MarbletsGame.Settings);
                    }
                }
            }
        }

        /// <summary>
        /// Loads settings from a file
        /// </summary>
        /// <param name="filename">The filename to load</param>
        public static void Load()
        {
            // TODO Guide.BeginShowStorageDeviceSelector(new AsyncCallback(LoadSettingsCallback), null);
        }

        private static void LoadSettingsCallback(IAsyncResult result)
        {
            if ((result != null) && result.IsCompleted)
            {
                // TODO MarbletsGame.StorageDevice = Guide.EndShowStorageDeviceSelector(result);
            }

            if ((MarbletsGame.StorageDevice != null) &&
                MarbletsGame.StorageDevice.IsConnected)
            {
                using (StorageContainer storageContainer =
                    MarbletsGame.StorageDevice.OpenContainer("Marblets"))
                {
                    string settingsPath = Path.Combine(storageContainer.Path,
                        "settings.xml");

                    if (File.Exists(settingsPath))
                    {
                        using (FileStream file =
                            File.Open(settingsPath, FileMode.Open))
                        {
                            XmlSerializer serializer =
                                new XmlSerializer(typeof(Settings));
                            MarbletsGame.Settings = 
                                (Settings)serializer.Deserialize(file);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
