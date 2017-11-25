using System;
using Eto;
using Eto.Forms;
using MonoDevelop.Core.Assemblies;
using MonoDevelop.Ide.Templates;

namespace MonoGame.IDE.VisualStudioForMac
{
	public class MonoGameTemplateWizard : MonoDevelop.Ide.Templates.TemplateWizard
	{
		public override string Id => "MonoGame.IDE.VisualStudioForMac.TemplateWizard";

		public override WizardPage GetPage(int pageNumber)
		{
			return new MonoGameTemplateWizardPage (this);
		}

		static MonoGameTemplateWizard()
		{
			EtoInitializer.Initialize();
		}

		public override void ItemsCreated(System.Collections.Generic.IEnumerable<MonoDevelop.Projects.IWorkspaceFileObject> items)
		{
			base.ItemsCreated(items);
		}
	}

	public static class EtoInitializer
	{
		static readonly string AddinPlatform = Eto.Platforms.Gtk2;

		public static void Initialize()
		{
			if (Platform.Instance == null)
			{
				try
				{
					new Eto.Forms.Application(AddinPlatform).Attach();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"{ex}");
				}
				if (EtoEnvironment.Platform.IsMac)
				{
					var plat = Platform.Instance;
					if (!plat.IsMac)
					{
						// use some Mac handlers even when using Gtk platform as base
						plat.Add<Cursor.IHandler>(() => new Eto.Mac.Forms.CursorHandler());
					}
				}
			}
		}
	}
}
