using System;

namespace Microsoft.Xna.Framework.Graphics
{
    // TODO: This class needs to be finished!

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

		public EffectParameterClass ParameterClass {get; private set;}
		public EffectParameterType ParameterType {get; private set;}
		public string Name {get; private set;}
		public int RowCount {get; private set;}
		public int ColumnCount {get; private set;}
		public string Semantic {get; private set;}
	}
}

