// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace MonoGame.Framework.Content
{

    /// <summary>
    /// The <see cref="IContentContext"/> represents some sort of context operation's context.
    /// Usually, this represents a <see cref="ContentImporterContext"/>
    /// or <see cref="ContentProcessorContext"/>.
    ///
    /// However, because those types are part of the XNA namespace, they cannot be modified directly.
    /// This interface is an adapter over those types.
    /// </summary>
    internal interface IContentContext : IDisposable
    {
        /// <inheritdoc cref="ContentImporterContext.IntermediateDirectory"/>
        public string IntermediateDirectory { get; }

        /// <inheritdoc cref="ContentImporterContext.Logger"/>
        public ContentBuildLogger Logger { get; }

        /// <inheritdoc cref="ContentProcessorContext.SourceIdentity"/>
        public ContentIdentity SourceIdentity { get; }
    }

    /// <summary>
    /// <para>
    /// The <see cref="ContextScopeFactory"/> facilitates access to a <see cref="IContentContext"/>
    /// instance without direct access to the actual context.
    /// </para>
    ///
    /// <para>
    /// Anytime a content context operation is about to start, the operation should signal
    /// the <see cref="BeginContext(IContentContext)"/> method.
    /// <b> Critically </b>, the content operation must <b>dispose</b> the resulting context
    /// when the operation has concluded.
    /// </para>
    ///
    /// <para>
    /// The most recent content operation can be retrieved with the <see cref="ActiveContext"/>
    /// property.
    /// </para>
    ///
    /// </summary>
    internal static class ContextScopeFactory
    {
        private static AsyncLocal<List<IContentContext>> _contextStack = new AsyncLocal<List<IContentContext>>
        {
            Value = new List<IContentContext>(1)
        };
        private static AsyncLocal<IContentContext> _activeContext = new AsyncLocal<IContentContext>();

        /// <summary>
        /// Returns true when the <see cref="ActiveContext"/> is a valid <see cref="IContentContext"/> instance.
        /// </summary>
        public static bool HasActiveContext => _activeContext.Value != null;

        /// <summary>
        /// Access the latest <see cref="IContentContext"/> operation.
        /// If no operations are running (and therefor there is no context), this
        /// accessor will throw a <see cref="PipelineException"/>.
        ///
        /// Use the <see cref="HasActiveContext"/> to check if there is an active context.
        ///
        /// <para>
        /// Each Task-chain may have its own unique ActiveContext, but if a task-chain
        /// does not have an active context, then the parent task's active context will be used
        /// recursively. This is the behaviour of AsyncLocal.
        /// </para>
        /// </summary>
        /// <exception cref="PipelineException"></exception>
        public static IContentContext ActiveContext
        {
            get
            {
                if (!HasActiveContext)
                {
                    throw new PipelineException(
                        $"Cannot access {nameof(ActiveContext)} because there is no active context. Make sure that {nameof(ContextScopeFactory)}.{nameof(BeginContext)} has been called with the `using` keyword");
                }

                return _activeContext.Value;
            }
        }

        /// <summary>
        /// Start a <see cref="ContentProcessorContext"/> operation.
        /// The <see cref="ContentProcessorContext"/> instance will be adapted into a
        /// <see cref="IContentContext"/>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <returns>
        /// <b>this return value must be disposed when the context operation is complete!</b>
        /// </returns>
        public static IContentContext BeginContext(ContentProcessorContext context)
        {
            return BeginContext(new ContentProcessorContextAdapter(context));
        }


        /// <summary>
        /// Start a <see cref="ContentImporterContext"/> operation.
        /// The <see cref="ContentImporterContext"/> instance will be adapted into a
        /// <see cref="IContentContext"/>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="evt">
        /// A <see cref="ContentImporterContext"/> does not include a source file,
        /// but the originating <see cref="PipelineBuildEvent"/> <i>does</i>. The event
        /// will be used to fulfill the <see cref="IContentContext.SourceIdentity"/> value.
        /// </param>
        /// <returns>
        /// <b>this return value must be disposed when the context operation is complete!</b>
        /// </returns>
        public static IContentContext BeginContext(ContentImporterContext context, PipelineBuildEvent evt)
        {
            return BeginContext(new ContentImporterContextAdapter(context, evt));
        }

        /// <summary>
        /// Start a content context operation.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns>
        /// <b>this return value must be disposed when the context operation is complete!</b>
        /// </returns>
        public static IContentContext BeginContext(IContentContext scope)
        {
            if (_contextStack.Value == null)
                _contextStack.Value = new List<IContentContext>(1);

            _contextStack.Value.Add(scope);
            _activeContext.Value = scope;
            return scope;
        }

        /// <summary>
        /// The default implementation of the <see cref="IContentContext"/>
        /// provides a basic Dispose() method that will remove the context
        /// from the history in the <see cref="ContextScopeFactory"/>
        /// </summary>
        public abstract class ContextScope : IContentContext
        {
            public abstract string IntermediateDirectory { get; }
            public abstract ContentBuildLogger Logger { get; }
            public abstract ContentIdentity SourceIdentity { get; }

            /// <summary>
            /// Remove this context operation from the history in the <see cref="ContextScopeFactory"/>.
            /// If this context was the <see cref="ContextScopeFactory.ActiveContext"/>, then this
            /// method will reset the <see cref="ContextScopeFactory.ActiveContext"/> value to the next
            /// most-recent context operation, or null if none exist.
            /// </summary>
            public virtual void Dispose()
            {
                _contextStack.Value.Remove(this);

                // if someone else has already claimed the activeContext, then we don't need to care.
                if (_activeContext.Value != this) return;

                // either use the "most recent" (aka, last) context, or if the list is empty,
                //  there is no context.
                _activeContext.Value = _contextStack.Value.Count > 0
                    ? _contextStack.Value[^1]
                    : null;
            }

        }

        private class ContentProcessorContextAdapter : ContextScope
        {
            private readonly ContentProcessorContext _context;

            public ContentProcessorContextAdapter(ContentProcessorContext context)
            {
                _context = context;
            }

            public override string IntermediateDirectory => _context.IntermediateDirectory;
            public override ContentBuildLogger Logger => _context.Logger;
            public override ContentIdentity SourceIdentity => _context.SourceIdentity;
        }

        private class ContentImporterContextAdapter : ContextScope
        {
            private readonly ContentImporterContext _context;

            public ContentImporterContextAdapter(ContentImporterContext context, PipelineBuildEvent evt)
            {
                _context = context;
                SourceIdentity = new ContentIdentity(sourceFilename: evt.SourceFile);
            }

            public override string IntermediateDirectory => _context.IntermediateDirectory;
            public override ContentBuildLogger Logger => _context.Logger;
            public override ContentIdentity SourceIdentity { get; }
        }
    }
}
