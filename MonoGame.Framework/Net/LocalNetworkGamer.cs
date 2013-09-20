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
using System.Collections.Generic;

using Microsoft.Xna.Framework.GamerServices;

#endregion Using clause

namespace Microsoft.Xna.Framework.Net
{
	public sealed class LocalNetworkGamer : NetworkGamer
	{

		private SignedInGamer sig;
		internal Queue<CommandReceiveData> receivedData;
		
		public LocalNetworkGamer () : base(null, 0, 0)
		{
			sig = new SignedInGamer ();
			receivedData = new Queue<CommandReceiveData>();
		}

		public LocalNetworkGamer (NetworkSession session,byte id,GamerStates state)
			: base(session, id, state | GamerStates.Local)
		{
			sig = new SignedInGamer ();
			receivedData = new Queue<CommandReceiveData>();
		}

        /*
		public void EnableSendVoice (
			NetworkGamer remoteGamer, 
			bool enable)
		{
			throw new NotImplementedException ();
		}
        */

		public int ReceiveData (
			byte[] data, 
			int offset,
			out NetworkGamer sender)
		{
			if (data == null)
				throw new ArgumentNullException("data");
			
			if (receivedData.Count <= 0) {
				sender = null;
				return 0;
			}
			
			lock (receivedData) {

				CommandReceiveData crd;
				
				// we will peek at the value first to see if we can process it
				crd = (CommandReceiveData)receivedData.Peek();
				
				if (offset + crd.data.Length > data.Length)
					throw new ArgumentOutOfRangeException("data","The length + offset is greater than parameter can hold.");
				
				// no exception thrown yet so let's process it
				// take it off the queue
				receivedData.Dequeue();
				
				Array.Copy(crd.data, offset, data, 0, data.Length);
				sender = crd.gamer;
				return data.Length;			
			}
			
		}

		public int ReceiveData (
			byte[] data,
			out NetworkGamer sender)
		{
			return ReceiveData(data, 0, out sender);
		}

		public int ReceiveData (
			PacketReader data,
			out NetworkGamer sender)
		{
			lock (receivedData) {
				if (receivedData.Count >= 0) {
					data.Reset(0);

					// take it off the queue
					CommandReceiveData crd = (CommandReceiveData)receivedData.Dequeue();
					
					// lets make sure that we can handle the data
					if (data.Length < crd.data.Length) {
						data.Reset(crd.data.Length);
					}

					Array.Copy(crd.data, data.Data, data.Length);
					sender = crd.gamer;
					return data.Length;	
					
				}
				else {
					sender = null;
					return 0;
				}
				
			}
		}

		public void SendData (
			byte[] data,
			int offset,
			int count,
			SendDataOptions options)
		{
			CommandEvent cme = new CommandEvent(new CommandSendData(data, offset, count, options, null, this ));
			Session.commandQueue.Enqueue(cme);
		}

		public void SendData (
			byte[] data,
			int offset,
			int count,
			SendDataOptions options,
			NetworkGamer recipient)
		{
			CommandEvent cme = new CommandEvent(new CommandSendData(data, offset, count, options, recipient,this ));
			Session.commandQueue.Enqueue(cme);
		}

		public void SendData (
			byte[] data,
			SendDataOptions options)
		{
			CommandEvent cme = new CommandEvent(new CommandSendData(data, 0, data.Length, options, null, this ));
			Session.commandQueue.Enqueue(cme);
		}

		public void SendData (
			byte[] data,
			SendDataOptions options,
			NetworkGamer recipient)
		{
			CommandEvent cme = new CommandEvent(new CommandSendData(data, 0, data.Length, options, recipient, this ));
			Session.commandQueue.Enqueue(cme);
		}

		public void SendData (
			PacketWriter data,
			SendDataOptions options)
		{
			SendData(data.Data, 0, data.Length, options, null);
			data.Reset();
		}

		public void SendData (
			PacketWriter data,
			SendDataOptions options,
			NetworkGamer recipient)
		{
			SendData(data.Data, 0, data.Length, options, recipient);
			data.Reset();
		}

		public bool IsDataAvailable { 
			get {
				lock (receivedData) {
					return receivedData.Count > 0;
				}
			}
		}

		public SignedInGamer SignedInGamer { 
			get {
				return sig;
			}
			
			internal set {
				sig = value;
				DisplayName = sig.DisplayName;
				Gamertag = sig.Gamertag;
			}
		}
	}
}
