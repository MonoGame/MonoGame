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

