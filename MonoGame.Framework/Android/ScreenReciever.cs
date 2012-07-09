using System;
using Android.Content;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Framework
{
	internal class ScreenReceiver : BroadcastReceiver
	{	
		public static bool ScreenLocked;
		
		public override void OnReceive(Context context, Intent intent)
		{
			Android.Util.Log.Info("MonoGameInfo", intent.Action.ToString());
			if(intent.Action == Intent.ActionScreenOff)
			{
				ScreenReceiver.ScreenLocked = true;     
				
				MediaPlayer.IsMuted = true;
			}
			else if(intent.Action == Intent.ActionScreenOn)
			{
				MediaPlayer.IsMuted = true;
			}
			else if(intent.Action == Intent.ActionUserPresent)
			{
				ScreenReceiver.ScreenLocked = false;
				
				MediaPlayer.IsMuted = false;
			}
		}
	}
}

