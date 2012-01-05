using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectAnnotationCollection : IEnumerable<EffectAnnotation>
	{
		internal List<EffectAnnotation> _annotations;
		public EffectAnnotationCollection ()
		{
		}
		
		public int Count {
			get { return _annotations.Count; }
		}
		
		public EffectAnnotation this[int index]
        {
            get { return _annotations[index]; }
            internal set { _annotations[index] = value; }
        }
		
		public EffectAnnotation this[string name]
        {
            get {
				foreach (EffectAnnotation annotation in _annotations) {
					if (annotation.Name == name) {
						return annotation;
					}
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
	}
}

