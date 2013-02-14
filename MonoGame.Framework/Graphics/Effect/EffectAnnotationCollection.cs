using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectAnnotationCollection : IEnumerable<EffectAnnotation>
	{
        List<EffectAnnotation> _annotations = new List<EffectAnnotation>();
		
		public int Count 
        {
			get { return _annotations.Count; }
		}
		
		public EffectAnnotation this[int index]
        {
            get { return _annotations[index]; }
        }
		
		public EffectAnnotation this[string name]
        {
            get 
            {
				foreach (var annotation in _annotations) 
                {
					if (annotation.Name == name)
						return annotation;
				}
				return null;
			}
        }
		
		public IEnumerator<EffectAnnotation> GetEnumerator()
        {
            return _annotations.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _annotations.GetEnumerator();
        }

        internal void Add(EffectAnnotation annotation)
        {
            _annotations.Add(annotation);
        }
	}
}

