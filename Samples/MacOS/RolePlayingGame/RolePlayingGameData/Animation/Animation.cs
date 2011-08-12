#region File Description
//-----------------------------------------------------------------------------
// Animation.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
#endregion

namespace RolePlayingGameData
{
    /// <summary>
    /// An animation description for an AnimatingSprite object.
    /// </summary>
#if !XBOX
    [DebuggerDisplay("Name = {Name}")]
#endif
    public class Animation : ContentObject
    {
        /// <summary>
        /// The name of the animation.
        /// </summary>
        private string name;

        /// <summary>
        /// The name of the animation.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }



        /// <summary>
        /// The first frame of the animation.
        /// </summary>
        private int startingFrame;

        /// <summary>
        /// The first frame of the animation.
        /// </summary>
        public int StartingFrame
        {
            get { return startingFrame; }
            set { startingFrame = value; }
        }


        /// <summary>
        /// The last frame of the animation.
        /// </summary>
        private int endingFrame;

        /// <summary>
        /// The last frame of the animation.
        /// </summary>
        public int EndingFrame
        {
            get { return endingFrame; }
            set { endingFrame = value; }
        }


        /// <summary>
        /// The interval between frames of the animation.
        /// </summary>
        private int interval;

        /// <summary>
        /// The interval between frames of the animation.
        /// </summary>
        public int Interval
        {
            get { return interval; }
            set { interval = value; }
        }


        /// <summary>
        /// If true, the animation loops.
        /// </summary>
        private bool isLoop;

        /// <summary>
        /// If true, the animation loops.
        /// </summary>
        public bool IsLoop
        {
            get { return isLoop; }
            set { isLoop = value; }
        }


        #region Constructors


        /// <summary>
        /// Creates a new Animation object.
        /// </summary>
        public Animation() { }


        /// <summary>
        /// Creates a new Animation object by full specification.
        /// </summary>
        public Animation(string name, int startingFrame, int endingFrame, int interval, 
            bool isLoop)
        {
            this.Name = name;
            this.StartingFrame = startingFrame;
            this.EndingFrame = endingFrame;
            this.Interval = interval;
            this.IsLoop = isLoop;
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Read an Animation object from the content pipeline.
        /// </summary>
        public class AnimationReader : ContentTypeReader<Animation>
        {
            /// <summary>
            /// Read an Animation object from the content pipeline.
            /// </summary>
            protected override Animation Read(ContentReader input, 
                Animation existingInstance)
            {
                Animation animation = existingInstance;
                if (animation == null)
                {
                    animation = new Animation();
                }

                animation.AssetName = input.AssetName;

                animation.Name = input.ReadString();
                animation.StartingFrame = input.ReadInt32();
                animation.EndingFrame = input.ReadInt32();
                animation.Interval = input.ReadInt32();
                animation.IsLoop = input.ReadBoolean();

                return animation;
            }
        }


        #endregion
    }
}
