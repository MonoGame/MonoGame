#region License
// /*
// Microsoft Public License (Ms-PL)
// XnaTouch - Copyright © 2009 The XnaTouch Team
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
#endregion License

#region Using clause
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using MonoTouch.UIKit;
using MonoTouch.GameKit;
using XnaTouch.Framework.GamerServices;
#endregion Using clause

namespace XnaTouch.Framework.Net
{
	// The delegate must have the same signature as the method
    // it will call asynchronously.
	public delegate NetworkSession NetworkSessionAsynchronousCreate(
         NetworkSessionType sessionType, // Type of session being hosted.
         int maxLocalGamers,             // Maximum number of local players on the same gaming machine in this network session.
         int maxGamers                   // Maximum number of players allowed in this network session.  For Zune-based games, this value must be between 2 and 8; 8 is the maximum number of players supported in the session.
		);
    public delegate AvailableNetworkSessionCollection  NetworkSessionAsynchronousFind(
         NetworkSessionType sessionType,
         int maxLocalGamers,
         NetworkSessionProperties searchProperties );
	
	public delegate NetworkSession NetworkSessionAsynchronousJoin(AvailableNetworkSession availableSession);
	
	public delegate NetworkSession NetworkSessionAsynchronousJoinInvited(int maxLocalGamers);
	
	public sealed class NetworkSession : IDisposable
	{
		private static NetworkSessionState networkSessionState;
		private static NetworkSessionType networkSessionType;
		private static GKSession gkSession;		

		public NetworkSession()
		{
			
		}
		
		public static NetworkSession Create (
         NetworkSessionType sessionType,			// Type of session being hosted.
         IEnumerable<SignedInGamer> localGamers,	// Maximum number of local players on the same gaming machine in this network session.
         int maxGamers,								// Maximum number of players allowed in this network session.  For Zune-based games, this value must be between 2 and 8; 8 is the maximum number of players supported in the session.
         int privateGamerSlots,						// Number of reserved private session slots created for the session. This value must be less than maximumGamers. 
         NetworkSessionProperties sessionProperties // Properties of the session being created.
		)
		{
			try
			{
				if ( maxGamers < 2 || maxGamers > 8 ) 
					throw new ArgumentOutOfRangeException( "Maximum number of gamers must be between 2 and 8."  );
				if ( privateGamerSlots < 0 || privateGamerSlots > maxGamers ) 
					throw new ArgumentOutOfRangeException( "Private session slots must be between 0 and maximum number of gamers."  );
			
				networkSessionType = sessionType;
			
				throw new NotImplementedException();
			}
			finally
			{
			}
		} 
		
		public static NetworkSession Create (
         NetworkSessionType sessionType, // Type of session being hosted.
         int maxLocalGamers,             // Maximum number of local players on the same gaming machine in this network session.
         int maxGamers                   // Maximum number of players allowed in this network session.  For Zune-based games, this value must be between 2 and 8; 8 is the maximum number of players supported in the session.
		)
		{
			try
			{
				if ( maxLocalGamers > 2 ) 
					throw new ArgumentOutOfRangeException( "Maximum local players can only be 2 on the iPhone." );
				if ( maxGamers < 2 || maxGamers > 8 ) 
					throw new ArgumentOutOfRangeException( "Maximum number of gamers must be between 2 and 8." );
				
				networkSessionType = sessionType;
				
				GKSessionMode gkSessionMode;
				switch (sessionType)
				{
				   case NetworkSessionType.Local:
				      gkSessionMode = GKSessionMode.Client;
				      break;
				   case NetworkSessionType.SystemLink:
				      gkSessionMode = GKSessionMode.Peer;
				      break;
				   case NetworkSessionType.PlayerMatch:
				      gkSessionMode = GKSessionMode.Server;
				      break;
				   default:
				      gkSessionMode = GKSessionMode.Peer;
				      break;
				}
				
				gkSession = new GKSession( null, null, gkSessionMode );
				gkSession.Available = true;
				
				throw new NotImplementedException();
			}
			finally
			{
			}
		}
		
