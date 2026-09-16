// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

const query = new URLSearchParams(globalThis.location.search);
const canvasResizePolicy = query.get("canvas-resize-policy") ?? "Adaptive";

function dispatchKeyboardEvent(type, code, key) {
    globalThis.dispatchEvent(new KeyboardEvent(type, {
        code,
        key
    }));
}

globalThis.MonoGameWebHostConfiguration = {
    runtimeScriptUri: "./_framework/dotnet.js",
    hostExportsTypeName: "BrowserHostValidation.BrowserHostValidationHostExports",
    mainAssemblyName: "BrowserHostValidation.dll",
    canvasResizePolicy
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
    requestKeyboardKeyDown(code, key) {
        dispatchKeyboardEvent("keydown", code, key);
    },
    requestKeyboardKeyUp(code, key) {
        dispatchKeyboardEvent("keyup", code, key);
    },
    requestKeyboardFocusLoss() {
        const canvas = document.getElementById("canvas");
        if (canvas == null) {
            throw new Error("The validation canvas was not found.");
        }

        canvas.dispatchEvent(new Event("blur"));
    },
    requestKeyboardFocusRestore() {
        const canvas = document.getElementById("canvas");
        if (canvas == null) {
            throw new Error("The validation canvas was not found.");
        }

        canvas.dispatchEvent(new Event("focus"));
    },
    isManualFocusValidationEnabled() {
        return !query.has("auto-exit") || query.has("fullscreen") || query.has("embed-fullscreen");
    },
    isFullscreenValidationEnabled() {
        return query.has("fullscreen");
    },
    isEmbeddingFullscreenValidationEnabled() {
        return query.has("embed-fullscreen");
    },
    isAdaptiveCanvasResizeValidationEnabled() {
        return canvasResizePolicy === "Adaptive";
    },
    isKeyboardValidationEnabled() {
        return query.has("keyboard");
    }
};

try {
    await import("./monogame-web-host.js");

    const canvas = document.getElementById("canvas");
    if (canvas != null
        && canvasResizePolicy === "Adaptive"
        && !query.has("embed-fullscreen")) {
        canvas.style.width = "960px";
        canvas.style.height = "540px";
    }

    if (query.has("fullscreen")) {
        document.addEventListener("fullscreenchange", () => {
            const fullscreen = document.fullscreenElement === canvas;
            globalThis.MonoGameWebHostValidation.reportPhase(
                "browserFullscreenChanged",
                `Browser canvas fullscreen is ${fullscreen ? "active" : "inactive"}.`);
        });
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
