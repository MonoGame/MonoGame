// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

const HostStage = Object.freeze({
    HostBootstrap: "HostBootstrap",
    CanvasCreation: "CanvasCreation",
    WebGL2Creation: "WebGL2Creation",
    WasmLoad: "WasmLoad",
    ContentStaging: "ContentStaging",
    ManagedBootstrap: "ManagedBootstrap",
    RuntimeBoundary: "RuntimeBoundary"
});

let activeHost = null;
const contentBase64ByPath = new Map();

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
        this.runtime = null;
        this.bootstrapExports = null;
        this.managedFrameHandle = null;
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
        void host.startAsync();
        return host;
    }

    readConfig(root) {
        const dataset = root.dataset;
        return {
            applicationName: dataset.applicationName || "MonoGame.Web",
            canvasId: dataset.canvasId || "canvas",
            contentBaseUri: dataset.contentBaseUri || "./",
            startupContentManifestUri: dataset.startupContentManifestUri || "Content/content-manifest.txt",
            statusId: dataset.statusId || "monogame-host-status",
            runtimeScriptUri: this.getOptionalConfigValue(dataset.runtimeScriptUri),
            bootstrapAssemblyName: this.getOptionalConfigValue(dataset.bootstrapAssemblyName),
            bootstrapTypeName: this.getOptionalConfigValue(dataset.bootstrapTypeName),
            mainAssemblyName: this.getOptionalConfigValue(dataset.mainAssemblyName)
        };
    }

    getOptionalConfigValue(value) {
        if (value == null || value.length === 0) {
            return null;
        }

        return value;
    }

    async startAsync() {
        try {
            this.logStage(HostStage.HostBootstrap, "Bootstrapping browser host.");
            this.resolveCanvas();
            this.createWebGL2Context();

            if (!this.hasManagedRuntimeConfiguration()) {
                this.logStage(HostStage.RuntimeBoundary, "Host is ready.");
                return;
            }

            await this.loadManagedRuntimeAsync();
            await this.stageStartupContentAsync();
            await this.initializeManagedHostAsync();
            await this.launchManagedApplicationAsync();
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
        if (canvas.id !== "canvas") {
            canvas.id = "canvas";
        }

        globalThis.Module = globalThis.Module || {};
        globalThis.Module.canvas = canvas;
        this.logStage(
            HostStage.CanvasCreation,
            `Canvas '${canvas.id}' ready at ${canvas.width}x${canvas.height}.`);
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

        const context = this.canvas.getContext("webgl2", contextOptions);
        if (context == null) {
            throw new BrowserHostStartupError(
                HostStage.WebGL2Creation,
                "webgl2_unavailable",
                "The browser host could not create a WebGL2 context.");
        }

        this.logStage(HostStage.WebGL2Creation, "WebGL2 context created.");
    }

    hasManagedRuntimeConfiguration() {
        return this.config.runtimeScriptUri != null
            && this.config.bootstrapAssemblyName != null
            && this.config.bootstrapTypeName != null
            && this.config.mainAssemblyName != null;
    }

    async loadManagedRuntimeAsync() {
        this.logStage(HostStage.WasmLoad, "Loading managed runtime.");

        const runtimeModule = await import(this.config.runtimeScriptUri);
        const dotnet = runtimeModule.dotnet ?? globalThis.dotnet;
        if (dotnet == null) {
            throw new BrowserHostStartupError(
                HostStage.WasmLoad,
                "dotnet_runtime_missing",
                "The configured runtime script did not expose a dotnet runtime entry.");
        }

        let runtimeBuilder = dotnet;
        if (typeof runtimeBuilder.withDiagnosticTracing === "function") {
            runtimeBuilder = runtimeBuilder.withDiagnosticTracing(false);
        }

        if (typeof runtimeBuilder.withModuleConfig === "function") {
            runtimeBuilder = runtimeBuilder.withModuleConfig({
                canvas: this.canvas
            });
        }

        if (typeof runtimeBuilder.create !== "function") {
            throw new BrowserHostStartupError(
                HostStage.WasmLoad,
                "dotnet_runtime_shape_unsupported",
                "The configured dotnet runtime does not expose a supported create() API.");
        }

        this.runtime = await runtimeBuilder.create();

        if (typeof this.runtime.getAssemblyExports !== "function") {
            throw new BrowserHostStartupError(
                HostStage.WasmLoad,
                "dotnet_exports_missing",
                "The configured dotnet runtime does not expose getAssemblyExports().");
        }

        if (typeof this.runtime.runMain !== "function" && typeof this.runtime.runMainAndExit !== "function") {
            throw new BrowserHostStartupError(
                HostStage.WasmLoad,
                "dotnet_main_missing",
                "The configured dotnet runtime does not expose runMain() or runMainAndExit().");
        }
    }

    async initializeManagedHostAsync() {
        this.logStage(HostStage.ManagedBootstrap, "Initializing managed browser host.");

        const exportsRoot = await this.runtime.getAssemblyExports(
            this.normalizeAssemblyName(this.config.bootstrapAssemblyName));
        const bootstrapExports = this.resolveExportPath(exportsRoot, this.config.bootstrapTypeName);
        if (bootstrapExports == null) {
            throw new BrowserHostStartupError(
                HostStage.ManagedBootstrap,
                "managed_bootstrap_missing",
                `The configured bootstrap type '${this.config.bootstrapTypeName}' was not found.`);
        }

        if (typeof bootstrapExports.Tick !== "function") {
            throw new BrowserHostStartupError(
                HostStage.ManagedBootstrap,
                "managed_tick_missing",
                "The configured bootstrap type does not expose Tick().");
        }

        this.bootstrapExports = bootstrapExports;
        this.logStage(HostStage.ManagedBootstrap, "Managed browser host initialized.");
    }

    async stageStartupContentAsync() {
        this.logStage(HostStage.ContentStaging, "Staging startup content.");
        await this.stageContentManifestAsync(this.config.startupContentManifestUri);
        this.logStage(HostStage.ContentStaging, "Startup content staged.");
    }

    async stageContentManifestAsync(manifestUri) {
        const fileSystem = this.getFileSystem();
        const requestUri = this.resolveContentUri(manifestUri);
        const response = await fetch(requestUri);
        if (!response.ok) {
            throw new BrowserHostStartupError(
                HostStage.ContentStaging,
                "content_manifest_fetch_failed",
                `The content manifest '${manifestUri}' could not be downloaded. HTTP ${response.status}.`);
        }

        const manifestText = await response.text();
        const contentPaths = manifestText
            .split(/\r?\n/)
            .map((path) => path.trim())
            .filter((path) => path.length > 0)
            .map((path) => this.normalizeVfsPath(path));

        // TODO: Just going to request all startup assets
        //       we might need add a bounded request pool
        //       if projects exceed browser connection limits
        await Promise.all(contentPaths.map(async (contentPath) => {
            const contentResponse = await fetch(this.resolveContentUri(contentPath));
            if (!contentResponse.ok) {
                throw new BrowserHostStartupError(
                    HostStage.ContentStaging,
                    "content_asset_fetch_failed",
                    `The startup asset '${contentPath}' could not be downloaded. HTTP ${contentResponse.status}.`);
            }

            const directoryPath = contentPath.substring(0, contentPath.lastIndexOf("/"));
            if (directoryPath.length > 0) {
                fileSystem.mkdirTree(`/${directoryPath}`);
            }

            fileSystem.writeFile(`/${contentPath}`, new Uint8Array(await contentResponse.arrayBuffer()));
        }));
    }

    getFileSystem() {
        const fileSystem = this.runtime?.Module?.FS;
        if (fileSystem == null
            || typeof fileSystem.mkdirTree !== "function"
            || typeof fileSystem.writeFile !== "function") {
            throw new BrowserHostStartupError(
                HostStage.ContentStaging,
                "emscripten_filesystem_missing",
                "The managed runtime did not expose an Emscripten filesystem capable of staging content.");
        }

        return fileSystem;
    }

    normalizeVfsPath(path) {
        const normalizedPath = normalizeContentPath(path);
        if (normalizedPath == null
            || !normalizedPath.startsWith("Content/")
            || normalizedPath.includes("/../")
            || normalizedPath.endsWith("/..")) {
            throw new BrowserHostStartupError(
                HostStage.ContentStaging,
                "content_manifest_path_invalid",
                `The content manifest contains an invalid path '${path}'.`);
        }

        return normalizedPath;
    }

    resolveContentUri(relativePath) {
        const normalizedPath = normalizeContentPath(relativePath);
        if (normalizedPath == null) {
            throw new BrowserHostStartupError(
                HostStage.ContentStaging,
                "content_uri_invalid",
                "The browser host received an empty content path.");
        }

        return new URL(normalizedPath, new URL(this.config.contentBaseUri, document.baseURI)).toString();
    }

    async launchManagedApplicationAsync() {
        this.logStage(HostStage.RuntimeBoundary, "Launching managed application.");

        const mainAssemblyName = this.normalizeAssemblyName(this.config.mainAssemblyName);
        let runMainPromise;

        if (typeof this.runtime.runMain === "function") {
            runMainPromise = Promise.resolve(this.runtime.runMain(mainAssemblyName, []));
        }
        else {
            runMainPromise = Promise.resolve(this.runtime.runMainAndExit(mainAssemblyName, []));
        }

        this.scheduleManagedFrame();
        this.logStage(HostStage.RuntimeBoundary, "Managed application launched.");
        await runMainPromise;
    }

    normalizeAssemblyName(assemblyName) {
        if (assemblyName.endsWith(".dll")) {
            return assemblyName;
        }

        return `${assemblyName}.dll`;
    }

    resolveExportPath(root, path) {
        const parts = path.split(".");
        let current = root;

        for (const part of parts) {
            if (current == null) {
                return null;
            }

            current = current[part];
        }

        return current ?? null;
    }

    scheduleManagedFrame() {
        this.managedFrameHandle = requestAnimationFrame(async () => {
            this.managedFrameHandle = null;

            try {
                const shouldContinue = await this.bootstrapExports.Tick();
                if (shouldContinue) {
                    this.scheduleManagedFrame();
                }
                else {
                    this.logStage(HostStage.RuntimeBoundary, "Managed run loop ended.");
                }
            }
            catch (error) {
                this.handleStartupFailure(
                    new BrowserHostStartupError(
                        HostStage.ManagedBootstrap,
                        "managed_tick_failed",
                        error instanceof Error ? error.message : String(error)));
            }
        });
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

function normalizeContentPath(relativePath) {
    if (relativePath == null || relativePath.length === 0) {
        return null;
    }

    return relativePath.replace(/\\/g, "/");
}

function resolveContentUri(relativePath) {
    const normalizedPath = normalizeContentPath(relativePath);
    if (normalizedPath == null) {
        return null;
    }

    const contentBaseUri = activeHost?.config?.contentBaseUri || "./";
    return new URL(normalizedPath, new URL(contentBaseUri, document.baseURI)).toString();
}

function tryGetContentBase64(relativePath) {
    const normalizedPath = normalizeContentPath(relativePath);
    if (normalizedPath == null) {
        return null;
    }

    if (contentBase64ByPath.has(normalizedPath)) {
        return contentBase64ByPath.get(normalizedPath);
    }

    const requestUri = resolveContentUri(normalizedPath);
    if (requestUri == null) {
        return null;
    }

    // TODO: Using synchronous XHR preserves TitleContainer semantics for now.
    //       We can replace this with an async browser asset pipeline once the
    //       framework loading can do async boundaries.
    const request = new XMLHttpRequest();
    request.open("GET", requestUri, false);
    request.overrideMimeType("text/plain; charset=x-user-defined");

    try {
        request.send();
    }
    catch {
        return null;
    }

    if (request.status < 200 || request.status >= 300) {
        return null;
    }

    const encodedContent = encodeResponseTextAsBase64(request.responseText);
    contentBase64ByPath.set(normalizedPath, encodedContent);
    return encodedContent;
}

function encodeResponseTextAsBase64(responseText) {
    let binary = "";

    for (let index = 0; index < responseText.length; index += 1) {
        binary += String.fromCharCode(responseText.charCodeAt(index) & 0xff);
    }

    return btoa(binary);
}

globalThis.MonoGameWebHost = {
    getActiveHost: () => activeHost,
    stageContentManifestAsync: (manifestUri) => activeHost?.stageContentManifestAsync(manifestUri),
    tryGetContentBase64
};

MonoGameWebHost.bootFromDocument(document);
