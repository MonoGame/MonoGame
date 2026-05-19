// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content;
using NUnit.Framework;

namespace MonoGame.Tools.Tests;

public class ContextScopeFactoryTests
{

    /// <summary>
    /// This test is attempting to show that multiple tasks can inherit the active context
    /// from a top-level task.
    /// </summary>
    [Test]
    public async Task ContextScopeFactory_HierarchicalAccess()
    {
        var tasks = new List<Task>();
        long errorCount = 0;
        var ctx = new NoOpContext(0);
        ContextScopeFactory.BeginContext(ctx);
        for (var i = 0; i < 100; i++)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(10);
                    // get the context and make sure it is the
                    //  the scope from the original task
                    var retrievedCtx = ContextScopeFactory.ActiveContext;
                    Assert.That(ctx, Is.EqualTo(retrievedCtx));
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    Console.WriteLine($"exception {ex.GetType().Name} - {ex.Message} \n {ex.StackTrace}");
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        ctx.Dispose();
        Assert.That(errorCount, Is.EqualTo(0), "there were exceptions thrown in the task looping.");
    }

    /// <summary>
    /// This test is trying to show that
    /// - there can be an active context _per_ Task, and that if
    ///   code fetches the active context, it'll find the one on
    ///   the current task. This is useful if/when the Content Builder
    ///   is running multiple import jobs at once
    /// - there can still be nested contexts. If you start an import-job,
    ///   and then have a sub-task to import a sub-asset, the ActiveContext
    ///   will use the sub-task's context and then switch back to the top
    ///   level one
    /// </summary>
    [Test]
    public async Task ContextScopeFactory_AsyncAccessNestedAccess()
    {
        var tasks = new List<Task>();
        long errorCount = 0;
        for (var i = 0; i < 100; i++)
        {
            var capturedIndex = i;
            var task = Task.Run(async () =>
            {
                try
                {
                    // spawn a context to be used
                    using var ctx = new NoOpContext(capturedIndex);
                    ContextScopeFactory.BeginContext(ctx);

                    // wait some "arbitrary" amount of time
                    await Task.Delay(10);

                    using (var nestedCtx = new NoOpContext(capturedIndex * 100))
                    {
                        ContextScopeFactory.BeginContext(nestedCtx);
                        // wait some more "arbitrary" amount of time
                        await Task.Delay(10);

                        // get the context and make sure it is the same as the one we expect
                        var retrievedNestedCtx = ContextScopeFactory.ActiveContext;
                        Assert.That(nestedCtx, Is.EqualTo(retrievedNestedCtx));
                    }

                    // get the context and make sure it is the same as the one we expect
                    var retrievedCtx = ContextScopeFactory.ActiveContext;
                    Assert.That(ctx, Is.EqualTo(retrievedCtx));
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    Console.WriteLine($"exception {ex.GetType().Name} - {ex.Message} \n {ex.StackTrace}");
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        Assert.That(errorCount, Is.EqualTo(0), "there were exceptions thrown in the task looping.");
    }

    /// <summary>
    /// This is a dummy class to act as a stand-in for tests that
    /// just need to check the _identity_ of the context being used
    /// </summary>
    class NoOpContext : ContextScopeFactory.ContextScope
    {
        public readonly int Id;
        public override string IntermediateDirectory { get; }
        public override ContentBuildLogger Logger { get; }
        public override ContentIdentity SourceIdentity { get; }

        public NoOpContext(int id)
        {
            Id = id;
        }

        // helpful during debugging or broken tests to be able
        //  to see that one ToString()'d instance is different than another
        public override string ToString() => "test-ctx-" + Id;
    }

}
