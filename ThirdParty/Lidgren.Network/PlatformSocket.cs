using System;
using System.Net.Sockets;

namespace Lidgren.Network
{
    /// <summary>
    /// Platform Specific implementation of the Socket Class    
    /// </summary>
    public class PlatformSocket
    {
        private Socket socket;

        /// <summary>
        /// 
        /// </summary>
        public PlatformSocket()
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Broadcast 
        {             
            set
            {
                this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Available 
        { 
            get { return this.socket.Available; } 
        }

        /// <summary>
        /// 
        /// </summary>
        public int ReceiveBufferSize 
        {
            get { return this.socket.ReceiveBufferSize; }
            set { this.socket.ReceiveBufferSize = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int SendBufferSize 
        {
            get { return this.socket.SendBufferSize; }
            set { this.socket.SendBufferSize = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Blocking
        {
            get { return this.socket.Blocking; }
            set { this.socket.Blocking = value; }
        }

        internal void Bind(System.Net.EndPoint ep)
        {
            this.socket.Bind(ep);
        }

        /// <summary>
        /// 
        /// </summary>
        public System.Net.EndPoint LocalEndPoint
        {
            get { return this.socket.LocalEndPoint; }            
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsBound
        {
            get { return this.socket.IsBound; }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        public void Close(int timeout)
        {
            this.socket.Close(timeout);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool DontFragment
        {
            get { return this.socket.DontFragment; }
            set { this.socket.DontFragment = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="microseconds"></param>
        /// <returns></returns>
        public bool Poll(int microseconds)
        {
            return this.socket.Poll(microseconds, SelectMode.SelectRead);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiveBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="numBytes"></param>
        /// <param name="senderRemote"></param>
        /// <returns></returns>
        public int ReceiveFrom(byte[] receiveBuffer, int offset, int numBytes, ref System.Net.EndPoint senderRemote)
        {
            return this.socket.ReceiveFrom(receiveBuffer, offset, numBytes, SocketFlags.None, ref senderRemote);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="numBytes"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int SendTo(byte[] data, int offset, int numBytes, System.Net.EndPoint target)
        {
            return this.socket.SendTo(data, offset, numBytes, SocketFlags.None, target);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socketShutdown"></param>
        public void Shutdown(SocketShutdown socketShutdown)
        {
            this.socket.Shutdown(socketShutdown);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Setup()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="res"></param>
        public void EndSendTo(IAsyncResult res)
        {
            
        }
    }
}
