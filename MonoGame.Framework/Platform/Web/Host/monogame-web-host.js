// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

const HostStage = Object.freeze({
    HostBootstrap: "HostBootstrap",
    CanvasCreation: "CanvasCreation",
    WebGL2Creation: "WebGL2Creation",
    RuntimeBoundary: "RuntimeBoundary"
});

let activeHost = null;
let nextHandleId = 1;
const hostObjectRegistry = new Map();

class BrowserHostStartupError extends Error {
    constructor(stage, code, message) {
        super(message);
        this.name = "BrowserHostStartupError";
        this.stage = stage;
        this.code = code;
    }
}

class MonoGameWebHost {
    constructor(root) {
        this.root = root;
        this.config = this.readConfig(root);
        this.statusElement = document.getElementById(this.config.statusId);
        this.canvas = null;
        this.gl = null;
        this.canvasHandle = null;
        this.graphicsContextHandle = null;
    }

    static bootFromDocument(document_) {
        const root = document_.getElementById("monogame-host");
        if (root == null) {
            throw new BrowserHostStartupError(
                HostStage.HostBootstrap,
                "host_root_missing",
                "The browser host root '#monogame-host' was not found.");
        }

        const host = new MonoGameWebHost(root);
        activeHost = host;
        host.start();
        return host;
    }

    readConfig(root) {
        const dataset = root.dataset;
        return {
            applicationName: dataset.applicationName || "MonoGame.Web",
            canvasId: dataset.canvasId || "monogame-canvas",
            contentBaseUri: dataset.contentBaseUri || "./",
            statusId: dataset.statusId || "monogame-host-status"
        };
    }

    start() {
        try {
            this.logStage(HostStage.HostBootstrap, "Bootstrapping browser host.");
            this.resolveCanvas();
            this.createWebGL2Context();
            this.logStage(HostStage.RuntimeBoundary, "Host is ready.");
        }
        catch (error) {
            this.handleStartupFailure(error);
        }
    }

    resolveCanvas() {
        this.logStage(HostStage.CanvasCreation, "Resolving host canvas.");

        let canvas = document.getElementById(this.config.canvasId);
        if (canvas == null) {
            canvas = document.createElement("canvas");
            canvas.id = this.config.canvasId;
            canvas.width = 1280;
            canvas.height = 720;
            canvas.setAttribute("aria-label", "MonoGame browser host canvas");
            this.root.appendChild(canvas);
        }

        this.canvas = canvas;
        this.canvasHandle = createOpaqueHandle("canvas", canvas);
        this.logStage(
            HostStage.CanvasCreation,
            `Canvas '${this.config.canvasId}' ready at ${canvas.width}x${canvas.height}.`);
    }

    createWebGL2Context() {
        this.logStage(HostStage.WebGL2Creation, "Requesting WebGL2 context.");

        const contextOptions = {
            alpha: true,
            antialias: false,
            depth: true,
            desynchronized: false,
            powerPreference: "high-performance",
            premultipliedAlpha: true,
            preserveDrawingBuffer: false,
            stencil: true
        };

        const gl = this.canvas.getContext("webgl2", contextOptions);
        if (gl == null) {
            throw new BrowserHostStartupError(
                HostStage.WebGL2Creation,
                "webgl2_unavailable",
                "The browser host could not create a WebGL2 context.");
        }

        this.gl = gl;
        this.graphicsContextHandle = createOpaqueHandle("webgl2", gl);
        this.logStage(HostStage.WebGL2Creation, "WebGL2 context created.");
    }

    handleStartupFailure(error) {
        const startupError =
            error instanceof BrowserHostStartupError
                ? error
                : new BrowserHostStartupError(
                    HostStage.HostBootstrap,
                    "unexpected_error",
                    error instanceof Error ? error.message : String(error));

        const message = `${startupError.stage}: ${startupError.code} - ${startupError.message}`;
        if (this.statusElement != null) {
            this.statusElement.hidden = false;
            this.statusElement.textContent = message;
        }

        console.error("[MonoGame.Web Host]", message, startupError);
    }

    logStage(stage, message) {
        console.info("[MonoGame.Web Host]", stage, message);
    }
}

function createOpaqueHandle(kind, value) {
    const handle = `${kind}:${nextHandleId}`;
    nextHandleId += 1;
    hostObjectRegistry.set(handle, value);
    return handle;
}

function resolveOpaqueHandle(handle) {
    return hostObjectRegistry.get(handle) ?? null;
}

function runSelfCheck() {
    const marker = { ok: true };
    const handle = createOpaqueHandle("self-check", marker);

    console.assert(
        resolveOpaqueHandle(handle) === marker,
        "[MonoGame.Web Host] Opaque handle registry self-check failed.");

    hostObjectRegistry.delete(handle);
}

globalThis.MonoGameWebHost = {
    getActiveHost: () => activeHost,
    resolveOpaqueHandle
};

runSelfCheck();
MonoGameWebHost.bootFromDocument(document);
