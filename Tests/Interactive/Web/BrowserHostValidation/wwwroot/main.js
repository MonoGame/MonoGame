// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

globalThis.MonoGameWebHostConfiguration = {
    runtimeScriptUri: "./_framework/dotnet.js",
    bootstrapAssemblyName: "BrowserHostValidation.dll",
    bootstrapTypeName: "BrowserHostValidation.BrowserHostValidationBootstrap",
    mainAssemblyName: "BrowserHostValidation.dll"
};

globalThis.MonoGameWebHostValidation = {
    reportPhase(phaseName, message) {
        console.info("[BrowserHostValidation]", phaseName, message);
    }
};

try {
    await import("./monogame-web-host.js");
}
catch (error) {
    const statusElement = document.getElementById("monogame-host-status");
    if (statusElement != null) {
        statusElement.hidden = false;
        statusElement.textContent = "Validation bootstrap failed before the browser host finished loading.";
    }

    console.error("[BrowserHostValidation]", "Validation bootstrap failed.", error);
}
