using System;
using System.Collections.Generic;
using System.Threading;

namespace MonoGame.Tests.Utilities
{
    internal class ActionDaemon
    {
        private readonly Queue<Action> _actions = new Queue<Action>();
        private Thread _thread;

        public Thread Thread
        {
            get { return _thread; }
        }

        public bool Finished
        {
            get { return _actions.Count == 0 && (_thread == null || _thread.IsAlive == false); }
        }

        public void AddAction(Action action)
        {
            lock (_actions)
                _actions.Enqueue(action);

            if (_thread == null || !_thread.IsAlive)
                Start();
        }

        public void ForceTermination()
        {
            _thread.Abort();
        }

        public void Clear(bool abortCurrent = false)
        {
            lock (_actions)
                _actions.Clear();

            if (abortCurrent)
                ForceTermination();
        }

        private void Start()
        {
            _thread = new Thread(DoActions);
            _thread.Priority = ThreadPriority.Lowest;
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void DoActions()
        {
            while (true)
            {
                Action currentAction;
                lock (_actions)
                {
                    if (_actions.Count == 0)
                        break;
                    currentAction = _actions.Dequeue();
                }
                currentAction();
            }
        }

    }
}