		public static NetworkSession Create (
         NetworkSessionType sessionType,
         int maxLocalGamers,
         int maxGamers,
         int privateGamerSlots,
         NetworkSessionProperties sessionProperties
		)
		{
			try
			{
				if ( maxLocalGamers != 1 ) 
					throw new ArgumentOutOfRangeException( "Maximum local players can only be 1 on the iPhone." );
				if ( maxGamers < 2 || maxGamers > 8 ) 
					throw new ArgumentOutOfRangeException( "Maximum number of gamers must be between 2 and 8." );
				if ( privateGamerSlots < 0 || privateGamerSlots > maxGamers ) 
					throw new ArgumentOutOfRangeException( "Private session slots must be between 0 and maximum number of gamers."  );
			
			
				networkSessionType = sessionType;
				throw new NotImplementedException();
			}
			finally
			{
			}
		}
		
		#region IDisposable Members

        public void Dispose()
        {
			// TODO this.Dispose(true);
    			GC.SuppressFinalize(this);
        }

        #endregion
		
		public void AddLocalGamer ( SignedInGamer gamer )
		{
			throw new NotImplementedException();
		}
		
		public static IAsyncResult BeginCreate (
         NetworkSessionType sessionType,
         IEnumerable<SignedInGamer> localGamers,
         int maxGamers,
         int privateGamerSlots,
         NetworkSessionProperties sessionProperties,
         AsyncCallback callback,
         Object asyncState )
		{
			if ( maxGamers < 2 || maxGamers > 8 ) 
				throw new ArgumentOutOfRangeException( "Maximum number of gamers must be between 2 and 8."  );
			if ( privateGamerSlots < 0 || privateGamerSlots > maxGamers ) 
				throw new ArgumentOutOfRangeException( "Private session slots must be between 0 and maximum number of gamers."  );
			
			try
			{
				throw new NotImplementedException();
			}
			finally
			{
			}
		}
		
		public static IAsyncResult BeginCreate (
         NetworkSessionType sessionType,
         int maxLocalGamers,
         int maxGamers,
         AsyncCallback callback,
         Object asyncState
)
		{
			if ( maxLocalGamers != 1 ) 
				throw new ArgumentOutOfRangeException( "Maximum local players can only be 1 on the iPhone." );
			if ( maxGamers < 2 || maxGamers > 8 ) 
				throw new ArgumentOutOfRangeException( "Maximum number of gamers must be between 2 and 8." );
			
			try
			{
				NetworkSessionAsynchronousCreate AsynchronousCreate = new NetworkSessionAsynchronousCreate(Create);
            	return AsynchronousCreate.BeginInvoke(sessionType, maxLocalGamers, maxGamers, callback, asyncState);
			}
			finally
			{
			}
		}
		
		public static IAsyncResult BeginCreate (
         NetworkSessionType sessionType,
         int maxLocalGamers,
         int maxGamers,
         int privateGamerSlots,
         NetworkSessionProperties sessionProperties,
         AsyncCallback callback,
         Object asyncState )
		{
			if ( maxLocalGamers != 1 ) 
				throw new ArgumentOutOfRangeException( "Maximum local players can only be 1 on the iPhone." );
			if ( maxGamers < 2 || maxGamers > 8 ) 
				throw new ArgumentOutOfRangeException( "Maximum number of gamers must be between 2 and 8." );
			if ( privateGamerSlots < 0 || privateGamerSlots > maxGamers ) 
				throw new ArgumentOutOfRangeException( "Private session slots must be between 0 and maximum number of gamers."  );
			
			try
			{
				throw new NotImplementedException();
			}
			finally
			{
			}
		}
		
		public static IAsyncResult BeginFind (
         NetworkSessionType sessionType,
         IEnumerable<SignedInGamer> localGamers,
         NetworkSessionProperties searchProperties,
         AsyncCallback callback,
         Object asyncState
)
		{
			if ( sessionType == NetworkSessionType.Local )
				throw new ArgumentException( "NetworkSessionType cannot be NetworkSessionType.Local." );
			
			try
			{
				throw new NotImplementedException();
			}
			finally
			{
			}
			
		}
		
		public static IAsyncResult BeginFind (
         NetworkSessionType sessionType,
         int maxLocalGamers,
         NetworkSessionProperties searchProperties,
         AsyncCallback callback,
         Object asyncState
)
		{
			if ( sessionType == NetworkSessionType.Local )
				throw new ArgumentException( "NetworkSessionType cannot be NetworkSessionType.Local" );
			if ( maxLocalGamers < 1 ||   maxLocalGamers > 4 )
				throw new ArgumentOutOfRangeException( "maxLocalGamers must be between 1 and 4." );
			
			try
			{
				NetworkSessionAsynchronousFind AsynchronousFind = new NetworkSessionAsynchronousFind(Find);
            	return AsynchronousFind.BeginInvoke(sessionType, maxLocalGamers, searchProperties, callback, asyncState);
			}
			finally
			{
			}
		}
		
