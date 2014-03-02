using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class MainView : Form, IView, IModelObserver
    {
        IController _controller;

        public MainView()
        {
            InitializeComponent();
        }

        public event SelectionChanged OnSelectionChanged;

        public void Attach(IController controller)
        {
            _controller = controller;
        }
    }
}
