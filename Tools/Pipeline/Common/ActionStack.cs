using System.Collections.Generic;
using System.Linq;

namespace MonoGame.Tools.Pipeline
{
    internal interface IProjectAction
    {
        void Do();
        void Undo();
    }

    internal delegate void CanUndoRedoChanged(bool canUndo, bool canRedo);

    partial class PipelineController
    {                
        /// <summary>
        /// Represents a stack of undo/redo-able actions.
        /// </summary>
        private class ActionStack
        {
            private readonly List<IProjectAction> _undoStack;
            private readonly List<IProjectAction> _redoStack;

            public bool CanUndo { get; private set; }
            public bool CanRedo { get; private set; }
            
            public event CanUndoRedoChanged OnCanUndoRedoChanged;

            public ActionStack()
            {
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
                _undoStack.Remove(action);

                action.Undo();
                _redoStack.Add(action);

                Update();
            }

            public void Redo()
            {
                if (!_redoStack.Any())
                    return;

                var action = _redoStack.Last();
                _redoStack.Remove(action);

                action.Do();
                _undoStack.Add(action);

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
                var canUndo = _undoStack.Any();
                var canRedo = _redoStack.Any();

                if (canUndo != CanUndo || canRedo != CanRedo)
                {
                    CanUndo = canUndo;
                    CanRedo = canRedo;

                    if (OnCanUndoRedoChanged != null)
                        OnCanUndoRedoChanged(canUndo, canRedo);
                }
            }
        }
    }
}
