using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.ComponentModel;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    public class MonoGameNetworkConfiguration
    {
        public static IPAddress Broadcast = IPAddress.None;
    }

	internal class MonoGamerPeer
	{
		private BackgroundWorker MGServerWorker = new BackgroundWorker ();
		bool done = false;
		NetServer peer;
		NetworkSession session;
		AvailableNetworkSession availableSession;
		string myLocalAddress = string.Empty;
		IPEndPoint myLocalEndPoint = null;
		Dictionary<long, NetConnection> pendingGamers = new Dictionary<long, NetConnection> ();
		//Dictionary<long, NetConnection> connectedGamers = new Dictionary<long, NetConnection>();
		bool online = false;
		private static int port = 3074;
		private static IPEndPoint m_masterServer;
		private static int masterserverport = 6000;
		private static string masterServer = "monolive.servegame.com";
		internal static string applicationIdentifier = "monogame";
		
		static MonoGamerPeer()
		{
#if !WINDOWS_PHONE
			// This code looks up the Guid for the host app , this is used to identify the
			// application on the network . We use the Guid as that is unique to that application.			
			var assembly = System.Reflection.Assembly.GetAssembly(Game.Instance.GetType());
			if (assembly != null)
			{
				object[] objects = assembly.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
	  			if (objects.Length > 0)
	 			{
	   				applicationIdentifier = ((System.Runtime.InteropServices.GuidAttribute)objects[0]).Value;
	 			} 			
			}
#else
            
#endif
		}

		public MonoGamerPeer (NetworkSession session,AvailableNetworkSession availableSession)
			{            
			this.session = session;
			this.online = this.session.SessionType == NetworkSessionType.PlayerMatch;
			this.availableSession = availableSession;            
			//MGServerWorker.WorkerReportsProgress = true;
			MGServerWorker.WorkerSupportsCancellation = true;
			MGServerWorker.DoWork += new DoWorkEventHandler (MGServer_DoWork);
			MGServerWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler (MGServer_RunWorkerCompleted);
			MGServerWorker.RunWorkerAsync ();

			HookEvents ();
		}

		private void HookEvents ()
		{
			session.GameEnded += HandleSessionStateChanged;

			session.SessionEnded += HandleSessionStateChanged;

			session.GameStarted += HandleSessionStateChanged;		

		}

		void HandleSessionStateChanged (object sender, EventArgs e)
		{
#if !WINDOWS_PHONE
            Game.Instance.Log("session state change");
#endif
			SendSessionStateChange ();

			if (session.SessionState == NetworkSessionState.Ended)
				MGServerWorker.CancelAsync ();			
		}

		internal void ShutDown ()
		{
			MGServerWorker.CancelAsync ();			
		}

		private void MGServer_DoWork (object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = sender as BackgroundWorker;

			NetPeerConfiguration config = new NetPeerConfiguration (applicationIdentifier);
			config.EnableMessageType (NetIncomingMessageType.DiscoveryRequest);
			config.EnableMessageType (NetIncomingMessageType.DiscoveryResponse);
			config.EnableMessageType (NetIncomingMessageType.NatIntroductionSuccess);

			if (availableSession == null)
				config.Port = port;

			// create and start server
			peer = new NetServer (config);
			peer.Start ();

			myLocalAddress = GetMyLocalIpAddress ();

			IPAddress adr = IPAddress.Parse (myLocalAddress);
			myLocalEndPoint = new IPEndPoint (adr, port);

            // force a little wait until we have a LocalGamer otherwise things
            // break. This is the first item in the queue so it shouldnt take long before we
            // can continue.
            while (session.LocalGamers.Count <= 0)
            {
                Thread.Sleep(10);
            }

			if (availableSession != null) {
				if (!this.online) {
					peer.Connect (availableSession.EndPoint);
				} else {
					RequestNATIntroduction (availableSession.EndPoint, peer);                    
				}
			} else {
				if (this.online) {
					IPAddress ipaddr = NetUtility.Resolve (masterServer);
					if (ipaddr != null) {
						m_masterServer = new IPEndPoint (ipaddr, masterserverport);
						LocalNetworkGamer localMe = session.LocalGamers [0];

						NetOutgoingMessage om = peer.CreateMessage ();

						om.Write ((byte)0);
						om.Write (session.AllGamers.Count);
						om.Write (localMe.Gamertag);
						om.Write (session.PrivateGamerSlots);
						om.Write (session.MaxGamers);
						om.Write (localMe.IsHost);
						om.Write (myLocalEndPoint);
						om.Write (peer.Configuration.AppIdentifier);
						// send up session properties
						int[] propertyData = new int[session.SessionProperties.Count * 2];
						NetworkSessionProperties.WriteProperties (session.SessionProperties, propertyData);
						for (int x = 0; x < propertyData.Length; x++) {
							om.Write (propertyData [x]);
						}
						peer.SendUnconnectedMessage (om, m_masterServer); // send message to peer
					} else {
						throw new Exception ("Could not resolve live host");
					}
				}
			}

			// run until we are done
			do {

				NetIncomingMessage msg;
				while ((msg = peer.ReadMessage ()) != null) {

					switch (msg.MessageType) {
					case NetIncomingMessageType.UnconnectedData :
						break;
					case NetIncomingMessageType.NatIntroductionSuccess:
#if !WINDOWS_PHONE
                        Game.Instance.Log("NAT punch through OK " + msg.SenderEndpoint);                            
#endif
						peer.Connect (msg.SenderEndpoint);                            
						break;
					case NetIncomingMessageType.DiscoveryRequest:
						//
						// Server received a discovery request from a client; send a discovery response (with no extra data attached)
						//
						// Get the primary local gamer
						LocalNetworkGamer localMe = session.LocalGamers [0];

						NetOutgoingMessage om = peer.CreateMessage ();

						om.Write (session.RemoteGamers.Count);
						om.Write (localMe.Gamertag);
						om.Write (session.PrivateGamerSlots);
						om.Write (session.MaxGamers);
						om.Write (localMe.IsHost);
						int[] propertyData = new int[session.SessionProperties.Count * 2];
						NetworkSessionProperties.WriteProperties (session.SessionProperties, propertyData);
						for (int x = 0; x < propertyData.Length; x++) {
							om.Write (propertyData [x]);
						}

						peer.SendDiscoveryResponse (om, msg.SenderEndpoint);
						break;
					case NetIncomingMessageType.VerboseDebugMessage:
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.ErrorMessage:
						//
						// Just print diagnostic messages to console
						//
#if !WINDOWS_PHONE
                        Game.Instance.Log(msg.ReadString());
#endif
						break;
					case NetIncomingMessageType.StatusChanged:
						NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte ();
						if (status == NetConnectionStatus.Disconnected) {
#if !WINDOWS_PHONE
                            Game.Instance.Log(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " disconnected! from " + msg.SenderEndpoint);
#endif
							CommandGamerLeft cgj = new CommandGamerLeft (msg.SenderConnection.RemoteUniqueIdentifier);
							CommandEvent cmde = new CommandEvent (cgj);
							session.commandQueue.Enqueue (cmde);					
						}
						if (status == NetConnectionStatus.Connected) {
							//
							// A new player just connected!
							//
							if (!pendingGamers.ContainsKey (msg.SenderConnection.RemoteUniqueIdentifier)) {
#if !WINDOWS_PHONE
                                Game.Instance.Log(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " connected! from " + msg.SenderEndpoint);
#endif
								pendingGamers.Add (msg.SenderConnection.RemoteUniqueIdentifier, msg.SenderConnection);
								SendProfileRequest (msg.SenderConnection);
							} else {
#if !WINDOWS_PHONE
                                Game.Instance.Log("Already have a connection for that user, this is probably due to both NAT intro requests working");
#endif
							}
						}

						break;

					case NetIncomingMessageType.Data:

						NetworkMessageType mt = (NetworkMessageType)msg.ReadByte ();
						switch (mt) {
						case NetworkMessageType.Data:
							byte[] data = new byte[msg.LengthBytes - 1];
							msg.ReadBytes (data, 0, data.Length);
							CommandEvent cme = new CommandEvent (new CommandReceiveData (msg.SenderConnection.RemoteUniqueIdentifier,
												data));
							session.commandQueue.Enqueue (cme);						
							break;
						case NetworkMessageType.Introduction:

							var introductionAddress = msg.ReadString ();

							try {
								IPEndPoint endPoint = ParseIPEndPoint (introductionAddress);

								if (myLocalEndPoint.ToString () != endPoint.ToString () && !AlreadyConnected (endPoint)) {

#if !WINDOWS_PHONE
                                    Game.Instance.Log("Received Introduction for: " + introductionAddress + 
									" and I am: " + myLocalEndPoint + " from: " + msg.SenderEndpoint);
#endif
									peer.Connect (endPoint);
								}
							} catch (Exception exc) {
#if !WINDOWS_PHONE
                                Game.Instance.Log("Error parsing Introduction: " + introductionAddress + " : " + exc.Message);
#endif
							}

							break;
						case NetworkMessageType.GamerProfile:
#if !WINDOWS_PHONE
                            Game.Instance.Log("Profile recieved from: " + NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier));
#endif
							if (pendingGamers.ContainsKey (msg.SenderConnection.RemoteUniqueIdentifier)) {
								pendingGamers.Remove (msg.SenderConnection.RemoteUniqueIdentifier);
								msg.ReadInt32 ();
								string gamerTag = msg.ReadString ();
								msg.ReadInt32 ();
								msg.ReadInt32 ();
								GamerStates state = (GamerStates)msg.ReadInt32 ();
								state &= ~GamerStates.Local;
								CommandGamerJoined cgj = new CommandGamerJoined (msg.SenderConnection.RemoteUniqueIdentifier);
								cgj.GamerTag = gamerTag;
								cgj.State = state;
								CommandEvent cmde = new CommandEvent (cgj);
								session.commandQueue.Enqueue (cmde);					
							} else {
#if !WINDOWS_PHONE
                                Game.Instance.Log("We received a profile for an existing gamer.  Need to update it.");
#endif
							}
							break;
						case NetworkMessageType.RequestGamerProfile:
#if !WINDOWS_PHONE
                            Game.Instance.Log("Profile Request recieved from: " + msg.SenderEndpoint);
#endif
							SendProfile (msg.SenderConnection);
							break;	
						case NetworkMessageType.GamerStateChange:
							GamerStates gamerstate = (GamerStates)msg.ReadInt32 ();
							gamerstate &= ~GamerStates.Local;
#if !WINDOWS_PHONE
                            Game.Instance.Log("State Change from: " + msg.SenderEndpoint + " new State: " + gamerstate);
#endif
							foreach (var gamer in session.RemoteGamers) {
								if (gamer.RemoteUniqueIdentifier == msg.SenderConnection.RemoteUniqueIdentifier)
									gamer.State = gamerstate;
							}
							break;								
						case NetworkMessageType.SessionStateChange:
							NetworkSessionState sessionState = (NetworkSessionState)msg.ReadInt32 ();

							foreach (var gamer in session.RemoteGamers) {
								if (gamer.RemoteUniqueIdentifier == msg.SenderConnection.RemoteUniqueIdentifier) {
#if !WINDOWS_PHONE
                                    Game.Instance.Log("Session State change from: " + NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + 
										" session is now: " + sessionState);
#endif
									if (gamer.IsHost && sessionState == NetworkSessionState.Playing) {
										session.StartGame ();
									}

								}
							}

							break;								
						}						
						break;
					}

				}

				// sleep to allow other processes to run smoothly
				// This may need to be changed depending on network throughput
				Thread.Sleep (1);

				if (worker.CancellationPending) {
#if !WINDOWS_PHONE
                    Game.Instance.Log("worker CancellationPending");
#endif
					e.Cancel = true;
					done = true;
				}
			} while (!done);
		}

		private bool AlreadyConnected (IPEndPoint endPoint)
		{
			foreach (NetConnection player in peer.Connections) {
				if (player.RemoteEndpoint == endPoint) {
					return true;
				}
			}

			return false;
		}

		private void MGServer_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
		{
			if ((e.Cancelled == true)) {
#if !WINDOWS_PHONE
                Game.Instance.Log("Canceled");
#endif
			} else if (!(e.Error == null)) {
#if !WINDOWS_PHONE
                Game.Instance.Log("Error: " + e.Error.Message);
#endif
			}
#if !WINDOWS_PHONE
            Game.Instance.Log("worker Completed");
#endif

			if (online && this.availableSession == null) {
				// inform the master server we have closed
				NetOutgoingMessage om = peer.CreateMessage ();

				om.Write ((byte)3);
				om.Write (this.session.Host.Gamertag);
				om.Write (peer.Configuration.AppIdentifier);
				peer.SendUnconnectedMessage (om, m_masterServer); // send message to peer
			}
			peer.Shutdown ("app exiting");
		}	

		internal void SendProfile (NetConnection player)
		{
			NetOutgoingMessage om = peer.CreateMessage ();
			om.Write ((byte)NetworkMessageType.GamerProfile);
			om.Write (session.AllGamers.Count);
			om.Write (session.LocalGamers [0].Gamertag);
			om.Write (session.PrivateGamerSlots);
			om.Write (session.MaxGamers);
			om.Write ((int)session.LocalGamers[0].State);
#if !WINDOWS_PHONE
            Game.Instance.Log("Sent profile to: " + NetUtility.ToHexString(player.RemoteUniqueIdentifier));
#endif
			peer.SendMessage (om, player, NetDeliveryMethod.ReliableOrdered);			
		}

		internal void SendProfileRequest (NetConnection player)
		{
			NetOutgoingMessage om = peer.CreateMessage ();
			om.Write ((byte)NetworkMessageType.RequestGamerProfile);
#if !WINDOWS_PHONE
            Game.Instance.Log("Sent profile request to: " + NetUtility.ToHexString(player.RemoteUniqueIdentifier));
#endif            
			peer.SendMessage (om, player, NetDeliveryMethod.ReliableOrdered);			
		}

		internal void SendPeerIntroductions (NetworkGamer gamer)
		{

			NetConnection playerConnection = null;

			foreach (NetConnection player in peer.Connections) {
				if (player.RemoteUniqueIdentifier == gamer.RemoteUniqueIdentifier) {
					playerConnection = player;
				}
			}

			if (playerConnection == null) {
				return;
			}

			foreach (NetConnection player in peer.Connections) {
#if !WINDOWS_PHONE
                Game.Instance.Log("Introduction sent to: " + player.RemoteEndpoint);
#endif
				NetOutgoingMessage om = peer.CreateMessage ();
				om.Write ((byte)NetworkMessageType.Introduction);
				om.Write (playerConnection.RemoteEndpoint.ToString ()); 

				peer.SendMessage (om, player, NetDeliveryMethod.ReliableOrdered);
			}
		}

		internal void SendGamerStateChange (NetworkGamer gamer)
		{
#if !WINDOWS_PHONE
            Game.Instance.Log("SendGamerStateChange " + gamer.RemoteUniqueIdentifier);
#endif
			NetOutgoingMessage om = peer.CreateMessage ();
			om.Write ((byte)NetworkMessageType.GamerStateChange);
			om.Write ((int)gamer.State);

			SendMessage (om, SendDataOptions.Reliable, gamer);
		}

		internal void SendSessionStateChange ()
		{
#if !WINDOWS_PHONE
            Game.Instance.Log("Send Session State Change");
#endif
			NetOutgoingMessage om = peer.CreateMessage ();
			om.Write ((byte)NetworkMessageType.SessionStateChange);
			om.Write ((int)session.SessionState);

			SendMessage (om, SendDataOptions.Reliable, null);
		}		

		public static IPEndPoint ParseIPEndPoint (string endPoint)
		{
			string[] ep = endPoint.Split (':');
			if (ep.Length != 2)
				throw new FormatException ("Invalid endpoint format");
			IPAddress ip;
			if (!IPAddress.TryParse (ep [0], out ip)) {
				throw new FormatException ("Invalid ip-adress");
			}
			int port;
			if (!int.TryParse (ep [1], out port)) {
				throw new FormatException ("Invalid port");
			}
			return new IPEndPoint (ip, port);
		}

		internal static string GetMyLocalIpAddress ()
		{
			string localIP = "?";
#if !WINDOWS_PHONE
			IPHostEntry host;
			
			host = Dns.GetHostEntry (Dns.GetHostName ());
			foreach (IPAddress ip in host.AddressList) {
				// We only want those of type InterNetwork
				if (ip.AddressFamily == AddressFamily.InterNetwork) {
					// We will return the first one in the list
					localIP = ip.ToString ();
					break;
				}
			}
#else            
            FindMyIP.MyIPAddress ip = new FindMyIP.MyIPAddress();
            var addr = ip.Find();
            localIP = addr.ToString();

 
#endif			
			return localIP;
		}


        /// <summary>
        /// Used to Simulate the delay between computers
        /// </summary>
        public TimeSpan SimulatedLatency
        {
            get
            {                
#if DEBUG
		if (peer != null)
                	return new TimeSpan(0,0,(int)peer.Configuration.SimulatedAverageLatency);
		else
			return new TimeSpan(0);
#else
                return new TimeSpan(0);
#endif
            }
            set
            {
#if DEBUG
		if (peer != null) {
                	peer.Configuration.SimulatedMinimumLatency = (float)value.TotalSeconds;
		}
#endif
            }
        }


        /// <summary>
        /// Used to simulate the number of packets you might expect to loose.
        /// </summary>
        public float SimulatedPacketLoss
        {
            get
            {
#if DEBUG
		if (peer != null)
                	return peer.Configuration.SimulatedLoss;
		else
			return 0.0f;
#else
                return 0.0f;
#endif
            }
            set
            {
#if DEBUG
		if (peer != null) {
                	peer.Configuration.SimulatedLoss = value;
		}
#endif
            }
        }		

		internal void DiscoverPeers ()
		{
			peer.DiscoverLocalPeers (port);			    
		}

		internal void SendData (
			byte[] data,
			SendDataOptions options)
		{
			this.SendMessage (NetworkMessageType.Data, data, options, null);
		}

		internal void SendData (
			byte[] data,
			SendDataOptions options,
			NetworkGamer gamer)
		{
			this.SendMessage (NetworkMessageType.Data, data, options, gamer);
		}

		private void SendMessage (NetworkMessageType messageType, byte[] data, SendDataOptions options, NetworkGamer gamer)
		{

			NetOutgoingMessage om = peer.CreateMessage ();

			om.Write ((byte)messageType);
			om.Write (data);

			SendMessage (om, options, gamer);

		}

		private void SendMessage (NetOutgoingMessage om, SendDataOptions options, NetworkGamer gamer)
		{
			//Console.WriteLine("Data to send: " + data.Length);

			//			foreach (NetConnection player in server.Connections) {
			//				// ... send information about every other player (actually including self)
			//				foreach (NetConnection otherPlayer in server.Connections) {

			//					if (gamer != null && gamer.RemoteUniqueIdentifier != otherPlayer.RemoteUniqueIdentifier) {
			//						continue;
			//					}

			NetDeliveryMethod ndm = NetDeliveryMethod.Unreliable;
			switch (options) {
			case SendDataOptions.Reliable:
				ndm = NetDeliveryMethod.ReliableSequenced;
				break;
			case SendDataOptions.ReliableInOrder:
				ndm = NetDeliveryMethod.ReliableOrdered;
				break;
			case SendDataOptions.InOrder:
				ndm = NetDeliveryMethod.UnreliableSequenced;
				break;
			case SendDataOptions.None:
				ndm = NetDeliveryMethod.Unknown;
				break;
			}
			// send message
			//server.SendToAll (om, player, ndm);
			peer.SendToAll (om, ndm);

			//				}
			//			}				
		}		

		static NetPeer netPeer;
		static List<NetIncomingMessage> discoveryMsgs;        

		internal static void Find (NetworkSessionType sessionType)
		{
			NetPeerConfiguration config = new NetPeerConfiguration (applicationIdentifier);			
			if (sessionType == NetworkSessionType.PlayerMatch) {
				config.EnableMessageType (NetIncomingMessageType.UnconnectedData);
				config.EnableMessageType (NetIncomingMessageType.NatIntroductionSuccess);
			} else {
				config.EnableMessageType (NetIncomingMessageType.DiscoveryRequest);
			}
            if (MonoGameNetworkConfiguration.Broadcast != IPAddress.None)
            {
                config.BroadcastAddress = MonoGameNetworkConfiguration.Broadcast;
            }
			netPeer = new NetPeer (config);
			netPeer.Start ();

			if (sessionType == NetworkSessionType.PlayerMatch) {
				GetServerList (netPeer);
			} else {
				netPeer.DiscoverLocalPeers (port);
			}

			DateTime now = DateTime.Now;

			discoveryMsgs = new List<NetIncomingMessage> ();

			do {
				NetIncomingMessage msg;
				while ((msg = netPeer.ReadMessage ()) != null) {
					switch (msg.MessageType) {
					case NetIncomingMessageType.DiscoveryResponse:
						discoveryMsgs.Add (msg);
						break;
					case NetIncomingMessageType.UnconnectedData:
						if (msg.SenderEndpoint.Equals (m_masterServer)) {
							discoveryMsgs.Add (msg);
							/*
				* // it's from the master server - must be a host
				IPEndPoint hostInternal = msg.ReadIPEndpoint();
				IPEndPoint hostExternal = msg.ReadIPEndpoint();

				m_hostList.Add(new IPEndPoint[] { hostInternal, hostExternal });                            
				*/
						}
						break;
					case NetIncomingMessageType.VerboseDebugMessage:
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.ErrorMessage:
						//
						// Just print diagnostic messages to console
						//
#if DEBUG
						Console.WriteLine ("Find: " + msg.ReadString ());
#endif
						break;
					}
				}
			} while ((DateTime.Now - now).Seconds <= 2);

			netPeer.Shutdown ("Find shutting down");
		}

		/// <summary>
		/// Contacts the Master Server on the net and gets a list of available host games
		/// </summary>
		/// <param name="netPeer"></param>
		private static void GetServerList (NetPeer netPeer)
		{
			m_masterServer = new IPEndPoint (NetUtility.Resolve (masterServer), masterserverport);

			NetOutgoingMessage listRequest = netPeer.CreateMessage ();
			listRequest.Write ((byte)1);
			listRequest.Write (netPeer.Configuration.AppIdentifier);
			netPeer.SendUnconnectedMessage (listRequest, m_masterServer);

		}

		public static void RequestNATIntroduction (IPEndPoint host, NetPeer peer)
		{
			if (host == null) {
				return;
			}

			if (m_masterServer == null)
				throw new Exception ("Must connect to master server first!");

			NetOutgoingMessage om = peer.CreateMessage ();
			om.Write ((byte)2); // NAT intro request

			// write internal ipendpoint
			IPAddress addr = IPAddress.Parse (GetMyLocalIpAddress ());
			om.Write (new IPEndPoint (addr, peer.Port));

			// write external address of host to request introduction to
			IPEndPoint hostEp = new IPEndPoint (host.Address, port);
			om.Write (hostEp);
			om.Write (peer.Configuration.AppIdentifier); // send the app id

			peer.SendUnconnectedMessage (om, m_masterServer);
		}

		internal static void FindResults (List<AvailableNetworkSession> networkSessions)
		{

			foreach (NetIncomingMessage im in discoveryMsgs) {

				AvailableNetworkSession available = new AvailableNetworkSession ();
				switch (im.MessageType) {
				case NetIncomingMessageType.DiscoveryResponse :                        
					int currentGameCount = im.ReadInt32 ();
					string gamerTag = im.ReadString ();
					int openPrivateGamerSlots = im.ReadInt32 ();
					int openPublicGamerSlots = im.ReadInt32 ();
					bool isHost = im.ReadBoolean ();

					NetworkSessionProperties properties = new NetworkSessionProperties ();
					int[] propertyData = new int[properties.Count * 2];
					for (int x = 0; x < propertyData.Length; x++) {
						propertyData [x] = im.ReadInt32 ();
					}

					NetworkSessionProperties.ReadProperties (properties, propertyData);
					available.SessionProperties = properties;

					available.SessionType = NetworkSessionType.SystemLink;
					available.CurrentGamerCount = currentGameCount;
					available.HostGamertag = gamerTag;
					available.OpenPrivateGamerSlots = openPrivateGamerSlots;
					available.OpenPublicGamerSlots = openPublicGamerSlots;
					available.EndPoint = im.SenderEndpoint;
					available.InternalEndpont = null;
					break;
				case NetIncomingMessageType.UnconnectedData :
					if (im.SenderEndpoint.Equals (m_masterServer)) {
						currentGameCount = im.ReadInt32 ();
						gamerTag = im.ReadString ();
						openPrivateGamerSlots = im.ReadInt32 ();
						openPublicGamerSlots = im.ReadInt32 ();
						isHost = im.ReadBoolean ();
						IPEndPoint hostInternal = im.ReadIPEndpoint ();
						IPEndPoint hostExternal = im.ReadIPEndpoint ();
						available.SessionType = NetworkSessionType.PlayerMatch;
						available.CurrentGamerCount = currentGameCount;
						available.HostGamertag = gamerTag;
						available.OpenPrivateGamerSlots = openPrivateGamerSlots;
						available.OpenPublicGamerSlots = openPublicGamerSlots;
						// its data from the master server so it includes the internal and external endponts
						available.EndPoint = hostExternal;
						available.InternalEndpont = hostInternal;
					}
					break;
				}


				networkSessions.Add (available);

			}
		}

		internal void UpdateLiveSession (NetworkSession networkSession)
		{
			if (peer != null && m_masterServer != null && networkSession.IsHost) {
				NetOutgoingMessage om = peer.CreateMessage ();

				om.Write ((byte)0);
				om.Write (session.AllGamers.Count);
				om.Write (session.LocalGamers [0].Gamertag);
				om.Write (session.PrivateGamerSlots);
				om.Write (session.MaxGamers);
				om.Write (session.LocalGamers [0].IsHost);
				IPAddress adr = IPAddress.Parse (GetMyLocalIpAddress ());
				om.Write (new IPEndPoint (adr, port));
				om.Write (peer.Configuration.AppIdentifier);
				peer.SendUnconnectedMessage (om, m_masterServer); // send message to peer
			}
		}

        internal bool IsReady { get { return this.peer != null; } }
	}


}


