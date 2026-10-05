// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

import {
    BrowserHostError,
    CanvasResizePolicy,
    HostStage
} from "./browser-host-common.js";

/**
 * Handles browser canvas, lifecycle, fullscreen, and motion events for the native runtime.
 */
export class BrowserWindow {
    /**
     * Creates the browser window service.
     *
     * @param {{ canvas: HTMLCanvasElement, canvasResizePolicy: string, pointerLockEnabled: boolean }} config Window configuration.
     * @param {() => object | null} getRuntime Gets the managed runtime.
     */
    constructor(config, getRuntime) {
        this.config = config;
        this.getRuntime = getRuntime;
        this.canvas = null;
        this.canvasResizeObserver = null;
        this.contextLost = false;
        this.eventAbortController = new AbortController();
        this.isDisposed = false;
    }

    /**
     * Resolves the configured game canvas and prepares it for the native runtime.
     *
     * @param {(stage: string, message: string) => void} logStage Logs canvas setup progress.
     */
    resolveCanvas(logStage) {
        logStage(HostStage.CanvasCreation, "Resolving host canvas.");

        const canvas = this.config.canvas;

        if (!(canvas instanceof HTMLCanvasElement)) {
            throw new BrowserHostError(
                HostStage.CanvasCreation,
                "host_canvas_invalid",
                "The browser host canvas must be an HTMLCanvasElement.");
        }

        this.canvas = canvas;

        // The native SDL/Emscripten integration resolves the conventional canvas ID rather than a project-specific ID.
        if (canvas.id !== "canvas") {
            canvas.id = "canvas";
        }

        // Canvas elements are not focusable by default, but SDL keyboard input and focus lifecycle require focus.
        if (!canvas.hasAttribute("tabindex")) {
            canvas.tabIndex = 0;
        }

        this.suppressBrowserMouseDefaults();
        globalThis.Module = globalThis.Module || {};
        globalThis.Module.canvas = canvas;
        logStage(HostStage.CanvasCreation, `Canvas '${canvas.id}' ready at ${canvas.width}x${canvas.height}.`);
    }

    /** Suppresses browser mouse actions that interrupt a canvas game. */
    suppressBrowserMouseDefaults() {
        const eventOptions = { signal: this.eventAbortController.signal };
        this.canvas.addEventListener("contextmenu", (event) => {
            event.preventDefault();
        }, eventOptions);

        const suppressUnsupportedButtonAction = (event) => {
            if (event.button === 3 || event.button === 4) {
                event.preventDefault();
            }
        };

        // SDL2 does not translate buttons 3 and 4, so suppress their browser Back and Forward defaults.
        // Firefox reserves these buttons before dispatching cancellable page events: https://bugzilla.mozilla.org/show_bug.cgi?id=1933746
        this.canvas.addEventListener("pointerdown", suppressUnsupportedButtonAction, eventOptions);
        this.canvas.addEventListener("pointerup", suppressUnsupportedButtonAction, eventOptions);
        this.canvas.addEventListener("mousedown", suppressUnsupportedButtonAction, eventOptions);
        this.canvas.addEventListener("mouseup", suppressUnsupportedButtonAction, eventOptions);
        this.canvas.addEventListener("auxclick", suppressUnsupportedButtonAction, eventOptions);
    }

    /**
     * Creates the WebGL2 context required by the native renderer.
     *
     * @param {(stage: string, message: string) => void} logStage Logs WebGL setup progress.
     * @throws {BrowserHostError} When the browser cannot create a WebGL2 context.
     */
    createWebGL2Context(logStage) {
        logStage(HostStage.WebGL2Creation, "Requesting WebGL2 context.");

        // WebGL2 context options are fixed renderer requirements.
        // Application facing settings remain in MonoGame APIs.
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
            throw new BrowserHostError(
                HostStage.WebGL2Creation,
                "webgl2_unavailable",
                "The browser host could not create a WebGL2 context.");
        }

