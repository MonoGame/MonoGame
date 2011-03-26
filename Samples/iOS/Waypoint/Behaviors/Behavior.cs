#region File Description
//-----------------------------------------------------------------------------
// Behavior.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
#if IPHONE
using Microsoft.Xna.Framework;
#else
using Microsoft.Xna.Framework;
#endif

#endregion

namespace Waypoint
{
    /// <summary>
    /// Behavior is the base class for the two behaviors in this sample: linear
    /// and steering. It is an abstract class, leaving the implementation of 
    /// Update up to its subclasses.
    /// </summary>
    public abstract class Behavior
    {
        #region Fields

        // Keeps track of the tank that this behavior will modify
        protected Tank tank;

        #endregion

        #region Initialization

        protected Behavior(Tank tank)
        {
            this.tank = tank;
            tank.MoveSpeed = Tank.MaxMoveSpeed;
        }

        #endregion

        #region Update

        public abstract void Update(GameTime gameTime);

        #endregion
    }
}
