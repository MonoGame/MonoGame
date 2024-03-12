// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{

	public sealed class DirectionalLight
	{
		internal EffectParameter diffuseColorParameter;
		internal EffectParameter directionParameter;
		internal EffectParameter specularColorParameter;
		
		Vector3 diffuseColor;
		Vector3 direction;
		Vector3 specularColor;
		bool enabled;

        /// <summary>
        /// Creates a new instance of the DirectionalLight class with or without a copy of a DirectionalLight instance.
        /// </summary>
        /// <param name="directionParameter">The light direction.</param>
        /// <param name="diffuseColorParameter">The diffuse color.</param>
        /// <param name="specularColorParameter">The specular color.</param>
        /// <param name="cloneSource">The cloned source to copy from.</param>
        /// <remarks>
        /// The initial parameter values are either copied from the cloned object or set to default values (if the
        /// coned object is null).  The three Effect Parameters are updated whenever the direction, diffuse color, or
        /// specular color properties are changed; or you can set these to null if you are using the cloned object.
        /// </remarks>
		public DirectionalLight (EffectParameter directionParameter, EffectParameter diffuseColorParameter, EffectParameter specularColorParameter, DirectionalLight cloneSource)
		{
			this.diffuseColorParameter = diffuseColorParameter;
			this.directionParameter = directionParameter;
			this.specularColorParameter = specularColorParameter;
			if (cloneSource != null) {
				this.diffuseColor = cloneSource.diffuseColor;
				this.direction = cloneSource.direction;
				this.specularColor = cloneSource.specularColor;
				this.enabled = cloneSource.enabled;
			} else {
				this.diffuseColorParameter = diffuseColorParameter;
				this.directionParameter = directionParameter;
				this.specularColorParameter = specularColorParameter;
			}
		}

        /// <summary>
        /// Gets or Sets the diffuse color of the light.
        /// </summary>
		public Vector3 DiffuseColor {
			get {
				return diffuseColor;
			}
			set {
				diffuseColor = value;
				if (this.enabled && this.diffuseColorParameter != null)
					diffuseColorParameter.SetValue (diffuseColor);
			}
		}

        /// <summary>
        /// Gets or Sets the light direction.
        /// </summary>
        /// <remarks>
        /// This value must be a unit vector.
        /// </remarks>
		public Vector3 Direction {
			get {
				return direction;
			}
			set {
				direction = value;
				if (this.directionParameter != null)
					directionParameter.SetValue (direction);
			}
		}
		
		public Vector3 SpecularColor {
			get {
				return specularColor;
			}
			set {
				specularColor = value;
				if (this.enabled && this.specularColorParameter != null)
					specularColorParameter.SetValue (specularColor);
			}
		}
		public bool Enabled 
		{
			get { return enabled; }
			set 
			{
				if (this.enabled != value)
				{
				    this.enabled = value;
				    if (this.enabled)
				    {
				        if (this.diffuseColorParameter != null)
				        {
				            this.diffuseColorParameter.SetValue(this.diffuseColor);
				        }
				        if (this.specularColorParameter != null)
				        {
				            this.specularColorParameter.SetValue(this.specularColor);
				        }
				    }
				    else
				    {
				        if (this.diffuseColorParameter != null)
				        {
				            this.diffuseColorParameter.SetValue(Vector3.Zero);
				        }
				        if (this.specularColorParameter != null)
				        {
				            this.specularColorParameter.SetValue(Vector3.Zero);
				        }
				    }
				}

			}
		}
	}
}

