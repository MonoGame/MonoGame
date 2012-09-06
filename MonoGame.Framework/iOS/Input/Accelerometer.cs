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

using Microsoft.Xna.Framework;

using MonoTouch.CoreMotion;
using MonoTouch.Foundation;

namespace Microsoft.Xna.Framework.Input
{
    public class SensorFailedException : Exception
    {
        public int ErrorId { get; protected set; }
    }

    public struct AccelerometerReading
    {
        public Vector3 Acceleration { get; internal set; }
        public DateTimeOffset Timestamp { get; internal set; }
    }

    public class AccelerometerReadingEventArgs : EventArgs
    {
        public AccelerometerReading SensorReading { get; set; }
        
        public AccelerometerReadingEventArgs(AccelerometerReading sensorReading)
        {
            this.SensorReading = sensorReading;
        }
    }

    public sealed class Accelerometer
    {
        // SensorBase
        private TimeSpan timeBetweenUpdates;
        public AccelerometerReading CurrentValue { get; private set; }
        public bool IsDataValid { get; private set; }
        public TimeSpan TimeBetweenUpdates
        {
            get { return this.timeBetweenUpdates; }
            set
            {
                if (this.timeBetweenUpdates != value)
                {
                    this.timeBetweenUpdates = value;
                    if (this.TimeBetweenUpdatesChanged != null)
                        this.TimeBetweenUpdatesChanged(this, EventArgs.Empty);
                }
            }
        }
        
        public event EventHandler<AccelerometerReadingEventArgs> CurrentValueChanged;
        private event EventHandler<EventArgs> TimeBetweenUpdatesChanged;
       
        // Accelerometer
        private static CMMotionManager motionManager = new CMMotionManager();
        private static bool started = false;
        
        public static bool IsSupported
        {
            get { return motionManager.AccelerometerAvailable; }
        }
        
        private static event CMAccelerometerHandler readingChanged;
        
        public Accelerometer()
        {
            this.TimeBetweenUpdates = TimeSpan.FromMilliseconds(2);

            if (!IsSupported)
                throw new SensorFailedException();
            
            this.TimeBetweenUpdatesChanged += this.UpdateInterval;
            readingChanged += ReadingChangedHandler;
            
        }
        
        public void Start()
        {
            if (started == false)
            {
                motionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue, AccelerometerHandler);
                started = true;
            }
        }
        
        public void Stop()
        {
            motionManager.StopAccelerometerUpdates();
            started = false;
        }
        
        private void AccelerometerHandler(CMAccelerometerData data, NSError error)
        {
            readingChanged(data, error);
        }
        
        private void ReadingChangedHandler(CMAccelerometerData data, NSError error)
        {
            AccelerometerReading reading = new AccelerometerReading();
            this.IsDataValid = error == null;
            if (this.IsDataValid)
            {
                this.IsDataValid = true;
                reading.Acceleration = new Vector3((float)data.Acceleration.X, (float)data.Acceleration.Y, (float)data.Acceleration.Z);
                reading.Timestamp = DateTime.Now;
                this.CurrentValue = reading;
                this.IsDataValid = error == null;
            }
            FireOnCurrentValueChanged(this, new AccelerometerReadingEventArgs(reading));
        }
        
        private void UpdateInterval(object sender, EventArgs args)
        {
            motionManager.AccelerometerUpdateInterval = this.TimeBetweenUpdates.TotalSeconds;
        }

        private void FireOnCurrentValueChanged(object sender, AccelerometerReadingEventArgs sample)
        {
            if (this.CurrentValueChanged != null)
                this.CurrentValueChanged(this, sample);
        }
    }
}


