// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { dotnet as dn } from "./_framework/dotnet.js";

/** @type {import('./dotnet.d.ts').DotnetHostBuilder} */
const dotnet = dn;

const { setModuleImports, getAssemblyExports, getConfig, Module } = await dotnet
    .withDiagnosticTracing(true)
    .withEnvironmentVariable("MONO_LOG_LEVEL", "debug")
    .withEnvironmentVariable("MONO_LOG_MASK", "all")
    .withApplicationArgumentsFromQuery()
    .withDebugging(1)
    .create();

//this allows us to handle the main loop from C#
setModuleImports("main.js", {
    setMainLoop: (cb) => {
        Module.setMainLoop(cb);
        Module._emscripten_set_main_loop_timing(
            Module.EMSCRIPTEN_WEBGL_ANIMATIONFRAME,
            1
        );
    },
});

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);
console.log(exports);

const cSharpMainLoop = exports.Example.Program.MainLoop;

//this allows us to set the main loop from JS
Module.setMainLoop(cSharpMainLoop);

//set canvas
var canvas = document.getElementById("canvas");
Module.canvas = canvas;

await dotnet.run();
