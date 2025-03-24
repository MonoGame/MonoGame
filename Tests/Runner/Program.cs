// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using NUnitLite;
using NUnit.Framework;
using NUnit.Common;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using System.Reflection;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Filters;
using System.Collections.Generic;
using NUnit;
using System.Diagnostics;
using NUnit.Framework.Internal.Execution;
using NUnit.Framework.Internal.Commands;
using System.Threading;
using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;

namespace MonoGame.Tests
{
    static class Program
    {
        static Thread mainThread = Thread.CurrentThread;
        static MainThreadSynchronizationContext mainThreadSynchronizationContext;
        public static TestResult Invoke (Func<TestResult> func, TestResult result)
        {
            if (Thread.CurrentThread != mainThread) {
                TestResult r = result;
                mainThreadSynchronizationContext.Send (state => {
                    try {
                    r = func();
                    } catch (Exception ex) {
                        r.RecordException (ex);
                    }
                }, null);
                return r;
            } else {
                return func();
            }
        }
        static async Task<int> Main(string [] args)
        {
            mainThread = Thread.CurrentThread;
            mainThreadSynchronizationContext = new MainThreadSynchronizationContext(mainThread);
            SynchronizationContext.SetSynchronizationContext (mainThreadSynchronizationContext);
            int result = 0;
            var task = Task.Run (() => result = new AutoRun().Execute(args)).ContinueWith ((t) => {
                mainThreadSynchronizationContext.End ();
            }, continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
            mainThreadSynchronizationContext.ExecuteQueuedCallbacks ();
            await task;
            return result;
        }
    }
}
