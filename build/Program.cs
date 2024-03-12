
return new CakeHost()
    .UseWorkingDirectory("../")
    .UseContext<BuildScripts.BuildContext>()
    .Run(args);
