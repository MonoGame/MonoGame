#region File Description
//-----------------------------------------------------------------------------
// Ship.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Content;
#endregion

namespace NetRumble
{
    /// <summary>
    /// The ship, which is the primary playing-piece in the game.
    /// </summary>
    public class Ship : GameplayObject
    {
        #region Constants


        /// <summary>
        /// The full speed possible for the ship.
        /// </summary>
        const float fullSpeed = 320f;

        /// <summary>
        /// The amount of drag applied to velocity per second, 
        /// as a percentage of velocity.
        /// </summary>
        const float dragPerSecond = 0.9f;

        /// <summary>
        /// The amount that the right-stick must be pressed to fire, squared so that
        /// we can use LengthSquared instead of Length, which has a square-root in it.
        /// </summary>
        const float fireThresholdSquared = 0.25f;

        /// <summary>
        /// The number of radians that the ship can turn in a second at full left-stick.
        /// </summary>
        const float rotationRadiansPerSecond = 6f;

        /// <summary>
        /// The maximum length of the velocity vector on a ship.
        /// </summary>
        const float velocityMaximum = 320f;

        /// <summary>
        /// The maximum strength of the shield.
        /// </summary>
        const float shieldMaximum = 100f;

        /// <summary>
        /// The maximum opacity for the shield, when it's fully recharged.
        /// </summary>
        const float shieldAlphaMaximum = 150f;

        /// <summary>
        /// How much the shield recharges per second.
        /// </summary>
        const float shieldRechargePerSecond = 50f;

        /// <summary>
        /// The duration of the shield-recharge timer when the ship is hit.
        /// </summary>
        const float shieldRechargeTimerMaximum = 2.5f;

        /// <summary>
        /// The base scale for the shield, compared to the size of the ship.
        /// </summary>
        const float shieldScaleBase = 1.2f;

        /// <summary>
        /// The amplitude of the shield pulse
        /// </summary>
        const float shieldPulseAmplitude = 0.15f;

        /// <summary>
        /// The rate of the shield pulse.
        /// </summary>
        const float shieldPulseRate = 0.2f;

        /// <summary>
        /// The maximum value of the "safe" timer.
        /// </summary>
        const float safeTimerMaximum = 4f;

        /// <summary>
        /// The maximum amount of life that a ship can have.
        /// </summary>
        const float lifeMaximum = 25f;

        /// <summary>
        /// The value of the spawn timer set when the ship dies.
        /// </summary>
        const float respawnTimerOnDeath = 5f;

        /// <summary>
        /// The number of variations in textures for ships.
        /// </summary>
        const int variations = 4;


        #endregion


        #region Static Graphics Data


        /// <summary>
        /// The primary ship textures.
        /// </summary>
        private static Texture2D[] primaryTextures = new Texture2D[variations];

        /// <summary>
        /// The overlay ship textures, which get tinted.
        /// </summary>
        private static Texture2D[] overlayTextures = new Texture2D[variations];

        /// <summary>
        /// The ship shield texture.
        /// </summary>
        private static Texture2D shieldTexture;

        /// <summary>
        /// The colors used for each ship.
        /// </summary>
        public static readonly Color[] ShipColors = 
            {
                Color.Lime,      Color.CornflowerBlue, Color.Fuchsia,
                Color.Red,       Color.LightSeaGreen,  Color.LightGray,
                Color.Gold,      Color.ForestGreen,    Color.Beige, 
                Color.LightPink, Color.Lavender,       Color.OrangeRed, 
                Color.Plum,      Color.Tan,            Color.YellowGreen,
                Color.Azure,     Color.Aqua,           Color.Salmon
            };

        /// <summary>
        /// The particle-effect manager which recieves the effects from ships.
        /// </summary>
        public static ParticleEffectManager ParticleEffectManager;


        #endregion


        #region Gameplay Data


        /// <summary>
        /// The score for this ship.
        /// </summary>
        private int score = 0;
        public int Score
        {
            get { return score; }
            set { score = value; }
        }

        /// <summary>
        /// The amount of damage that the ship can take before exploding.
        /// </summary>
        private float life;
        public float Life
        {
            get { return life; }
            set { life = value; }
        }

