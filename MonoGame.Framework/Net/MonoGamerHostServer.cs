using System;
using System.IO;

using System.Threading;
using System.ComponentModel;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
	internal class MonoGamerHostServer
	{
		private BackgroundWorker MGServerWorker = new BackgroundWorker ();
		bool done = false;
		NetServer server;
		NetworkSession session;
		bool isToBeCancelled = false;
		
		public MonoGamerHostServer (NetworkSession session)
		{
			this.session = session;
			
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
			config.Port = 3074;

			// create and start server
			server = new NetServer (config);
			server.Start ();

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
						//om.Write((byte)localMe.Gamertag.Length);
						om.Write(localMe.Gamertag);
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
			
			foreach (NetConnection player in server.Connections) {
				// ... send information about every other player (actually including self)
				foreach (NetConnection otherPlayer in server.Connections) {
					
					if (gamer != null && gamer.Id != otherPlayer.RemoteUniqueIdentifier) {
						continue;
					}
					// send position update about 'otherPlayer' to 'player'
					NetOutgoingMessage om = server.CreateMessage ();
					//Console.WriteLine("Data to send: " + data.Length);
					// write who this position is for (only for tests)
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
					server.SendMessage (om, player, ndm);
				}
			}				
		}
	
	}
}

