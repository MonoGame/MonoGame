using System;
using System.Net;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class IntroducerToken
    {
        public IntroducerToken(GuidEndPoint hostEndPoint, IPEndPoint hostExternalIp, IPEndPoint clientExternalIp)
        {
            HostEndPoint = hostEndPoint ?? throw new ArgumentNullException(nameof(hostEndPoint));
            HostExternalIp = new IPEndPoint(hostExternalIp.Address, 0) ?? throw new ArgumentNullException(nameof(hostExternalIp));
            ClientExternalIp = new IPEndPoint(clientExternalIp.Address, 0) ?? throw new ArgumentNullException(nameof(clientExternalIp));
        }

        /// <summary>
        /// The host end point identification
        /// </summary>
        public GuidEndPoint HostEndPoint { get; private set; }

        /// <summary>
        /// The external ip of the host as observed by the introducer
        /// </summary>
        public IPEndPoint HostExternalIp { get; private set; }

        /// <summary>
        /// The external ip of the client as observed by the introducer
        /// </summary>
        public IPEndPoint ClientExternalIp { get; private set; }

        public string Serialize()
        {
            var hostEndPoint = HostEndPoint.ToString();
            var hostAddress = HostExternalIp.Address.ToString();
            var hostPort = HostExternalIp.Port.ToString();
            var clientAddress = ClientExternalIp.Address.ToString();
            var clientPort = ClientExternalIp.Port.ToString();
            return string.Join(";", new string[] {
                hostEndPoint, hostAddress, hostPort, clientAddress, clientPort
            });
        }

        public static bool Deserialize(string str, out IntroducerToken token)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            var parts = str.Split(';');
            if (parts.Length != 5)
            {
                token = null;
                return false;
            }
            try
            {
                var hostEndPoint = GuidEndPoint.Parse(parts[0]);
                var hostAddress = IPAddress.Parse(parts[1]);
                var hostPort = int.Parse(parts[2]);
                var clientAddress = IPAddress.Parse(parts[3]);
                var clientPort = int.Parse(parts[4]);

                token = new IntroducerToken(hostEndPoint,
                                                    new IPEndPoint(hostAddress, hostPort),
                                                    new IPEndPoint(clientAddress, clientPort));
            }
            catch
            {
                token = null;
            }
            return token != null;
        }
    }
}
