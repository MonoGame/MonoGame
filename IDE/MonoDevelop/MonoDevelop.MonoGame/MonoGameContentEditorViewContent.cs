using System;
using MonoDevelop.Ide.Gui;
using Gtk;
using System.IO;

namespace MonoDevelop.MonoGame
{
	#if BUILDINEDITOR
	class MonoGameContentEditorViewContent : AbstractBaseViewContent, IViewContent
	{
		Alignment control;
		Pipeline.MacOS.MainView view;

		public event EventHandler ContentNameChanged;
		public event EventHandler ContentChanged;
		public event EventHandler DirtyChanged;
		public event EventHandler BeforeSave;
		public void Load (string fileName)
		{
			view.Load (fileName);
		}
		public void LoadNew (Stream content, string mimeType)
		{
			view.LoadNew (content, mimeType);
		}
		public void Save (string fileName)
		{
			view.Save (fileName);
		}
		public void Save ()
		{
			view.Save ();
		}
		public void DiscardChanges ()
		{
			view.Undo ();
		}
		public MonoDevelop.Projects.Project Project {
			get ;
			set;
		}
		public string PathRelativeToProject {
			get {
				return string.Empty;
			}
		}
		public string ContentName {
			get;
			set;
		}
		public string UntitledName {
			get;
			set;
		}
		public string StockIconId {
			get {
				return "monogame-project";
			}
		}
		public bool IsUntitled {
			get {
				return false;
			}
		}
		public bool IsViewOnly {
			get {
				return true;
			}
		}
		public bool IsFile {
			get {
				return true;
			}
		}
		public bool IsDirty {
			get { return view.IsDirty; }
			set { view.IsDirty = value; }
		}
		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public override Gtk.Widget Control {
			get {
				return control;
			}
		}

		public MonoGameContentEditorViewContent (MonoDevelop.Core.FilePath filename, MonoDevelop.Projects.Project project)
		{
			this.ContentName = Path.GetFileName (filename.ToString());
			this.Project = project;
			control = new Alignment (0, 0, 1, 1);
			control.SetPadding (5, 5, 5, 5);

			view = new Pipeline.MacOS.MainView (null);

			if (filename != null) {
				view.OpenProjectPath = filename.ToString();
			}
			Pipeline.MacOS.MainView.CreateControllers (view);
			view.BuildUI ();

			control.Add (view);

			control.ShowAll ();
		}

	}
	#endif
}

