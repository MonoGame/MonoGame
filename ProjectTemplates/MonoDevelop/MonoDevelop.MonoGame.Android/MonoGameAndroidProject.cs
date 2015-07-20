using System;

namespace MonoDevelop.MonoGame.Android
{
	public class MonoGameAndroidProject : MonoDevelop.MonoDroid.MonoDroidProject
	{
		public MonoGameAndroidProject () : base()
		{
		}

		public override MonoDevelop.Projects.SolutionItemConfiguration CreateConfiguration (string name)
		{
			return base.CreateConfiguration (name);
		}

		protected override void PopulateSupportFileList (MonoDevelop.Projects.FileCopySet list, MonoDevelop.Projects.ConfigurationSelector configuration)
		{
			base.PopulateSupportFileList (list, configuration);
		}

		[Obsolete]
		public override string ProjectType {
			get {
				return "MonoGameAndroid";
			}
		}
	}
}

