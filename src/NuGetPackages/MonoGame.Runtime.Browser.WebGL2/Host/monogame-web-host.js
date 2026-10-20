// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

import { BrowserAudio } from "./browser-audio.js";
import { BrowserAccelerometer } from "./browser-accelerometer.js";
import { BrowserContent } from "./browser-content.js";
import { BrowserHostError, CanvasResizePolicy, HostStage } from "./browser-host-common.js";
import { BrowserWindow } from "./browser-window.js";

let activeHost = null;

function getCanvasResizePolicy(value) {
    if (value === CanvasResizePolicy.Adaptive
        || value === CanvasResizePolicy.Project
        || value === CanvasResizePolicy.None) {
        return value;
    }

    throw new BrowserHostError(HostStage.HostBootstrap, "canvas_resize_policy_invalid", `Canvas resize policy '${value}' is invalid. Use Adaptive, Project, or None.`);
}

/**
 * The configuration resolved from package defaults and project settings.
 *
 * @typedef {object} BrowserHostConfiguration
 * @property {string} applicationName
 * @property {HTMLCanvasElement} canvas
 * @property {string} contentBaseUri
 * @property {string} startupContentManifestUri
 * @property {string} runtimeScriptUri
 * @property {"Adaptive" | "Project" | "None"} canvasResizePolicy
 * @property {boolean} pointerLockEnabled
 */

/**
 * The application-owned settings used to start the browser host.
 *
 * @typedef {object} BrowserHostStartOptions
 * @property {HTMLElement} root Element that contains the game canvas.
 * @property {HTMLCanvasElement} canvas Canvas supplied to the browser runtime.
 * @property {string=} applicationName Application name used for host diagnostics.
 * @property {string=} runtimeScriptUri URI of the .NET browser runtime script.
 * @property {string=} contentBaseUri Base URI used to resolve published content.
 * @property {string=} startupContentManifestUri URI of the startup content manifest.
 * @property {HTMLElement | null=} statusElement Element used for terminal failure text.
 * @property {CanvasResizePolicy} canvasResizePolicy Canvas resize behavior.
 * @property {boolean=} pointerLock Enables the browser Pointer Lock compatibility path.
 * @property {(status: { stage: string, message: string }) => void=} onStatus Receives startup status messages.
 * @property {() => void=} onFirstFrame Receives the first successful managed frame notification.
 * @property {(error: { stage: string, code: string, message: string }) => void=} onError Receives terminal host failures.
 */

/** Coordinates browser startup and delegates browser-specific behavior to private host modules. */
class MonoGameWebHost {
    /**
     * @param {HTMLElement} root Root host element.
     * @param {BrowserHostConfiguration} config Host configuration.
     * @param {HTMLElement | null} statusElement Host failure element.
     * @param {{ onStatus: Function | null, onFirstFrame: Function | null, onError: Function | null }} callbacks Presentation callbacks.
     */
    constructor(root, config, statusElement, callbacks) {
        this.root = root;
        this.config = config;
        this.root.dataset.canvasResizePolicy = this.config.canvasResizePolicy;
        this.statusElement = statusElement;
        this.callbacks = callbacks;
        this.runtime = null;
        this.native = null;
        this.browserGameExports = null;
        this.contextLost = false;
        this.firstFrameReported = false;
        this.managedFailure = null;
        this.isDisposed = false;
        this.window = new BrowserWindow(this);
        this.accelerometer = new BrowserAccelerometer(this);
        this.audio = new BrowserAudio(this);
        this.content = new BrowserContent(this);
    }

