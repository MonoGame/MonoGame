#region File Description
//-----------------------------------------------------------------------------
// GameScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Xml.Linq;


#endregion

namespace HoneycombRush
{
    /// <summary>
    /// Repesent the difficulty mode of the game.
    /// </summary>
    public enum DifficultyMode
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }

    /// <summary>
    /// Supplies access to configuration data.
    /// </summary>
    public static class ConfigurationManager
    {
        #region Fields


        static Dictionary<DifficultyMode, Configuration> modesConfiguration;
        public static Dictionary<DifficultyMode, Configuration> ModesConfiguration
        {
            get
            {
                return modesConfiguration;
            } 
        }


        static bool isLoaded = false;
        public static bool IsLoaded 
        { 
            get 
            { 
                return isLoaded; 
            } 
        }

        public static DifficultyMode? DifficultyMode { get; set; }


        #endregion

        #region Public Static Methods


        /// <summary>
        /// Load configuration from an XML document.
        /// </summary>
        /// <param name="doc">The XML document containing configuration.</param>
        public static void LoadConfiguration(XDocument doc)
        {
            modesConfiguration = new Dictionary<DifficultyMode, Configuration>();
            foreach (XElement element in doc.Element("Difficulties").Elements("Difficulty"))
            {
                modesConfiguration.Add((DifficultyMode)Enum.Parse(typeof(DifficultyMode),
                    element.Attribute("ID").Value, true),
                    new Configuration()
                    {
                        DecreaseAmountSpeed = int.Parse(element.Element("DecreaseAmountSpeed").Value),
                        GameElapsed = TimeSpan.Parse(element.Element("GameElapsed").Value),
                        IncreaseAmountSpeed = int.Parse(element.Element("IncreaseAmountSpeed").Value),
                        MaxSoldierBeeVelocity = int.Parse(element.Element("MaxSoldierBeeVelocity").Value),
                        MaxWorkerBeeVelocity = int.Parse(element.Element("MaxWorkerBeeVelocity").Value),
                        MinSoldierBeeVelocity = int.Parse(element.Element("MinSoldierBeeVelocity").Value),
                        MinWorkerBeeVelocity = int.Parse(element.Element("MinWorkerBeeVelocity").Value),
                        TotalSmokeAmount = int.Parse(element.Element("TotalSmokeAmount").Value),
                        HighScoreFactor = int.Parse(element.Element("HighScoreFactor").Value)
                    });
            }
            isLoaded = true;
        }


        #endregion
    }

    /// <summary>
    /// Contains all configuration memebers.
    /// </summary>
    public struct Configuration
    {
        public TimeSpan GameElapsed { get; set; }

        public float MinWorkerBeeVelocity { get; set; }
        public float MaxWorkerBeeVelocity { get; set; }
        public float MinSoldierBeeVelocity { get; set; }
        public float MaxSoldierBeeVelocity { get; set; }

        public int TotalSmokeAmount { get; set; }
        public int DecreaseAmountSpeed { get; set; }
        public int IncreaseAmountSpeed { get; set; }
        public int HighScoreFactor { get; set; }
    }
}
