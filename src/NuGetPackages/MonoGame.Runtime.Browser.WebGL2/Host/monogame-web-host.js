// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

import { BrowserAudio } from "./browser-audio.js";
import { BrowserAccelerometer } from "./browser-accelerometer.js";
import { BrowserContent } from "./browser-content.js";
import { BrowserHostStartupError, CanvasResizePolicy, HostStage } from "./browser-host-common.js";
import { BrowserWindow } from "./browser-window.js";

let activeHost = null;

/**
 * The configuration resolved from package defaults and project settings.
 *
 * @typedef {object} BrowserHostConfiguration
 * @property {string} applicationName
 * @property {string} canvasId
 * @property {string} contentBaseUri
 * @property {string} startupContentManifestUri
 * @property {string} statusId
 * @property {string | null} runtimeScriptUri
 * @property {string | null} hostExportsTypeName
 * @property {string | null} mainAssemblyName
 * @property {"Adaptive" | "Project" | "None"} canvasResizePolicy
 */

/** Coordinates browser startup and delegates browser-specific behavior to private host modules. */
class MonoGameWebHost {
    /** @param {HTMLElement} root Root host element. */
    constructor(root) {
        this.root = root;
        this.config = this.readConfig(root);
        this.root.dataset.canvasResizePolicy = this.config.canvasResizePolicy;
        this.statusElement = document.getElementById(this.config.statusId);
        this.runtime = null;
        this.hostExports = null;
        this.managedFrameHandle = null;
        this.window = new BrowserWindow(root, this.config, () => this.runtime);
        this.accelerometer = new BrowserAccelerometer(() => this.runtime);
        this.audio = new BrowserAudio(root, this.config.contentBaseUri, () => this.runtime);
        this.content = new BrowserContent(this.config, () => this.runtime, (stage, message) => this.logStage(stage, message));
    }

    /**
     * Creates the host from the package's `#monogame-host` element and begins asynchronous startup.
     *
     * @param {Document} document_ Document containing the host element.
     * @returns {MonoGameWebHost} Host coordinator before startup completes.
     */
    static bootFromDocument(document_) {
        const root = document_.getElementById("monogame-host");
        if (root == null) {
            throw new BrowserHostStartupError(HostStage.HostBootstrap, "host_root_missing", "The browser host root '#monogame-host' was not found.");
        }

        const host = new MonoGameWebHost(root);
        activeHost = host;
        void host.startAsync();
        return host;
    }

    /**
     * Combines package defaults with project configuration, preferring `data-*` values on the host element.
     *
     * @param {HTMLElement} root Host element that supplies package configuration.
     * @returns {BrowserHostConfiguration} Configuration used to initialize the host.
     */
    readConfig(root) {
        const dataset = root.dataset;
        const runtimeConfiguration = globalThis.MonoGameWebHostConfiguration ?? {};
        return {
            applicationName: dataset.applicationName || "MonoGame.Web",
            canvasId: dataset.canvasId || "canvas",
            contentBaseUri: dataset.contentBaseUri || "./",
            startupContentManifestUri: dataset.startupContentManifestUri || "Content/content-manifest.txt",
            statusId: dataset.statusId || "monogame-host-status",
            runtimeScriptUri: this.getOptionalConfigValue(dataset.runtimeScriptUri) ?? this.getOptionalConfigValue(runtimeConfiguration.runtimeScriptUri),
            hostExportsTypeName: this.getOptionalConfigValue(dataset.hostExportsTypeName) ?? this.getOptionalConfigValue(runtimeConfiguration.hostExportsTypeName),
            mainAssemblyName: this.getOptionalConfigValue(dataset.mainAssemblyName) ?? this.getOptionalConfigValue(runtimeConfiguration.mainAssemblyName),
            canvasResizePolicy: this.getCanvasResizePolicy(
                this.getOptionalConfigValue(dataset.canvasResizePolicy)
                ?? this.getOptionalConfigValue(runtimeConfiguration.canvasResizePolicy))
        };
    }

