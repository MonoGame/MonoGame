#region License
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
#endregion License

#region Using clause
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading;

using Microsoft.Xna.Framework.GamerServices;

#endregion Using clause

namespace Microsoft.Xna.Framework.Net
{
	// The delegate must have the same signature as the method
	// it will call asynchronously.
	public delegate NetworkSession NetworkSessionAsynchronousCreate (
		NetworkSessionType sessionType,// Type of session being hosted.
		int maxLocalGamers,// Maximum number of local players on the same gaming machine in this network session.
		int maxGamers,		// Maximum number of players allowed in this network session.  For Zune-based games, this value must be between 2 and 8; 8 is the maximum number of players supported in the session.
		int privateGamerSlots, // Number of reserved private session slots created for the session. This value must be less than maximumGamers. 
		NetworkSessionProperties sessionProperties, // Properties of the session being created.
		int hostGamer,		// Gamer Index of the host
		bool isHost	// If the session is for host or not 
	);

	public delegate AvailableNetworkSessionCollection  NetworkSessionAsynchronousFind (
			NetworkSessionType sessionType,
			int hostGamer,
			int maxLocalGamers,
			NetworkSessionProperties searchProperties);

	public delegate NetworkSession NetworkSessionAsynchronousJoin (AvailableNetworkSession availableSession);

	public delegate NetworkSession NetworkSessionAsynchronousJoinInvited (int maxLocalGamers);

	public sealed class NetworkSession : IDisposable
	{
		internal static List<NetworkSession> activeSessions = new List<NetworkSession>();
		
		private NetworkSessionState sessionState;
		//private static NetworkSessionType networkSessionType;	
		private GamerCollection<NetworkGamer> _allGamers;
		private GamerCollection<LocalNetworkGamer> _localGamers;
		private GamerCollection<NetworkGamer> _remoteGamers;
		private GamerCollection<NetworkGamer> _previousGamers;
		
		internal Queue<CommandEvent> commandQueue;

		// use the static Create or BeginCreate methods
		private NetworkSession ()
		{
			activeSessions.Add(this);
		}
		
        ~NetworkSession()
        {
            Dispose(false);
        }

		private NetworkSessionType sessionType;
		private int maxGamers;
		private int privateGamerSlots;
		private NetworkSessionProperties sessionProperties;
		private bool isHost = false;
		private NetworkGamer hostingGamer;

		internal MonoGamerPeer networkPeer;
		
		private NetworkSession (NetworkSessionType sessionType, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, bool isHost, int hostGamer)
			: this(sessionType, maxGamers, privateGamerSlots, sessionProperties, isHost, hostGamer, null)
		{
		}
		
		private NetworkSession (NetworkSessionType sessionType, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, bool isHost, int hostGamer, AvailableNetworkSession availableSession) : this()
		{
			if (sessionProperties == null) {
				throw new ArgumentNullException ("sessionProperties");
			}
			
			_allGamers = new GamerCollection<NetworkGamer>();
			_localGamers = new GamerCollection<LocalNetworkGamer>();
//			for (int x = 0; x < Gamer.SignedInGamers.Count; x++) {
//				GamerStates states = GamerStates.Local;
//				if (x == 0)
//					states |= GamerStates.Host;
//				LocalNetworkGamer localGamer = new LocalNetworkGamer(this, (byte)x, states);
//				localGamer.SignedInGamer = Gamer.SignedInGamers[x];
//				_allGamers.AddGamer(localGamer);
//				_localGamers.AddGamer(localGamer);
//				
//				// We will attach a property change handler to local gamers
//				//  se that we can broadcast the change to other peers.
//				localGamer.PropertyChanged += HandleGamerPropertyChanged;	
//				
//			}

			_remoteGamers = new GamerCollection<NetworkGamer>();
			_previousGamers = new GamerCollection<NetworkGamer>();
			hostingGamer = null;
			
			commandQueue = new Queue<CommandEvent>();			
			
			this.sessionType = sessionType;
			this.maxGamers = maxGamers;
			this.privateGamerSlots = privateGamerSlots;
			this.sessionProperties = sessionProperties;
			this.isHost = isHost;
            if (isHost)
                networkPeer = new MonoGamerPeer(this, null);
            else
            {
                if (networkPeer == null)
                    networkPeer = new MonoGamerPeer(this, availableSession);
            }
            			
			CommandGamerJoined gj = new CommandGamerJoined(hostGamer, this.isHost, true);
			commandQueue.Enqueue(new CommandEvent(gj));
		}
		
