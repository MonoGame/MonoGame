#region File Description
//-----------------------------------------------------------------------------
// WaypointList.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

#endregion

namespace Waypoint
{
    /// <summary>
    /// WaypointList is a drawable List of screen locations
    /// </summary>
    public class WaypointList : Queue<Vector2>
    {
        #region Fields

        // Draw data
        Texture2D waypointTexture;
        Vector2 waypointCenter;

        #endregion

        #region Initialization
        
        /// <summary>
        /// Load the WaypointList's texture resources
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            waypointTexture = content.Load<Texture2D>("dot");
            waypointCenter = 
                new Vector2(waypointTexture.Width / 2, waypointTexture.Height / 2);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw the waypoint list, fading from red for the next waypoint to blue 
        /// for the last
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (Count == 1)
            {
                // If we only have a single waypoint in the list we don’t have to 
                // worry about creating a nice color gradient, so we just draw it 
                // as completely red here
                spriteBatch.Begin();

                spriteBatch.Draw(waypointTexture, Peek(), null, Color.Red,
                    0f, waypointCenter, 1f, SpriteEffects.None, 0f);

                spriteBatch.End();
            }
            else if (Count > 0)
            {
                // In this case we have more than one waypoint on the list, so we 
                // want to fade smoothly from red for the first and blue for the 
                // last so we can tell visually what order they’re in
                float numberPoints = this.Count - 1;
                float i = 0;

                spriteBatch.Begin();
                foreach (Vector2 location in this)
                {
                    // This creates a gradient between 0 for the first waypoint on 
                    // the list and 1 for the last, 0 creates a color that's 
                    // completely red and 1 creates a color that's completely blue    
                    spriteBatch.Draw(waypointTexture, location, null, 
                        new Color(Vector4.Lerp(Color.Red.ToVector4(), 
                        Color.Blue.ToVector4(), i / numberPoints)), 
                        0f, waypointCenter, 1f, SpriteEffects.None, 0f);
					spriteBatch.Draw(waypointTexture, location, null, Color.Blue,
                    0f, waypointCenter, 1f, SpriteEffects.None, 0f);

                    i++;
                }
                spriteBatch.End();
            }
        }

        #endregion
    }
}
