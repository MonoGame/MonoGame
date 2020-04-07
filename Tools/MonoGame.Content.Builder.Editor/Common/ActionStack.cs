// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;

namespace MonoGame.Tools.Pipeline
{        
    /// <summary>
    /// Represents a stack of undo/redo-able actions.
    /// </summary>
    public class ActionStack
    {
        private readonly PipelineController _controller;
        private readonly List<IProjectAction> _undoStack;
        private readonly List<IProjectAction> _redoStack;

        public bool CanUndo { get; private set; }
        public bool CanRedo { get; private set; }

        public ActionStack(PipelineController controller)
        {
            _controller = controller;
            _undoStack = new List<IProjectAction>();
            _redoStack = new List<IProjectAction>();
        }

        public void Add(IProjectAction action)
        {
            _undoStack.Add(action);

            if (_redoStack.Count > 0)
                _redoStack.Clear();

            Update();
        }

        public void Undo()
        {
            if (!_undoStack.Any())
                return;

            var action = _undoStack.Last();
            if (action.Undo())
            {
                _undoStack.Remove(action);
                _redoStack.Add(action);
            }

            Update();
        }

        public void Redo()
        {
            if (!_redoStack.Any())
                return;

            var action = _redoStack.Last();
            if (action.Do())
            {
                _redoStack.Remove(action);
                _undoStack.Add(action);
            }

            Update();
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();

            Update();
        }

        private void Update()
        {
            CanUndo = _undoStack.Any();
            CanRedo = _redoStack.Any();
            _controller.UpdateMenu();
        }
    }
}