		public static IAsyncResult BeginJoin (
         AvailableNetworkSession availableSession,
         AsyncCallback callback,
         Object asyncState
)
		{
			if ( availableSession == null )
				throw new ArgumentNullException();			
			
			try
			{
				NetworkSessionAsynchronousJoin AsynchronousJoin  = new NetworkSessionAsynchronousJoin(Join);
            	return AsynchronousJoin.BeginInvoke(availableSession, callback, asyncState);
			}
			finally
			{
			}
		}
		
		public static IAsyncResult BeginJoinInvited (
         IEnumerable<SignedInGamer> localGamers,
         AsyncCallback callback,
         Object asyncState
		)
		{	
			try
			{
				throw new NotImplementedException();
			}
			finally
			{
			}
		}
		
		public static IAsyncResult BeginJoinInvited(
         int maxLocalGamers,
         AsyncCallback callback,
         Object asyncState
)
		{
			if ( maxLocalGamers < 1 ||   maxLocalGamers > 4 )
				throw new ArgumentOutOfRangeException( "maxLocalGamers must be between 1 and 4." );
			
			try
			{
				NetworkSessionAsynchronousJoinInvited AsynchronousJoinInvited  = new NetworkSessionAsynchronousJoinInvited(JoinInvited);
            	return AsynchronousJoinInvited.BeginInvoke(maxLocalGamers, callback, asyncState);
			}
			finally
			{
			}
		}
		
		public static NetworkSession EndCreate (IAsyncResult result)
		{
			NetworkSession returnValue = null;
			try
			{
				// Retrieve the delegate.
            	AsyncResult asyncResult = (AsyncResult)result;
								
				// Wait for the WaitHandle to become signaled.
	            result.AsyncWaitHandle.WaitOne();
	            
	            // Call EndInvoke to retrieve the results.
				if(asyncResult.AsyncDelegate is NetworkSessionAsynchronousCreate)
				{
            		returnValue = ((NetworkSessionAsynchronousCreate)asyncResult.AsyncDelegate).EndInvoke(result);
				}	            		                     
			}
			finally
			{
				// Close the wait handle.
	            result.AsyncWaitHandle.Close();	 
			}
			return returnValue;
		}
		
		public static AvailableNetworkSessionCollection EndFind(IAsyncResult result)
		{
			AvailableNetworkSessionCollection returnValue = null;
			try
			{
				// Retrieve the delegate.
            	AsyncResult asyncResult = (AsyncResult)result;            	
				
				// Wait for the WaitHandle to become signaled.
	            result.AsyncWaitHandle.WaitOne();
	            
	            // Call EndInvoke to retrieve the results.
				if(asyncResult.AsyncDelegate is NetworkSessionAsynchronousFind)
				{
            		returnValue = ((NetworkSessionAsynchronousFind)asyncResult.AsyncDelegate).EndInvoke(result);
				}		            	            
			}
			finally
			{
				// Close the wait handle.
	            result.AsyncWaitHandle.Close();
			}
			return returnValue;
		}
		
		public void EndGame ()
		{
			try
			{
				networkSessionState = NetworkSessionState.Lobby;
				if (gkSession != null)
				{
					gkSession.DisconnectFromAllPeers();
					gkSession.Available = false;
					gkSession.ReceiveData -= null;
					gkSession.Delegate = null;
					gkSession.Dispose();
					gkSession = null;
				}
			}
			finally
			{
			}
		}
				
		
		public static NetworkSession EndJoin (IAsyncResult result)
		{
			NetworkSession returnValue = null;
			try
			{
				// Retrieve the delegate.
            	AsyncResult asyncResult = (AsyncResult)result;            	
				
				// Wait for the WaitHandle to become signaled.
	            result.AsyncWaitHandle.WaitOne();
	            
	            // Call EndInvoke to retrieve the results.
				if(asyncResult.AsyncDelegate is NetworkSessionAsynchronousJoin)
				{
            		returnValue = ((NetworkSessionAsynchronousJoin)asyncResult.AsyncDelegate).EndInvoke(result);
				}		            	            
			}
			finally
			{
				// Close the wait handle.
	            result.AsyncWaitHandle.Close();
			}
			return returnValue;
		}
		
