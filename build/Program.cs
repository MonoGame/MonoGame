using Cake.Frosting;
using MonoGame.Framework.Build;

return new CakeHost()
    .UseWorkingDirectory("../")
    .UseContext<BuildContext>()
    .Run(args);
