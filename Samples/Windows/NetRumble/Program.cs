/*
 * Created by SharpDevelop.
 * User: d_ellis
 * Date: 02/08/2011
 * Time: 16:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace NetRumble
{
	class Program
	{
		public static void Main(string[] args)
		{
			using(NetRumbleGame game = new NetRumbleGame())
			{
				game.Run();
			}
		}
	}
}