#region File Description
//-----------------------------------------------------------------------------
// IMessageDisplay.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;

#endregion

namespace NetworkStateManagement
{
	/// <summary>
	/// Interface used to display notification messages when interesting events occur,
	/// for instance when gamers join or leave the network session. This interface
	/// is registered as a service, so any piece of code wanting to display a message
	/// can look it up from Game.Services, without needing to worry about how the
	/// message display is implemented. In this sample, the MessageDisplayComponent
	/// class implement this IMessageDisplay service.
	/// </summary>
	interface IMessageDisplay : IDrawable, IUpdateable
	{
		void ShowMessage (string message, params object[] parameters);
	}
}