		public static NetworkSession EndJoinInvited(IAsyncResult result)
		{
			NetworkSession returnValue = null;
			try
			{
				// Retrieve the delegate.
            	AsyncResult asyncResult = (AsyncResult)result;            	
				
				// Wait for the WaitHandle to become signaled.
	            result.AsyncWaitHandle.WaitOne();
	            
	            // Call EndInvoke to retrieve the results.
				if(asyncResult.AsyncDelegate is NetworkSessionAsynchronousJoinInvited)
				{
            		returnValue = ((NetworkSessionAsynchronousJoinInvited)asyncResult.AsyncDelegate).EndInvoke(result);
				}		            	            
			}
			finally
			{
				// Close the wait handle.
	            result.AsyncWaitHandle.Close();
			}
			return returnValue;
		}
		
		public static AvailableNetworkSessionCollection Find (
         NetworkSessionType sessionType,
         IEnumerable<SignedInGamer> localGamers,
         NetworkSessionProperties searchProperties
)
		{
			if ( sessionType != NetworkSessionType.SystemLink )
				throw new ArgumentException( "NetworkSessionType must be NetworkSessionType.SystemLink" );
			
			try
			{
				throw new NotImplementedException();
			}
			finally
			{
			}
		}
		
		public static AvailableNetworkSessionCollection Find (
         NetworkSessionType sessionType,
         int maxLocalGamers,
         NetworkSessionProperties searchProperties
)
		{
			try
			{
				if ( maxLocalGamers < 1 ||   maxLocalGamers > 4 )
					throw new ArgumentOutOfRangeException( "maxLocalGamers must be between 1 and 4." );
			
				networkSessionType = sessionType;
				
				GKPeerPickerController peerPickerController = new GKPeerPickerController();
				peerPickerController.Delegate = new XnaTouchPeerPickerControllerDelegate(gkSession, ReceiveData);
				if ( sessionType == NetworkSessionType.SystemLink )
				{
				 	peerPickerController.ConnectionTypesMask = GKPeerPickerConnectionType.Nearby;
				}
				else if ( sessionType == NetworkSessionType.PlayerMatch )
				{
					peerPickerController.ConnectionTypesMask = GKPeerPickerConnectionType.Nearby | GKPeerPickerConnectionType.Online;
				}				
				peerPickerController.Show();
				List<AvailableNetworkSession> availableNetworkSessions = new List<AvailableNetworkSession>();
				
				return new AvailableNetworkSessionCollection( availableNetworkSessions );
			}
			finally
			{
			}
		}
		
		public NetworkGamer FindGamerById (byte gamerId)
		{
			try
			{
				throw new NotImplementedException();
			}
			finally
			{
			}
		}
		
		public static NetworkSession Join (AvailableNetworkSession availableSession)
		{
			if ( availableSession == null )
				throw new ArgumentNullException();
			
			try
			{
				throw new NotImplementedException();
			}
			finally
			{
			}
		}
		
		public static NetworkSession JoinInvited (
         IEnumerable<SignedInGamer> localGamers
)
		{
			try
			{
				throw new NotImplementedException();
			}
			finally
			{
			}
		}
		
		public static NetworkSession JoinInvited (
         int maxLocalGamers
)
		{
			if ( maxLocalGamers < 1 ||   maxLocalGamers > 4 )
				throw new ArgumentOutOfRangeException( "maxLocalGamers must be between 1 and 4." );
			
			try
			{
				throw new NotImplementedException();
			}
			finally
			{
			}
		}
		
		public void ResetReady ()
		{
			try
			{
				throw new NotImplementedException();
			}
			finally
			{
			}
		}
		
		public void StartGame ()
		{
			try
			{
				networkSessionState = NetworkSessionState.Playing;
			}
			finally
			{
			}
		}
		
		public void Update ()
		{
			// Updates the state of the multiplayer session. 
			try
			{
				throw new NotImplementedException();
			}
			finally
			{
			}
		}
		
		#region Properties
		public GamerCollection<NetworkGamer> AllGamers 
		{ 
			get
			{
				throw new NotImplementedException();
			}
		}
		
		bool _AllowHostMigration = false;
		public bool AllowHostMigration 
		{ 
			get
			{
				return _AllowHostMigration;
			}
			set
			{
				if (_AllowHostMigration != value)
				{
					_AllowHostMigration = value;
				}
			}
		}
		
		bool _AllowJoinInProgress = false;
		public bool AllowJoinInProgress 
		{ 
			get
			{
				return _AllowJoinInProgress;
			}
			set
			{
				if (_AllowJoinInProgress != value)
				{
					_AllowJoinInProgress = value;
				}
			}
		}
		
