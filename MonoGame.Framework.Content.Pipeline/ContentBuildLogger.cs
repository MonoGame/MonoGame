// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace Microsoft.Xna.Framework.Content.Pipeline;

/// <summary>
/// Provides methods for reporting informational messages or warnings from content importers and processors.
/// Do not use this class to report errors. Instead, report errors by throwing a PipelineException or InvalidContentException.
/// </summary>
public class ContentBuildLogger
{
    private readonly Stack<string> _filenames;
    private int _indentCount;
    private string _indentString;
    private char _indentCharacter;
    private bool _recreateIndentString;
    private readonly Stopwatch _stopWatch;

    /// <summary>
    /// Initializes a new instance of ContentBuildLogger.
    /// </summary>
    public ContentBuildLogger()
    {
        _filenames = [];
        _indentCount = 0;
        _indentString = " ";
        _recreateIndentString = false;
        _stopWatch = new();
        _stopWatch.Start();

        IndentCharacter = '\t';
        IndentCharacterSize = 1;
        LoggerRootDirectory = "";
    }

    /// <summary>
    /// Indicates if the log should should current time with each logged message as opposed to the time since the logging started.
    /// </summary>
    /// <value><c>false</c> by default.</value>
    public bool ShowRealTime { get; set; }

    /// <summary>
    /// A character to be used for indentation of <see cref="Log(LogLevel, string)"/> messages.
    /// </summary>
    public char IndentCharacter
    {
        get => _indentCharacter;
        set
        {
            _indentCharacter = value;
            _recreateIndentString = true;
        }
    }

    /// <summary>
    /// Indicates how many of <see cref="IndentCharacter"/> should be used for indentation of <see cref="Log(LogLevel, string)"/> messages.
    /// </summary>
    public uint IndentCharacterSize { get; set; }

    /// <summary>
    /// Gets or sets the base reference path used when reporting errors during the content build process.
    /// </summary>
    public string LoggerRootDirectory { get; set; }

    /// <summary>
    /// Creates a string that is the combination of <see cref="IndentCharacter"/> * <see cref="Indent"/> * <see cref="IndentCharacterSize"/>.
    /// </summary>
    protected string IndentString
    {
        get
        {
            if (_recreateIndentString || _indentString.Length != (_indentCount + _filenames.Count) * IndentCharacterSize)
                _indentString = new string(_indentCharacter, (_indentCount + _filenames.Count) * (int)IndentCharacterSize);

            return _indentString;
        }
    }

    /// <summary>
    /// Gets the filename currently being processed, for use in warning and error messages.
    /// </summary>
    /// <param name="contentIdentity">
    /// Identity of a content item.
    /// If specified, GetCurrentFilename uses this value to refine the search.
    /// If no value is specified, the current PushFile state is used.
    /// </param>
    /// <returns>Name of the file being processed.</returns>
    public string GetCurrentFilename(ContentIdentity? contentIdentity = null)
    {
        if ((contentIdentity != null) && !string.IsNullOrEmpty(contentIdentity.SourceFilename))
            return Path.GetRelativePath(LoggerRootDirectory, contentIdentity.SourceFilename);
        if (_filenames.Count > 0)
            return Path.GetRelativePath(LoggerRootDirectory, _filenames.Peek());
        return string.Empty;
    }

    /// <summary>
    /// Outputs a message from the content system with the specified log level.
    /// </summary>
    /// <param name="level">Log level of the message.</param>
    /// <param name="message"></param>
    public virtual void Log(LogLevel level, string message)
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
            var time = ShowRealTime ? DateTime.Now.ToString("HH:mm:ss.fff") : _stopWatch.Elapsed.ToString("hh\\:mm\\:ss\\.fff");
            Console.WriteLine($"{time} [{level.ToString()[0]}] {IndentString}{subMessage}");
        }

        Console.ForegroundColor = ConsoleColor.Gray;
    }

    /// <summary>
    /// Outputs a message from the content system with the <see cref="LogLevel.Info"/> log level.
    /// </summary>
    public void Log(string message)
        => Log(LogLevel.Info, message);

    /// <summary>
    /// Outputs a high-priority status message from the content system.
    /// </summary>
    /// <param name="message">Message being reported.</param>
    /// <param name="messageArgs">Arguments for the reported message.</param>
    [Obsolete("LogImportantMessage is deprecated, please use Log instead.")]
    public virtual void LogImportantMessage(string message, params object[] messageArgs)
        => Log(LogLevel.Error, string.Format(message, messageArgs));

    /// <summary>
    /// Outputs a low priority status message from the content system.
    /// </summary>
    /// <param name="message">Message being reported.</param>
    /// <param name="messageArgs">Arguments for the reported message.</param>
    [Obsolete("LogMessage is deprecated, please use Log instead.")]
    public virtual void LogMessage(string message, params object[] messageArgs)
        => Log(string.Format(message, messageArgs));

    /// <summary>
    /// Outputs a warning message from the content system.
    /// </summary>
    /// <param name="helpLink">Link to an existing online help topic containing related information.</param>
    /// <param name="contentIdentity">Identity of the content item that generated the message.</param>
    /// <param name="message">Message being reported.</param>
    /// <param name="messageArgs">Arguments for the reported message.</param>
    [Obsolete("LogWarning is deprecated, please use Log instead.")]
    public virtual void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
        => Log(LogLevel.Warning, $"{string.Format(message, messageArgs)}: {GetCurrentFilename(contentIdentity)}");

    /// <summary>
    /// Outputs a message indicating that a content asset has begun processing.
    /// All logger warnings or error exceptions from this time forward to the next PopFile call refer to this file.
    /// </summary>
    /// <param name="filename">Name of the file containing future messages.</param>
    public virtual void PushFile(string filename)
    {
        Log(filename);
        _filenames.Push(filename);
    }

    /// <summary>
    /// Outputs a message indicating that a content asset has completed processing.
    /// </summary>
    public virtual void PopFile() => _filenames.Pop();

    /// <summary>
    /// Adds an indent for all future Log calls.
    /// </summary>
    public virtual void Indent() => _indentCount++;

    /// <summary>
    /// Removes an indent for all future Log calls.
    /// </summary>
    public virtual void Unindent() => _indentCount = Math.Max(0, _indentCount - 1);
}