    /**
     * Starts the browser host using an explicit application-owned configuration object.
     *
     * @param {BrowserHostStartOptions} options Application-owned host configuration.
     * @returns {MonoGameWebHost} Host coordinator before startup completes.
     */
    static start(options) {
        if (options == null || typeof options !== "object") {
            throw new BrowserHostError(HostStage.HostBootstrap, "host_configuration_invalid", "The browser host requires a configuration object.");
        }

        const root = options.root;
        if (!(root instanceof HTMLElement)) {
            throw new BrowserHostError(HostStage.HostBootstrap, "host_root_invalid", "The browser host requires an HTMLElement root.");
        }

        const canvas = options.canvas;
        if (!(canvas instanceof HTMLCanvasElement)) {
            throw new BrowserHostError(HostStage.HostBootstrap, "host_canvas_invalid", "The browser host requires an HTMLCanvasElement canvas.");
        }

        if (canvas.id !== "canvas") {
            throw new BrowserHostError(HostStage.HostBootstrap, "host_canvas_id_invalid", "The browser host canvas must have id 'canvas'.");
        }

        if (!root.contains(canvas)) {
            throw new BrowserHostError(HostStage.HostBootstrap, "host_canvas_outside_root", "The browser host canvas must be contained by its root element.");
        }

        const statusElement = options.statusElement ?? null;
        if (statusElement != null && !(statusElement instanceof HTMLElement)) {
            throw new BrowserHostError(HostStage.HostBootstrap, "host_status_element_invalid", "The browser host statusElement must be an HTMLElement.");
        }

        const onStatus = options.onStatus ?? null;
        if (onStatus != null && typeof onStatus !== "function") {
            throw new BrowserHostError(HostStage.HostBootstrap, "configuration_callback_invalid", "The onStatus setting must be a function.");
        }

        const onFirstFrame = options.onFirstFrame ?? null;
        if (onFirstFrame != null && typeof onFirstFrame !== "function") {
            throw new BrowserHostError(HostStage.HostBootstrap, "configuration_callback_invalid", "The onFirstFrame setting must be a function.");
        }

        const onError = options.onError ?? null;
        if (onError != null && typeof onError !== "function") {
            throw new BrowserHostError(HostStage.HostBootstrap, "configuration_callback_invalid", "The onError setting must be a function.");
        }

        const canvasResizePolicy = options.canvasResizePolicy ?? CanvasResizePolicy.Adaptive;
        const config = {
            applicationName: options.applicationName ?? "MonoGame.Web",
            canvas,
            contentBaseUri: options.contentBaseUri ?? "./",
            startupContentManifestUri: options.startupContentManifestUri ?? "_monogame/content-manifest.txt",
            runtimeScriptUri: options.runtimeScriptUri ?? "./_framework/dotnet.js",
            canvasResizePolicy: getCanvasResizePolicy(canvasResizePolicy),
            pointerLockEnabled: options.pointerLock ?? false
        };
        const callbacks = {
            onStatus,
            onFirstFrame,
            onError
        };

        if (activeHost != null) {
            throw new BrowserHostError(
                HostStage.HostBootstrap,
                "host_already_active",
                "A MonoGame.Web host is already active. Wait for it to exit before starting another host.");
        }

        const host = new MonoGameWebHost(root, config, statusElement, callbacks);
        activeHost = host;
        void host.startAsync();
        return host;
    }

    /**
     * Starts the browser host and reports startup errors.
     *
     * @returns {Promise<void>} Completes when the application exits or a startup error is reported.
     */
    async startAsync() {
        try {
            this.logStage(HostStage.HostBootstrap, "Bootstrapping browser host.");
            this.resolveCanvas();
            this.window.observeContextLoss(() => this.handleContextLost());
            await this.initializeManagedRuntimeAsync();
            await this.content.stageStartupContentAsync();
            await this.initializeBrowserGameExportsAsync();
            await this.launchManagedApplicationAsync();
        }
        catch (error) {
            if (!this.isDisposed) {
                this.handleHostFailure(this.managedFailure ?? error);
            }
        }
        finally {
            this.dispose();
        }
    }

    /** Resolves the game canvas and listens for browser-activation gestures. */
    resolveCanvas() {
        this.window.resolveCanvas();
        this.audio.observeActivation(this.window.canvas);
    }

