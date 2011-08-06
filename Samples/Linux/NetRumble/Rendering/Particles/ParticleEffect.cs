#region File Description
//-----------------------------------------------------------------------------
// ParticleEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace NetRumble
{
    /// <summary>
    /// A multi-emitter particle effect, comprised of multiple particle systems.
    /// </summary>
    public class ParticleEffect
    {
        #region Description Data


        /// <summary>
        /// The name of the particle effect.
        /// </summary>
        private string name;

        /// <summary>
        /// The particle systems in this effect.
        /// </summary>
        private Collection<ParticleSystem> particleSystems =
            new Collection<ParticleSystem>();


        #endregion


        #region Graphics Data


        /// <summary>
        /// The position of the particle effect in the world.
        /// </summary>
        private Vector2 position;


        /// <summary>
        /// The gameplay object that the system is following, if any.
        /// </summary>
        private GameplayObject followObject;
        [XmlIgnore()]
        public GameplayObject GameplayObject
        {
            get { return followObject; }
            set { followObject = value; }
        }


        #endregion


        #region Status Data


        /// <summary>
        /// If true, the particle effect is currently active.
        /// </summary>
        private bool active = false;
        [XmlIgnore()]
        public bool Active
        {
            get { return active; }
        }


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Create a new particle effect.
        /// </summary>
        public ParticleEffect() { }


        /// <summary>
        /// Create a new particle effect that is a clone of another one.
        /// </summary>
        /// <returns></returns>
        public ParticleEffect Clone()
        {
            ParticleEffect clone = new ParticleEffect();

            // copy the data
            clone.name = this.name;
            clone.position = this.position;

            // copy each system
            for (int i = 0; i < this.particleSystems.Count; ++i)
            {
                clone.ParticleSystems.Add(this.particleSystems[i].Clone());
            }

            return clone;
        }

        
        /// <summary>
        /// Initialize the particle effect.
        /// </summary>
        /// <param name="content">The contenet manager that owns the texture.</param>
        public virtual void Initialize(ContentManager content)
        {
            // initialize each system
            for (int i = 0; i < particleSystems.Count; ++i)
            {
                particleSystems[i].Initialize(content);
            }

            // allow us to start updating and drawing
            active = true;
        }


        /// <summary>
        /// Reset the particle effect.
        /// </summary>
        public virtual void Reset()
        {
            // reset each system
            for (int i = 0; i < particleSystems.Count; ++i)
            {
                particleSystems[i].Reset();
            }

            // allow us to start updating and drawing
            active = true;
        }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Update the particle effect.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public virtual void Update(float elapsedTime)
        {
            // update the position based on the follow-object, if any
            if (followObject != null)
            {
                if (followObject.Active)
                {
                    Position = followObject.Position;
                }
                else
                {
                    followObject = null;
                    Stop(false);
                }
            }

            // update each system
            active = false;
            for (int i = 0; i < particleSystems.Count; ++i)
            {
                if (particleSystems[i].Active)
                {
                    particleSystems[i].Update(elapsedTime);
                    active = true;
                }
            }
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw the particle effect.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch object used to draw.</param>
        /// <param name="blendMode">Filters the systems drawn in this pass.</param>
        public virtual void Draw(SpriteBatch spriteBatch, SpriteBlendMode blendMode)
        {
            if (!active)
                return;

            // draw each system
            for (int i = 0; i < particleSystems.Count; ++i)
            {
                if (particleSystems[i].BlendMode == blendMode)
                {
                    particleSystems[i].Draw(spriteBatch);
                }
            }
        }


        #endregion


        #region Control Methods


        /// <summary>
        /// Stop the particle effect.
        /// </summary>
        /// <param name="immediately">
        /// If true, particles are no longer drawn or updated.
        /// Otherwise, only generation is halted.
        /// </param>
        public void Stop(bool immediately)
        {
            // stop all of the systems
            for (int i = 0; i < particleSystems.Count; ++i)
            {
                particleSystems[i].Stop(immediately);
            }

            // halt updating/drawing of the particles if requested
            if (immediately)
            {
                active = false;
            }
        }


        #endregion


        #region Serialization Interface


        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                for (int i = 0; i < particleSystems.Count; ++i)
                {
                    particleSystems[i].Position = position;
                }
            }
        }

        public Collection<ParticleSystem> ParticleSystems
        {
            get { return particleSystems as Collection<ParticleSystem>; }
        }


        #endregion


        #region Static Initialization Methods


        /// <summary>
        /// Create a new ParticleEffect object from the data in an XML file.
        /// </summary>
        /// <param name="filepath">The filename of the XML file.</param>
        /// <returns>A new ParticleEffect object.</returns>
        public static ParticleEffect Load(string filepath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ParticleEffect));
            return (ParticleEffect)serializer.Deserialize(File.OpenRead(filepath));
        }


        #endregion
    }
}