		public static NetworkSession Create (
			NetworkSessionType sessionType,// Type of session being hosted.
			IEnumerable<SignedInGamer> localGamers, // Maximum number of local players on the same gaming machine in this network session.
			int maxGamers, // Maximum number of players allowed in this network session.  For Zune-based games, this value must be between 2 and 8; 8 is the maximum number of players supported in the session.
			int privateGamerSlots, // Number of reserved private session slots created for the session. This value must be less than maximumGamers. 
			NetworkSessionProperties sessionProperties // Properties of the session being created.
			)
		{
			try {
				return EndCreate(BeginCreate(sessionType, localGamers, maxGamers,privateGamerSlots, sessionProperties,null, null));
			} finally {
				
			}
			
		} 
		
		public static NetworkSession Create (
			NetworkSessionType sessionType,	// Type of session being hosted.
			int maxLocalGamers,		// Maximum number of local players on the same gaming machine in this network session.
			int maxGamers			// Maximum number of players allowed in this network session.  For Zune-based games, this value must be between 2 and 8; 8 is the maximum number of players supported in the session.
		)
		{
			try {
				return EndCreate(BeginCreate(sessionType,maxLocalGamers,maxGamers,null, null));
			} finally {
				
			}
			
		}

		public static NetworkSession Create (
			NetworkSessionType sessionType,
			int maxLocalGamers,
			int maxGamers,
			int privateGamerSlots,
			NetworkSessionProperties sessionProperties)
		{
			try {
				return EndCreate(BeginCreate(sessionType,maxLocalGamers,maxGamers,privateGamerSlots,sessionProperties,null, null));
			} finally {
				
			}
			
		}
		
		private static NetworkSession Create (
			NetworkSessionType sessionType,
			int maxLocalGamers,
			int maxGamers,
			int privateGamerSlots,
			NetworkSessionProperties sessionProperties,
			int hostGamer,
			bool isHost)
		{
			
			NetworkSession session = null;
			
			try {
				if (sessionProperties == null)
					sessionProperties = new NetworkSessionProperties();
				session = new NetworkSession (sessionType, maxGamers, privateGamerSlots, sessionProperties, isHost, hostGamer);
				
			} finally {
			}
			
			return session;
		}
		
		#region IDisposable Members

