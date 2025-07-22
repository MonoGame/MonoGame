// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Reflection;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Content.Pipeline.Builder.Server;

namespace MonoGame.Framework.Content.Pipeline.Builder;

/// <summary>
/// A list of arguments used by <see cref="ContentBuilder"/>.
/// <para>Use <see cref="ContentBuilderParams.Parse"/> to acquire the arguments from the passed cli args.</para>
/// </summary>
public record ContentBuilderParams
{
    class RootOptions : BinderBase<ContentBuilderParams>
    {
        private readonly Func<BindingContext, ContentBuilderParams> _contentBuilderArgsFunc;

        public RootOptions(RootCommand rootCommand)
        {
            var defaultValues = new ContentBuilderParams();

            var workingDirectoryOption = new Option<string>(
                name: "--workingDir",
                description: "The working directory of the content builder.",
                getDefaultValue: () => defaultValues.WorkingDirectory);
            rootCommand.AddGlobalOption(workingDirectoryOption);

            var srcDirectoryOptions = new Option<string>(
                name: "--src",
                description: "The source directory of content relative to the workingDir.",
                getDefaultValue: () => defaultValues.SourceDirectory);
            srcDirectoryOptions.AddAlias("-s");
            rootCommand.AddGlobalOption(srcDirectoryOptions);

            var outputDirectoryOption = new Option<string>(
                name: "--output",
                description: "The output directory relative to the workingDir.",
                getDefaultValue: () => defaultValues.OutputDirectory);
            outputDirectoryOption.AddAlias("-o");
            rootCommand.AddGlobalOption(outputDirectoryOption);

            var intermediateDirectoryOption = new Option<string>(
                name: "--intermediate",
                description: "The intermediate directory relative to the workingDir.",
                getDefaultValue: () => defaultValues.IntermediateDirectory);
            intermediateDirectoryOption.AddAlias("-i");
            rootCommand.AddGlobalOption(intermediateDirectoryOption);

            var platformOption = new Option<TargetPlatform>(
                name: "--platform",
                description: "The target platform to build the content for.",
                getDefaultValue: () => defaultValues.Platform);
            platformOption.AddAlias("-p");
            rootCommand.AddGlobalOption(platformOption);

            var graphicsProfileOption = new Option<GraphicsProfile>(
                name: "--graphics-profile",
                description: "The graphics profile to build the content for.",
                getDefaultValue: () => defaultValues.GraphicsProfile);
            graphicsProfileOption.AddAlias("-g");
            rootCommand.AddGlobalOption(graphicsProfileOption);

            var compressContentOption = new Option<bool>(
                name: "--compress",
                description: "Tells the builder that the content should be compressed.",
                getDefaultValue: () => defaultValues.CompressContent);
            rootCommand.AddGlobalOption(compressContentOption);

            var logLevelOption = new Option<LogLevel>(
                name: "--loglevel",
                description: "The log level of messages that get outputed to the console.",
                getDefaultValue: () => defaultValues.LogLevel);
            logLevelOption.AddAlias("-l");
            rootCommand.AddGlobalOption(logLevelOption);

            _contentBuilderArgsFunc = (bindingContext) => new ContentBuilderParams
            {
                WorkingDirectory = bindingContext.ParseResult.GetValueForOption(workingDirectoryOption) ?? defaultValues.WorkingDirectory,
                SourceDirectory = bindingContext.ParseResult.GetValueForOption(srcDirectoryOptions) ?? defaultValues.SourceDirectory,
                OutputDirectory = bindingContext.ParseResult.GetValueForOption(outputDirectoryOption) ?? defaultValues.OutputDirectory,
                IntermediateDirectory = bindingContext.ParseResult.GetValueForOption(intermediateDirectoryOption) ?? defaultValues.IntermediateDirectory,
                Platform = bindingContext.ParseResult.GetValueForOption(platformOption),
                GraphicsProfile = bindingContext.ParseResult.GetValueForOption(graphicsProfileOption),
                CompressContent = bindingContext.ParseResult.GetValueForOption(compressContentOption),
                LogLevel = bindingContext.ParseResult.GetValueForOption(logLevelOption)
            };
        }

