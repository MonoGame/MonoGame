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
 * @property {string} hostExportsTypeName
 * @property {string} mainAssemblyName
 * @property {"Adaptive" | "Project" | "None"} canvasResizePolicy
 * @property {boolean} pointerLockEnabled
 */

/**
 * The application-owned settings used to start the browser host.
 *
 * @typedef {object} BrowserHostStartOptions
 * @property {HTMLElement} root Element that contains the game canvas.
 * @property {HTMLCanvasElement} canvas Canvas supplied to the browser runtime.
 * @property {{ assemblyName: string, hostExportsTypeName: string }} managed Managed application entry points.
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
        this.hostExports = null;
        this.managedFrameHandle = null;
        this.contextLost = false;
        this.firstFrameReported = false;
        this.isDisposed = false;
        this.window = new BrowserWindow(this.config, () => this.runtime);
        this.accelerometer = new BrowserAccelerometer(() => this.runtime);
        this.audio = new BrowserAudio(root, this.config.contentBaseUri, () => this.runtime);
        this.content = new BrowserContent(this.config, () => this.runtime, (stage, message) => this.logStage(stage, message));
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

        if (!root.contains(canvas)) {
            throw new BrowserHostError(HostStage.HostBootstrap, "host_canvas_outside_root", "The browser host canvas must be contained by its root element.");
        }

        const managed = options.managed;
        if (managed == null || typeof managed !== "object") {
            throw new BrowserHostError(HostStage.HostBootstrap, "managed_configuration_invalid", "The browser host requires a managed configuration object.");
        }

        const assemblyName = managed.assemblyName;
        if (typeof assemblyName !== "string" || assemblyName.trim().length === 0) {
            throw new BrowserHostError(HostStage.HostBootstrap, "managed_configuration_invalid", "The managed.assemblyName setting must be a non-empty string.");
        }

        const hostExportsTypeName = managed.hostExportsTypeName;
        if (typeof hostExportsTypeName !== "string" || hostExportsTypeName.trim().length === 0) {
            throw new BrowserHostError(HostStage.HostBootstrap, "managed_configuration_invalid", "The managed.hostExportsTypeName setting must be a non-empty string.");
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
            startupContentManifestUri: options.startupContentManifestUri ?? "Content/content-manifest.txt",
            runtimeScriptUri: options.runtimeScriptUri ?? "./_framework/dotnet.js",
            hostExportsTypeName,
            mainAssemblyName: assemblyName,
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
            this.validateManagedRuntimeConfiguration();
            this.resolveCanvas();
            this.window.observeContextLoss(() => this.handleContextLost());
            await this.initializeManagedRuntimeAsync();
            await this.content.stageStartupContentAsync();
            await this.initializeManagedExportsAsync();
            await this.launchManagedApplicationAsync();
        }
        catch (error) {
            this.handleHostFailure(error);
        }
        finally {
            this.dispose();
        }
    }

    /** Resolves the game canvas and listens for browser-activation gestures. */
    resolveCanvas() {
        this.window.resolveCanvas((stage, message) => this.logStage(stage, message));
        this.accelerometer.observePermissionGesture(this.window.canvas);
        this.audio.observeActivation(this.window.canvas);
    }

    /** Validates the project owned managed runtime configuration before browser resources are created. */
    validateManagedRuntimeConfiguration() {
        const missingConfigurationNames = [];
        if (this.config.runtimeScriptUri == null) {
            missingConfigurationNames.push("runtimeScriptUri");
        }

        if (this.config.hostExportsTypeName == null) {
            missingConfigurationNames.push("hostExportsTypeName");
        }

        if (this.config.mainAssemblyName == null) {
            missingConfigurationNames.push("mainAssemblyName");
        }

        if (missingConfigurationNames.length > 0) {
            throw new BrowserHostError(HostStage.HostBootstrap, "managed_runtime_configuration_missing", `The browser host requires ${missingConfigurationNames.join(", ")}.`);
        }
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

        let runtimeBuilder = dotnet;
        if (typeof runtimeBuilder.withDiagnosticTracing === "function") {
            runtimeBuilder = runtimeBuilder.withDiagnosticTracing(false);
        }

        if (typeof runtimeBuilder.withModuleConfig === "function") {
            runtimeBuilder = runtimeBuilder.withModuleConfig({ canvas: this.window.canvas });
        }

        if (typeof runtimeBuilder.create !== "function") {
            throw new BrowserHostError(HostStage.WasmLoad, "dotnet_runtime_shape_unsupported", "The configured dotnet runtime does not expose a supported create() API.");
        }

        this.runtime = await runtimeBuilder.create();
        this.window.configurePointerLock();
        this.window.observeCanvasSize();
        this.window.observeBrowserLifecycle();
        this.window.observeFullscreen();
        this.window.observeFileDrop(this.content.getFileSystem());

        if (typeof this.runtime.getAssemblyExports !== "function") {
            throw new BrowserHostError(HostStage.WasmLoad, "dotnet_exports_missing", "The configured dotnet runtime does not expose getAssemblyExports().");
        }

        if (typeof this.runtime.runMain !== "function" && typeof this.runtime.runMainAndExit !== "function") {
            throw new BrowserHostError(HostStage.WasmLoad, "dotnet_main_missing", "The configured dotnet runtime does not expose runMain() or runMainAndExit().");
        }
    }

    /**
     * Resolves the configured managed exports and checks that `Tick` is available.
     *
     * @returns {Promise<void>} Completes when the host can begin the game loop.
     * @throws {BrowserHostError} When the configured exports or `Tick` method are missing.
     */
    async initializeManagedExportsAsync() {
        this.logStage(HostStage.ManagedExports, "Initializing managed host exports.");
        const exportsRoot = await this.runtime.getAssemblyExports(this.normalizeAssemblyName(this.config.mainAssemblyName));
        const hostExports = this.resolveExportPath(exportsRoot, this.config.hostExportsTypeName);
        if (hostExports == null) {
            throw new BrowserHostError(HostStage.ManagedExports, "managed_host_exports_missing", `The configured host exports type '${this.config.hostExportsTypeName}' was not found.`);
        }

        if (typeof hostExports.Tick !== "function") {
            throw new BrowserHostError(HostStage.ManagedExports, "managed_tick_missing", "The configured host exports type does not expose Tick().");
        }

        this.hostExports = hostExports;
        this.logStage(HostStage.ManagedExports, "Managed host exports initialized.");
    }

    /**
     * Starts the managed application and schedules its first game frame.
     *
     * @returns {Promise<void>} Completes when the managed application exits.
     */
    async launchManagedApplicationAsync() {
        this.logStage(HostStage.RuntimeBoundary, "Launching managed application.");
        const mainAssemblyName = this.normalizeAssemblyName(this.config.mainAssemblyName);
        const runMainPromise = typeof this.runtime.runMain === "function"
            ? Promise.resolve(this.runtime.runMain(mainAssemblyName, []))
            : Promise.resolve(this.runtime.runMainAndExit(mainAssemblyName, []));

        this.scheduleManagedFrame();
        this.logStage(HostStage.RuntimeBoundary, "Managed application launched.");
        await runMainPromise;
    }

    /**
     * Returns the assembly name as a DLL filename.
     *
     * @param {string} assemblyName Managed assembly name.
     * @returns {string} Assembly filename.
     */
    normalizeAssemblyName(assemblyName) {
        return assemblyName.endsWith(".dll") ? assemblyName : `${assemblyName}.dll`;
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

    /**
     * Schedules the next game frame.
     *
     * A later frame is scheduled only when `Tick` returns `true`.
     */
    scheduleManagedFrame() {
        if (this.contextLost || this.isDisposed) {
            return;
        }

        this.managedFrameHandle = requestAnimationFrame(async () => {
            this.managedFrameHandle = null;
            if (this.contextLost || this.isDisposed) {
                return;
            }

            try {
                const shouldContinue = await this.hostExports.Tick();
                if (!this.firstFrameReported) {
                    this.firstFrameReported = true;
                    this.notify(this.callbacks.onFirstFrame, undefined, "onFirstFrame");
                }

                if (shouldContinue && !this.contextLost && !this.isDisposed) {
                    this.scheduleManagedFrame();
                }
                else {
                    this.logStage(HostStage.RuntimeBoundary, "Managed run loop ended.");
                    this.dispose();
                }
            }
            catch (error) {
                this.handleHostFailure(new BrowserHostError(HostStage.ManagedExports, "managed_tick_failed", error instanceof Error ? error.message : String(error)));
                this.dispose();
            }
        });
    }

    /** Stops the browser game loop after an unrecoverable WebGL context loss. */
    handleContextLost() {
        if (this.contextLost) {
            return;
        }

        this.contextLost = true;
        if (this.managedFrameHandle != null) {
            cancelAnimationFrame(this.managedFrameHandle);
            this.managedFrameHandle = null;
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
        if (this.managedFrameHandle != null) {
            cancelAnimationFrame(this.managedFrameHandle);
            this.managedFrameHandle = null;
        }

        this.window.dispose();
        this.accelerometer.dispose();
        this.audio.dispose();
        this.hostExports = null;
        this.runtime = null;

        if (activeHost === this) {
            activeHost = null;
        }

        this.logStage(HostStage.RuntimeBoundary, "Browser host disposed.");
    }

    playSong(songId, mediaPath, positionMilliseconds, volume, commandId) {
        return this.audio.playSong(songId, mediaPath, positionMilliseconds, volume, commandId);
    }

    pauseSong(songId, commandId) {
        this.audio.pauseSong(songId, commandId);
    }

    resumeSong(songId, commandId) {
        this.audio.resumeSong(songId, commandId);
    }

    stopSong(songId) {
        this.audio.stopSong(songId);
    }

    setSongVolume(songId, volume) {
        this.audio.setSongVolume(songId, volume);
    }

    getSongPosition(songId) {
        return this.audio.getSongPosition(songId);
    }

    getSongDuration(songId) {
        return this.audio.getSongDuration(songId);
    }

    requestFullscreen() {
        this.window.requestFullscreen();
    }

    exitFullscreen() {
        this.window.exitFullscreen();
    }

    requestAccelerometer() {
        return this.accelerometer.request();
    }

    stopAccelerometer() {
        this.accelerometer.stop();
    }

    isAccelerometerSupported() {
        return this.accelerometer.isSupported();
    }

    takeDroppedFilePath() {
        return this.window.takeDroppedFilePath();
    }

    /**
     * Displays and logs a host failure.
     *
     * @param {unknown} error Error to report.
     */
    handleHostFailure(error) {
        const startupError = error instanceof BrowserHostError
            ? error
            : new BrowserHostError(HostStage.HostBootstrap, "unexpected_error", error instanceof Error ? error.message : String(error));
        const message = `${startupError.stage}: ${startupError.code} - ${startupError.message}`;
        if (this.statusElement != null) {
            this.statusElement.hidden = false;
            this.statusElement.textContent = message;
        }

        console.error("[MonoGame.Web Host]", message, startupError);
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
    stageAssetPackAsync: async (assetPackName) => {
        if (activeHost == null) {
            throw new BrowserHostError(HostStage.ContentStaging, "asset_pack_host_unavailable", "The browser host is not available to stage an asset pack.");
        }

        await activeHost.content.stageAssetPackAsync(assetPackName);
    },
    stageContentManifestAsync: (manifestUri) => activeHost?.content.stageContentManifestAsync(manifestUri),
    tryGetContentBase64: (relativePath) => activeHost?.content.tryGetContentBase64(relativePath) ?? null,
    takeDroppedFilePath: () => activeHost?.takeDroppedFilePath() ?? null
};
