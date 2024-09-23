using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace MonoGame.Tests 
{
    /// <summary>
    /// Marshall the test onto the main UI thread.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    sealed class RunOnUIAttribute : Attribute, IWrapSetUpTearDown, IWrapTestMethod
    {
//#if !DESKTOPGL
//        public TestCommand Wrap(TestCommand command) => command;
//#else
        public TestCommand Wrap(TestCommand command) => new RunOnUICommand(command);

        class RunOnUICommand : DelegatingTestCommand
        {
            public RunOnUICommand(TestCommand innerCommand)
                : base(innerCommand)
            {
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                return Program.Invoke(() => innerCommand.Execute(context), context.CurrentTest.MakeTestResult ());
            }
        }
//#endif
    }
}