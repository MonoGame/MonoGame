using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using System.Threading;
using System.ComponentModel;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
	internal class MonoGamerPeer
	{
		private BackgroundWorker MGServerWorker = new BackgroundWorker ();
		bool done = false;
		NetServer server;
		NetworkSession session;
		AvailableNetworkSession availableSession;
		bool isToBeCancelled = false;
		
		public MonoGamerPeer (NetworkSession session, AvailableNetworkSession availableSession)
		{
			this.session = session;
			this.availableSession = availableSession;
			
			//MGServerWorker.WorkerReportsProgress = true;
			MGServerWorker.WorkerSupportsCancellation = true;
			MGServerWorker.DoWork += new DoWorkEventHandler (MGServer_DoWork);
			MGServerWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler (MGServer_RunWorkerCompleted);
			MGServerWorker.RunWorkerAsync();
			
			HookEvents();
		}
		
		private void HookEvents()
		{
//			session.GameEnded += delegate {
//				Console.WriteLine("game ended");
//				MGServerWorker.CancelAsync();
//				//server.Shutdown("byebye");
//			};
			
			session.SessionEnded += delegate {
				Console.WriteLine("session ended");
				MGServerWorker.CancelAsync();
				//server.Shutdown("byebye");
			};
			
		}

		internal void ShutDown() {
			MGServerWorker.CancelAsync();			
		}
		
		private void MGServer_DoWork (object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = sender as BackgroundWorker;

			NetPeerConfiguration config = new NetPeerConfiguration ("MonoGame");
			config.EnableMessageType (NetIncomingMessageType.DiscoveryRequest);
			config.EnableMessageType (NetIncomingMessageType.DiscoveryResponse);
			
			if (availableSession == null)
				config.Port = 3074;

			// create and start server
			server = new NetServer (config);
			server.Start ();
			
			if (availableSession != null) {
				//NetOutgoingMessage om = server.CreateMessage(address + ":" + server.Port);
				server.Connect(availableSession.EndPoint);
			}
			
			// run until we are done
			do {
				
				NetIncomingMessage msg;
				while ((msg = server.ReadMessage ()) != null) {
					
					switch (msg.MessageType) {
					case NetIncomingMessageType.DiscoveryRequest:
						//
						// Server received a discovery request from a client; send a discovery response (with no extra data attached)
						//
						// Get the primary local gamer
						LocalNetworkGamer localMe = session.LocalGamers[0];
						
						NetOutgoingMessage om = server.CreateMessage ();
						
						om.Write(session.AllGamers.Count);
						om.Write(localMe.Gamertag);
						om.Write(session.PrivateGamerSlots);
						om.Write(session.MaxGamers);
						om.Write(localMe.IsHost);

						server.SendDiscoveryResponse (om, msg.SenderEndpoint);
						break;
					case NetIncomingMessageType.VerboseDebugMessage:
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.ErrorMessage:
						//
						// Just print diagnostic messages to console
						//
						Console.WriteLine (msg.ReadString ());
						break;
					case NetIncomingMessageType.StatusChanged:
						NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte ();
						if (status == NetConnectionStatus.Connected) {
							//
							// A new player just connected!
							//
							Console.WriteLine (NetUtility.ToHexString (msg.SenderConnection.RemoteUniqueIdentifier) + " connected! from " + msg.SenderEndpoint);
							CommandGamerJoined cgj = new CommandGamerJoined(msg.SenderConnection.RemoteUniqueIdentifier);
							CommandEvent cmde = new CommandEvent(cgj);
							session.commandQueue.Enqueue(cmde);
						}

						break;
					case NetIncomingMessageType.Data:
						
						NetworkMessageType mt = (NetworkMessageType)msg.ReadByte();
						switch (mt) {
						case NetworkMessageType.Data:
							byte[] data = new byte[msg.LengthBytes - 1];
							msg.ReadBytes(data, 0, data.Length);
							CommandEvent cme = new CommandEvent(new CommandReceiveData(msg.SenderConnection.RemoteUniqueIdentifier,
												data));
							session.commandQueue.Enqueue(cme);						
							break;
						case NetworkMessageType.Introduction:
							
							var intrduciontAddress = msg.ReadString();
							//Console.WriteLine("Received Introduction for: " + intrduciontAddress);
							
							try {
								IPEndPoint endPoint = ParseIPEndPoint(intrduciontAddress);
								server.Connect(endPoint);
							}
							catch (Exception exc) {
								Console.WriteLine("Error parsing Introduction: " + intrduciontAddress + " : " + exc.Message);
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
					Console.WriteLine("worker CancellationPending");
					e.Cancel = true;
					done = true;
				}
			} while (!done);

		}

		private void MGServer_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
		{
			if ((e.Cancelled == true)) {
				Console.WriteLine("Canceled");

			} else if (!(e.Error == null)) {
				Console.WriteLine("Error: " + e.Error.Message);
			} 
			Console.WriteLine("worker Completed");
			
			server.Shutdown ("app exiting");
		}	

		internal void SendPeerIntroductions(NetworkGamer gamer) {
			
			NetConnection playerConnection = null;
			
			foreach (NetConnection player in server.Connections) {
				if (player.RemoteUniqueIdentifier == gamer.RemoteUniqueIdentifier) {
					playerConnection = player;
				}
			}
			
			if (playerConnection == null) {
				return;
			}
			
			foreach (NetConnection player in server.Connections) {
				//Console.WriteLine("Introduce sent to: " + player.RemoteEndpoint);
				NetOutgoingMessage om = server.CreateMessage ();
				om.Write((byte)NetworkMessageType.Introduction);
				om.Write(playerConnection.RemoteEndpoint.ToString()); 

				server.SendMessage(om, player, NetDeliveryMethod.ReliableOrdered);
			}
		}

		
		public static IPEndPoint ParseIPEndPoint(string endPoint)
		{
			string[] ep = endPoint.Split(':');
			if(ep.Length != 2) throw new FormatException("Invalid endpoint format");
			IPAddress ip;
			if(!IPAddress.TryParse(ep[0], out ip))
			{
				throw new FormatException("Invalid ip-adress");
			}
			int port;
			if(!int.TryParse(ep[1], out port))
			{
				throw new FormatException("Invalid port");
			}
			return new IPEndPoint(ip, port);
		}
		
		internal void DiscoverPeers() 
		{
			server.DiscoverLocalPeers(3074);			
		}
		
		internal void SendData (
			byte[] data,
			SendDataOptions options)
		{
			this.SendMessage(NetworkMessageType.Data, data, options, null);
		}
		
		internal void SendData (
			byte[] data,
			SendDataOptions options,
			NetworkGamer gamer)
		{
			this.SendMessage(NetworkMessageType.Data, data, options, gamer);
		}
		
		private void SendMessage (NetworkMessageType messageType, byte[] data, SendDataOptions options, NetworkGamer gamer) 
		{
			//Console.WriteLine("Data to send: " + data.Length);
			
//			foreach (NetConnection player in server.Connections) {
//				// ... send information about every other player (actually including self)
//				foreach (NetConnection otherPlayer in server.Connections) {
//					
//					if (gamer != null && gamer.RemoteUniqueIdentifier != otherPlayer.RemoteUniqueIdentifier) {
//						continue;
//					}

					NetOutgoingMessage om = server.CreateMessage ();

					om.Write((byte)messageType);
					om.Write (data);
					
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
					server.SendToAll (om, ndm);

//				}
//			}				
		}
		
		static NetPeer netPeer;
		static List<NetIncomingMessage> discoveryMsgs;
		
		internal static void Find() {
			
			NetPeerConfiguration config = new NetPeerConfiguration ("MonoGame");
			config.EnableMessageType (NetIncomingMessageType.DiscoveryRequest);
			netPeer = new NetPeer(config);
			
			netPeer.Start();
			
			netPeer.DiscoverLocalPeers(3074);
			
			DateTime now = DateTime.Now;
			
			discoveryMsgs = new List<NetIncomingMessage>();
			
			do {
				NetIncomingMessage msg;
				while((msg = netPeer.ReadMessage()) != null) {
					switch (msg.MessageType)
					{
					case NetIncomingMessageType.DiscoveryResponse:
						discoveryMsgs.Add(msg);
						break;
					case NetIncomingMessageType.VerboseDebugMessage:
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.ErrorMessage:
						//
						// Just print diagnostic messages to console
						//
						Console.WriteLine ("Find: " + msg.ReadString ());
						break;
					}
				}
			} while ((DateTime.Now - now).Seconds <= 2);
			

			netPeer.Shutdown("Find shutting down");
		}	
	}
}

