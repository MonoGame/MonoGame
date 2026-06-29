using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
namespace MonoGame.Tests
{
    /// <summary>
    /// This class will run any test on the main UI thread. 
    /// This is required for SDL Graphics Tests.
    /// Add the <see cref="RunOnUiTestFixtureAttribute"/> instead of <see cref="TestFixtureAttribute"/> to the test class to get all the tests in that class to run on the UI thread.
    /// Alternatively, add <see cref="RunOnUiAttribute"/> to individual test methods to run only those tests on the UI thread.
    /// </summary>
    public class MainThreadSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<(SendOrPostCallback, object, ManualResetEvent, TestContext)> _callbackQueue = new BlockingCollection<(SendOrPostCallback, object, ManualResetEvent, TestContext)>();
        private readonly Thread _mainThread;

        public MainThreadSynchronizationContext(Thread mainThread)
        {
            _mainThread = mainThread;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _callbackQueue.TryAdd((d, state, new ManualResetEvent(false), TestContext.CurrentContext));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            var ev = new ManualResetEvent(false);
            _callbackQueue.TryAdd((d, state, ev, TestContext.CurrentContext));
            ev.WaitOne();
        }

        public void End()
        {
            _callbackQueue.CompleteAdding();
        }

        public void ExecuteQueuedCallbacks()
        {
            foreach (var workItem in _callbackQueue.GetConsumingEnumerable())
            {
                var (callback, state, ev, context) = workItem;
                Extensions.RegisterTestWtihContext(TestContext.CurrentContext, context.Test);
                try
                {
                    callback(state);
                }
                finally
                {
                    Extensions.UnRegisterTestWtihContext(TestContext.CurrentContext);
                }
                if (ev != null)
                {
                    ev.Set();
                }
            }
        }
    }
}
