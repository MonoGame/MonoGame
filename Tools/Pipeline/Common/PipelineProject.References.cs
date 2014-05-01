
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace MonoGame.Tools.Pipeline
{
    /// <summary>
    /// Custom converter for the References property of a PipelineProject.
    /// </summary>
    internal class AssemblyReferenceListConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof (string))
                return true;

            return false;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof (string))
                return true;

            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            var words = ((string)value).Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            return new List<string>(words);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            return string.Join("\n", (List<string>)value);
        }
    }

    /// <summary>
    /// Custom editor for a the References property of a PipelineProject.
    /// </summary>
    public class AssemblyReferenceListEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            var svc = provider.GetService(typeof (IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            var lines = (List<string>)value;
            if (svc != null && lines != null)
            {
                using (var form = new AssemblyReferenceListEditForm())
                {
                    form.Lines = lines.ToArray();
                    if (svc.ShowDialog(form) == DialogResult.OK)
                    {
                        lines = new List<string>(form.Lines);
                    }
                }
            }

            return lines;            
        }
    }

    /// <summary>
    /// Modal dialog for editing individual lines within PipelineProject.References.
    /// </summary>
    public class AssemblyReferenceListEditForm : Form
    {
        private readonly RichTextBox _textbox;
        private readonly Button _okButton;

        public AssemblyReferenceListEditForm()
        {            
            StartPosition = FormStartPosition.CenterScreen;

            _textbox = new RichTextBox()
                {
                    Dock = DockStyle.Fill,
                };
            Controls.Add(_textbox);

            _okButton = new Button
                {
                    Text = "OK",
                    Dock = DockStyle.Bottom,
                    DialogResult = DialogResult.OK
                };
            Controls.Add(_okButton);
        }

        public string[] Lines
        {
            get { return _textbox.Lines; }
            set 
            { 
                _textbox.Lines = value;
                var size = TextRenderer.MeasureText(_textbox.Text, _textbox.Font, _textbox.Size, TextFormatFlags.Internal);
                Size = size;
            }
        }
    }
}
