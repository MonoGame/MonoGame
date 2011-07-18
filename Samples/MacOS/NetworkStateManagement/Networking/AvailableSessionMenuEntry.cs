#region File Description
//-----------------------------------------------------------------------------
// AvailableSessionMenuEntry.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

#endregion

namespace NetworkStateManagement
{
	/// <summary>
	/// Helper class customizes the standard MenuEntry class
	/// for displaying AvailableNetworkSession objects.
	/// </summary>
	class AvailableSessionMenuEntry : MenuEntry
	{
	#region Fields

		AvailableNetworkSession availableSession;
		bool gotQualityOfService;

	#endregion

	#region Properties


		/// <summary>
		/// Gets the available network session corresponding to this menu entry.
		/// </summary>
		public AvailableNetworkSession AvailableSession {
			get { return availableSession; }
		}


	#endregion

	#region Initialization


		/// <summary>
		/// Constructs a menu entry describing an available network session.
		/// </summary>
		public AvailableSessionMenuEntry (AvailableNetworkSession availableSession)
		: base(GetMenuItemText(availableSession))
			{
			this.availableSession = availableSession;
		}


		/// <summary>
		/// Formats session information to create the menu text string.
		/// </summary>
		static string GetMenuItemText (AvailableNetworkSession session)
		{
			int totalSlots = session.CurrentGamerCount + 
				session.OpenPublicGamerSlots;

			return string.Format ("{0} ({1}/{2})", session.HostGamertag, 
						session.CurrentGamerCount, 
						totalSlots);
		}


	#endregion

	#region Update


		/// <summary>
		/// Updates the menu item text, adding information about the network
		/// quality of service as soon as that becomes available.
		/// </summary>
		public override void Update (MenuScreen screen, bool isSelected, 
							GameTime gameTime)
		{
			base.Update (screen, isSelected, gameTime);

			// Quality of service data can take some time to query, so it will not
			// be filled in straight away when NetworkSession.Find returns. We want
			// to display the list of available sessions straight away, and then
			// fill in the quality of service data whenever that becomes available,
			// so we keep checking until this data shows up.
			if (screen.IsActive && !gotQualityOfService) {
				QualityOfService qualityOfService = availableSession.QualityOfService;

				if (qualityOfService.IsAvailable) {
					TimeSpan pingTime = qualityOfService.AverageRoundtripTime;

					Text += string.Format (" - {0:0} ms", pingTime.TotalMilliseconds);

					gotQualityOfService = true;
				}
			}
		}


	#endregion
	}
}