        logStage(HostStage.WebGL2Creation, "WebGL2 context created.");
    }

    /** Configures the native runtime's optional pointer lock compatibility behavior. */
    configurePointerLock() {
        if (!this.config.pointerLockEnabled) {
            return;
        }

        const setPointerLockEnabled = this.getRuntime()?.Module?._MGP_Web_SetPointerLockEnabled;
        if (typeof setPointerLockEnabled !== "function") {
            throw new BrowserHostError(
                HostStage.WasmLoad,
                "native_pointer_lock_configuration_missing",
                "The managed runtime does not expose the native pointer lock configuration callback.");
        }

        setPointerLockEnabled(this.config.pointerLockEnabled ? 1 : 0);
    }

    /**
     * Stops the host when the browser loses the WebGL context.
     *
     * Context loss is uncommon but can follow a GPU reset, driver failures,
     * device change, or if there is resource exhaustion and the browser needs
     * to reclaim resources.
     * @param {() => void} onContextLost Stops managed frame scheduling.
     */
    observeContextLoss(onContextLost) {
        this.canvas.addEventListener("webglcontextlost", () => {
            if (this.contextLost || this.isDisposed) {
                return;
            }

            this.contextLost = true;
            onContextLost();
        }, { signal: this.eventAbortController.signal });
    }

    /**
     * Forwards CSS canvas size changes when the `Adaptive` policy is selected.
     *
     * @throws {BrowserHostError} When the runtime or browser cannot report canvas size changes.
     */
    observeCanvasSize() {
        if (this.config.canvasResizePolicy !== CanvasResizePolicy.Adaptive) {
            return;
        }

        const notifyCanvasResize = this.getRuntime()?.Module?._MGP_Web_NotifyCanvasResize;
        if (typeof notifyCanvasResize !== "function") {
            throw new BrowserHostError(
                HostStage.WasmLoad,
                "native_canvas_resize_missing",
                "The managed runtime does not expose the native canvas resize callback.");
        }

        const reportCanvasSize = (width, height) => {
            const normalizedWidth = Math.round(width);
            const normalizedHeight = Math.round(height);
            if (normalizedWidth > 0 && normalizedHeight > 0) {
                notifyCanvasResize(normalizedWidth, normalizedHeight);
            }
        };

        if (typeof ResizeObserver !== "function") {
            throw new BrowserHostError(
                HostStage.WasmLoad,
                "resize_observer_unavailable",
                "The browser does not support ResizeObserver.");
        }

        this.canvasResizeObserver = new ResizeObserver((entries) => {
            if (this.isDisposed) {
                return;
            }

            for (const entry of entries) {
                if (entry.target === this.canvas) {
                    reportCanvasSize(entry.contentRect.width, entry.contentRect.height);
                }
            }
        });
        this.canvasResizeObserver.observe(this.canvas);
    }

    /**
     * Forwards page, window, and canvas focus changes to the native runtime.
     *
     * @throws {BrowserHostError} When the runtime cannot receive focus changes.
     */
    observeBrowserLifecycle() {
        const notifyFocusChange = this.getRuntime()?.Module?._MGP_Web_NotifyFocusChange;
        if (typeof notifyFocusChange !== "function") {
            throw new BrowserHostError(
                HostStage.WasmLoad,
                "native_focus_change_missing",
                "The managed runtime does not expose the native focus callback.");
        }

        const notifyFocus = (focused) => notifyFocusChange(focused ? 1 : 0);
        const notifyWindowFocus = () => notifyFocus(!document.hidden && document.hasFocus());

        const eventOptions = { signal: this.eventAbortController.signal };
        document.addEventListener("visibilitychange", notifyWindowFocus, eventOptions);
        globalThis.addEventListener("focus", notifyWindowFocus, eventOptions);
        globalThis.addEventListener("blur", notifyWindowFocus, eventOptions);
        this.canvas.addEventListener("focus", () => notifyFocus(true), eventOptions);
        this.canvas.addEventListener("blur", () => notifyFocus(false), eventOptions);

        notifyWindowFocus();
    }

    /**
     * Forwards confirmed browser fullscreen changes and request failures.
     *
     * @throws {BrowserHostError} When the runtime cannot receive fullscreen changes.
     */
    observeFullscreen() {
        const notifyFullscreenChange = this.getRuntime()?.Module?._MGP_Web_NotifyFullscreenChange;
        const notifyFullscreenFailure = this.getRuntime()?.Module?._MGP_Web_NotifyFullscreenFailure;
        if (typeof notifyFullscreenChange !== "function"
            || typeof notifyFullscreenFailure !== "function") {
            throw new BrowserHostError(
                HostStage.WasmLoad,
                "native_fullscreen_change_missing",
                "The managed runtime does not expose the native fullscreen callbacks.");
        }

        const reportFullscreenChange = () => {
            const fullscreen = document.fullscreenElement === this.canvas;
            notifyFullscreenChange(fullscreen ? 1 : 0);
        };

        const eventOptions = { signal: this.eventAbortController.signal };
        document.addEventListener("fullscreenchange", reportFullscreenChange, eventOptions);
        document.addEventListener("fullscreenerror", () => {
            notifyFullscreenFailure();
        }, eventOptions);

        reportFullscreenChange();
    }

    /** Removes browser listeners and stops CSS canvas resize observation. */
    dispose() {
        if (this.isDisposed) {
            return;
        }

        this.isDisposed = true;
        this.eventAbortController.abort();
        this.canvasResizeObserver?.disconnect();
        this.canvasResizeObserver = null;
    }

    /** Requests fullscreen for the game canvas. */
    requestFullscreen() {
        if (document.fullscreenElement === this.canvas) {
            return;
        }

        void this.requestCanvasFullscreenAsync();
    }

    /** Requests exit from canvas fullscreen. */
    exitFullscreen() {
        if (document.fullscreenElement !== this.canvas) {
            this.getRuntime().Module._MGP_Web_NotifyFullscreenFailure();
            return;
        }

        void document.exitFullscreen().catch(() => {
            this.getRuntime().Module._MGP_Web_NotifyFullscreenChange(
                document.fullscreenElement === this.canvas ? 1 : 0);
        });
    }

    /**
     * Requests fullscreen for the game canvas and reports the result to the native runtime.
     *
     * @returns {Promise<void>} Completes after the browser accepts or rejects the request.
     */
    async requestCanvasFullscreenAsync() {
        if (typeof this.canvas.requestFullscreen !== "function") {
            this.getRuntime().Module._MGP_Web_NotifyFullscreenFailure();
            return;
        }

        try {
            await this.canvas.requestFullscreen();
        }
        catch {
            this.getRuntime().Module._MGP_Web_NotifyFullscreenFailure();
        }
    }

}
