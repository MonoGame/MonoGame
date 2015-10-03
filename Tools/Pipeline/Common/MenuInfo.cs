using System;

namespace MonoGame.Tools.Pipeline
{
    public class MenuSensitivity
    {
        public bool New { get; set; }

        public bool Open { get; set; }

        public bool Close { get; set; }

        public bool Import { get; set; }

        public bool Save { get; set; }

        public bool SaveAs { get; set; }

        public bool Exit { get; set; }

        public bool Undo { get; set; }

        public bool Redo { get; set; }

        public bool Add { get; set; }

        public bool Rename { get; set; }

        public bool Delete { get; set; }

        public bool BuildMenu { get; set; }

        public bool Build { get; set; }

        public bool Rebuild { get; set; }

        public bool Clean { get; set; }

        public bool Cancel { get; set; }

        public bool Debug { get; set; }
    }

    public class ContextMenuVisibility
    {
        public bool Open { get; set; }

        public bool Add { get; set; }

        public bool OpenFileLocation { get; set; }

        public bool Rebuild { get; set; }

        public bool Rename { get; set; }

        public bool Delete { get; set; }
    }
}

