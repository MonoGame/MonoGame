// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

globalThis.MonoGameWebHostConfiguration = {
    runtimeScriptUri: "./_framework/dotnet.js",
    hostExportsTypeName: "BrowserHostValidation.BrowserHostValidationHostExports",
    mainAssemblyName: "BrowserHostValidation.dll"
};

globalThis.MonoGameWebHostValidation = {
    reportPhase(phaseName, message) {
        console.info("[BrowserHostValidation]", phaseName, message);
    },
    validateCanvasSize(drawingBufferWidth, drawingBufferHeight, cssWidth, cssHeight) {
        const canvas = document.getElementById("canvas");
        if (canvas == null) {
            throw new Error("The validation canvas was not found.");
        }

        const canvasBounds = canvas.getBoundingClientRect();
        if (canvas.width !== drawingBufferWidth
            || canvas.height !== drawingBufferHeight
            || Math.round(canvasBounds.width) !== cssWidth
            || Math.round(canvasBounds.height) !== cssHeight) {
            throw new Error(
                `Expected canvas drawing buffer ${drawingBufferWidth}x${drawingBufferHeight} and CSS size ${cssWidth}x${cssHeight}, but found drawing buffer ${canvas.width}x${canvas.height} and CSS size ${canvasBounds.width}x${canvasBounds.height}.`);
        }
    },
    requestFocusLifecycleValidation() {
        const canvas = document.getElementById("canvas");
        if (canvas == null) {
            throw new Error("The validation canvas was not found.");
        }

        canvas.dispatchEvent(new Event("blur"));
        canvas.dispatchEvent(new Event("focus"));
    },
    isManualFocusValidationEnabled() {
        return !new URLSearchParams(globalThis.location.search).has("auto-exit");
    }
};

try {
    await import("./monogame-web-host.js");

    const canvas = document.getElementById("canvas");
    if (canvas != null) {
        canvas.style.width = "960px";
        canvas.style.height = "540px";
    }
}
catch (error) {
    const statusElement = document.getElementById("monogame-host-status");
    if (statusElement != null) {
        statusElement.hidden = false;
        statusElement.textContent = "Validation bootstrap failed before the browser host finished loading.";
    }

    console.error("[BrowserHostValidation]", "Validation bootstrap failed.", error);
}
