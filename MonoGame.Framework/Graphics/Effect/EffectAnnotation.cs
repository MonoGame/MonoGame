using System;

namespace Microsoft.Xna.Framework.Graphics
{
    // TODO: This class needs to be finished!

    /// <summary>
    /// Represents an annotation to an <see cref="EffectParameter"/>.
    /// </summary>
	public class EffectAnnotation
	{
		internal EffectAnnotation (
			EffectParameterClass class_,
			EffectParameterType type,
			string name,
			int rowCount,
			int columnCount,
			string semantic,
			object data)
		{
			ParameterClass = class_;
			ParameterType = type;
			Name = name;
			RowCount = rowCount;
			ColumnCount = columnCount;
			Semantic = semantic;
		}

		internal EffectAnnotation (EffectParameter parameter)
		{
			ParameterClass = parameter.ParameterClass;
			ParameterType = parameter.ParameterType;
			Name = parameter.Name;
			RowCount = parameter.RowCount;
			ColumnCount = parameter.ColumnCount;
			Semantic = parameter.Semantic;
		}

        /// <summary>
        /// Gets the parameter class of this effect annotation.
        /// </summary>
		public EffectParameterClass ParameterClass {get; private set;}
        /// <summary>
        /// Gets the parameter type of this effect annotation.
        /// </summary>
		public EffectParameterType ParameterType {get; private set;}
        /// <summary>
        /// Gets the name of the effect annotation.
        /// </summary>
		public string Name {get; private set;}
        /// <summary>
        /// Gets the row count of this effect annotation.
        /// </summary>
		public int RowCount {get; private set;}
        /// <summary>
        /// Gets the number of columns in this effect annotation.
        /// </summary>
		public int ColumnCount {get; private set;}
        /// <summary>
        /// Gets the semantic of this effect annotation.
        /// </summary>
		public string Semantic {get; private set;}
	}
}