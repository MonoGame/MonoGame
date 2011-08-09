#region File Description
//-----------------------------------------------------------------------------
// AnimatedGameComponentAnimation.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
#endregion

namespace CardsFramework
{
    /// <summary>
    /// Represents an animation that can alter an animated component.
    /// </summary>
    public class AnimatedGameComponentAnimation
    {
        #region Fields and Properties
        protected TimeSpan Elapsed { get; set; }
        public AnimatedGameComponent Component { get; internal set; }
        /// <summary>
        /// An action to perform before the animation begins.
        /// </summary>
        public Action<object> PerformBeforeStart;
        public object PerformBeforSartArgs { get; set; }
        /// <summary>
        /// An action to perform once the animation is complete.
        /// </summary>
        public Action<object> PerformWhenDone;
        public object PerformWhenDoneArgs { get; set; }

        uint animationCycles = 1;
        /// <summary>
        /// Sets the amount of cycles to perform for the animation.
        /// </summary>
        public uint AnimationCycles
        {
            get
            {
                return animationCycles;
            }
            set
            {
                if (value > 0)
                {
                    animationCycles = value;
                }
            }
        }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Returns the time at which the animation is estimated to end.
        /// </summary>
        public TimeSpan EstimatedTimeForAnimationCompletion
        {
            get
            {
                if (isStarted)
                {
                    return (Duration - Elapsed);
                }
                else
                {
                    return StartTime - DateTime.Now + Duration;
                }
            }
        }

        public bool IsLooped { get; set; }

        private bool isDone = false;

        private bool isStarted = false;
        #endregion

        #region Initiaizations
        /// <summary>
        /// Initializes a new instance of the class. Be default, an animation starts
        /// immediately and has a duration of 150 milliseconds.
        /// </summary>
        public AnimatedGameComponentAnimation()
        {
            StartTime = DateTime.Now;
            Duration = TimeSpan.FromMilliseconds(150);
        }
        #endregion

        /// <summary>
        /// Check whether or not the animation is done playing. Looped animations
        /// never finish playing.
        /// </summary>
        /// <returns>Whether or not the animation is done playing</returns>
        public bool IsDone()
        {
            if (!isDone)
            {
                isDone = !IsLooped && (Elapsed >= Duration);
                if (isDone && PerformWhenDone != null)
                {
                    PerformWhenDone(PerformWhenDoneArgs);
                    PerformWhenDone = null;
                }
            }
            return isDone;
        }

        /// <summary>
        /// Returns whether or not the animation is started. As a side-effect, starts
        /// the animation if it is not started and it is time for it to start.
        /// </summary>
        /// <returns>Whether or not the animation is started</returns>
        public bool IsStarted()
        {
            if (!isStarted)
            {
                if (StartTime <= DateTime.Now)
                {
                    if (PerformBeforeStart != null)
                    {
                        PerformBeforeStart(PerformBeforSartArgs);
                        PerformBeforeStart = null;
                    }
                    StartTime = DateTime.Now;
                    isStarted = true;
                }
            }
            return isStarted;
        }

        /// <summary>
        /// Increases the amount of elapsed time as seen by the animation, but only
        /// if the animation is started.
        /// </summary>
        /// <param name="elapsedTime">The timespan by which to incerase the animation's
        /// elapsed time.</param>
        internal void AccumulateElapsedTime(TimeSpan elapsedTime)
        {
            if (isStarted)
            {
                Elapsed += elapsedTime;
            }
        }

        /// <summary>
        /// Runs the animation.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public virtual void Run(GameTime gameTime)
        {
            bool isStarted = IsStarted();
        }
    }
}