        /// <summary>
        /// The ship's primary weapon.
        /// </summary>
        private Weapon weapon;
        public Weapon Weapon
        {
            get { return weapon; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                weapon = value;
            }
        }

        /// <summary>
        /// The ship's mine-laying weapon.
        /// </summary>
        private MineWeapon mineWeapon;

        /// <summary>
        /// All of the projectiles fired by this ship.
        /// </summary>
        private BatchRemovalCollection<Projectile> projectiles = 
            new BatchRemovalCollection<Projectile>();
        public BatchRemovalCollection<Projectile> Projectiles
        {
            get { return projectiles; }
        }

        /// <summary>
        /// The strength of the shield.
        /// </summary>
        private float shield;
        public float Shield
        {
            get { return shield; }
            set { shield = value; }
        }

        /// <summary>
        /// Timer for how long until the shield starts to recharge.
        /// </summary>
        private float shieldRechargeTimer;

        /// <summary>
        /// Timer for how long the player is safe after spawning.
        /// </summary>
        private float safeTimer;
        public bool Safe
        {
            get { return (safeTimer > 0f); }
            set
            {
                if (value)
                {
                    safeTimer = safeTimerMaximum;
                }
                else
                {
                    safeTimer = 0f;
                }
            }
        }

        /// <summary>
        /// The tint of the ship.
        /// </summary>
        private Color color = Color.White;
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }
        
        /// <summary>
        /// The amount of time left before respawning the ship.
        /// </summary>
        private float respawnTimer;
        public float RespawnTimer
        {
            get { return respawnTimer; }
            set { respawnTimer = value; }
        }

        /// <summary>
        /// The last object to damage the ship.
        /// </summary>
        private GameplayObject lastDamagedBy = null;
        public GameplayObject LastDamagedBy
        {
            get { return lastDamagedBy; }
        }


        #endregion


        #region Input Data


        /// <summary>
        /// The current player input for the ship.
        /// </summary>
        private ShipInput shipInput;
        public ShipInput ShipInput
        {
            get { return shipInput; }
            set { shipInput = value; }
        }


        #endregion


        #region Graphics Data


        /// <summary>
        /// The variation of this ship.
        /// </summary>
        private int variation = 0;
        public int Variation
        {
            get { return variation; }
            set 
            {
                if ((value < 0) || (value >= variations))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                variation = value;
            }
        }


        /// <summary>
        /// The time accumulator for the shield pulse.
        /// </summary>
        private float shieldPulseTime = 0f;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Construct a new ship.
        /// </summary>
        public Ship() 
            : base() 
        {
            // set the collision data
            this.radius = 24f;
            this.mass = 32f;
        }


