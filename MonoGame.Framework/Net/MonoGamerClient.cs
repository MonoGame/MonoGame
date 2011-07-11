using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using System.Threading;
using System.ComponentModel;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
	internal class MonoGamerClient
	{
		private BackgroundWorker MGClientWorker = new BackgroundWorker ();
		bool done = false;
		NetClient client;
		NetworkSession session;
		AvailableNetworkSession availableSession;
		bool isToBeCancelled = false;
		
		public MonoGamerClient (NetworkSession session, AvailableNetworkSession availableSession)
		{
			this.session = session;
			this.availableSession = availableSession;
			
			//MGServerWorker.WorkerReportsProgress = true;
			
			MGClientWorker.WorkerSupportsCancellation = true;
			MGClientWorker.DoWork += new DoWorkEventHandler (MGServer_DoWork);
			MGClientWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler (MGServer_RunWorkerCompleted);
			MGClientWorker.RunWorkerAsync();
			
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
				MGClientWorker.CancelAsync();
				//server.Shutdown("byebye");
			};
			
		}

		internal void ShutDown() {
			MGClientWorker.CancelAsync();
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
		
		internal static void FindResults(List<AvailableNetworkSession> networkSessions) 
		{
			foreach (var resp in discoveryMsgs) {
				string gamerTag = resp.ReadString();
				bool isHost = resp.ReadBoolean();
				Console.WriteLine(resp.SenderEndpoint + " gamerTag: " + gamerTag + " isHost: " + isHost);
				AvailableNetworkSession ans = new AvailableNetworkSession();
				ans.HostGamertag = gamerTag;
				ans.EndPoint = resp.SenderEndpoint;
				networkSessions.Add(ans);
			}
		}
		
		private void MGServer_DoWork (object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = sender as BackgroundWorker;

			NetPeerConfiguration config = new NetPeerConfiguration ("MonoGame");
			config.EnableMessageType (NetIncomingMessageType.DiscoveryRequest);
			config.DisableMessageType (NetIncomingMessageType.StatusChanged);
			//config.Port = 3074;

			// create and start server
			client = new NetClient(config);
			
			client.Start ();
			
			Console.WriteLine("Connecting to: " + availableSession.EndPoint);
			NetConnection connection = client.Connect(availableSession.EndPoint);
			
			
			bool connectedToHost = false;
			
			do {
				Thread.Sleep(2);
				if (connection.RemoteUniqueIdentifier != 0) {
					//Console.WriteLine(client.ServerConnection.RemoteEndpoint);
					CommandGamerJoined cgj = new CommandGamerJoined(connection.RemoteUniqueIdentifier);
					//cgj.State |= GamerStates.Host;
					CommandEvent cmde = new CommandEvent(cgj);
					session.commandQueue.Enqueue(cmde);
					Console.WriteLine (NetUtility.ToHexString (connection.RemoteUniqueIdentifier) + " connected to host!");					
					connectedToHost = true;
				}
				
			} while (!connectedToHost);
			
			// run until we are done
			do {

				
				NetIncomingMessage msg;
				while ((msg = client.ReadMessage ()) != null) {
					switch (msg.MessageType) {
					case NetIncomingMessageType.DiscoveryRequest:
						//
						// Server received a discovery request from a client; send a discovery response (with no extra data attached)
						//
						client.DiscoverLocalPeers(3074);
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
//					case NetIncomingMessageType.StatusChanged:
//						NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte ();
//						if (status == NetConnectionStatus.Connected) {
//							//
//							// A new player just connected!
//							//
//							Console.WriteLine (NetUtility.ToHexString (msg.SenderConnection.RemoteUniqueIdentifier) + " connected!");
//							CommandGamerJoined cgj = new CommandGamerJoined(msg.SenderConnection.RemoteUniqueIdentifier);
//							CommandEvent cmde = new CommandEvent(cgj);
//							session.commandQueue.Enqueue(cmde);
//						}
//
//						break;
					case NetIncomingMessageType.Data:
						
							byte[] data = msg.ReadBytes(msg.LengthBytes);
							CommandEvent cme = new CommandEvent(new CommandReceiveData(msg.SenderConnection.RemoteUniqueIdentifier,
											data));
							session.commandQueue.Enqueue(cme);
							//Console.WriteLine("data enqueued " + data.Length);
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
		
		internal void DiscoverPeers() 
		{
			client.DiscoverLocalPeers(3074);			
		}
		
		private void MGServer_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
		{
			if ((e.Cancelled == true)) {
				Console.WriteLine("Canceled");

			} else if (!(e.Error == null)) {
				Console.WriteLine("Error: " + e.Error.Message);
			} 
			Console.WriteLine("worker Completed");
			
			client.Shutdown ("client exiting");
		}	
		
		internal void SendData (
			byte[] data,
			SendDataOptions options)
		{
			this.SendMessage(data, options, null);
		}
		
		internal void SendData (
			byte[] data,
			SendDataOptions options,
			NetworkGamer gamer)
		{
			this.SendMessage(data, options, gamer);
		}
		
		private void SendMessage (byte[] data, SendDataOptions options, NetworkGamer gamer) 
		{
			//Console.WriteLine("Data to send: " + data.Length);
			
			foreach (NetConnection player in client.Connections) {
				// ... send information about every other player (actually including self)
				foreach (NetConnection otherPlayer in client.Connections) {
					
					if (gamer != null && gamer.Id != otherPlayer.RemoteUniqueIdentifier) {
						continue;
					}
					// send position update about 'otherPlayer' to 'player'
					NetOutgoingMessage om = client.CreateMessage ();
					//Console.WriteLine("Data to send: " + data.Length);
					// write who this position is for (only for test)
					//om.Write (otherPlayer.RemoteUniqueIdentifier);
					//Console.WriteLine(otherPlayer.RemoteUniqueIdentifier);
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
					client.SendMessage (om, player, ndm);
				}
			}				
		}
	
	}
}

