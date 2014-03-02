// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Tools.Pipeline
{
    public delegate void SelectionChanged();

    public enum AskResult
    {
        Yes,
        No,
        Cancel
    }

    public interface IView
    {
        event SelectionChanged OnSelectionChanged;

        void Attach(IController controller);

        AskResult AskSaveOrCancel();

        bool AskSaveName(out string filePath);
    }
}