        /// <summary>
        /// Initialize a ship to its default gameplay states.
        /// </summary>
        public override void Initialize()
        {
            if (!active)
            {
                // set the initial gameplay data values
                shipInput = ShipInput.Empty;
                rotation = 0f;
                velocity = Vector2.Zero;
                life = lifeMaximum;
                shield = shieldMaximum;
                shieldRechargeTimer = 0f;
                safeTimer = safeTimerMaximum;
                weapon = new LaserWeapon(this);
                mineWeapon = new MineWeapon(this);

                // play the player-spawn sound
                AudioManager.PlaySoundEffect("player_spawn");

                // add the ship-spawn particle effect
                if (ParticleEffectManager != null)
                {
                    ParticleEffectManager.SpawnEffect(ParticleEffectType.ShipSpawn,
                        this);
                }

                // clear out the projectiles list
                projectiles.Clear();
            }

            base.Initialize();
        }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Update the ship.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public override void Update(float elapsedTime)
        {
            // calculate the current forward vector
            Vector2 forward = new Vector2((float)Math.Sin(Rotation),
                -(float)Math.Cos(Rotation));
            Vector2 right = new Vector2(-forward.Y, forward.X);

            // calculate the new forward vector with the left stick
            shipInput.LeftStick.Y *= -1f;
            if (shipInput.LeftStick.LengthSquared() > 0f)
            {
                // change the direction
                Vector2 wantedForward = Vector2.Normalize(shipInput.LeftStick);
                float angleDiff = (float)Math.Acos(
                    Vector2.Dot(wantedForward, forward));
                float facing = (Vector2.Dot(wantedForward, right) > 0f) ?
                    1f : -1f;
                if (angleDiff > 0.001f)
                {
                    Rotation += Math.Min(angleDiff, facing * elapsedTime *
                        rotationRadiansPerSecond);
                }

                // add velocity
                Velocity += shipInput.LeftStick * (elapsedTime * fullSpeed);
                if (Velocity.Length() > velocityMaximum)
                {
                    Velocity = Vector2.Normalize(Velocity) *
                        velocityMaximum;
                }
            }
            shipInput.LeftStick = Vector2.Zero;

            // apply drag to the velocity
            Velocity -= Velocity * (elapsedTime * dragPerSecond);
            if (Velocity.LengthSquared() <= 0f)
            {
                Velocity = Vector2.Zero;
            }

            // check for firing with the right stick
            shipInput.RightStick.Y *= -1f;
            if (shipInput.RightStick.LengthSquared() > fireThresholdSquared)
            {
                weapon.Fire(Vector2.Normalize(shipInput.RightStick));
            }
            shipInput.RightStick = Vector2.Zero;

            // check for laying mines
            if (shipInput.MineFired)
            {
                // fire behind the ship
                mineWeapon.Fire(-forward);
            }
            shipInput.MineFired = false;
            
            // recharge the shields
            if (shieldRechargeTimer > 0f)
            {
                shieldRechargeTimer = Math.Max(shieldRechargeTimer - elapsedTime, 
                    0f);
            }
            if (shieldRechargeTimer <= 0f)
            {
                if (shield < shieldMaximum)
                {
                    shield = Math.Min(shieldMaximum,
                        shield + shieldRechargePerSecond * elapsedTime);
                }
            }

            // update the radius based on the shield
            radius = (shield > 0f) ? 24f : 20f;

            // update the weapons
            if (weapon != null)
            {
                weapon.Update(elapsedTime);
            }
            if (mineWeapon != null)
            {
                mineWeapon.Update(elapsedTime);
            }

            // decrement the safe timer
            if (safeTimer > 0f)
            {
                safeTimer = Math.Max(safeTimer - elapsedTime, 0f);
            }

            // update the projectiles
            foreach (Projectile projectile in projectiles)
            {
                if (projectile.Active)
                {
                    projectile.Update(elapsedTime);
                }
                else
                {
                    projectiles.QueuePendingRemoval(projectile);
                }
            }
            projectiles.ApplyPendingRemovals();

            base.Update(elapsedTime);
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw the ship.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="spriteBatch">The SpriteBatch object used to draw.</param>
        public void Draw(float elapsedTime, SpriteBatch spriteBatch)
        {
            // draw the uniform section of the ship
            base.Draw(elapsedTime, spriteBatch, primaryTextures[variation], 
                null, Color.White);

            // draw the tinted section of the ship
            base.Draw(elapsedTime, spriteBatch, overlayTextures[variation],
                null, color);

            if (shield > 0)
            {
                // calculate the current shield radius
                float oldRadius = radius;
                shieldPulseTime += elapsedTime;
                radius *= shieldScaleBase + shieldPulseAmplitude * 
                    (float)Math.Sin(shieldPulseTime / shieldPulseRate);
                // draw the shield
                base.Draw(elapsedTime, spriteBatch, shieldTexture, null, 
                    new Color(color.R, color.G, color.B,
                       (byte)Math.Floor(shieldAlphaMaximum * shield / shieldMaximum)));
                // return to the old radius
                radius = oldRadius;
            }

            // draw the projectiles
            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw(elapsedTime, spriteBatch);
            }
        }


        #endregion


        #region Interaction Methods


