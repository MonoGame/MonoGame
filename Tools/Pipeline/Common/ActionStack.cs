using System.Collections.Generic;
using System.Linq;

namespace MonoGame.Tools.Pipeline
{
    public interface IProjectAction
    {
        void Do();
        void Undo();        
    }

    public class ActionStack
    {
        private readonly List<IProjectAction> _undoStack;
        private readonly List<IProjectAction> _redoStack;

        public bool CanUndo { get { return _undoStack.Any(); } }
        public bool CanRedo { get { return _redoStack.Any(); } }

        public ActionStack()
        {
            _undoStack = new List<IProjectAction>();
            _redoStack = new List<IProjectAction>();
        }

        public void Add(IProjectAction action)
        {
            action.Do();
            _undoStack.Add(action);

            if (_redoStack.Count > 0)
                _redoStack.Clear();
        }

        public void Undo()
        {
            if (!_undoStack.Any())
                return;

            var action = _undoStack.Last();
            _undoStack.Remove(action);

            action.Undo();
            _redoStack.Add(action);
        }

        public void Redo()
        {
            if (!_redoStack.Any())
                return;

            var action = _redoStack.Last();
            _redoStack.Remove(action);

            action.Do();
            _undoStack.Add(action);
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
