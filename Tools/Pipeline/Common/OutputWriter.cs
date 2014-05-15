using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MonoGame.Tools.Pipeline.Common
{
    internal class OutputWriter : TextWriter
    {
        private readonly IView _view;

        public OutputWriter(IView view)
        {
            _view = view;
        }

        public override void Write(string value)
        {
            _view.OutputAppend(value);
        }

        public override void WriteLine(string value)
        {
            _view.OutputAppendLine(value);
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
