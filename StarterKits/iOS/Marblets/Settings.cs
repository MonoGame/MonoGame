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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
	}
}
