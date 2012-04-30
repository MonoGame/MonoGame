/* Copyright (c) 2010 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#if !ANDROID && !IOS && !PSS 
#define IS_FULL_NET_AVAILABLE
#endif

using System;
using System.Net;
#if IS_FULL_NET_AVAILABLE
using System.Net.NetworkInformation;
#endif
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Lidgren.Network
{
	/// <summary>
	/// Utility methods
	/// </summary>
	public static class NetUtility
	{
		/// <summary>
		/// Get IPv4 endpoint from notation (xxx.xxx.xxx.xxx) or hostname and port number
		/// </summary>
		public static IPEndPoint Resolve(string ipOrHost, int port)
		{
			IPAddress adr = Resolve(ipOrHost);
			return new IPEndPoint(adr, port);
		}

		/// <summary>
		/// Get IPv4 address from notation (xxx.xxx.xxx.xxx) or hostname
		/// </summary>
		public static IPAddress Resolve(string ipOrHost)
		{
			if (string.IsNullOrEmpty(ipOrHost))
				throw new ArgumentException("Supplied string must not be empty", "ipOrHost");

			ipOrHost = ipOrHost.Trim();

			IPAddress ipAddress = null;
			if (IPAddress.TryParse(ipOrHost, out ipAddress))
			{
				if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
					return ipAddress;
				throw new ArgumentException("This method will not currently resolve other than ipv4 addresses");
			}

			// ok must be a host name
			IPHostEntry entry;
			try
			{
				entry = Dns.GetHostEntry(ipOrHost);
				if (entry == null)
					return null;

				// check each entry for a valid IP address
				foreach (IPAddress ipCurrent in entry.AddressList)
				{
					if (ipCurrent.AddressFamily == AddressFamily.InterNetwork)
						return ipCurrent;
				}

				return null;
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode == SocketError.HostNotFound)
				{
					//LogWrite(string.Format(CultureInfo.InvariantCulture, "Failed to resolve host '{0}'.", ipOrHost));
					return null;
				}
				else
				{
					throw;
				}
			}
		}

#if IS_FULL_NET_AVAILABLE

		private static NetworkInterface GetNetworkInterface()
		{
			//IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
			//if (computerProperties == null)
			//	return null;

			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			if (nics == null || nics.Length < 1)
				return null;

			NetworkInterface best = null;
			foreach (NetworkInterface adapter in nics)
			{
				if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback || adapter.NetworkInterfaceType == NetworkInterfaceType.Unknown)
					continue;
				if (!adapter.Supports(NetworkInterfaceComponent.IPv4))
					continue;
				if (best == null)
					best = adapter;
				if (adapter.OperationalStatus != OperationalStatus.Up)
					continue;

				// A computer could have several adapters (more than one network card)
				// here but just return the first one for now...
				return adapter;
			}
			return best;
		}

		/// <summary>
		/// Returns the physical (MAC) address for the first usable network interface
		/// </summary>
		public static PhysicalAddress GetMacAddress()
		{
			NetworkInterface ni = GetNetworkInterface();
			if (ni == null)
				return null;
			return ni.GetPhysicalAddress();
		}
#endif

		/// <summary>
		/// Create a hex string from an Int64 value
		/// </summary>
		public static string ToHexString(long data)
		{
			return ToHexString(BitConverter.GetBytes(data));
		}

		/// <summary>
		/// Create a hex string from an array of bytes
		/// </summary>
		public static string ToHexString(byte[] data)
		{
			char[] c = new char[data.Length * 2];
			byte b;
			for (int i = 0; i < data.Length; ++i)
			{
				b = ((byte)(data[i] >> 4));
				c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
				b = ((byte)(data[i] & 0xF));
				c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
			}
			return new string(c);
		}
		
		public static IPAddress GetBroadcastAddress()
		{
#if ANDROID
			try{
			Android.Net.Wifi.WifiManager wifi = (Android.Net.Wifi.WifiManager)Android.App.Application.Context.GetSystemService(Android.App.Activity.WifiService);
			if (wifi.IsWifiEnabled)
			{
				var dhcp = wifi.DhcpInfo;
					
				int broadcast = (dhcp.IpAddress & dhcp.Netmask) | ~dhcp.Netmask;
	    		byte[] quads = new byte[4];
	    		for (int k = 0; k < 4; k++)
				{
	      			quads[k] = (byte) ((broadcast >> k * 8) & 0xFF);
				}
				return new IPAddress(quads);
			}
			}
			catch // Catch Access Denied Errors
			{
				return IPAddress.Broadcast;
			}
#endif		
#if IS_FULL_NET_AVAILABLE
			try
			{
				NetworkInterface ni = GetNetworkInterface();
				if (ni == null)
				{
					return null;
				}
	
				IPInterfaceProperties properties = ni.GetIPProperties();
				foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
				{
					if (unicastAddress != null && unicastAddress.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
					{
						var mask = unicastAddress.IPv4Mask;
						byte[] ipAdressBytes = unicastAddress.Address.GetAddressBytes();
				        byte[] subnetMaskBytes = mask.GetAddressBytes();
				
				        if (ipAdressBytes.Length != subnetMaskBytes.Length)
				            throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
				
				        byte[] broadcastAddress = new byte[ipAdressBytes.Length];
				        for (int i = 0; i < broadcastAddress.Length; i++)
				        {
				            broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
				        }
				        return new IPAddress(broadcastAddress);				
					}
				}
			}
			catch // Catch any errors 
			{
			    return IPAddress.Broadcast;
			}
#endif		
			return IPAddress.Broadcast;
		}

		/// <summary>
		/// Gets my local IP address (not necessarily external) and subnet mask
		/// </summary>
		public static IPAddress GetMyAddress(out IPAddress mask)
		{
			mask = null;
#if ANDROID
			try
			{
				Android.Net.Wifi.WifiManager wifi = (Android.Net.Wifi.WifiManager)Android.App.Application.Context.GetSystemService(Android.App.Activity.WifiService);
				if (!wifi.IsWifiEnabled) return null;
				var dhcp = wifi.DhcpInfo;
					
				int addr = dhcp.IpAddress;
	    		byte[] quads = new byte[4];
	    		for (int k = 0; k < 4; k++)
				{
	      			quads[k] = (byte) ((addr >> k * 8) & 0xFF);
				}			
				return new IPAddress(quads);
			}
			catch // Catch Access Denied errors
			{
				return null;
			}
				
#endif			
#if IS_FULL_NET_AVAILABLE
			NetworkInterface ni = GetNetworkInterface();
			if (ni == null)
			{
				mask = null;
				return null;
			}

			IPInterfaceProperties properties = ni.GetIPProperties();
			foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
			{
				if (unicastAddress != null && unicastAddress.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
				{
					mask = unicastAddress.IPv4Mask;
					return unicastAddress.Address;
				}
			}
#endif
			return null;
		}

		/// <summary>
		/// Returns true if the IPEndPoint supplied is on the same subnet as this host
		/// </summary>
		public static bool IsLocal(IPEndPoint endpoint)
		{
			if (endpoint == null)
				return false;
			return IsLocal(endpoint.Address);
		}

		/// <summary>
		/// Returns true if the IPAddress supplied is on the same subnet as this host
		/// </summary>
		public static bool IsLocal(IPAddress remote)
		{
			IPAddress mask;
			IPAddress local = GetMyAddress(out mask);

			if (mask == null)
				return false;

			uint maskBits = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
			uint remoteBits = BitConverter.ToUInt32(remote.GetAddressBytes(), 0);
			uint localBits = BitConverter.ToUInt32(local.GetAddressBytes(), 0);

			// compare network portions
			return ((remoteBits & maskBits) == (localBits & maskBits));
		}

		/// <summary>
		/// Returns how many bits are necessary to hold a certain number
		/// </summary>
		[CLSCompliant(false)]
		public static int BitsToHoldUInt(uint value)
		{
			int bits = 1;
			while ((value >>= 1) != 0)
				bits++;
			return bits;
		}

		/// <summary>
		/// Returns how many bytes are required to hold a certain number of bits
		/// </summary>
		public static int BytesToHoldBits(int numBits)
		{
			return (numBits + 7) / 8;
		}

		internal static UInt32 SwapByteOrder(UInt32 value)
		{
			return
				((value & 0xff000000) >> 24) |
				((value & 0x00ff0000) >> 8) |
				((value & 0x0000ff00) << 8) |
				((value & 0x000000ff) << 24);
		}

		internal static UInt64 SwapByteOrder(UInt64 value)
		{
			return
				((value & 0xff00000000000000L) >> 56) |
				((value & 0x00ff000000000000L) >> 40) |
				((value & 0x0000ff0000000000L) >> 24) |
				((value & 0x000000ff00000000L) >> 8) |
				((value & 0x00000000ff000000L) << 8) |
				((value & 0x0000000000ff0000L) << 24) |
				((value & 0x000000000000ff00L) << 40) |
				((value & 0x00000000000000ffL) << 56);
		}

		internal static bool CompareElements(byte[] one, byte[] two)
		{
			if (one.Length != two.Length)
				return false;
			for (int i = 0; i < one.Length; i++)
				if (one[i] != two[i])
					return false;
			return true;
		}

		/// <summary>
		/// Convert a hexadecimal string to a byte array
		/// </summary>
		public static byte[] ToByteArray(String hexString)
		{
			byte[] retval = new byte[hexString.Length / 2];
			for (int i = 0; i < hexString.Length; i += 2)
				retval[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
			return retval;
		}

		/// <summary>
		/// Converts a number of bytes to a shorter, more readable string representation
		/// </summary>
		public static string ToHumanReadable(long bytes)
		{
			if (bytes < 4000) // 1-4 kb is printed in bytes
				return bytes + " bytes";
			if (bytes < 1000 * 1000) // 4-999 kb is printed in kb
				return Math.Round(((double)bytes / 1000.0), 2) + " kilobytes";
			return Math.Round(((double)bytes / (1000.0 * 1000.0)), 2) + " megabytes"; // else megabytes
		}

		internal static int RelativeSequenceNumber(int nr, int expected)
		{
			int retval = ((nr + NetConstants.NumSequenceNumbers) - expected) % NetConstants.NumSequenceNumbers;
			if (retval > (NetConstants.NumSequenceNumbers / 2))
				retval -= NetConstants.NumSequenceNumbers;
			return retval;
		}

		/// <summary>
		/// Gets the window size used internally in the library for a certain delivery method
		/// </summary>
		public static int GetWindowSize(NetDeliveryMethod method)
		{
			switch (method)
			{
				case NetDeliveryMethod.Unknown:
					return 0;

				case NetDeliveryMethod.Unreliable:
				case NetDeliveryMethod.UnreliableSequenced:
					return NetConstants.UnreliableWindowSize;

				case NetDeliveryMethod.ReliableOrdered:
					return NetConstants.ReliableOrderedWindowSize;

				case NetDeliveryMethod.ReliableSequenced:
				case NetDeliveryMethod.ReliableUnordered:
				default:
					return NetConstants.DefaultWindowSize;
			}
		}

		// shell sort
		internal static void SortMembersList(System.Reflection.MemberInfo[] list)
		{
			int h;
			int j;
			System.Reflection.MemberInfo tmp;

			h = 1;
			while (h * 3 + 1 <= list.Length)
				h = 3 * h + 1;

			while (h > 0)
			{
				for (int i = h - 1; i < list.Length; i++)
				{
					tmp = list[i];
					j = i;
					while (true)
					{
						if (j >= h)
						{
							if (string.Compare(list[j - h].Name, tmp.Name, StringComparison.InvariantCulture) > 0)
							{
								list[j] = list[j - h];
								j -= h;
							}
							else
								break;
						}
						else
							break;
					}

					list[j] = tmp;
				}
				h /= 3;
			}
		}

		internal static NetDeliveryMethod GetDeliveryMethod(NetMessageType mtp)
		{
			if (mtp >= NetMessageType.UserReliableOrdered1)
				return NetDeliveryMethod.ReliableOrdered;
			else if (mtp >= NetMessageType.UserReliableSequenced1)
				return NetDeliveryMethod.ReliableSequenced;
			else if (mtp >= NetMessageType.UserReliableUnordered)
				return NetDeliveryMethod.ReliableUnordered;
			else if (mtp >= NetMessageType.UserSequenced1)
				return NetDeliveryMethod.UnreliableSequenced;
			return NetDeliveryMethod.Unreliable;
		}
	}
}