// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoGame.Content.Builder.Editor.Launcher
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            IPlatform platform;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                platform = new MacPlatform();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                platform = new WindowsPlatform();
            else
                platform = new LinuxPlatform();

            Action runAction = null;
            var rootCommand = new RootCommand()
            {
                Name = "mgcb-editor",
                Handler = CommandHandler.Create<InvocationContext, string>((context, project) =>
                {
                    runAction = () => platform.Run(context, project);
                })
            };

            var projectArgument = new Argument<string>("project")
            {
                Arity = ArgumentArity.ZeroOrOne,
                Description = "The path to a .mgcb file."
            };

            var registerOption = new Option(new string[] { "/r", "-r", "/register", "--register" })
            {
                Argument = new Argument<bool>(),
                Description = "Register MGCB Editor as the default application for .mgcb files for the current user, replacing any previous registration."
            };

            var unregisterOption = new Option(new string[] { "/u", "-u", "/unregister", "--unregister" })
            {
                Argument = new Argument<bool>(),
                Description = "Unregister MGCB Editor as the default application for .mgcb files for the current user."
            };

            var versionOption = new Option(new string[] { "/v", "-v", "/version", "--version" })
            {
                Argument = new Argument<bool>(),
                Description = "Show version information."
            };

            var parser = new CommandLineBuilder(rootCommand)
                .AddArgument(projectArgument)
                .AddOption(registerOption)
                .AddOption(unregisterOption)
                .AddOption(versionOption)
                .UseMiddleware(CreateShortCircuitMiddleware(registerOption, platform.Register))
                .UseMiddleware(CreateShortCircuitMiddleware(unregisterOption, platform.Unregister))
                .UseMiddleware(CreateShortCircuitMiddleware(versionOption, ShowVersion))
                .UseDefaults()
                .Build()
                .Invoke(args);

            runAction?.Invoke();
        }

        private static void ShowVersion(InvocationContext context)
        {
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            context.Console.Out.WriteLine(version);
        }

        private static InvocationMiddleware CreateShortCircuitMiddleware(Option option, Action<InvocationContext> action)
        {
            return async (context, next) =>
            {
                if (context.ParseResult.HasOption(option))
                {
                    action(context);
                }
                else
                {
                    await next(context);
                }
            };
        }
    }
}
