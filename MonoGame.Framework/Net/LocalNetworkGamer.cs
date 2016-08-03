using System;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class LocalNetworkGamer : NetworkGamer
    {
        public LocalNetworkGamer(SignedInGamer signedInGamer, byte id) : base(true, id)
        {
            this.SignedInGamer = signedInGamer;
        }

        public bool IsDataAvailable { get; }
        public SignedInGamer SignedInGamer { get; }

        public void EnableSendVoice(NetworkGamer remoteGamer, bool enable)
        {
            throw new NotImplementedException();
        }

        // Receiving data
        public int ReceiveData(byte[] data, out NetworkGamer sender)
        {
            throw new NotImplementedException();
        }

        public int ReceiveData(byte[] data, int offset, out NetworkGamer sender)
        {
            throw new NotImplementedException();
        }

        public int ReceiveData(PacketReader data, out NetworkGamer sender)
        {
            throw new NotImplementedException();
        }

        // Sending data
        public void SendData(byte[] data, SendDataOptions options)
        {
            throw new NotImplementedException();
        }

        public void SendData(byte[] data, SendDataOptions options, NetworkGamer recipient)
        {
            throw new NotImplementedException();
        }

        public void SendData(byte[] data, int offset, int count, SendDataOptions options)
        {
            throw new NotImplementedException();
        }

        public void SendData(byte[] data, int offset, int count, SendDataOptions options, NetworkGamer recipient)
        {
            throw new NotImplementedException();
        }

        public void SendData(PacketWriter data, SendDataOptions options)
        {
            throw new NotImplementedException();
        }

        public void SendData(PacketWriter data, SendDataOptions options, NetworkGamer recipient)
        {
            throw new NotImplementedException();
        }

        public void SendPartyInvites()
        { }
    }
}