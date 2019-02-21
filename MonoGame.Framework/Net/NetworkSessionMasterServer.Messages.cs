using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Net
{
    internal enum MasterServerMessageType : byte
    {
        Undefined = 0,
        RequestGeneralInfo = 1,
        RegisterHost = 2,
        UnregisterHost = 3,
        RequestHosts = 4,
        RequestIntroduction = 5,
    };

    internal enum MasterServerMessageResult : byte
    {
        Undefined = 0,
        Ok = 1,
        InvalidPayload = 2,
    };

    public abstract partial class NetworkSessionMasterServer
    {
        internal static bool ParseResponseHeader(NetIncomingMessage response, out MasterServerMessageType type, out MasterServerMessageResult result)
        {
            type = MasterServerMessageType.Undefined;
            result = MasterServerMessageResult.Undefined;
            try
            {
                type = (MasterServerMessageType)response.ReadByte();
                result = (MasterServerMessageResult)response.ReadByte();
            }
            catch
            {
                return false;
            }
            if (!Enum.IsDefined(typeof(MasterServerMessageType), type) ||
                !Enum.IsDefined(typeof(MasterServerMessageResult), result) ||
                type == MasterServerMessageType.Undefined ||
                result == MasterServerMessageResult.Undefined)
            {
                return false;
            }
            return true;
        }

        internal static bool ParseExpectedResponseHeader(NetIncomingMessage response, MasterServerMessageType expectedType)
        {
            MasterServerMessageType type;
            MasterServerMessageResult result;
            if (ParseResponseHeader(response, out type, out result))
            {
                if (type == expectedType && result == MasterServerMessageResult.Ok)
                {
                    return true;
                }
                else
                {
                    Debug.WriteLine("Unexpected response header from master server, expected " + expectedType.ToString() +
                        " and got " + type.ToString() + " with result " + result.ToString());
                }
            }
            return false;
        }

        internal static void SendErrorResponse(NetPeer peer, IPEndPoint recipientEndPoint, MasterServerMessageType requestType, MasterServerMessageResult error)
        {
            var response = peer.CreateMessage();
            response.Write((byte)requestType);
            response.Write((byte)error);
            peer.SendUnconnectedMessage(response, recipientEndPoint);
        }

        internal static void SendRequestGeneralInfoResponse(NetPeer peer, IPEndPoint recipientEndPoint, string generalInfo)
        {
            var response = peer.CreateMessage();
            response.Write((byte)MasterServerMessageType.RequestGeneralInfo);
            response.Write((byte)MasterServerMessageResult.Ok);
            response.Write(generalInfo);
            peer.SendUnconnectedMessage(response, recipientEndPoint);
        }

        internal static void RegisterHost(NetPeer peer, Guid guid, IPEndPoint internalIp, NetworkSessionPublicInfo publicInfo)
        {
            var request = peer.CreateMessage();
            request.Write(NetworkSessionSettings.GameAppId);
            request.Write(NetworkSessionSettings.MasterServerPayload);
            request.Write((byte)MasterServerMessageType.RegisterHost);
            request.Write(guid.ToString());
            request.Write(internalIp);
            publicInfo.Pack(request);

            var serverEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);
            peer.SendUnconnectedMessage(request, serverEndPoint);

            Debug.WriteLine("Registering with master server (Guid: " + guid + ", InternalIp: " + internalIp + ", PublicInfo: ...)");
        }

        internal static bool ParseRegisterHost(NetIncomingMessage request, out Guid guid, out IPEndPoint internalIp, out IPEndPoint externalIp, out NetworkSessionPublicInfo publicInfo)
        {
            guid = Guid.Empty;
            internalIp = null;
            externalIp = null;
            publicInfo = null;
            try
            {
                guid = new Guid(request.ReadString());
                internalIp = request.ReadIPEndPoint();
                externalIp = request.SenderEndPoint;
                publicInfo = new NetworkSessionPublicInfo();
                if (!publicInfo.Unpack(request))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal static void UnregisterHost(NetPeer peer, Guid guid)
        {
            var request = peer.CreateMessage();
            request.Write(NetworkSessionSettings.GameAppId);
            request.Write(NetworkSessionSettings.MasterServerPayload);
            request.Write((byte)MasterServerMessageType.UnregisterHost);
            request.Write(guid.ToString());

            var serverEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);
            peer.SendUnconnectedMessage(request, serverEndPoint);

            Debug.WriteLine("Unregistering with master server (Guid: " + guid + ")");
        }

        internal static bool ParseUnregisterHost(NetIncomingMessage request, out Guid guid)
        {
            guid = Guid.Empty;
            try
            {
                guid = new Guid(request.ReadString());
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal static void RequestHosts(NetPeer peer)
        {
            var request = peer.CreateMessage();
            request.Write(NetworkSessionSettings.GameAppId);
            request.Write(NetworkSessionSettings.MasterServerPayload);
            request.Write((byte)MasterServerMessageType.RequestHosts);

            var serverEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);
            peer.SendUnconnectedMessage(request, serverEndPoint);
        }

        internal static void SendRequestHostsResponse(NetPeer peer, IPEndPoint recipientEndPoint, bool localDiscovery, Guid guid, NetworkSessionPublicInfo publicInfo)
        {
            var response = peer.CreateMessage();
            response.Write((byte)MasterServerMessageType.RequestHosts);
            response.Write((byte)MasterServerMessageResult.Ok);
            response.Write(guid.ToString());
            publicInfo.Pack(response);
            if (localDiscovery)
            {
                peer.SendDiscoveryResponse(response, recipientEndPoint);
            }
            else
            {
                peer.SendUnconnectedMessage(response, recipientEndPoint);
            }
        }

        internal static bool ParseRequestHostsResponse(NetIncomingMessage response, out Guid guid, out NetworkSessionPublicInfo publicInfo)
        {
            guid = Guid.Empty;
            publicInfo = null;
            try
            {
                guid = new Guid(response.ReadString());
                publicInfo = new NetworkSessionPublicInfo();
                if (!publicInfo.Unpack(response))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal static void RequestIntroduction(NetPeer peer, Guid guid, IPEndPoint internalIp)
        {
            var request = peer.CreateMessage();
            request.Write(NetworkSessionSettings.GameAppId);
            request.Write(NetworkSessionSettings.MasterServerPayload);
            request.Write((byte)MasterServerMessageType.RequestIntroduction);
            request.Write(guid.ToString());
            request.Write(internalIp);

            var serverEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);
            peer.SendUnconnectedMessage(request, serverEndPoint);
        }

        internal static bool ParseRequestIntroduction(NetIncomingMessage request, out Guid guid, out IPEndPoint clientInternalIp, out IPEndPoint clientExternalIp)
        {
            guid = Guid.Empty;
            clientInternalIp = null;
            clientExternalIp = null;
            try
            {
                guid = new Guid(request.ReadString());
                clientInternalIp = request.ReadIPEndPoint();
                clientExternalIp = request.SenderEndPoint;
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal static void RequestGeneralInfo(NetPeer peer)
        {
            var request = peer.CreateMessage();
            request.Write(NetworkSessionSettings.GameAppId);
            request.Write(NetworkSessionSettings.MasterServerPayload); // Note that payload does not need to match to get general info
            request.Write((byte)MasterServerMessageType.RequestGeneralInfo);

            var serverEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);
            peer.SendUnconnectedMessage(request, serverEndPoint);
        }

        internal static bool ParseRequestGeneralInfoResponse(NetIncomingMessage response, out string info)
        {
            info = null;
            try
            {
                info = response.ReadString();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
