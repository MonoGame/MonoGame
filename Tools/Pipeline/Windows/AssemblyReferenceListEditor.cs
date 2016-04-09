using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace MonoGame.Tools.Pipeline
{
    /// <summary>
    /// Custom editor for a the References property of a PipelineProject.
    /// </summary>    
    public class AssemblyReferenceListEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            var lines = (List<string>)value;
            if (svc != null && lines != null)
            {
                var dialog = new ReferenceDialog((PipelineController)MainView._controller, lines.ToArray());

                if (dialog.Run() == Eto.Forms.DialogResult.Ok)
                {
                    lines = dialog.References;
                    MainView._controller.OnProjectModified();
                }
            }

            return lines;
        }
    }
}
