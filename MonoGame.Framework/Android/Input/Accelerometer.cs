// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 

using System;
using Android.Content;
using Android.Hardware;


namespace Microsoft.Xna.Framework.Input
{
	public static class Accelerometer
	{
		private static AccelerometerState _state;
		private static AccelerometerCapabilities _capabilities = new AccelerometerCapabilities();
        private static SensorManager _sensorManger;
        private static Sensor _sensor;
		private static Vector3 _accelerometerVector = new Vector3(0, 0, 0);
        private static SensorListener listener = new SensorListener();
		
		public static void SetupAccelerometer()
		{
            _sensorManger = (SensorManager)Game.Activity.GetSystemService(Context.SensorService);
            _sensor = _sensorManger.GetDefaultSensor(SensorType.Accelerometer);

            if (_sensor != null) 
            {
                _state = new AccelerometerState { IsConnected = true };                
            }
            else _state = new AccelerometerState { IsConnected = false };
        }

		public static AccelerometerCapabilities GetCapabilities()
        {
			return _capabilities;
        }
		
		public static AccelerometerState GetState()
		{
			return _state;
		}

        private class SensorListener : Java.Lang.Object, ISensorEventListener
        {
            public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
            {
               //do nothing
            }

            public void OnSensorChanged(SensorEvent e)
            {
                try {
					
					if (e != null && e.Sensor.Type == SensorType.Accelerometer) 
					{
     				    var values = e.Values;
				        try 
						{
				            if (values != null && values.Count == 3) {
				                _accelerometerVector.X = values[0];
				                _accelerometerVector.Y = values[1];
				                _accelerometerVector.Z = values[2];  
							    _state.Acceleration = _accelerometerVector;
				            }
				        } 
						finally 
						{
				            IDisposable d = values as IDisposable;
				            if (d != null)
				                d.Dispose ();
				        }
    				}                
                }
                catch (NullReferenceException ex) {
                    //Occassionally an NullReferenceException is thrown when accessing e.Values??
                    // mono    : Unhandled Exception: System.NullReferenceException: Object reference not set to an instance of an object
                    // mono    :   at Android.Runtime.JNIEnv.GetObjectField (IntPtr jobject, IntPtr jfieldID) [0x00000] in <filename unknown>:0 
                    // mono    :   at Android.Hardware.SensorEvent.get_Values () [0x00000] in <filename unknown>:0
                }
            }
        }

        internal static void Pause()
        {
            if (_sensorManger != null && _sensor != null) _sensorManger.UnregisterListener(listener, _sensor);
        }

        internal static void Resume()
        {
            if (_sensorManger != null && _sensor != null) _sensorManger.RegisterListener(listener, _sensor, SensorDelay.Game);            
        }
    }
}
