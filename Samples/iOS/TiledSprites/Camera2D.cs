#region File Description
//-----------------------------------------------------------------------------
// Camera2D.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;

#if IPHONE
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
#else
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
#endif
#endregion

namespace TiledSprites
{
    public class Camera2D
    {
        #region Fields
        private Vector2 positionValue;
        private bool isMovingUsingScreenAxis;
        private float rotationValue;
        private float zoomValue;
        private bool cameraChanged;
        #endregion

        #region Public Properties
        /// <summary>
        /// Get/Set the postion value of the camera
        /// </summary>
        public Vector2 Position
        {
            set
            {
                if (positionValue != value)
                {
                    cameraChanged = true;
                    positionValue = value;
                }
            }
            get { return positionValue; }
        }

        /// <summary>
        /// Get/Set the rotation value of the camera
        /// </summary>
        public float Rotation
        {
            set
            {
                if (rotationValue != value)
                {
                    cameraChanged = true;
                    rotationValue = value;
                }
            }
            get { return rotationValue; }
        }

        /// <summary>
        /// Get/Set the zoom value of the camera
        /// </summary>
        public float Zoom
        {
            set
            {
                if (zoomValue != value)
                {
                    cameraChanged = true;
                    zoomValue = value;
                }
            }
            get { return zoomValue; }
        }

        /// <summary>
        /// Gets whether or not the camera has been changed since the last
        /// ResetChanged call
        /// </summary>
        public bool IsChanged
        {
            get { return cameraChanged; }
        }

        /// <summary>
        /// Set to TRUE to pan relative to the screen axis when
        /// the camera is rotated.
        /// </summary>
        public bool MoveUsingScreenAxis
        {
            set { isMovingUsingScreenAxis = value; }
            get { return isMovingUsingScreenAxis; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new Camera2D
        /// </summary>
        public Camera2D()
        {
            zoomValue = 1.0f;
            rotationValue = 0.0f;
            positionValue = Vector2.Zero;
        }
        #endregion

        #region Movement Methods
        
        /// <summary>
        /// Used to inform the camera that new values are updated by the application.
        /// </summary>
        public void ResetChanged()
        {
            cameraChanged = false;
        }

        /// <summary>
        /// Pan in the right direction.  Corrects for rotation if specified.
        /// </summary>
        public void MoveRight(ref float dist)
        {
            if (dist != 0)
            {
                cameraChanged = true;
                if (isMovingUsingScreenAxis)
                {
                    positionValue.X += (float)Math.Cos(-rotationValue) * dist;
                    positionValue.Y += (float)Math.Sin(-rotationValue) * dist;
                }
                else
                {
                    positionValue.X += dist;
                }
            }
        }
        /// <summary>
        /// Pan in the left direction.  Corrects for rotation if specified.
        /// </summary>
        public void MoveLeft(ref float dist)
        {
            if (dist != 0)
            {
                cameraChanged = true;
                if (isMovingUsingScreenAxis)
                {
                    positionValue.X -= (float)Math.Cos(-rotationValue) * dist;
                    positionValue.Y -= (float)Math.Sin(-rotationValue) * dist;
                }
                else
                {
                    positionValue.X += dist;
                }
            }
        }
        /// <summary>
        /// Pan in the up direction.  Corrects for rotation if specified.
        /// </summary>
        public void MoveUp(ref float dist)
        {
            if (dist != 0)
            {
                cameraChanged = true;
                if (isMovingUsingScreenAxis)
                {
                    //using negative distance becuase 
                    //"up" actually decreases the y value
                    positionValue.X -= (float)Math.Sin(rotationValue) * dist;
                    positionValue.Y -= (float)Math.Cos(rotationValue) * dist;
                }
                else
                {
                    positionValue.Y -= dist;
                }
            }
        }
        /// <summary>
        /// Pan in the down direction.  Corrects for rotation if specified.
        /// </summary>
        public void MoveDown(ref float dist)
        {
            if (dist != 0)
            {
                cameraChanged = true;
                if (isMovingUsingScreenAxis)
                {
                    //using negative distance becuase 
                    //"up" actually decreases the y value
                    positionValue.X += (float)Math.Sin(rotationValue) * dist;
                    positionValue.Y += (float)Math.Cos(rotationValue) * dist;
                }
                else
                {
                    positionValue.Y -= dist;
                }
            }
        }
        #endregion
    }
}