#if WINDOWS_PHONE
namespace FindMyIP
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    public class MyIPAddress
    {
        Action<IPAddress> FoundCallback;
        UdpAnySourceMulticastClient MulticastSocket;
        const int PortNumber = 50000;       // pick a number, any number
        string MulticastMessage = "FIND-MY-IP-PLEASE" + new Random().Next().ToString();
 
        public void Find(Action<IPAddress> callback)
        {
            FoundCallback = callback;
 
            MulticastSocket = new UdpAnySourceMulticastClient(IPAddress.Parse("239.255.255.250"), PortNumber);
            MulticastSocket.BeginJoinGroup((result) =>
            {
                try
                {
                    MulticastSocket.EndJoinGroup(result);
                    GroupJoined(result);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("EndjoinGroup exception {0}", ex.Message);
                    // This can happen eg when wifi is off
                    FoundCallback(null);
                }
            },
                null);
        }
 
        void callback_send(IAsyncResult result)
        {
        }
 
        byte[] MulticastData;
        bool keepsearching;
 
        void GroupJoined(IAsyncResult result)
        {
            MulticastData = Encoding.UTF8.GetBytes(MulticastMessage);
            keepsearching = true;
            MulticastSocket.BeginSendToGroup(MulticastData, 0, MulticastData.Length, callback_send, null);
 
            while (keepsearching)
            {
                try
                {
                    byte[] buffer = new byte[MulticastData.Length];
                    MulticastSocket.BeginReceiveFromGroup(buffer, 0, buffer.Length, DoneReceiveFromGroup, buffer);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Stopped Group read due to " + ex.Message);
                    keepsearching = false;
                }
            }
        }
 
        void DoneReceiveFromGroup(IAsyncResult result)
        {
            IPEndPoint where;
            int responselength = MulticastSocket.EndReceiveFromGroup(result, out where);
            byte[] buffer = result.AsyncState as byte[];
            if (responselength == MulticastData.Length && buffer.SequenceEqual(MulticastData))
            {
                Debug.WriteLine("FOUND myself at " + where.Address.ToString());
                keepsearching = false;
                FoundCallback(where.Address);
            }
        }

        static ManualResetEvent _clientDone = new ManualResetEvent(false);

        public IPAddress Find()
        {
            var ip = IPAddress.None;
            _clientDone.Reset();
            Find((a) =>
            {
                ip = a;
                _clientDone.Set();
            });
            
            _clientDone.WaitOne(1000);
            return ip;
        }
    }
}
#endif
