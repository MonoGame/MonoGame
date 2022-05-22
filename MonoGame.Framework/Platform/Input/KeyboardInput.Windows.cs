using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class KeyboardInput
    {
        private static Form _dialog;
        private static TaskCompletionSource<string> _tcs;
        
        private static Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
        {
            _tcs = new TaskCompletionSource<string>();

            var parent = Application.OpenForms[0];

            parent.Invoke(new MethodInvoker(() =>
            {
                var dialog = _dialog = new Form();
                dialog.Text = title;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ControlBox = false;
                dialog.StartPosition = FormStartPosition.CenterParent;

                var desc = new Label();
                desc.Text = description;
                desc.Parent = dialog;
                desc.Top = 25;
                desc.TextAlign = ContentAlignment.MiddleCenter;
                desc.AutoSize = true;
                desc.Margin = new Padding(25, 0, 25, 0);
                desc.Left = (desc.Parent.ClientSize.Width - desc.Width) / 2;

                var input = new TextBox();
                input.Text = defaultText;
                input.Parent = dialog;
                input.Top = desc.Bottom + 15;
                input.UseSystemPasswordChar = usePasswordMode;
                input.AutoSize = true;
                input.Margin = new Padding(25, 0, 25, 0);
                input.Left = 25;
                input.Width = input.Parent.ClientSize.Width - 25;
                input.TabIndex = 0;

                var bgroup = new FlowLayoutPanel();
                bgroup.FlowDirection = FlowDirection.LeftToRight;
                bgroup.Parent = dialog;
                bgroup.Top = input.Bottom + 15;
                bgroup.AutoSize = true;
                bgroup.Margin = new Padding(15);
                bgroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;

                var button = new Button();
                button.Text = "&Ok";
                button.DialogResult = DialogResult.OK;
                button.Parent = bgroup;
                button.TabIndex = 1;
                dialog.AcceptButton = button;

                button = new Button();
                button.Text = "&Cancel";
                button.DialogResult = DialogResult.Cancel;
                button.Parent = bgroup;
                button.TabIndex = 2;
                dialog.CancelButton = button;

                bgroup.Left = (bgroup.Parent.ClientSize.Width - bgroup.Width) / 2;
                dialog.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                dialog.AutoSize = true;

                var result = dialog.ShowDialog(parent);
                _dialog = null;

                if (_tcs.Task.IsCompleted)
                    return;

                if (result == DialogResult.OK)
                    _tcs.SetResult(input.Text);
                else
                    _tcs.SetResult(null);

            }));

            return _tcs.Task;
        }

        private static void PlatformCancel(string result)
        {
            if (_dialog != null)
                _dialog.Close();
            _tcs.SetResult(result);
        }
    }
}
