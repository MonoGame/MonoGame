// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder.Logger;

class ContentBuilderLogger : ContentBuildLogger
{
    private readonly Stack<string> _relativePaths = [];
    private int _indentCount = 0;
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

        Console.ForegroundColor = level switch
        {
            LogLevel.Debug => ConsoleColor.Black,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => ConsoleColor.Gray
        };

        var currentPath = _relativePaths.Count > 0 ? $"{string.Join(": ", _relativePaths)}: " : "";
        var spacing = string.Empty.PadLeft(Math.Max(0, _indentCount * 2), ' ');
        var time = LoggerLogLevel <= LogLevel.Debug ?  $"{_stopWatch.Elapsed:hh\\:mm\\:ss\\.fff} " : "";

        foreach (var subMessage in message.Split(['\r', '\n'], StringSplitOptions.None))
            Console.WriteLine($"{time}[{level.ToString()[0]}] {currentPath}{spacing}{subMessage}");

        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public override void PushFile(string filename)
    {
        var filepath = Path.GetFullPath(filename);
        _relativePaths.Push(Path.GetRelativePath(LoggerRootDirectory, filepath).Sanitize());
    }

    public override void PopFile() => _relativePaths.Pop();

    public override void Indent() => _indentCount++;

    public override void Unindent() => _indentCount = Math.Max(0, _indentCount - 1);
}