    /**
     * Loads the managed runtime, connects it to the game canvas, and starts browser event handling.
     *
     * @returns {Promise<void>} Completes when the runtime can start the application.
     * @throws {BrowserHostError} When the configured runtime cannot be loaded or started.
     */
    async initializeManagedRuntimeAsync() {
        this.logStage(HostStage.WasmLoad, "Loading managed runtime.");
        const runtimeModule = await import(new URL(this.config.runtimeScriptUri, document.baseURI).toString());
        const dotnet = runtimeModule.dotnet ?? globalThis.dotnet;
        if (dotnet == null) {
            throw new BrowserHostError(HostStage.WasmLoad, "dotnet_runtime_missing", "The configured runtime script did not expose a dotnet runtime entry.");
        }

        const runtimeBuilder = dotnet
            .withDiagnosticTracing(false)
            .withModuleConfig({ canvas: this.window.canvas });
        this.runtime = await runtimeBuilder.create();
        this.native = this.resolveNativeExports(this.runtime.Module);
        this.window.configurePointerLock();
        this.window.observeCanvasSize();
        this.window.observeBrowserLifecycle();
        this.window.observeFullscreen();
        this.window.observeFileDrop(this.native.fileSystem);
    }

    /**
     * Resolves the Emscripten callbacks used by browser services.
     *
     * @param {object | null | undefined} module Emscripten runtime module.
     * @returns {object} Immutable native browser services.
     * @throws {BrowserHostError} When a required native export is unavailable.
     */
    resolveNativeExports(module) {
        if (module == null) {
            throw new BrowserHostError(HostStage.WasmLoad, "native_module_missing", "The managed runtime did not expose an Emscripten module.");
        }

        const requiredExports = {
            _MGM_Web_NotifySongEvent: module._MGM_Web_NotifySongEvent,
            _MGP_Web_NotifyAccelerometerReading: module._MGP_Web_NotifyAccelerometerReading,
            _MGP_Web_NotifyAccelerometerState: module._MGP_Web_NotifyAccelerometerState,
            _MGP_Web_NotifyCanvasResize: module._MGP_Web_NotifyCanvasResize,
            _MGP_Web_NotifyFileDrop: module._MGP_Web_NotifyFileDrop,
            _MGP_Web_NotifyFileDropComplete: module._MGP_Web_NotifyFileDropComplete,
            _MGP_Web_NotifyFocusChange: module._MGP_Web_NotifyFocusChange,
            _MGP_Web_NotifyFullscreenChange: module._MGP_Web_NotifyFullscreenChange,
            _MGP_Web_NotifyFullscreenFailure: module._MGP_Web_NotifyFullscreenFailure,
            _MGP_Web_SetCanvasResizeManaged: module._MGP_Web_SetCanvasResizeManaged,
            _MGP_Web_SetPointerLockEnabled: module._MGP_Web_SetPointerLockEnabled,
            ccall: module.ccall
        };

        for (const [name, value] of Object.entries(requiredExports)) {
            if (typeof value !== "function") {
                throw new BrowserHostError(HostStage.WasmLoad, "native_export_missing", `The managed runtime does not expose the native '${name}' callback.`);
            }
        }

        const fileSystem = module.FS;
        if (fileSystem == null
            || typeof fileSystem.mkdirTree !== "function"
            || typeof fileSystem.rmdir !== "function"
            || typeof fileSystem.unlink !== "function"
            || typeof fileSystem.writeFile !== "function") {
            throw new BrowserHostError(HostStage.WasmLoad, "emscripten_filesystem_missing", "The managed runtime did not expose an Emscripten filesystem capable of staging and releasing content.");
        }

        return Object.freeze({
            fileSystem: Object.freeze({
                mkdirTree: fileSystem.mkdirTree.bind(fileSystem),
                rmdir: fileSystem.rmdir.bind(fileSystem),
                unlink: fileSystem.unlink.bind(fileSystem),
                writeFile: fileSystem.writeFile.bind(fileSystem)
            }),
            notifyAccelerometerReading: module._MGP_Web_NotifyAccelerometerReading.bind(module),
            notifyAccelerometerState: module._MGP_Web_NotifyAccelerometerState.bind(module),
            notifyCanvasResize: module._MGP_Web_NotifyCanvasResize.bind(module),
            notifyFileDrop: (filePath) => module.ccall("MGP_Web_NotifyFileDrop", null, ["string"], [filePath]),
            notifyFileDropComplete: module._MGP_Web_NotifyFileDropComplete.bind(module),
            notifyFocusChange: module._MGP_Web_NotifyFocusChange.bind(module),
            notifyFullscreenChange: module._MGP_Web_NotifyFullscreenChange.bind(module),
            notifyFullscreenFailure: module._MGP_Web_NotifyFullscreenFailure.bind(module),
            notifySongEvent: module._MGM_Web_NotifySongEvent.bind(module),
            setCanvasResizeManaged: module._MGP_Web_SetCanvasResizeManaged.bind(module),
            setPointerLockEnabled: module._MGP_Web_SetPointerLockEnabled.bind(module)
        });
    }