        protected override ContentBuilderParams GetBoundValue(BindingContext bindingContext) => _contentBuilderArgsFunc(bindingContext);
    }

    class ServerOptions : BinderBase<List<ContentServer>>
    {
        private readonly Func<BindingContext, List<ContentServer>> _contentBuilderArgsFunc;

        public ServerOptions(Command rootCommand)
        {
            var contentServers = new List<ContentServer>();
            var options = new List<(Type, PropertyInfo, Option)>();

            foreach (var serverType in ContentBuilderHelper.GetServerTypes())
            {
                var contentServer = (ContentServer)Activator.CreateInstance(serverType)!;
                foreach (var (attribute, propertyInfo) in ContentBuilderHelper.GetServerProperties(serverType))
                {
                    var optionType = typeof(Option<>).MakeGenericType(propertyInfo.PropertyType);
                    var option = (Option)Activator.CreateInstance(optionType, new object[] {
                        "--" + attribute.Name,
                        attribute.Description
                    })!;
                    option.SetDefaultValueFactory(() => propertyInfo.GetValue(contentServer));
                    rootCommand.AddGlobalOption(option);

                    options.Add((serverType, propertyInfo, option));
                }
                contentServers.Add(contentServer);
            }

            _contentBuilderArgsFunc = (bindingContext) =>
            {
                foreach (var (type, propInfo, option) in options)
                {
                    var value = bindingContext.ParseResult.GetValueForOption(option);
                    if (value != null)
                    {
                        var server = contentServers.Find(s => s.GetType() == type);
                        propInfo.SetValue(server, value);
                    }
                }

                return contentServers;
            };
        }

        protected override List<ContentServer> GetBoundValue(BindingContext bindingContext) => _contentBuilderArgsFunc(bindingContext);
    }

    /// <summary>
    /// Parses out the main entry point args into a <see cref="ContentBuilderParams"/> to be used by <see cref="ContentBuilder"/>.
    /// </summary>
    /// <param name="args">Arguments passed to the main entry point of the app.</param>
    /// <returns>
    /// <see cref="ContentBuilderParams"/> containing the parsed arguments, or an empty <see cref="ContentBuilderParams"/> if no arguments were passed.
    /// </returns>
    public static ContentBuilderParams Parse(params string[] args)
    {
        var ret = new ContentBuilderParams();
        var defaultValues = new ContentBuilderParams();
        var rootCommand = new RootCommand("Content builder and conntent server for MonoGame.");
        var rootOptions = new RootOptions(rootCommand);

        var buildCommand = new Command("build", "Build all the content.");
        var skipCleanOption = new Option<bool>(
                name: "--skip-clean",
                description: "Should the builder skip cleaning up old content cache data after the build is finished.",
                getDefaultValue: () => defaultValues.SkipClean);
        buildCommand.AddOption(skipCleanOption);
        buildCommand.SetHandler(
            (contentBuilder, skipCleanOption) => ret = contentBuilder with { Mode = ContentBuilderMode.Builder, SkipClean = skipCleanOption },
            rootOptions,
            skipCleanOption);
        rootCommand.AddCommand(buildCommand);

        var serverCommand = new Command("server", "Start a content server.");
        var sererOptions = new ServerOptions(serverCommand);
        serverCommand.SetHandler(
            (contentBuilder, sererOptions) => ret = contentBuilder with { Mode = ContentBuilderMode.Server, Servers = sererOptions },
            rootOptions,
            sererOptions);
        rootCommand.AddCommand(serverCommand);

        bool helpShown = false;
        var parser = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseHelp(ctx => helpShown = true)
            .Build();
        parser.Invoke(args);

        if (helpShown)
        {
            ret = ret with { Mode = ContentBuilderMode.None };
        }

        return ret;
    }

