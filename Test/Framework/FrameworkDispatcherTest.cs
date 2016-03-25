using System.Threading;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class FrameworkDispatcherTest
    {
        [Test]
        public void CallOnPrimaryThread()
        {
            FrameworkDispatcher.Update();
        }

        [Test]
        public void CallOnAnotherThread()
        {
            _callOnAnotherThreadResult = CallOnAnotherThreadTestResult.NotRun;

            var thread = new Thread(() => {
                _callOnAnotherThreadResult = CallOnAnotherThreadTestResult.Exception;
                FrameworkDispatcher.Update();

                // If executing this line, no exception was thrown.
                _callOnAnotherThreadResult = CallOnAnotherThreadTestResult.NoException;

            });

            thread.Start();
            if (!thread.Join(1000))
                Assert.Fail("Secondary thread did not terminate in time.");

            Assert.AreEqual(CallOnAnotherThreadTestResult.NoException, _callOnAnotherThreadResult);
        }
        private static CallOnAnotherThreadTestResult _callOnAnotherThreadResult;

        enum CallOnAnotherThreadTestResult
        {
            NotRun,
            NoException,
            Exception
        }
    }
}
