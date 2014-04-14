using System;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.DesignerSupport;
using System.ComponentModel;
using System.Globalization;


namespace MonoDevelop.MonoGameContent
{
	public class ContentPropertyProvider : IPropertyProvider
	{
		
		#region Constructor
		
		public ContentPropertyProvider ()
		{
		}
		
		#endregion Constructor
		
		#region IPropertyProvider Implementation
		
		public bool SupportsObject (object o)
		{
			ProjectFile file = o as ProjectFile;
			if (file != null)
				return file.Project is MonoGameContentProject;// || file.Project is ContentProject;

			return false;
		}
		
		public object CreateProvider (object o)
		{
			return new ContentFileWrapper ((ProjectFile)o);
		}	

		#endregion IPropertyProvider Implementation
		
		#region Nested ContentFileWrapper Class
		
		public class ContentFileWrapper: CustomDescriptor
		{
			#region Private Fields
			
			const string nameKey = "Name";
			const string importerKey = "Importer";
			const string processorKey = "Processor";
			
			ProjectFile file;

			#endregion Private Fields
			
			#region Constructor
			
			public ContentFileWrapper (ProjectFile file)
			{
				this.file = file;
			}
			
			#endregion Constructor
			
			#region Properties
			
			[LocalizedCategory("MonoGame")]
			[LocalizedDisplayName("Name")]
			[LocalizedDescription("The name of the content.")]
			public string Name {
				get {
					object result = file.ExtendedProperties[nameKey];
					return result == null ? "" : (string)result;
				}
				set { file.ExtendedProperties[nameKey] = value; }
			}
			
			[LocalizedCategory("MonoGame")]
			[LocalizedDisplayName("Importer")]
			[LocalizedDescription("The importer to use when reading the content file.")]
			[TypeConverter(typeof(ImporterStringsConverter))]
			public string Importer {
				get {
					object result = file.ExtendedProperties[importerKey];
					return result == null ? "" : (string)result;
				}
				set { file.ExtendedProperties[importerKey] = value; }
			}
			
			[LocalizedCategory("MonoGame")]
			[LocalizedDisplayName("Processor")]
			[LocalizedDescription("The processor to use when building the content file.")]
			[TypeConverter(typeof(ProcessorStringsConverter))]
			public string Processor {
				get {
					object result = file.ExtendedProperties[processorKey];
					return result == null ? "" : (string)result;
				}
				set { file.ExtendedProperties[processorKey] = value; }
			}

			#endregion Properties

			#region Nested ImporterStringConverter Class
			
			[MonoDevelop.Components.PropertyGrid.PropertyEditors.StandardValuesSeparator("--")]
			class ImporterStringsConverter : ContentStringsConverter
			{
				#region TypeConverter Overrides
				
				public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
				{
					ContentFileWrapper wrapper = context != null ? context.Instance as ContentFileWrapper : null;
					if (wrapper != null && wrapper.file != null)
					{
						MonoGameContentProject project = wrapper.file.Project as MonoGameContentProject;
						if (project != null)
							return new StandardValuesCollection(project.GetImporterNames());
					}
					return new StandardValuesCollection(null);
				}
				
				public override bool IsValid (ITypeDescriptorContext context, object value)
				{
					if (!(value is string))
						return false;
					string str = value as string;
					ContentFileWrapper wrapper = context != null ? context.Instance as ContentFileWrapper : null;
					if (wrapper != null && wrapper.file != null)
					{
						MonoGameContentProject project = wrapper.file.Project as MonoGameContentProject;
						if (project != null)
						{
							return project.IsImporterNameValid(str);
						}
					}
					return false;
				}
				
				#endregion TypeConverter Overrides
			}
			
			#endregion Nested ImporterStringsConverter Class
			
			#region Nested ProcessorStringsConverter Class
			
			[MonoDevelop.Components.PropertyGrid.PropertyEditors.StandardValuesSeparator("--")]
			class ProcessorStringsConverter : ContentStringsConverter
			{
				#region TypeConverter Overrides
				
				public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
				{
					ContentFileWrapper wrapper = context != null ? context.Instance as ContentFileWrapper : null;
					if (wrapper != null && wrapper.file != null)
					{
						MonoGameContentProject project = wrapper.file.Project as MonoGameContentProject;
						if (project != null)
							return new StandardValuesCollection(project.GetProcessorNames());
					}
					return new StandardValuesCollection(null);
				}
				
				public override bool IsValid (ITypeDescriptorContext context, object value)
				{
					if (!(value is string))
						return false;
					string str = value as string;
					ContentFileWrapper wrapper = context != null ? context.Instance as ContentFileWrapper : null;
					if (wrapper != null && wrapper.file != null)
					{
						MonoGameContentProject project = wrapper.file.Project as MonoGameContentProject;
						if (project != null)
						{
							return project.IsProcessorNameValid(str);
						}
					}
					return false;
				}
				
				#endregion TypeConverter Overrides
			}
			
			#endregion Nested ProcessorStringsConverter Class
			
		}
		
		#endregion Nested ContentFileWrapper Class		
	}
}