		public int BytesPerSecondReceived 
		{ 
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public int BytesPerSecondSent 
		{ 
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public NetworkGamer Host 
		{ 
			get
			{
				throw new NotImplementedException();
			}
		}
		
		bool _isDisposed = false;
		public bool IsDisposed 
		{ 
			get
			{
				return _isDisposed; // TODO (this.kernelHandle == 0);
			}
		}
		
		public bool IsEveryoneReady
		{ 
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public bool IsHost
		{ 
			get
			{
				throw new NotImplementedException();
			}
		}
		
		private GamerCollection<LocalNetworkGamer> _LocalGamers;
		public GamerCollection<LocalNetworkGamer> LocalGamers
		{ 
			get
			{
				return _LocalGamers;
			}
		}	
		
		public int MaxGamers
		{ 
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}		
			
		public GamerCollection<NetworkGamer> PreviousGamers
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public int PrivateGamerSlots
		{ 
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}	
		
		public GamerCollection<NetworkGamer> RemoteGamers
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public NetworkSessionProperties SessionProperties
		{
			get
			{
				throw new NotImplementedException();
			}
		}	
		
		public NetworkSessionState SessionState
		{
			get
			{
				return networkSessionState;
			}
		}
		
		public NetworkSessionType SessionType
		{
			get
			{
				return networkSessionType;
			}
		}
		
		public TimeSpan SimulatedLatency
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
		
		public float SimulatedPacketLoss
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}			
		#endregion
		
		#region Events
		public event EventHandler<GameEndedEventArgs> GameEnded;
		public event EventHandler<GamerJoinedEventArgs> GamerJoined;
		public event EventHandler<GamerLeftEventArgs> GamerLeft;
		public event EventHandler<GameStartedEventArgs> GameStarted;
		public event EventHandler<HostChangedEventArgs> HostChanged;
		public static event EventHandler<InviteAcceptedEventArgs> InviteAccepted;
		public event EventHandler<NetworkSessionEndedEventArgs> SessionEnded;
		#endregion
		
		private static void ReceiveData(object o, GKDataReceivedEventArgs args )
		{
			Console.WriteLine( "ReceiveData" );
			// Transform GKSession Data into NetworkSesson Data
		}
	}
	
	public class GameEndedEventArgs : EventArgs
	{
	}
	
	public class GamerJoinedEventArgs : EventArgs
	{
		private NetworkGamer gamer;
		
		public GamerJoinedEventArgs (NetworkGamer aGamer)
		{
			gamer = aGamer;
		}
		
		public NetworkGamer Gamer 
		{ 
			get
			{
				return gamer;
			}
		}
	}
	
	public class GamerLeftEventArgs : EventArgs
	{
		private NetworkGamer gamer;
		
		public GamerLeftEventArgs (NetworkGamer aGamer)
		{
			gamer = aGamer;
		}
		
		public NetworkGamer Gamer 
		{ 
			get
			{
				return gamer;
			}
		}
	}
	
	public class GameStartedEventArgs : EventArgs
	{
		
	}
	
	
	public class HostChangedEventArgs : EventArgs
	{
		private NetworkGamer newHost;
		private NetworkGamer oldHost;
		
		public HostChangedEventArgs( NetworkGamer aNewHost, NetworkGamer aOldHost )
		{
			newHost = aNewHost;
			oldHost = aOldHost;
		}
		
		public NetworkGamer NewHost 
		{ 
			get
			{
				return newHost;
			}
		}
		public NetworkGamer OldHost 
		{ 
			get
			{
				return oldHost;
			}
		}
	}
	
	public class InviteAcceptedEventArgs : EventArgs
	{
		private SignedInGamer gamer;
		
		public InviteAcceptedEventArgs (SignedInGamer aGamer)
		{
			gamer = aGamer;
		}
		
		public SignedInGamer Gamer 
		{ 
			get
			{
				return gamer;
			}
		}
		
		public bool IsCurrentSession 
		{ 
			get
			{
				return false;
			}
		}
	}
	
	public class NetworkSessionEndedEventArgs : EventArgs
	{
		NetworkSessionEndReason endReason;
			
		public NetworkSessionEndedEventArgs (NetworkSessionEndReason aEndReason)
		{
			endReason = aEndReason;
		}
		
		public NetworkSessionEndReason EndReason 
		{ 
			get
			{
				return endReason;
			}
		}
		
	}
}
