#!/bin/bash

dotnet restore

# I had to improvise
dotnet msbuild /property:ONLYCORE=True /property:Configuration=Release
msbuild /property:ONLYNET=True /property:Configuration=Release

nuget pack MonoGame.Content.Builder.nuspec
