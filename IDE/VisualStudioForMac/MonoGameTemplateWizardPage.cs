using System;
using Eto;
using Eto.Forms;

namespace MonoGame.IDE.VisualStudioForMac
{
	public class MonoGameTemplateWizardPage : MonoDevelop.Ide.Templates.WizardPage
	{
		public override string Title => "Configure your MonoGame Project";

		readonly MonoGameTemplateWizard wizard;
		TemplateWizard.ProjectSelectionPanel view;

		public MonoGameTemplateWizardPage(MonoGameTemplateWizard wizard)
		{
			this.wizard = wizard;
		}

		protected override object CreateNativeWidget<T>()
		{
			if (view == null)
			{
				view = new TemplateWizard.ProjectSelectionPanel();
				view.OnUpdateOptions += (s, e) => {
					wizard.Parameters["Flags"] = ((int)view.Flags).ToString();
					wizard.Parameters["CodeSharing"] = ((int)view.CodeSharingOption).ToString();
					wizard.Parameters["ReferenceOption"] = ((int)view.ReferenceOption).ToString();
				};
			}
			if (Platform.Instance.IsMac)
				return XamMac2Helpers.ToNative(view, true);
			else
				return Gtk2Helpers.ToNative(view, true);
		}

		protected override void Dispose(bool disposing)
		{
			if (view != null)
			{
				view.Dispose();
			}
		}
	}
}
