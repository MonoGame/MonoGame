using System;
using Android.Content;
using Microsoft.Xna.Framework.Media;
using Android.App;

namespace Microsoft.Xna.Framework
{
	internal class ScreenReceiver : BroadcastReceiver
	{	
		public static bool ScreenLocked;
		
		public override void OnReceive(Context context, Intent intent)
		{
			Android.Util.Log.Info("MonoGame", intent.Action.ToString());
			if(intent.Action == Intent.ActionScreenOff)
			{
				ScreenReceiver.ScreenLocked = true;
				MediaPlayer.IsMuted = true;
			}
			else if(intent.Action == Intent.ActionScreenOn)
			{
                // If the user turns the screen on just after it has automatically turned off, 
                // the keyguard will not have had time to activate and the ActionUserPreset intent
                // will not be broadcast. We need to check if the lock is currently active
                // and if not re-enable the game related functions.
                // http://stackoverflow.com/questions/4260794/how-to-tell-if-device-is-sleeping
                KeyguardManager keyguard = (KeyguardManager)context.GetSystemService(Context.KeyguardService);
                if (!keyguard.InKeyguardRestrictedInputMode())
                {
                    ScreenReceiver.ScreenLocked = false;
                    MediaPlayer.IsMuted = false;
                }
			}
			else if(intent.Action == Intent.ActionUserPresent)
			{
                // This intent is broadcast when the user unlocks the phone
				ScreenReceiver.ScreenLocked = false;
				MediaPlayer.IsMuted = false;
			}
		}
	}
}

