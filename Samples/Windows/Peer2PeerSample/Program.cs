using System;

namespace Peer2Peer
{
	class Program
	{
		public static void Main(string[] args)
		{
			using ( PeerToPeer.PeerToPeerGame game = new  PeerToPeer.PeerToPeerGame())
			{
				game.Run();
			}
		}
	}
}