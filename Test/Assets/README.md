### Quick iOS Note

For iOS, a separate .csproj is needed because of target framework
conflicts referencing a non-MonoTouch project from a MonoTouch project.  So,
until a better solution is possible, the iOS Assets project should be synced
after making changes to MonoTouch.Tests.Assets.csproj.  The sync-ios.sh script
can do this automatically.