        /// <summary>
        /// Damage this ship by the amount provided.
        /// </summary>
        /// <remarks>
        /// This function is provided in lieu of a Life mutation property to allow 
        /// classes of objects to restrict which kinds of objects may damage them,
        /// and under what circumstances they may be damaged.
        /// </remarks>
        /// <param name="source">The GameplayObject responsible for the damage.</param>
        /// <param name="damageAmount">The amount of damage.</param>
        /// <returns>If true, this object was damaged.</returns>
        public override bool Damage(GameplayObject source, float damageAmount)
        {
            // if the safe timer hasn't yet gone off, then the ship can't be hurt
            if ((safeTimer > 0f) || (damageAmount <= 0f))
            {
                return false;
            }

            // once you're hit, the shield-recharge timer starts over
            shieldRechargeTimer = 2.5f;

            // damage the shield first, then life
            if (shield <= 0f)
            {
                life -= damageAmount;
            }
            else
            {
                shield -= damageAmount;
                if (shield < 0f)
                {
                    // shield has the overflow value as a negative value, just add it
                    life += shield;
                    shield = 0f;
                }
            }

            Projectile sourceAsProjectile = source as Projectile;
            if (sourceAsProjectile != null)
            {
                lastDamagedBy = sourceAsProjectile.Owner;
            }
            else
            {
                lastDamagedBy = source;
            }

            return true;
        }


        /// <summary>
        /// Kills this ship, in response to the given GameplayObject.
        /// </summary>
        /// <param name="source">The GameplayObject responsible for the kill.</param>
        /// <param name="cleanupOnly">
        /// If true, the object dies without any further effects.
        /// </param>
        public override void Die(GameplayObject source, bool cleanupOnly)
        {
            if (active)
            {
                if (!cleanupOnly)
                {
                    // update the score
                    Ship ship = source as Ship;
                    if (ship == null)
                    {
                        Projectile projectile = source as Projectile;
                        if (projectile != null)
                        {
                            ship = projectile.Owner;
                        }
                    }
                    if (ship != null)
                    {
                        if (ship == this)
                        {
                            // reduce the score, since i blew myself up
                            ship.Score--;
                        }
                        else
                        {
                            // add score to the ship who shot this object
                            ship.Score++;
                        }
                    }
                    else
                    {
                        // if it wasn't a ship, then this object loses score
                        this.Score--;
                    }

                    // play the player-death sound
                    AudioManager.PlaySoundEffect("explosion_shockwave");
                    AudioManager.PlaySoundEffect("explosion_large");

                    // display the ship explosion
                    if (ParticleEffectManager != null)
                    {
                        ParticleEffectManager.SpawnEffect(
                            ParticleEffectType.ShipExplosion, Position);
                    }
                }

                // clear out the projectiles list
                foreach (Projectile projectile in projectiles)
                {
                    projectile.Die(null, true);
                }
                projectiles.Clear();

                // set the respawn timer
                respawnTimer = respawnTimerOnDeath;
            }

            base.Die(source, cleanupOnly);
        }


        #endregion


        #region Static Graphics Methods


        /// <summary>
        /// Load all of the static graphics content for this class.
        /// </summary>
        /// <param name="contentManager">The content manager to load with.</param>
        public static void LoadContent(ContentManager contentManager)
        {
            // safety-check the parameters
            if (contentManager == null)
            {
                throw new ArgumentNullException("contentManager");
            }

            // load each ship's texture
            for (int i = 0; i < variations; i++)
            {
                primaryTextures[i] = contentManager.Load<Texture2D>(
                    "Textures/ship" + i.ToString());
                overlayTextures[i] = contentManager.Load<Texture2D>(
                    "Textures/ship" + i.ToString() + "Overlay");
            }

            // load the shield texture
            shieldTexture = contentManager.Load<Texture2D>("Textures/shipShields");
        }


        /// <summary>
        /// Unload all of the static graphics content for this class.
        /// </summary>
        public static void UnloadContent()
        {
            for (int i = 0; i < variations; i++)
            {
                primaryTextures[i] = overlayTextures[i] = null;
            }
            shieldTexture = null;
        }


