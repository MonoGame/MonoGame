using System;
using System.Reflection;
using Xwt;

namespace MonoGame.Tools.Pipeline
{
	public class PropertyGrid : VBox
	{
		ToggleButton _catButton;
		ToggleButton _alphButton;

		VBox _propertyList;

		//SortStyle _sorted;

		object _propertyObject;

		public PropertyGrid()
		{
			CreateButtonBar();


			_propertyList = new VBox();

			var scrollView = new ScrollView() {
				BorderVisible = true,
				Content = _propertyList,
				HorizontalScrollPolicy = ScrollPolicy.Never,
				VerticalScrollPolicy = ScrollPolicy.Always,
				ExpandHorizontal = true,
			};

			this.PackStart(scrollView, true);

//			if (_propertyObject != null)
//				PopulatePropertyList();
			//PopulateMockList();
		}

		public object PropertyObject {			get;			set;		}

		enum SortStyle 
		{
			Categories,
			Alphabetic
		}

		void CreateButtonBar ()
		{
			var buttonBar = new HBox();
			_catButton = new SortButton("C");
			buttonBar.PackStart(_catButton);
			_alphButton = new SortButton("A");
			buttonBar.PackStart(_alphButton);
			this.PackStart(buttonBar);

			_catButton.Clicked += (sender, e) => SelectedSort(SortStyle.Categories);
			_alphButton.Clicked += (sender, e) => SelectedSort(SortStyle.Alphabetic);
		}

		void SelectedSort(SortStyle style) {
//			_sorted = style;
//
//			switch (style) {
//			case SortStyle.Alphabetic:
//				_catButton.Active = false;
//				_alphButton.Active = true;
//				break;
//			case SortStyle.Categories:
//				_alphButton.Active = false;
//				_catButton.Active = true;
//				break;
//			default:
//				// Should not enter here.
//				break;
//			}
		}

		void PopulateMockList() {

			for (int i = 0; i < 10; i++) {
				var firstExpander = new Expander() {
					Label = "Category 1",
					Expanded = true,
					ExpandHorizontal = true,
					MarginLeft = 6
				};

				int currentLine = 0;

				var firstTable = new Table() {
					DefaultRowSpacing = 1
				};

				addEntry (firstTable, currentLine++, "Name", EntryType.Text);
				addEntry (firstTable, currentLine++, "Type", EntryType.Readonly);
				addEntry (firstTable, currentLine++, "Combo", EntryType.Combo);
				addEntry (firstTable, currentLine++, "Color (RGBA)", EntryType.Color);
				addEntry (firstTable, currentLine++, "Content", EntryType.LongText);
				addEntry (firstTable, currentLine++, "Active", EntryType.Check);

				firstExpander.Content = firstTable;
				_propertyList.PackStart(firstExpander);
			}
		}

		enum EntryType {
			Text,
			Readonly,
			LongText,
			Color,
			Combo,
			Check
		}

		void addEntry(Table table, int line, string label, EntryType type) {
			table.Add(new Label(label), 0, line, hpos: WidgetPlacement.End, hexpand: true);

			Widget entry;
			switch (type) {
			case EntryType.Text:
				entry = new TextEntry () { Text = "Name" };
				break;
			case EntryType.Readonly:
				entry = new TextEntry () {
					Text = "Obj",
					Sensitive = false
				};
				break;
			case EntryType.LongText:
				entry = new LongTextEntry ("this is a long text");
				break;
			case EntryType.Color:
				entry = new ColorEntry ();
				break;
			case EntryType.Combo:
				var cbe = new ComboBoxEntry ();
				cbe.Items.Add ("One");
				cbe.Items.Add ("Two");
				cbe.Items.Add ("Three");
				//cbe.SelectedIndex = 0;
				entry = cbe;
				break;
			case EntryType.Check:
				entry = new CheckBox () {
					Active = true
				};
				break;
			default:
				// Shouldn't get here
				entry = null;
				break;
			}

			table.Add (entry, 1, line, hpos: WidgetPlacement.Fill, hexpand: true);
		}

		void PopulatePropertyList() {
			Type typeOfObject = _propertyObject.GetType ();

			foreach (var prop in typeOfObject.GetProperties()) {
				//TODO List the properties
			}
		}
	}

	class DialogEntry : HBox {

		protected TextEntry _textEntry;
		protected Button _button;

		public DialogEntry(string text = "") {
			_textEntry = new TextEntry();
			_button = new Button(){
				Style = ButtonStyle.Flat,
				Label = "..."
			};

			PackEnd(_button);
			PackStart(_textEntry, true);

		}

		public string Text {
			get { return _textEntry.Text; }
			set { _textEntry.Text = value; }
		}
	}

	class ColorEntry : DialogEntry {
		public ColorEntry() : base() {
			_button.Clicked += ShowColorDialog;
		}

		void ShowColorDialog (object sender, EventArgs e)
		{
			var colorDialog = new SelectColorDialog() {
				SupportsAlpha = true,
				// FIXME Uncomment the next line when updated with Xwt
				//Color = Xwt.Drawing.Color.FromHex(Text);
			};

			if (colorDialog.Run ()) {
				var color = colorDialog.Color;

				// FIXME Use the next line instead when updated with Xwt
				// _textEntry.Text = color.ToHexString ().ToUpper ();
				_textEntry.Text = "#"
					+ ((int)(color.Red * 255)).ToString ("X2")
					+ ((int)(color.Green * 255)).ToString ("X2")
					+ ((int)(color.Blue * 255)).ToString ("X2")
					+ ((int)(color.Alpha * 255)).ToString ("X2");
			}
		}
	}

	class LongTextEntry : DialogEntry {
		public LongTextEntry(string text = "") : base(text) {
			_button.Clicked += ShowDialog;
			_textEntry.Text = text;
		}

		void ShowDialog (object sender, EventArgs e)
		{
			var dialog = new LongTextDialog (_textEntry.Text);

			if (dialog.Run () == Command.Ok)
				_textEntry.Text = dialog.Text;
			dialog.Hide ();
		}
	}

	class LongTextDialog : Dialog {

		TextEntry _textEntry;

		public LongTextDialog(string text = "") : base() {
			// TODO See if the Xwt corrects the multiline bug on GTK
			_textEntry = new TextEntry () {
				MultiLine = true,
				MinHeight = 100,
				MinWidth = 200,
				Text = text
			};

			Content = _textEntry;

			var okButton = new DialogButton ("OK", Command.Ok);
			var cancelButton = new DialogButton("Cancel", Command.Cancel);

			Buttons.Add (okButton);
			Buttons.Add (cancelButton);
		}

		public string Text {
			get { return _textEntry.Text; }
		}
	}

	class SortButton : ToggleButton
	{
		public SortButton(string label) {
			Label = label;

			Margin = new WidgetSpacing(2, 2, 2, 2);
			HeightRequest = 24;
			WidthRequest = 24;
		}
	}
}

