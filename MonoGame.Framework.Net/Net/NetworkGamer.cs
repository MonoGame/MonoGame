// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 

#region Using clause
using System;
using System.ComponentModel;

using Microsoft.Xna.Framework.GamerServices;
#endregion Using clause

namespace Microsoft.Xna.Framework.Net
{
	public class NetworkGamer : Gamer, INotifyPropertyChanged
	{
		
		private byte id;
		NetworkSession session; 
		//bool isHost;
		//bool isLocal;
		//bool hasVoice;
		long remoteUniqueIdentifier = -1;
		GamerStates gamerState;
		GamerStates oldGamerState;
		
		// Declare the event
		public event PropertyChangedEventHandler PropertyChanged;
		
		
		public NetworkGamer ( NetworkSession session, byte id, GamerStates state)
		{
			this.id = id;
			this.session = session;
			this.gamerState = state;
			// We will modify these HasFlags to inline code because MonoTouch does not support
			// the HasFlag method.  Also after reading this : http://msdn.microsoft.com/en-us/library/system.enum.hasflag.aspx#2
			// it just might be better to inline it anyway.
			//this.isHost = (state & GamerStates.Host) != 0; // state.HasFlag(GamerStates.Host);
			//this.isLocal = (state & GamerStates.Local) != 0; // state.HasFlag(GamerStates.Local);
			//this.hasVoice = (state & GamerStates.HasVoice) != 0; //state.HasFlag(GamerStates.HasVoice);
			
			// *** NOTE TODO **
			// This whole state stuff need to be looked at again.  Maybe we should not be using local
			//  variables here and instead just use the flags within the gamerState.
			
			this.gamerState = state;
			this.oldGamerState = state;
		}
		
		internal long RemoteUniqueIdentifier
		{
			get { return remoteUniqueIdentifier; }
			set { remoteUniqueIdentifier = value; }
		}
		
		public bool HasLeftSession 
		{ 
			get
			{
				return false;
			}
		}
		
		public bool HasVoice 
		{ 
			get
			{
				return (gamerState & GamerStates.HasVoice) != 0;
			}
		}
		
		public byte Id 
		{ 
			get
			{
				return id;
			}
		}
		
		public bool IsGuest 
		{ 
			get
			{
				return (gamerState & GamerStates.Guest) != 0;
			}
		}
		
		public bool IsHost 
		{ 
			get
			{
				return (gamerState & GamerStates.Host) != 0;
			}
		}
		
		public bool IsLocal 
		{ 
			get
			{
				return (gamerState & GamerStates.Local) != 0;
			}
		}
		
		public bool IsMutedByLocalUser 
		{ 
			get
			{
				return true;
			}
		}
		
		public bool IsPrivateSlot 
		{ 
			get
			{
				return false;
			}
		}
		
		public bool IsReady 
		{ 
			get
			{
				return (gamerState & GamerStates.Ready) != 0;
			}
			set
			{
				if (((gamerState & GamerStates.Ready) != 0) != value) {
					if (value)
						gamerState |= GamerStates.Ready;
					else
						gamerState &= ~GamerStates.Ready;
					OnPropertyChanged("Ready");
				}
			}
		}
		
		public bool IsTalking 
		{ 
			get
			{
				return false;
			}
		}
		
		private NetworkMachine _Machine;
		public NetworkMachine Machine 
		{ 
			get
			{
				return _Machine;
			}
			set
			{
				if (_Machine != value )
					_Machine = value;
			}
		}
		
		public TimeSpan RoundtripTime 
		{ 
			get
			{
				return TimeSpan.MinValue;
			}
		}
		
		public NetworkSession Session 
		{ 
			get
			{
				return session;
			}
		} 
		
		internal GamerStates State {
			get { return gamerState; }
			set { gamerState = value; }
		}
		
		internal GamerStates OldState {
			get { return oldGamerState; }
		}		
		
		// Create the OnPropertyChanged method to raise the event
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
		
	}
}