    /**
     * Resolves the fixed browser runtime exports used for context-loss handling.
     *
     * @returns {Promise<void>} Completes when the browser runtime can handle context loss.
     * @throws {BrowserHostError} When the browser runtime exports are missing.
     */
    async initializeBrowserGameExportsAsync() {
        this.logStage(HostStage.ManagedExports, "Initializing managed host exports.");
        const exportsRoot = await this.runtime.getAssemblyExports(`${this.getMainAssemblyName()}.dll`);
        const browserGameExports = this.resolveExportPath(exportsRoot, "MonoGame.Runtime.Browser.WebGL2.BrowserGame");
        if (browserGameExports == null) {
            throw new BrowserHostError(HostStage.ManagedExports, "browser_game_exports_missing", "The browser runtime does not expose BrowserGame.");
        }

        if (typeof browserGameExports.NotifyContextLost !== "function") {
            throw new BrowserHostError(HostStage.ManagedExports, "browser_context_loss_export_missing", "The browser runtime does not expose BrowserGame.NotifyContextLost().");
        }

        this.browserGameExports = browserGameExports;
        this.logStage(HostStage.ManagedExports, "Managed host exports initialized.");
    }

    /**
     * Starts the managed application.
     *
     * @returns {Promise<void>} Completes when the managed application exits.
     */
    async launchManagedApplicationAsync() {
        this.logStage(HostStage.RuntimeBoundary, "Launching managed application.");
        const runMainPromise = Promise.resolve(this.runtime.runMain());

        this.logStage(HostStage.RuntimeBoundary, "Managed application launched.");
        await runMainPromise;
    }

    /**
     * Gets the application's main assembly from the .NET runtime configuration.
     *
     * @returns {string} Assembly name.
     * @throws {BrowserHostError} When the runtime does not expose a main assembly.
     */
    getMainAssemblyName() {
        const mainAssemblyName = this.runtime.getConfig().mainAssemblyName;
        if (typeof mainAssemblyName !== "string" || mainAssemblyName.trim().length === 0) {
            throw new BrowserHostError(HostStage.WasmLoad, "runtime_main_assembly_missing", "The .NET runtime configuration does not specify a main assembly.");
        }

        return mainAssemblyName;
    }

    /**
     * Returns a managed export from its dot separated path.
     *
     * @param {object} root Managed exports root.
     * @param {string} path Export path to resolve.
     * @returns {object | null} Requested export, or `null` when the path is missing.
     */
    resolveExportPath(root, path) {
        let current = root;
        for (const part of path.split(".")) {
            if (current == null) {
                return null;
            }

            current = current[part];
        }

        return current ?? null;
    }

    /** Reports the first frame completed by the Emscripten-managed loop. */
    notifyManagedFirstFrame() {
        if (this.isDisposed || this.firstFrameReported) {
            return;
        }

        this.firstFrameReported = true;
        this.notify(this.callbacks.onFirstFrame, undefined, "onFirstFrame");
    }