        /// <summary>
        /// Determines if a color index is unique.
        /// </summary>
        /// <param name="networkGamer">The gamer with the color index.</param>
        /// <param name="networkSession">The session the player belongs to.</param>
        /// <returns>If true, the gamer has a unique color ID.</returns>
        public static bool HasUniqueColorIndex(NetworkGamer networkGamer, 
            NetworkSession networkSession)
        {
            // safety-check the parameters
            if (networkGamer == null)
            {
                throw new ArgumentNullException("networkGamer");
            }
            if (networkSession == null)
            {
                throw new ArgumentNullException("networkSession");
            }
            PlayerData playerData = networkGamer.Tag as PlayerData;
            if (playerData == null)
            {
                throw new ArgumentNullException("networkGamer.Tag as PlayerData");
            }

            // search for a match
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                if (gamer == networkGamer)
                {
                    continue;
                }
                PlayerData gamerData = gamer.Tag as PlayerData;
                if ((gamerData != null) && 
                    (gamerData.ShipColor == playerData.ShipColor))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Find the next unique color index among the players.
        /// </summary>
        /// <param name="currentColorIndex">The current color index.</param>
        /// <param name="networkSession">The network session.</param>
        /// <returns>The next unique color index.</returns>
        public static byte GetNextUniqueColorIndex(byte currentColorIndex, 
            NetworkSession networkSession)
        {
            // safety-check the parameters
            if ((currentColorIndex < 0) || (currentColorIndex >= ShipColors.Length))
            {
                throw new ArgumentOutOfRangeException("currentColorIndex");
            }
            if (networkSession == null)
            {
                throw new ArgumentNullException("networkSession");
            }
            if (networkSession.AllGamers.Count > ShipColors.Length)
            {
                throw new InvalidOperationException(
                    "There are more gamers than there are colors.");
            }

            // if there are as many gamers as there are colors, then we can't change
            if (networkSession.AllGamers.Count == ShipColors.Length)
            {
                return currentColorIndex;
            }

            bool colorFound;
            byte newColorIndex = currentColorIndex;
            do
            {
                newColorIndex++;
                if (newColorIndex >= ShipColors.Length)
                {
                    newColorIndex = 0;
                }
                colorFound = false;
                foreach (NetworkGamer networkGamer in networkSession.AllGamers)
                {
                    PlayerData playerData = networkGamer.Tag as PlayerData;
                    if ((playerData != null) && (playerData.ShipColor == newColorIndex))
                    {
                        colorFound = true;
                        break;
                    }
                }
            }
            while (colorFound && (newColorIndex != currentColorIndex));

            return newColorIndex;
        }


        /// <summary>
        /// Find the previous unique color index among the players.
        /// </summary>
        /// <param name="currentColorIndex">The current color index.</param>
        /// <param name="networkSession">The network session..</param>
        /// <returns>The previous unique color index.</returns>
        public static byte GetPreviousUniqueColorIndex(byte currentColorIndex, 
            NetworkSession networkSession)
        {
            // safety-check the parameters
            if ((currentColorIndex < 0) || (currentColorIndex >= ShipColors.Length))
            {
                throw new ArgumentOutOfRangeException("currentColorIndex");
            }
            if (networkSession == null)
            {
                throw new ArgumentNullException("networkSession");
            }
            if (networkSession.AllGamers.Count > ShipColors.Length)
            {
                throw new InvalidOperationException(
                    "There are more gamers than there are colors.");
            }

            // if there are as many gamers as there are colors, then we can't change
            if (networkSession.AllGamers.Count == ShipColors.Length)
            {
                return currentColorIndex;
            }

            bool colorFound;
            byte newColorIndex = currentColorIndex;
            do
            {
                if (newColorIndex == 0)
                {
                    newColorIndex = (byte)(ShipColors.Length - 1);
                }
                else
                {
                    newColorIndex--;
                }
                colorFound = false;
                foreach (NetworkGamer networkGamer in networkSession.AllGamers)
                {
                    PlayerData playerData = networkGamer.Tag as PlayerData;
                    if ((playerData != null) && (playerData.ShipColor == newColorIndex))
                    {
                        colorFound = true;
                        break;
                    }
                }
            }
            while (colorFound && (newColorIndex != currentColorIndex));

            return newColorIndex;
        }

        /// <summary>
        /// The number of variations in ships.
        /// </summary>
        public static int Variations
        {
            get { return variations; }
        }


        #endregion
    }
}
