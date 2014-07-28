using System;
using MonoDevelop.Projects;
using MonoGame.Framework.Content.Pipeline.Builder;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Collections.Generic;

namespace MonoDevelop.MonoGameContent
{
	public class MonoGameContentBuildExtension : ProjectServiceExtension
	{

		public MonoGameContentBuildExtension ()
		{
			
		}
		
		protected override BuildResult Build (MonoDevelop.Core.IProgressMonitor monitor, SolutionEntityItem item, ConfigurationSelector configuration)
		{
#if DEBUG			
			monitor.Log.WriteLine("MonoGame Extension Build Called");	
#endif			
			try
			{
				return base.Build (monitor, item, configuration);
			}
			finally
			{
#if DEBUG				
				monitor.Log.WriteLine("MonoGame Extension Build Ended");	
#endif				
			}
		}

		protected override void Clean (MonoDevelop.Core.IProgressMonitor monitor, SolutionEntityItem item, ConfigurationSelector configuration)
		{
			var proj = item as MonoGameContentProject;
			MonoGameContentProjectConfiguration cfg = null;
			if (proj != null)
				cfg = proj.GetConfiguration (configuration) as MonoGameContentProjectConfiguration;
			if (proj == null) {
				monitor.Log.WriteLine("Cleaning for Unknown MonoGame Project");
				base.Clean (monitor, item, configuration);
			}
			var manager = new PipelineManager(proj.BaseDirectory.FullPath,
			                                  Path.Combine(cfg.OutputDirectory, cfg.MonoGamePlatform),
			                                  cfg.IntermediateOutputDirectory);
			monitor.Log.WriteLine("Detected {0} MonoGame Platform", cfg.MonoGamePlatform);                              
			foreach(var file in proj.Files)
			{
				if (file.BuildAction == "Compile") {
					try {
						monitor.Log.WriteLine("Cleaning {0}", file.FilePath.FileName);
						manager.CleanContent(file.FilePath.FullPath, null);
					}
					catch(Exception ex)
					{
						monitor.Log.WriteLine(ex.Message);
					}
				}
			}
			base.Clean (monitor, item, configuration);
		}

		protected override BuildResult Compile (MonoDevelop.Core.IProgressMonitor monitor, SolutionEntityItem item, BuildData buildData)
		{
#if DEBUG			
			monitor.Log.WriteLine("MonoGame Extension Compile Called");	
#endif			
			try
			{	
				var cfg = buildData.Configuration as MonoGameContentProjectConfiguration;
				var proj = item as MonoGameContentProject;
				if (proj == null || cfg == null)
				{
					monitor.Log.WriteLine("Compiling for Unknown MonoGame Project");
					return base.Compile(monitor, item, buildData);
				}
				monitor.Log.WriteLine("Detected {0} MonoGame Platform", cfg.MonoGamePlatform);
				var result = new BuildResult();
				var manager = new PipelineManager(proj.BaseDirectory.FullPath,
				                                  Path.Combine(buildData.Configuration.OutputDirectory, cfg.MonoGamePlatform),
				                                  buildData.Configuration.IntermediateOutputDirectory);
			
				manager.Logger = new MonitorBuilder(monitor, result);
				manager.Platform =  (TargetPlatform)Enum.Parse(typeof(TargetPlatform), cfg.MonoGamePlatform);
				try {
				foreach(var pr in proj.ParentSolution.GetAllProjects()) {
					if (pr is DotNetAssemblyProject) {
						var dot = pr as DotNetAssemblyProject;
						foreach(var r in dot.References)
						{
							if (r.Package.Name == "monogame-contentpipeline" ) {

								var output = dot.GetOutputFileName(buildData.ConfigurationSelector).FullPath;
								monitor.Log.WriteLine("Adding {0} to Content Pipeline Assembly List", output);
								manager.AddAssembly(output);
								proj.Manager.AddAssembly(output);
								break;
							}
						}
					}
				}
				}
				catch(Exception ex) {
					result.AddWarning(string.Format("Problem processing Content Extensions {0}", ex.Message));
				}

				var dict = new Microsoft.Xna.Framework.Content.Pipeline.OpaqueDataDictionary();
				if (cfg != null) {
					if (cfg.XnaCompressContent.ToLowerInvariant() == "true") {
						// tell the manager to compress the output
					}
				}
				foreach(var file in proj.Files)
				{
					if (file.BuildAction == "Compile") {
						try 
						{
							dict.Clear();
							foreach(object key in file.ExtendedProperties.Keys) {
								string k = key as string;
								if (k != null && k.StartsWith("ProcessorParameters_")) {
									if (!dict.ContainsKey(k.Replace("ProcessorParameters_", String.Empty))) {
										dict.Add(k.Replace("ProcessorParameters_", String.Empty), file.ExtendedProperties[k]);
									} else {
										dict[k.Replace("ProcessorParameters_", String.Empty)] = file.ExtendedProperties[k];
									}
								}
							}

							// check if the file has changed and rebuild if required.
							manager.BuildContent(file.FilePath.FullPath, 
					                     null, 
							             file.ExtendedProperties.Contains("Importer") ? (string)file.ExtendedProperties["Importer"] : null,
							             file.ExtendedProperties.Contains("Processor") ? (string)file.ExtendedProperties["Processor"] : null, 
							             dict);
						}
						catch(Exception ex)
						{
							monitor.Log.WriteLine(ex.ToString());
							result.AddError(ex.Message);
						}

					}
				}
				return result;
			}
			finally
			{
#if DEBUG				
				monitor.Log.WriteLine("MonoGame Extension Compile Ended");	
#endif				
			}
		}

	}

	public class MonitorBuilder : Microsoft.Xna.Framework.Content.Pipeline.ContentBuildLogger
	{
		MonoDevelop.Core.IProgressMonitor monitor;
		BuildResult result;

		public MonitorBuilder (MonoDevelop.Core.IProgressMonitor monitor, BuildResult result)
		{
			this.monitor = monitor;
			this.result = result;
		}

		public override void LogImportantMessage (string message, params object[] messageArgs)
		{
			monitor.Log.WriteLine (string.Format (message, messageArgs));
			if (result != null)
				result.AddError(string.Format (message, messageArgs));
		}

		public override void LogMessage (string message, params object[] messageArgs)
		{
			monitor.Log.WriteLine (string.Format (message, messageArgs));
		}

		public override void LogWarning (string helpLink, Microsoft.Xna.Framework.Content.Pipeline.ContentIdentity contentIdentity, string message, params object[] messageArgs)
		{
			var msg = string.Format(message, messageArgs);
			var fileName = GetCurrentFilename(contentIdentity);
			monitor.Log.WriteLine(string.Format("{0}: {1}", fileName, msg));
			if (result != null)
				result.AddWarning(string.Format("{0}: {1}", fileName, msg));
		}
	}
}