		public void Dispose ()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);				
		}
		
		public void Dispose (bool disposing) 
		{
            if (!_isDisposed)
            {
                if (disposing)
                {
                    foreach (Gamer gamer in _allGamers)
                    {
                        gamer.Dispose();
                    }

                    // Make sure we shut down our server instance as we no longer need it.
                    if (networkPeer != null)
                    {
                        networkPeer.ShutDown();
                    }
                    if (networkPeer != null)
                    {
                        networkPeer.ShutDown();
                    }
                }

                this._isDisposed = true;
            }
		}

	#endregion

		public void AddLocalGamer (SignedInGamer gamer)
		{
			if (gamer == null)
				throw new ArgumentNullException ("gamer");
			
//			_allGamers.AddGamer(gamer);
//			_localGamers.AddGamer((LocalNetworkGamer)gamer);
//			
//			// We will attach a property change handler to local gamers
//			//  se that we can broadcast the change to other peers.
//			gamer.PropertyChanged += HandleGamerPropertyChanged;	
		}

		public static IAsyncResult BeginCreate (NetworkSessionType sessionType,
			IEnumerable<SignedInGamer> localGamers,
			int maxGamers,
			int privateGamerSlots,
			NetworkSessionProperties sessionProperties,
			AsyncCallback callback,
			Object asyncState)
		{
			int hostGamer = -1;
			hostGamer = GetHostingGamerIndex (localGamers);            
			return BeginCreate (sessionType, hostGamer, 4, maxGamers, privateGamerSlots, sessionProperties, callback, asyncState);
		}

		public static IAsyncResult BeginCreate (
			NetworkSessionType sessionType,
			int maxLocalGamers,
			int maxGamers,
			AsyncCallback callback,
			Object asyncState)
		{
			return BeginCreate (sessionType, -1, maxLocalGamers, maxGamers, 0, null, callback, asyncState);
		}

		public static IAsyncResult BeginCreate (
			NetworkSessionType sessionType,
			int maxLocalGamers,
			int maxGamers,
			int privateGamerSlots,
			NetworkSessionProperties sessionProperties,
			AsyncCallback callback,
			Object asyncState)
		{
			return BeginCreate (sessionType, -1, maxLocalGamers, maxGamers, privateGamerSlots, sessionProperties, callback, asyncState);
		}

		private static IAsyncResult BeginCreate (NetworkSessionType sessionType,
			int hostGamer,
			int maxLocalGamers,
			int maxGamers,
			int privateGamerSlots,
			NetworkSessionProperties sessionProperties,
			AsyncCallback callback,
			Object asyncState)
		{
			if (maxLocalGamers < 1 || maxLocalGamers > 4) 
				throw new ArgumentOutOfRangeException ( "Maximum local players must be between 1 and 4." );
			if (maxGamers < 2 || maxGamers > 32) 
				throw new ArgumentOutOfRangeException ( "Maximum number of gamers must be between 2 and 32." );
			try {
				NetworkSessionAsynchronousCreate AsynchronousCreate = new NetworkSessionAsynchronousCreate (Create);
				return AsynchronousCreate.BeginInvoke (sessionType, maxLocalGamers, maxGamers, privateGamerSlots, sessionProperties, hostGamer, true, callback, asyncState);
			} finally {
			}		
			
		}

		internal static int GetHostingGamerIndex (IEnumerable<SignedInGamer> localGamers)
		{
			SignedInGamer hostGamer = null;

			if (localGamers == null) {
				throw new ArgumentNullException ("localGamers");
			}
			foreach (SignedInGamer gamer in localGamers) {
				if (gamer == null) {
					throw new ArgumentException ("gamer can not be null in list of localGamers.");
				}
				if (gamer.IsDisposed) {
					throw new ObjectDisposedException ("localGamers", "A gamer is disposed in the list of localGamers");
				}
				if (hostGamer == null) {
					hostGamer = gamer;
				}
			}
			if (hostGamer == null) {
				throw new ArgumentException ("Invalid gamer in localGamers.");
			}

			return (int)hostGamer.PlayerIndex;
		}		

		public static IAsyncResult BeginFind (
			NetworkSessionType sessionType,
			IEnumerable<SignedInGamer> localGamers,
			NetworkSessionProperties searchProperties,
			AsyncCallback callback,
			Object asyncState)
		{
			int hostGamer = -1;
			hostGamer = GetHostingGamerIndex (localGamers);

			return BeginFind (sessionType, hostGamer, 4, searchProperties, callback, asyncState);


		}

		public static IAsyncResult BeginFind (
			NetworkSessionType sessionType,
			int maxLocalGamers,
			NetworkSessionProperties searchProperties,
			AsyncCallback callback,
			Object asyncState)
		{
			return BeginFind (sessionType, -1, 4, searchProperties, callback, asyncState);
		}
		
		private static IAsyncResult BeginFind (
			NetworkSessionType sessionType,
			int hostGamer,
			int maxLocalGamers,
			NetworkSessionProperties searchProperties,
			AsyncCallback callback,
			Object asyncState)
		{
			if (sessionType == NetworkSessionType.Local)
				throw new ArgumentException ( "NetworkSessionType cannot be NetworkSessionType.Local" );
			if (maxLocalGamers < 1 || maxLocalGamers > 4)
				throw new ArgumentOutOfRangeException ( "maxLocalGamers must be between 1 and 4." );

			try {
				NetworkSessionAsynchronousFind AsynchronousFind = new NetworkSessionAsynchronousFind (Find);
				return AsynchronousFind.BeginInvoke (sessionType, hostGamer, maxLocalGamers, searchProperties, callback, asyncState);
			} finally {
			}
		}

		public static IAsyncResult BeginJoin (
			AvailableNetworkSession availableSession,
			AsyncCallback callback,
			Object asyncState)
		{
			if (availableSession == null)
				throw new ArgumentNullException ();			

			try {
				NetworkSessionAsynchronousJoin AsynchronousJoin = new NetworkSessionAsynchronousJoin (JoinSession);
				return AsynchronousJoin.BeginInvoke (availableSession, callback, asyncState);
			} finally {
			}
		}

        /*
		public static IAsyncResult BeginJoinInvited (
			IEnumerable<SignedInGamer> localGamers,
			AsyncCallback callback,
			Object asyncState)
		{	
			try {
				throw new NotImplementedException ();
			} finally {
			}
		}

		public static IAsyncResult BeginJoinInvited (
			int maxLocalGamers,
			AsyncCallback callback,
			Object asyncState)
		{
			if (maxLocalGamers < 1 || maxLocalGamers > 4)
				throw new ArgumentOutOfRangeException ( "maxLocalGamers must be between 1 and 4." );

			try {
				NetworkSessionAsynchronousJoinInvited AsynchronousJoinInvited = new NetworkSessionAsynchronousJoinInvited (JoinInvited);
				return AsynchronousJoinInvited.BeginInvoke (maxLocalGamers, callback, asyncState);
			} finally {
			}
		}
        */

        public static NetworkSession EndCreate (IAsyncResult result)
		{
			NetworkSession returnValue = null;
			try {
				// Retrieve the delegate.
				AsyncResult asyncResult = (AsyncResult)result;

				// Wait for the WaitHandle to become signaled.
				result.AsyncWaitHandle.WaitOne ();


				// Call EndInvoke to retrieve the results.
				if (asyncResult.AsyncDelegate is NetworkSessionAsynchronousCreate) {
					returnValue = ((NetworkSessionAsynchronousCreate)asyncResult.AsyncDelegate).EndInvoke (result);
				}	
			} finally {
				// Close the wait handle.
				result.AsyncWaitHandle.Close ();	 
			}
			
			return returnValue;
		}

		public static AvailableNetworkSessionCollection EndFind (IAsyncResult result)
		{
			AvailableNetworkSessionCollection returnValue = null;
			List<AvailableNetworkSession> networkSessions = new List<AvailableNetworkSession>();
			
			try {
				// Retrieve the delegate.
                AsyncResult asyncResult = (AsyncResult)result;            	

      
				// Wait for the WaitHandle to become signaled.
				result.AsyncWaitHandle.WaitOne ();
				               
				
				// Call EndInvoke to retrieve the results.
				if (asyncResult.AsyncDelegate is NetworkSessionAsynchronousFind) {
					returnValue = ((NetworkSessionAsynchronousFind)asyncResult.AsyncDelegate).EndInvoke (result);                    
				
					MonoGamerPeer.FindResults(networkSessions);
                }

            } finally {
				// Close the wait handle.
				result.AsyncWaitHandle.Close ();
			}
			returnValue = new AvailableNetworkSessionCollection(networkSessions);
			return returnValue;
		}

		public void EndGame ()
		{
			try {
				CommandSessionStateChange ssc = new CommandSessionStateChange(NetworkSessionState.Lobby, sessionState);
				commandQueue.Enqueue(new CommandEvent(ssc));

			} finally {
			}
		}

		public static NetworkSession EndJoin (IAsyncResult result)
		{
			NetworkSession returnValue = null;
			try {
				// Retrieve the delegate.
				AsyncResult asyncResult = (AsyncResult)result;            	

				// Wait for the WaitHandle to become signaled.
				result.AsyncWaitHandle.WaitOne ();

				// Call EndInvoke to retrieve the results.
				if (asyncResult.AsyncDelegate is NetworkSessionAsynchronousJoin) {
					returnValue = ((NetworkSessionAsynchronousJoin)asyncResult.AsyncDelegate).EndInvoke (result);
				}		            	            
			} finally {
				// Close the wait handle.
				result.AsyncWaitHandle.Close ();
			}
			return returnValue;
		}

        /*
		public static NetworkSession EndJoinInvited (IAsyncResult result)
		{
			NetworkSession returnValue = null;
			try {
				// Retrieve the delegate.
				AsyncResult asyncResult = (AsyncResult)result;            	

				// Wait for the WaitHandle to become signaled.
				result.AsyncWaitHandle.WaitOne ();

				// Call EndInvoke to retrieve the results.
				if (asyncResult.AsyncDelegate is NetworkSessionAsynchronousJoinInvited) {
					returnValue = ((NetworkSessionAsynchronousJoinInvited)asyncResult.AsyncDelegate).EndInvoke (result);
				}		            	            
			} finally {
				// Close the wait handle.
				result.AsyncWaitHandle.Close ();
			}
			return returnValue;
		}
        */

		public static AvailableNetworkSessionCollection Find (
			NetworkSessionType sessionType,
			IEnumerable<SignedInGamer> localGamers,
			NetworkSessionProperties searchProperties)
		{
			int hostGamer = -1;
			hostGamer = GetHostingGamerIndex(localGamers);
			return EndFind(BeginFind(sessionType, hostGamer, 4, searchProperties,null,null));
		}

		public static AvailableNetworkSessionCollection Find (
			NetworkSessionType sessionType,
			int maxLocalGamers,
			NetworkSessionProperties searchProperties)
		{
			return EndFind(BeginFind(sessionType, -1, maxLocalGamers, searchProperties,null,null));
		}

		private static AvailableNetworkSessionCollection Find (
			NetworkSessionType sessionType,
			int hostGamer,
			int maxLocalGamers,
			NetworkSessionProperties searchProperties)
		{
			try {
				if (maxLocalGamers < 1 || maxLocalGamers > 4)
					throw new ArgumentOutOfRangeException ( "maxLocalGamers must be between 1 and 4." );

				List<AvailableNetworkSession> availableNetworkSessions = new List<AvailableNetworkSession> ();
				MonoGamerPeer.Find(sessionType);
				return new AvailableNetworkSessionCollection ( availableNetworkSessions );
			} finally {
			}
		}
		
		public NetworkGamer FindGamerById (byte gamerId)
		{
			try {
				foreach (NetworkGamer gamer in _allGamers) {
					if (gamer.Id == gamerId)
						return gamer;
				}
				
				return null;
			} finally {
			}
		}

		public static NetworkSession Join (AvailableNetworkSession availableSession)
		{
			return EndJoin(BeginJoin(availableSession, null, null));
		}
		
		private static NetworkSession JoinSession (AvailableNetworkSession availableSession) 
		{
			NetworkSession session = null;
			
			try {                
				NetworkSessionType sessionType = availableSession.SessionType;
				int maxGamers = 32;
				int privateGamerSlots = 0;
				bool isHost = false;
				int hostGamer = -1;
				NetworkSessionProperties sessionProperties = availableSession.SessionProperties;
				if (sessionProperties == null)
					sessionProperties = new NetworkSessionProperties();
				session = new NetworkSession (sessionType, maxGamers, privateGamerSlots, sessionProperties, isHost, hostGamer, availableSession);
				
			} finally {
			}
			
			return session;		
		}
		
        /*
		public static NetworkSession JoinInvited (IEnumerable<SignedInGamer> localGamers)
		{
			try {
				throw new NotImplementedException ();
			} finally {
			}
		}
        
		public static NetworkSession JoinInvited (int maxLocalGamers)
		{
			if (maxLocalGamers < 1 || maxLocalGamers > 4)
				throw new ArgumentOutOfRangeException ( "maxLocalGamers must be between 1 and 4." );

			try {
				throw new NotImplementedException ();
			} finally {
			}
		}
		*/

		// I am not really sure how this is suppose to work so am just fleshing it in
		//  for the way I think it should.  This will also send a message to all connected
		//  peers for a state change.
		public void ResetReady ()
		{
			foreach (NetworkGamer gamer in _localGamers) {
				gamer.IsReady = false;
			}
		}

		public void StartGame ()
		{
			try {
				CommandSessionStateChange ssc = new CommandSessionStateChange(NetworkSessionState.Playing, sessionState);
				commandQueue.Enqueue(new CommandEvent(ssc));
				//sessionState = NetworkSessionState.Playing;
			} finally {
			}
		}

		public void Update ()
		{
			// Updates the state of the multiplayer session. 
			try {
				while (commandQueue.Count > 0 && networkPeer.IsReady) {
					var command = (CommandEvent)commandQueue.Dequeue();
					
					// for some screwed up reason we are dequeueing something
					// that is null so we will just continue.  I am not sure
					// if is jumbled data coming in from the connection or
					// something that is not being done correctly in code
					//  For sure this needs to be looked at although it is not
					//  causing any real problems right now.
					if (command == null) {
						continue;
					}
					
					switch (command.Command) {
					case CommandEventType.SendData:
						ProcessSendData((CommandSendData)command.CommandObject);
						break;						
					case CommandEventType.ReceiveData:
						ProcessReceiveData((CommandReceiveData)command.CommandObject);
						break;	
					case CommandEventType.GamerJoined:
						ProcessGamerJoined((CommandGamerJoined)command.CommandObject);
						break;
					case CommandEventType.GamerLeft:
						ProcessGamerLeft((CommandGamerLeft)command.CommandObject);
						break;
					case CommandEventType.SessionStateChange:
						ProcessSessionStateChange((CommandSessionStateChange)command.CommandObject);
						break;
					case CommandEventType.GamerStateChange:
						ProcessGamerStateChange((CommandGamerStateChange)command.CommandObject);
						break;							
					
					}					
				}
			} 
			catch (Exception exc) {
                if (exc != null)
                {
#if DEBUG				
				Console.WriteLine("Error in NetworkSession Update: " + exc.Message);
#endif
                }
			}
			finally {
			}
		}
		
		private void ProcessGamerStateChange(CommandGamerStateChange command) 
		{
			
			networkPeer.SendGamerStateChange(command.Gamer);	
		}
		
		private void ProcessSendData(CommandSendData command)
		{
			networkPeer.SendData(command.data, command.options);

			CommandReceiveData crd = new CommandReceiveData (command.sender.RemoteUniqueIdentifier,
								command.data);
			crd.gamer = command.sender;
			foreach(LocalNetworkGamer gamer in _localGamers) {
				gamer.receivedData.Enqueue(crd);
			}
		}
		
		private void ProcessReceiveData(CommandReceiveData command)
		{
			
			// first let's look up the gamer that sent the data
			foreach (NetworkGamer gamer in _allGamers) {
				if (gamer.RemoteUniqueIdentifier == command.remoteUniqueIdentifier)
					command.gamer = gamer;
			}
			
			// for some reason this is null sometimes
			//  this needs to be looked into instead of the
			//  check below
			if (command.gamer == null)
				return;
			
			// now we loop through each of our local gamers and add the command
			// to be processed.
			foreach (LocalNetworkGamer localGamer in LocalGamers) {
				lock (localGamer.receivedData) {
					localGamer.receivedData.Enqueue(command);
				}
			}
			
		}
		
		private void ProcessSessionStateChange(CommandSessionStateChange command)
		{
			if (sessionState == command.NewState)
				return;
			
			sessionState = command.NewState;
			
			switch (command.NewState) {
			case NetworkSessionState.Ended:
				
				ResetReady();

                // Have to find an example of how this is used so that I can figure out how to pass
                // the EndReason
                EventHelpers.Raise(this, SessionEnded, new NetworkSessionEndedEventArgs(NetworkSessionEndReason.HostEndedSession));
				break;
			case NetworkSessionState.Playing:
				
				EventHelpers.Raise(this, GameStarted, new GameStartedEventArgs());
				break;
			}
			
			// if changing from playing to lobby
			if (command.NewState == NetworkSessionState.Lobby && command.OldState == NetworkSessionState.Playing) {
				ResetReady();
				EventHelpers.Raise(this, GameEnded, new GameEndedEventArgs());
			}
		}
		
		private void ProcessGamerJoined(CommandGamerJoined command) 
		{
			NetworkGamer gamer;
			
			if ((command.State & GamerStates.Local) != 0) {
				gamer = new LocalNetworkGamer(this, (byte)command.InternalIndex, command.State);
				_allGamers.AddGamer(gamer);
				_localGamers.AddGamer((LocalNetworkGamer)gamer);

				// Note - This might be in the wrong place for certain connections
				//  Take a look at HoneycombRush tut for debugging later.
				if (Gamer.SignedInGamers.Count >= _localGamers.Count)
					((LocalNetworkGamer)gamer).SignedInGamer = Gamer.SignedInGamers[_localGamers.Count - 1];
				
				// We will attach a property change handler to local gamers
				//  se that we can broadcast the change to other peers.
				gamer.PropertyChanged += HandleGamerPropertyChanged;				
			}
			else {
				gamer = new NetworkGamer (this, (byte)command.InternalIndex, command.State);
				gamer.DisplayName = command.DisplayName;
				gamer.Gamertag = command.GamerTag;
				gamer.RemoteUniqueIdentifier = command.remoteUniqueIdentifier;
				_allGamers.AddGamer(gamer);
				_remoteGamers.AddGamer(gamer);
			}
			
			if ((command.State & GamerStates.Host) != 0)
				hostingGamer = gamer;
			
			gamer.Machine = new NetworkMachine();
			gamer.Machine.Gamers.AddGamer(gamer);
			//gamer.IsReady = true;
			
			EventHelpers.Raise(this, GamerJoined, new GamerJoinedEventArgs(gamer));
			
			if (networkPeer !=  null && (command.State & GamerStates.Local) == 0) {
				
				networkPeer.SendPeerIntroductions(gamer);
			}
			
			if (networkPeer != null)
			{
				networkPeer.UpdateLiveSession(this);
			}
			
			
		}
		
		private void ProcessGamerLeft(CommandGamerLeft command) 
		{
			NetworkGamer gamer;
			
			for (int x = 0; x < _remoteGamers.Count; x++) {
				if (_remoteGamers[x].RemoteUniqueIdentifier == command.remoteUniqueIdentifier) {
					gamer = _remoteGamers[x];
					_remoteGamers.RemoveGamer(gamer);
					_allGamers.RemoveGamer(gamer);
					EventHelpers.Raise(this, GamerLeft, new GamerLeftEventArgs(gamer));
				}
				
			}
			
			if (networkPeer != null)
			{
				networkPeer.UpdateLiveSession(this);
			}
		}		

		void HandleGamerPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			NetworkGamer gamer = sender as NetworkGamer;
			if (gamer == null)
				return;
			
			// If the gamer is local then we need to broadcast that change to all other
			// connected peers.  This is a double check here as we should only be handling 
			//  property changes for local gamers for now.
			if (gamer.IsLocal) {
				CommandGamerStateChange sc = new CommandGamerStateChange(gamer);
				CommandEvent cmd = new CommandEvent(sc);
				commandQueue.Enqueue(cmd);
			}
		}
		
		#region Properties
		public GamerCollection<NetworkGamer> AllGamers { 
			get {
				return _allGamers;
			}
		}

		bool _AllowHostMigration = false;

		public bool AllowHostMigration { 
			get {
				return _AllowHostMigration;
			}
			set {
				if (_AllowHostMigration != value) {
					_AllowHostMigration = value;
				}
			}
		}

		bool _AllowJoinInProgress = false;

		public bool AllowJoinInProgress { 
			get {
				return _AllowJoinInProgress;
			}
			set {
				if (_AllowJoinInProgress != value) {
					_AllowJoinInProgress = value;
				}
			}
		}

        /*
		public int BytesPerSecondReceived { 
			get {
				throw new NotImplementedException ();
			}
		}

		public int BytesPerSecondSent { 
			get {
				throw new NotImplementedException ();
			}
		}
        */

		public NetworkGamer Host { 
			get {
				return hostingGamer;
			}
		}

		bool _isDisposed = false;

		public bool IsDisposed { 
			get {
				return _isDisposed; // TODO (this.kernelHandle == 0);
			}
		}

		public bool IsEveryoneReady { 
			get {
				if (_allGamers.Count == 0)
					return false;
				foreach (NetworkGamer gamer in _allGamers) {
					if (!gamer.IsReady) {
						return false;
					}
				}
				return true;
			}
		}

		public bool IsHost { 
			get {
				return isHost;
			}
		}

		public GamerCollection<LocalNetworkGamer> LocalGamers { 
			get {
				return _localGamers;
			}
		}	

		public int MaxGamers { 
			get {
				return maxGamers;
			}
			set {
				maxGamers = value;
			}
		}		

		public GamerCollection<NetworkGamer> PreviousGamers {
			get {
				return _previousGamers;
			}
		}

		public int PrivateGamerSlots { 
			get {
				return privateGamerSlots;
			}
			set {
				privateGamerSlots = value;
			}
		}	

		public GamerCollection<NetworkGamer> RemoteGamers {
			get {
				return _remoteGamers;
			}
		}

		public NetworkSessionProperties SessionProperties {
			get {
				return sessionProperties;
			}
		}	

		public NetworkSessionState SessionState {
			get {
				return sessionState;
			}
		}

		public NetworkSessionType SessionType {
			get {
				return sessionType;
			}
		}

        private TimeSpan defaultSimulatedLatency = new TimeSpan(0, 0, 0);

		public TimeSpan SimulatedLatency {
			get {
#if DEBUG
                if (networkPeer != null)
                {
                    return networkPeer.SimulatedLatency;
                }
#endif
                return defaultSimulatedLatency;				
			}
			set {
                defaultSimulatedLatency = value;
#if DEBUG
                if (networkPeer != null)
                {
                    networkPeer.SimulatedLatency = value;
                }
#endif
                
			}
		}

        private float simulatedPacketLoss = 0.0f;

		public float SimulatedPacketLoss {
			get {
                if (networkPeer != null)
                {
                    simulatedPacketLoss = networkPeer.SimulatedPacketLoss;                   
                }
                return simulatedPacketLoss;
			}
			set {
                if (networkPeer != null) networkPeer.SimulatedPacketLoss = value;
                simulatedPacketLoss = value;
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

        private bool SuppressEventHandlerWarningsUntilEventsAreProperlyImplemented()
        {
            return
                HostChanged != null &&
                InviteAccepted != null;
        }

		#endregion

        internal static void Exit()
        {
            if (Net.NetworkSession.activeSessions != null && Net.NetworkSession.activeSessions.Count > 0)
            {
                foreach (Net.NetworkSession session in Net.NetworkSession.activeSessions)
                {
                    if (!session.IsDisposed)
                    {
                        session.Dispose();
                    }
                }
            }
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

		public NetworkGamer Gamer { 
			get {
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

		public NetworkGamer Gamer { 
			get {
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

		public HostChangedEventArgs (NetworkGamer aNewHost, NetworkGamer aOldHost)
		{
			newHost = aNewHost;
			oldHost = aOldHost;
		}

		public NetworkGamer NewHost { 
			get {
				return newHost;
			}
		}

		public NetworkGamer OldHost { 
			get {
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

		public SignedInGamer Gamer { 
			get {
				return gamer;
			}
		}

		public bool IsCurrentSession { 
			get {
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

		public NetworkSessionEndReason EndReason { 
			get {
				return endReason;
			}
		}

	}
}
