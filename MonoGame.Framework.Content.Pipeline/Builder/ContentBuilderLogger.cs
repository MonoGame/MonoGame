// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder.Logger;

class ContentBuilderLogger : ContentBuildLogger
{
    private readonly AsyncLocal<ImmutableStack<string>> _relativePaths = new();
    private readonly AsyncLocal<int> _indentCount = new();
    private readonly object _consoleLock = new();
    private readonly Stopwatch _stopWatch;

    public ContentBuilderLogger()
    {
        _stopWatch = new();
        _stopWatch.Start();
    }

    public override void Log(LogLevel level, string message)
    {
        if (level < LoggerLogLevel)
            return;

        var relativePaths = _relativePaths.Value;
        var currentPath = relativePaths != null && !relativePaths.IsEmpty ? $"{string.Join(" > ", relativePaths.Reverse())}: " : "";
        var spacing = string.Empty.PadLeft(Math.Max(0, _indentCount.Value * 2), ' ');
        var time = LoggerLogLevel <= LogLevel.Debug ?  $"{_stopWatch.Elapsed:hh\\:mm\\:ss\\.fff} " : "";

        lock (_consoleLock)
        {
            Console.ForegroundColor = level switch
            {
                LogLevel.Debug => ConsoleColor.Black,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.Gray
            };

            foreach (var subMessage in message.Split(['\r', '\n'], StringSplitOptions.None))
            {
                Console.WriteLine($"{time}[{level.ToString()[0]}] {currentPath}{spacing}{subMessage}");
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    public override void PushFile(string filename)
    {
        base.PushFile(filename);
        var filepath = Path.GetFullPath(filename);
        _relativePaths.Value = (_relativePaths.Value ?? ImmutableStack<string>.Empty).Push(Path.GetRelativePath(LoggerRootDirectory, filepath).Sanitize());
    }

    public override void PopFile()
    {
        base.PopFile();
        _relativePaths.Value = (_relativePaths.Value ?? ImmutableStack<string>.Empty).Pop();
    }

    public override void Indent() => _indentCount.Value = _indentCount.Value + 1;

    public override void Unindent() => _indentCount.Value = Math.Max(0, _indentCount.Value - 1);
}