    /**
     * Returns the configuration value, or `null` when it is empty or missing.
     *
     * @param {string | null | undefined} value Configuration value to normalize.
     * @returns {string | null} The supplied value, or `null` when it is absent or empty.
     */
    getOptionalConfigValue(value) {
        return value == null || value.length === 0 ? null : value;
    }

    /**
     * Returns the requested canvas resize policy, or `Adaptive` when no policy is specified.
     *
     * @param {string | null} value Canvas resize policy to validate.
     * @returns {"Adaptive" | "Project" | "None"} Requested canvas resize policy.
     * @throws {BrowserHostStartupError} When the policy is not `Adaptive`, `Project`, or `None`.
     */
    getCanvasResizePolicy(value) {
        if (value == null) {
            return CanvasResizePolicy.Adaptive;
        }

        if (value === CanvasResizePolicy.Adaptive || value === CanvasResizePolicy.Project || value === CanvasResizePolicy.None) {
            return value;
        }

        throw new BrowserHostStartupError(HostStage.HostBootstrap, "canvas_resize_policy_invalid", `Canvas resize policy '${value}' is invalid. Use Adaptive, Project, or None.`);
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
            this.createWebGL2Context();
            await this.initializeManagedRuntimeAsync();
            await this.content.stageStartupContentAsync();
            await this.initializeManagedExportsAsync();
            await this.launchManagedApplicationAsync();
        }
        catch (error) {
            this.handleStartupFailure(error);
        }
    }

    /** Resolves the game canvas and listens for gestures that can activate browser audio. */
    resolveCanvas() {
        this.window.resolveCanvas((stage, message) => this.logStage(stage, message));
        this.audio.observeActivation(this.window.canvas);
    }

    /** Creates the browser WebGL2 context before the managed runtime starts. */
    createWebGL2Context() {
        this.window.createWebGL2Context((stage, message) => this.logStage(stage, message));
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
            throw new BrowserHostStartupError(HostStage.HostBootstrap, "managed_runtime_configuration_missing", `The browser host requires ${missingConfigurationNames.join(", ")}.`);
        }
    }

    /**
     * Loads the managed runtime, connects it to the game canvas, and starts browser event handling.
     *
     * @returns {Promise<void>} Completes when the runtime can start the application.
     * @throws {BrowserHostStartupError} When the configured runtime cannot be loaded or started.
     */
    async initializeManagedRuntimeAsync() {
        this.logStage(HostStage.WasmLoad, "Loading managed runtime.");
        const runtimeModule = await import(this.config.runtimeScriptUri);
        const dotnet = runtimeModule.dotnet ?? globalThis.dotnet;
        if (dotnet == null) {
            throw new BrowserHostStartupError(HostStage.WasmLoad, "dotnet_runtime_missing", "The configured runtime script did not expose a dotnet runtime entry.");
        }

        let runtimeBuilder = dotnet;
        if (typeof runtimeBuilder.withDiagnosticTracing === "function") {
            runtimeBuilder = runtimeBuilder.withDiagnosticTracing(false);
        }

        if (typeof runtimeBuilder.withModuleConfig === "function") {
            runtimeBuilder = runtimeBuilder.withModuleConfig({ canvas: this.window.canvas });
        }

        if (typeof runtimeBuilder.create !== "function") {
            throw new BrowserHostStartupError(HostStage.WasmLoad, "dotnet_runtime_shape_unsupported", "The configured dotnet runtime does not expose a supported create() API.");
        }

        this.runtime = await runtimeBuilder.create();
        this.window.observeCanvasSize();
        this.window.observeBrowserLifecycle();
        this.window.observeFullscreen();

        if (typeof this.runtime.getAssemblyExports !== "function") {
            throw new BrowserHostStartupError(HostStage.WasmLoad, "dotnet_exports_missing", "The configured dotnet runtime does not expose getAssemblyExports().");
        }

        if (typeof this.runtime.runMain !== "function" && typeof this.runtime.runMainAndExit !== "function") {
            throw new BrowserHostStartupError(HostStage.WasmLoad, "dotnet_main_missing", "The configured dotnet runtime does not expose runMain() or runMainAndExit().");
        }
    }

    /**
     * Resolves the configured managed exports and checks that `Tick` is available.
     *
     * @returns {Promise<void>} Completes when the host can begin the game loop.
     * @throws {BrowserHostStartupError} When the configured exports or `Tick` method are missing.
     */
    async initializeManagedExportsAsync() {
        this.logStage(HostStage.ManagedExports, "Initializing managed host exports.");
        const exportsRoot = await this.runtime.getAssemblyExports(this.normalizeAssemblyName(this.config.mainAssemblyName));
        const hostExports = this.resolveExportPath(exportsRoot, this.config.hostExportsTypeName);
        if (hostExports == null) {
            throw new BrowserHostStartupError(HostStage.ManagedExports, "managed_host_exports_missing", `The configured host exports type '${this.config.hostExportsTypeName}' was not found.`);
        }

        if (typeof hostExports.Tick !== "function") {
            throw new BrowserHostStartupError(HostStage.ManagedExports, "managed_tick_missing", "The configured host exports type does not expose Tick().");
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
        this.managedFrameHandle = requestAnimationFrame(async () => {
            this.managedFrameHandle = null;
            try {
                const shouldContinue = await this.hostExports.Tick();
                if (shouldContinue) {
                    this.scheduleManagedFrame();
                }
                else {
                    this.logStage(HostStage.RuntimeBoundary, "Managed run loop ended.");
                }
            }
            catch (error) {
                this.handleStartupFailure(new BrowserHostStartupError(HostStage.ManagedExports, "managed_tick_failed", error instanceof Error ? error.message : String(error)));
            }
        });
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

    isAccelerometerPermissionRequired() {
        return this.accelerometer.isPermissionRequired();
    }

    requestAccelerometerPermissionFromUserGestureAsync() {
        return this.accelerometer.requestPermissionFromUserGestureAsync();
    }

    /**
     * Displays and logs a host error.
     *
     * @param {unknown} error Error to report.
     */
    handleStartupFailure(error) {
        const startupError = error instanceof BrowserHostStartupError
            ? error
            : new BrowserHostStartupError(HostStage.HostBootstrap, "unexpected_error", error instanceof Error ? error.message : String(error));
        const message = `${startupError.stage}: ${startupError.code} - ${startupError.message}`;
        if (this.statusElement != null) {
            this.statusElement.hidden = false;
            this.statusElement.textContent = message;
        }

        console.error("[MonoGame.Web Host]", message, startupError);
    }

    /**
     * Logs host progress.
     *
     * @param {string} stage Startup stage.
     * @param {string} message Progress message.
     */
    logStage(stage, message) {
        console.info("[MonoGame.Web Host]", stage, message);
    }
}

/** Provides host services called by the browser native runtime. */
globalThis.MonoGameWebHost = {
    getActiveHost: () => activeHost,
    requestFullscreen: () => activeHost?.requestFullscreen(),
    exitFullscreen: () => activeHost?.exitFullscreen(),
    isAccelerometerPermissionRequired: () => activeHost?.isAccelerometerPermissionRequired() ?? false,
    requestAccelerometerPermissionFromUserGestureAsync: () => activeHost?.requestAccelerometerPermissionFromUserGestureAsync() ?? Promise.resolve(false),
    stageAssetPackAsync: async (assetPackName) => {
        if (activeHost == null) {
            throw new BrowserHostStartupError(HostStage.ContentStaging, "asset_pack_host_unavailable", "The browser host is not available to stage an asset pack.");
        }

        await activeHost.content.stageAssetPackAsync(assetPackName);
    },
    stageContentManifestAsync: (manifestUri) => activeHost?.content.stageContentManifestAsync(manifestUri),
    tryGetContentBase64: (relativePath) => activeHost?.content.tryGetContentBase64(relativePath) ?? null
};

MonoGameWebHost.bootFromDocument(document);
