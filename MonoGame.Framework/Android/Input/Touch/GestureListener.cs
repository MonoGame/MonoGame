using System;
using Android.Views;

namespace Microsoft.Xna.Framework.Input.Touch
{
	internal class GestureListener : GestureDetector.SimpleOnGestureListener
	{
		AndroidGameActivity activity;
		
		public GestureListener(AndroidGameActivity activity) : base()
		{
			this.activity = activity;
		}
		
		/// <summary>
		/// convert the DoubleTapEvent to a Gesture
		/// </summary>
		/// <param name='e'>
		/// If set to <c>true</c> e.
		/// </param>
		public override bool OnDoubleTap (MotionEvent e)
		{
			if ((TouchPanel.EnabledGestures & GestureType.DoubleTap) != 0)
			{				
				var gs = new GestureSample(GestureType.DoubleTap, activity.Game.TargetElapsedTime, 
					new Vector2(e.GetX(), e.GetY()), Vector2.Zero, Vector2.Zero, Vector2.Zero);
				TouchPanel.GestureList.Enqueue(gs);
			}
			return base.OnDoubleTap (e);
		}
		
		/// <summary>
		/// Process the Single Tag into a Gesture
		/// </summary>
		/// <param name='e'>
		/// If set to <c>true</c> e.
		/// </param>
		public override bool OnSingleTapConfirmed (MotionEvent e)
		{
			if ((TouchPanel.EnabledGestures & GestureType.Tap) != 0)
			{
				var gs = new GestureSample(GestureType.Tap, activity.Game.TargetElapsedTime, 
					new Vector2(e.GetX(), e.GetY()), Vector2.Zero, Vector2.Zero, Vector2.Zero);
				TouchPanel.GestureList.Enqueue(gs);
			}
			return base.OnSingleTapConfirmed (e);
		}
	}
}