    /// <summary>
    /// Set the mode in which the content builder is run in. See <see cref="ContentBuilderMode"/> for available modes.
    /// </summary>
    /// <value><see cref="ContentBuilderMode.None"/> by default.</value>
    public ContentBuilderMode Mode { get; init; } = ContentBuilderMode.None;

    /// <summary>
    /// Gets or sets the working directory of the <see cref="ContentBuilder"/>.
    /// </summary>
    /// <value><see cref="Directory.GetCurrentDirectory"/> by default.</value>
    public string WorkingDirectory { get; init; } = Directory.GetCurrentDirectory();

    /// <summary>
    /// Gets or sets the location of the content relative to the <see cref="WorkingDirectory"/>.
    /// </summary>
    /// <value><c>Content</c> by default.</value>
    public string SourceDirectory { get; init; } = "Content";

    /// <summary>
    /// Gets the rooted location of <see cref="SourceDirectory"/>.
    /// </summary>
    public string RootedSourceDirectory => Path.Combine(WorkingDirectory, SourceDirectory);

    /// <summary>
    /// Gets or sets the location for the content output relative to the <see cref="WorkingDirectory"/>.
    /// </summary>
    /// <value><c>bin/Content</c> by default.</value>
    public string OutputDirectory { get; init; } = "bin/Content";

    /// <summary>
    /// Gets the rooted location of <see cref="OutputDirectory"/>.
    /// </summary>
    public string RootedOutputDirectory => Path.Combine(WorkingDirectory, OutputDirectory, Platform.ToString());

    /// <summary>
    /// Gets or sets the location for the intermediate files for content build relative to the <see cref="WorkingDirectory"/>.
    /// </summary>
    /// <value><c>obj/Content</c> by default.</value>
    public string IntermediateDirectory { get; init; } = "obj/Content";

    /// <summary>
    /// Gets the rooted location of <see cref="IntermediateDirectory"/>.
    /// </summary>
    public string RootedIntermediateDirectory => Path.Combine(WorkingDirectory, IntermediateDirectory, Platform.ToString());

    /// <summary>
    /// Gets or sets the desired platform for <see cref="ContentBuilder"/> to build the content for.
    /// </summary>
    /// <value><see cref="TargetPlatform.DesktopGL"/> by default.</value>
    public TargetPlatform Platform { get; init; } = TargetPlatform.DesktopGL;

    /// <summary>
    /// Gets or sets the desired graphics profile for <see cref="ContentBuilder"/> to build the content for.
    /// </summary>
    /// <value><see cref="GraphicsProfile.HiDef"/> by default.</value>
    public GraphicsProfile GraphicsProfile { get; init; } = GraphicsProfile.HiDef;

    /// <summary>
    /// Gets or sets if <see cref="ContentBuilder"/> should compress each built content file.
    /// </summary>
    /// <value><c>false</c> by default.</value>
    public bool CompressContent { get; init; } = false;

    /// <summary>
    /// Gets or sets the logging level of information that <see cref="ContentBuilder"/> will display to console.
    /// </summary>
    /// <value><see cref="LogLevel.Info"/> by default.</value>
    public LogLevel LogLevel { get; init; } = LogLevel.Info;

    /// <summary>
    /// Should the content builder skip cleaning up old content cache data after the build is finished in <see cref="ContentBuilderMode.Builder"/> mode.
    /// </summary>
    /// <value><c>false</c> by default.</value>
    public bool SkipClean { get; init; } = false;

    /// <summary>
    /// A list of servers to start up when the <see cref="Mode"/> is set to <see cref="ContentBuilderMode.Server"/>.
    /// </summary>
    /// <value>A collection of <see cref="ContentServer"/> classes found by scaning all referenced assemblies.</value>
    public List<ContentServer> Servers { get; init; } = []; // TODO: Fix command line display
}
