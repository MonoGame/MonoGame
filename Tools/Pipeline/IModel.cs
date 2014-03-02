using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGame.Tools.Pipeline
{
    interface IModel
    {
        void Attach(IModelObserver observer);

        void NewProject();

        void OpenProject(string filePath);

        void CloseProject();
    }
}
