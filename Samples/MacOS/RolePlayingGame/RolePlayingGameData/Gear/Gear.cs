#region File Description
//-----------------------------------------------------------------------------
// Gear.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RolePlayingGameData
{
    /// <summary>
    /// An inventory element - items, equipment, etc.
    /// </summary>
#if !XBOX
    [DebuggerDisplay("Name = {name}")]
#endif
    public abstract class Gear : ContentObject
    {
        #region Description Data


        /// <summary>
        /// The name of this gear.
        /// </summary>
        private string name;

        /// <summary>
        /// The name of this gear.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        /// <summary>
        /// The long description of this gear.
        /// </summary>
        private string description;

        /// <summary>
        /// The long description of this gear.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }


        /// <summary>
        /// Builds and returns a string describing the power of this gear.
        /// </summary>
        public virtual string GetPowerText()
        {
            return String.Empty;
        }


        #endregion


        #region Value Data


        /// <summary>
        /// The value of this gear.
        /// </summary>
        /// <remarks>If the value is less than zero, it cannot be sold.</remarks>
        private int goldValue;

        /// <summary>
        /// The value of this gear.
        /// </summary>
        /// <remarks>If the value is less than zero, it cannot be sold.</remarks>
        public int GoldValue
        {
            get { return goldValue; }
            set { goldValue = value; }
        }


        /// <summary>
        /// If true, the gear can be dropped.  If false, it cannot ever be dropped.
        /// </summary>
        private bool isDroppable;

        /// <summary>
        /// If true, the gear can be dropped.  If false, it cannot ever be dropped.
        /// </summary>
        public bool IsDroppable
        {
            get { return isDroppable; }
            set { isDroppable = value; }
        }


        #endregion


        #region Restrictions


        /// <summary>
        /// The minimum character level required to equip or use this gear.
        /// </summary>
        private int minimumCharacterLevel;

        /// <summary>
        /// The minimum character level required to equip or use this gear.
        /// </summary>
        public int MinimumCharacterLevel
        {
            get { return minimumCharacterLevel; }
            set { minimumCharacterLevel = value; }
        }


        /// <summary>
        /// The list of the names of all supported classes.
        /// </summary>
        /// <remarks>Class names are compared case-insensitive.</remarks>
        private List<string> supportedClasses = new List<string>();

        /// <summary>
        /// The list of the names of all supported classes.
        /// </summary>
        /// <remarks>Class names are compared case-insensitive.</remarks>
        public List<string> SupportedClasses
        {
            get { return supportedClasses; }
        }


        /// <summary>
        /// Check the restrictions on this object against the provided character.
        /// </summary>
        /// <returns>True if the gear could be used, false otherwise.</returns>
        public virtual bool CheckRestrictions(FightingCharacter fightingCharacter)
        {
            if (fightingCharacter == null)
            {
                throw new ArgumentNullException("fightingCharacter");
            }

            return ((fightingCharacter.CharacterLevel >= MinimumCharacterLevel) &&
                   ((SupportedClasses.Count <= 0) ||
                    SupportedClasses.Contains(fightingCharacter.CharacterClass.Name)));
        }


        /// <summary>
        /// Builds a string describing the restrictions on this piece of gear.
        /// </summary>
        public virtual string GetRestrictionsText()
        {
            StringBuilder sb = new StringBuilder();

            // add the minimum character level, if any
            if (MinimumCharacterLevel > 0)
            {
                sb.Append("Level - ");
                sb.Append(MinimumCharacterLevel.ToString());
                sb.Append("; ");
            }

            // add the classes
            if (SupportedClasses.Count > 0)
            {
                sb.Append("Class - ");
                bool firstClass = true;
                foreach (string className in SupportedClasses)
                {
                    if (firstClass)
                    {
                        firstClass = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    sb.Append(className);
                }
            }

            return sb.ToString();
        }


        #endregion


        #region Graphics Data


        /// <summary>
        /// The content path and name of the icon for this gear.
        /// </summary>
        private string iconTextureName;

        /// <summary>
        /// The content path and name of the icon for this gear.
        /// </summary>
        public string IconTextureName
        {
            get { return iconTextureName; }
            set { iconTextureName = value; }
        }


        /// <summary>
        /// The icon texture for this gear.
        /// </summary>
        private Texture2D iconTexture;

        /// <summary>
        /// The icon texture for this gear.
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D IconTexture
        {
            get { return iconTexture; }
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw the icon for this gear.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch object to use when drawing.</param>
        /// <param name="position">The position of the icon on the screen.</param>
        public virtual void DrawIcon(SpriteBatch spriteBatch, Vector2 position)
        {
            // check the parameters
            if (spriteBatch == null)
            {
                throw new ArgumentNullException("spriteBatch");
            }

            // draw the icon, if we there is a texture for it
            if (iconTexture != null)
            {
                spriteBatch.Draw(iconTexture, position, Color.White);
            }
        }


        /// <summary>
        /// Draw the description for this gear in the space provided.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch object to use when drawing.</param>
        /// <param name="spriteFont">The font that the text is drawn with.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="position">The position of the text on the screen.</param>
        /// <param name="maximumCharactersPerLine">
        /// The maximum length of a single line of text.
        /// </param>
        /// <param name="maximumLines">The maximum number of lines to draw.</param>
        public virtual void DrawDescription(SpriteBatch spriteBatch,
            SpriteFont spriteFont, Color color, Vector2 position, 
            int maximumCharactersPerLine, int maximumLines)
        {
            // check the parameters
            if (spriteBatch == null)
            {
                throw new ArgumentNullException("spriteBatch");
            }
            if (spriteFont == null)
            {
                throw new ArgumentNullException("spriteFont");
            }
            if (maximumLines <= 0)
            {
                throw new ArgumentOutOfRangeException("maximumLines");
            }
            if (maximumCharactersPerLine <= 0)
            {
                throw new ArgumentOutOfRangeException("maximumCharactersPerLine");
            }

            // if the string is trivial, then this is really easy
            if (String.IsNullOrEmpty(description))
            {
                return;
            }

            // if the text is short enough to fit on one line, then this is still easy
            if (description.Length < maximumCharactersPerLine)
            {
                spriteBatch.DrawString(spriteFont, description, position, color);
                return;
            }

            // construct a new string with carriage returns
            StringBuilder stringBuilder = new StringBuilder(description);
            int currentLine = 0;
            int newLineIndex = 0;
            while (((description.Length - newLineIndex) > maximumCharactersPerLine) &&
                (currentLine < maximumLines))
            {
                description.IndexOf(' ', 0);
                int nextIndex = newLineIndex;
                while (nextIndex < maximumCharactersPerLine)
                {
                    newLineIndex = nextIndex;
                    nextIndex = description.IndexOf(' ', newLineIndex + 1);
                }
                stringBuilder.Replace(' ', '\n', newLineIndex, 1);
                currentLine++;
            }

            // draw the string
            spriteBatch.DrawString(spriteFont, stringBuilder.ToString(), 
                position, color);
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Reads a Gear object from the content pipeline.
        /// </summary>
        public class GearReader : ContentTypeReader<Gear>
        {
            /// <summary>
            /// Reads a Gear object from the content pipeline.
            /// </summary>
            protected override Gear Read(ContentReader input, Gear existingInstance)
            {
                Gear gear = existingInstance;
                if (gear == null)
                {
                    throw new ArgumentException("Unable to create new Gear objects.");
                }

                gear.AssetName = input.AssetName;

                // read gear settings
                gear.Name = input.ReadString();
                gear.Description = input.ReadString();
                gear.GoldValue = input.ReadInt32();
                gear.IsDroppable = input.ReadBoolean();
                gear.MinimumCharacterLevel = input.ReadInt32();
                gear.SupportedClasses.AddRange(input.ReadObject<List<string>>());
                gear.IconTextureName = input.ReadString();
                gear.iconTexture = input.ContentManager.Load<Texture2D>(
                    System.IO.Path.Combine(@"Textures\Gear", gear.IconTextureName));

                return gear;
            }
        }


        #endregion
    }
}