    /** Records a managed Emscripten-loop failure until runMain completes. */
    reportManagedFailure(message, details) {
        if (this.isDisposed || this.managedFailure != null) {
            return;
        }

        this.managedFailure = new BrowserHostError(
            HostStage.RuntimeBoundary,
            "managed_application_failed",
            message,
            { cause: new Error(details) });
    }

    /** Stops the browser game loop after an unrecoverable WebGL context loss. */
    handleContextLost() {
        if (this.contextLost) {
            return;
        }

        this.contextLost = true;
        if (this.browserGameExports != null) {
            this.browserGameExports.NotifyContextLost();
        }

        this.handleHostFailure(new BrowserHostError(
            HostStage.ContextLoss,
            "webgl_context_lost",
            "The WebGL2 context was lost. MonoGame.Web cannot recover this application; reload the page."));
        this.dispose();
    }

    /** Releases browser-owned resources after the application exits or the host fails. */
    dispose() {
        if (this.isDisposed) {
            return;
        }

        this.isDisposed = true;
        this.window.dispose();
        this.accelerometer.dispose();
        this.audio.dispose();
        this.browserGameExports = null;
        this.native = null;
        this.runtime = null;

        if (activeHost === this) {
            activeHost = null;
        }

        this.logStage(HostStage.RuntimeBoundary, "Browser host disposed.");
    }

    /**
     * Displays and logs a host failure.
     *
     * @param {unknown} error Error to report.
     */
    handleHostFailure(error) {
        const startupError = error instanceof BrowserHostError
            ? error
            : new BrowserHostError(
                HostStage.HostBootstrap,
                "unexpected_error",
                error instanceof Error ? error.message : String(error),
                { cause: error });
        const message = `${startupError.stage}: ${startupError.code} - ${startupError.message}`;
        if (this.statusElement != null) {
            this.statusElement.hidden = false;
            this.statusElement.textContent = message;
        }

        console.error("[MonoGame.Web Host]", message, startupError, startupError.cause);
        this.notify(this.callbacks.onError, {
            stage: startupError.stage,
            code: startupError.code,
            message: startupError.message
        }, "onError");
    }

    /**
     * Logs host progress.
     *
     * @param {string} stage Startup stage.
     * @param {string} message Progress message.
     */
    logStage(stage, message) {
        console.info("[MonoGame.Web Host]", stage, message);
        this.notify(this.callbacks.onStatus, { stage, message }, "onStatus");
    }

    /**
     * Delivers a presentation notification without allowing page code to
     * interrupt browser-host lifecycle handling.
     *
     * @param {Function | null | undefined} callback Callback to invoke.
     * @param {unknown} value Callback value.
     * @param {string} name Callback setting name.
     */
    notify(callback, value, name) {
        if (callback == null) {
            return;
        }

        try {
            callback(value);
        }
        catch (error) {
            console.error("[MonoGame.Web Host]", `${name} callback failed.`, error);
        }
    }
}

/**
 * Starts the browser host using an explicit application-owned configuration object.
 *
 * @param {BrowserHostStartOptions} options Application-owned host configuration.
 * @returns {MonoGameWebHost} Host coordinator before startup completes.
 */
export function start(options) {
    return MonoGameWebHost.start(options);
}

/** Provides host services called by the browser native runtime. */
globalThis.MonoGameWebHost = {
    getActiveHost: () => activeHost,
    notifyManagedFirstFrame: () => activeHost?.notifyManagedFirstFrame(),
    notifyManagedFailure: (message, details) => activeHost?.reportManagedFailure(message, details),
    stageAssetPackAsync: async (assetPackName) => {
        if (activeHost == null) {
            throw new BrowserHostError(HostStage.ContentStaging, "asset_pack_host_unavailable", "The browser host is not available to stage an asset pack.");
        }

        await activeHost.content.stageAssetPackAsync(assetPackName);
    },
    stageContentManifestAsync: (manifestUri) => activeHost?.content.stageContentManifestAsync(manifestUri)
};
