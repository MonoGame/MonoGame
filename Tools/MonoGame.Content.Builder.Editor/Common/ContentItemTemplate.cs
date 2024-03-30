// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Tools.Pipeline
{
    public class ContentItemTemplate
    {
        public string Label;
        public string Icon;
        public string ImporterName;
        public string ProcessorName;
        public string TemplateFile;

        public override string ToString()
        {
            return Label;
        }
    }
}