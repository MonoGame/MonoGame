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
		
		private static bool dragging = false;
			
						
		public override bool OnScroll (MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
		{			
			
			if ((TouchPanel.EnabledGestures & GestureType.FreeDrag) != 0)
			{								
				if (!dragging) dragging = true;
		
				Vector2 position = new Vector2(e2.GetX(), e2.GetY());
				Android.Util.Log.Info("MonoGame", position.ToString());
				AndroidGameActivity.Game.Window.UpdateTouchPosition(ref position);
				
				var gs = new GestureSample(GestureType.FreeDrag, AndroidGameActivity.Game.TargetElapsedTime, 
					position, Vector2.Zero, Vector2.Zero, Vector2.Zero);
				TouchPanel.GestureList.Enqueue(gs);
			}
			
			return base.OnScroll (e1, e2, distanceX, distanceY);
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
				Vector2 positon = new Vector2(e.GetX(), e.GetY());
				AndroidGameActivity.Game.Window.UpdateTouchPosition(ref positon);
				var gs = new GestureSample(GestureType.DoubleTap, AndroidGameActivity.Game.TargetElapsedTime, 
					positon, Vector2.Zero, Vector2.Zero, Vector2.Zero);
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
				Vector2 position = new Vector2(e.GetX(), e.GetY());
				AndroidGameActivity.Game.Window.UpdateTouchPosition(ref position);
				var gs = new GestureSample(GestureType.Tap, AndroidGameActivity.Game.TargetElapsedTime, 
					position, Vector2.Zero, Vector2.Zero, Vector2.Zero);
				TouchPanel.GestureList.Enqueue(gs);
				Android.Util.Log.Info("MonoGame", "Tap");
			}
			return base.OnSingleTapConfirmed (e);
		}
				
		// AJW
        public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            if ((TouchPanel.EnabledGestures & GestureType.Flick) != 0)
            {
				Vector2 positon = new Vector2(e1.GetX(), e1.GetY());
				AndroidGameActivity.Game.Window.UpdateTouchPosition(ref positon);
				Vector2 positon2 = new Vector2(e2.GetX(), e2.GetY());
				AndroidGameActivity.Game.Window.UpdateTouchPosition(ref positon2);
                var gs = new GestureSample(GestureType.Flick, AndroidGameActivity.Game.TargetElapsedTime,
                    positon,
                    positon2,
                    new Vector2(velocityX, velocityY),
                    Vector2.Zero);
                TouchPanel.GestureList.Enqueue(gs);
            }
            return base.OnFling(e1, e2, velocityX, velocityY);
        }
		
		public override void OnLongPress (MotionEvent e)
		{
			if ((TouchPanel.EnabledGestures & GestureType.Hold) != 0)
			{
				Vector2 positon = new Vector2(e.GetX(), e.GetY());
				AndroidGameActivity.Game.Window.UpdateTouchPosition(ref positon);
				var gs = new GestureSample(GestureType.Hold, AndroidGameActivity.Game.TargetElapsedTime, 
					positon, Vector2.Zero, Vector2.Zero, Vector2.Zero);
				TouchPanel.GestureList.Enqueue(gs);
			}
			base.OnLongPress (e);
		}
		
		internal static void CheckForDrag(MotionEvent e, Vector2 position)
		{
			
			switch (e.ActionMasked)
            {
				case  MotionEventActions.Up:
                case  MotionEventActions.PointerUp:				
                    if ((dragging && (TouchPanel.EnabledGestures & GestureType.DragComplete) != 0))
					{				
						var gs = new GestureSample(GestureType.DragComplete, AndroidGameActivity.Game.TargetElapsedTime, 
							position, Vector2.Zero, Vector2.Zero, Vector2.Zero);
						TouchPanel.GestureList.Enqueue(gs);
					   dragging = false;
					   Android.Util.Log.Info("MonoGame", "DragComplete");
					}
                    break;
			
			}
		}
		
	}
}

