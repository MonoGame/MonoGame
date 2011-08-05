#region File Description
//-----------------------------------------------------------------------------
// ParticleEffectManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace NetRumble
{
    public class ParticleEffectManager
    {
        #region Effect Collection Data

        /// <summary>
        /// Cache of registered particle effects.
        /// </summary>
        private Dictionary<ParticleEffectType, List<ParticleEffect>> particleEffectCache
            = new Dictionary<ParticleEffectType,List<ParticleEffect>>();

        /// <summary>
        /// Active particle effects.
        /// </summary>
        private BatchRemovalCollection<ParticleEffect> activeParticleEffects = 
            new BatchRemovalCollection<ParticleEffect>();


        #endregion


        #region Graphics Data


        /// <summary>
        /// The content manager used to load textures in the particle systems.
        /// </summary>
        private ContentManager contentManager;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Construct a new particle-effect manager.
        /// </summary>
        public ParticleEffectManager(ContentManager contentManager) 
        {
            // safety-check the parameters
            if (contentManager == null)
            {
                throw new ArgumentNullException("contentManager");
            }

            this.contentManager = contentManager;
        }


        #endregion


        #region Updating Methods

        
        /// <summary>
        /// Update the particle-effect manager.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public void Update(float elapsedTime)
        {
            for (int i = 0; i < activeParticleEffects.Count; ++i)
            {
                if (activeParticleEffects[i].Active)
                {
                    activeParticleEffects[i].Update(elapsedTime);
                    if (!activeParticleEffects[i].Active)
                    {
                        activeParticleEffects.QueuePendingRemoval(
                            activeParticleEffects[i]);
                    }
                }
            }
            activeParticleEffects.ApplyPendingRemovals();
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw all of the particle effects in the manager.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch object used to draw.</param>
        /// <param name="blendMode">Filters the systems drawn in this pass.</param>
        public virtual void Draw(SpriteBatch spriteBatch, SpriteBlendMode blendMode)
        {
            for (int i = 0; i < activeParticleEffects.Count; ++i)
            {
                if (activeParticleEffects[i].Active)
                {
                    activeParticleEffects[i].Draw(spriteBatch, blendMode);
                }
            }
        }


        #endregion


        #region Particle-Effect Creation Methods


        /// <summary>
        /// Spawn a new particle effect at a given location
        /// </summary>
        /// <param name="effectType">The effect in question.</param>
        /// <param name="position">The position of the effect.</param>
        /// <returns>The new particle effect.</returns>
        public ParticleEffect SpawnEffect(ParticleEffectType effectType, 
            Vector2 position)
        {
            return SpawnEffect(effectType, position, null);
        }


        /// <summary>
        /// Spawn a new particle effect at a the position of a given gameplay object
        /// </summary>
        /// <param name="effectType">The effect in question.</param>
        /// <param name="actor">The gameplay object.</param>
        /// <returns>The new particle effect.</returns>
        public ParticleEffect SpawnEffect(ParticleEffectType effectType,
            GameplayObject gameplayObject)
        {
            // safety-check the parameter
            if (gameplayObject == null)
            {
                throw new ArgumentNullException("gameplayObject");
            }

            return SpawnEffect(effectType, gameplayObject.Position, gameplayObject);
        }


        /// <summary>
        /// Spawn a new particle effect at a given location and gameplay object
        /// </summary>
        /// <param name="effectType">The effect in question.</param>
        /// <param name="position">The position of the effect.</param>
        /// <param name="actor">The gameplay object.</param>
        /// <returns>The new particle effect.</returns>
        public ParticleEffect SpawnEffect(ParticleEffectType effectType, 
            Vector2 position, GameplayObject gameplayObject)
        {
            ParticleEffect particleEffect = null;

            if (particleEffectCache.ContainsKey(effectType) == true)
            {
                List<ParticleEffect> availableSystems = particleEffectCache[effectType];

                for (int i = 0; i < availableSystems.Count; ++i)
                {
                    if (availableSystems[i].Active == false)
                    {
                        particleEffect = availableSystems[i];
                        break;
                    }
                }

                if (particleEffect == null)
                {
                    particleEffect = availableSystems[0].Clone();
                    particleEffect.Initialize(contentManager);
                    availableSystems.Add(particleEffect);
                }
            }

            if (particleEffect != null)
            {
                particleEffect.Reset();
                particleEffect.GameplayObject = gameplayObject;
                particleEffect.Position = position;
                activeParticleEffects.Add(particleEffect);
            }

            return particleEffect;
        }


        #endregion


        #region Registration Methods


        /// <summary>
        /// Register a new type of particle effect with the manager.
        /// </summary>
        /// <param name="effectType">The enumeration associated with this type.</param>
        /// <param name="filename">The path to the XML file to be deserialized.</param>
        /// <param name="initialCount">How many of these to pre-create.</param>
        public void RegisterParticleEffect(ParticleEffectType effectType, 
            string filename, int initialCount)
        {
            if (!particleEffectCache.ContainsKey(effectType))
            {
                string filepath = Path.Combine(contentManager.RootDirectory, filename);
                ParticleEffect particleEffect = ParticleEffect.Load(filepath);
                particleEffect.Initialize(contentManager);
                particleEffect.Stop(true);
                particleEffectCache.Add(effectType, new List<ParticleEffect>());
                particleEffectCache[effectType].Add(particleEffect);
                for (int i = 1; i < initialCount; i++)
                {
                    ParticleEffect cloneEffect = particleEffect.Clone();
                    cloneEffect.Initialize(contentManager);
                    cloneEffect.Stop(true);
                    particleEffectCache[effectType].Add(cloneEffect);
                }
            }
        }



        /// <summary>
        /// Remove the given particle-effect type from the maanger.
        /// </summary>
        /// <param name="effectType">The enumeration to be cleared against.</param>
        public void UnregisterParticleEffect(ParticleEffectType effectType)
        {
            if (particleEffectCache.ContainsKey(effectType) == true)
            {
                for (int i = 0; i < particleEffectCache[effectType].Count; ++i)
                {
                    activeParticleEffects.Remove(particleEffectCache[effectType][i]);
                }
                particleEffectCache.Remove(effectType);
            }
        }


        #endregion
    }
}
