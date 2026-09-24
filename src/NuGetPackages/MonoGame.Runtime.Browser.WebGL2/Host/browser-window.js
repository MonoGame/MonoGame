// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

import {
    BrowserHostStartupError,
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
     * @param {HTMLElement} root Host element that contains the game canvas.
     * @param {{ canvasId: string, canvasResizePolicy: string }} config Window configuration.
     * @param {() => object | null} getRuntime Gets the managed runtime.
     */
    constructor(root, config, getRuntime) {
        this.root = root;
        this.config = config;
        this.getRuntime = getRuntime;
        this.canvas = null;
        this.canvasResizeObserver = null;
    }

    /**
     * Finds or creates the game canvas and configures it for the native runtime.
     *
     * @param {(stage: string, message: string) => void} logStage Logs canvas setup progress.
     */
    resolveCanvas(logStage) {
        logStage(HostStage.CanvasCreation, "Resolving host canvas.");

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

        // The native SDL/Emscripten integration resolves the conventional canvas ID rather than a project-specific ID.
        if (canvas.id !== "canvas") {
            canvas.id = "canvas";
        }

        // Canvas elements are not focusable by default, but SDL keyboard input and focus lifecycle require focus.
        if (!canvas.hasAttribute("tabindex")) {
            canvas.tabIndex = 0;
        }

        globalThis.Module = globalThis.Module || {};
        globalThis.Module.canvas = canvas;
        logStage(HostStage.CanvasCreation, `Canvas '${canvas.id}' ready at ${canvas.width}x${canvas.height}.`);
    }

    /**
     * Creates the WebGL2 context required by the native renderer.
     *
     * @param {(stage: string, message: string) => void} logStage Logs WebGL setup progress.
     * @throws {BrowserHostStartupError} When the browser cannot create a WebGL2 context.
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
            throw new BrowserHostStartupError(
                HostStage.WebGL2Creation,
                "webgl2_unavailable",
                "The browser host could not create a WebGL2 context.");
        }

        logStage(HostStage.WebGL2Creation, "WebGL2 context created.");
    }

    /**
     * Forwards CSS canvas size changes when the `Adaptive` policy is selected.
     *
     * @throws {BrowserHostStartupError} When the runtime or browser cannot report canvas size changes.
     */
    observeCanvasSize() {
        if (this.config.canvasResizePolicy !== CanvasResizePolicy.Adaptive) {
            return;
        }

        const notifyCanvasResize = this.getRuntime()?.Module?._MGP_Web_NotifyCanvasResize;
        if (typeof notifyCanvasResize !== "function") {
            throw new BrowserHostStartupError(
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
            throw new BrowserHostStartupError(
                HostStage.WasmLoad,
                "resize_observer_unavailable",
                "The browser does not support ResizeObserver.");
        }

        this.canvasResizeObserver = new ResizeObserver((entries) => {
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
     * @throws {BrowserHostStartupError} When the runtime cannot receive focus changes.
     */
    observeBrowserLifecycle() {
        const notifyFocusChange = this.getRuntime()?.Module?._MGP_Web_NotifyFocusChange;
        if (typeof notifyFocusChange !== "function") {
            throw new BrowserHostStartupError(
                HostStage.WasmLoad,
                "native_focus_change_missing",
                "The managed runtime does not expose the native focus callback.");
        }

        const notifyFocus = (focused) => notifyFocusChange(focused ? 1 : 0);
        const notifyWindowFocus = () => notifyFocus(!document.hidden && document.hasFocus());

        document.addEventListener("visibilitychange", notifyWindowFocus);
        globalThis.addEventListener("focus", notifyWindowFocus);
        globalThis.addEventListener("blur", notifyWindowFocus);
        this.canvas.addEventListener("focus", () => notifyFocus(true));
        this.canvas.addEventListener("blur", () => notifyFocus(false));

        notifyWindowFocus();
    }

    /**
     * Forwards confirmed browser fullscreen changes and request failures.
     *
     * @throws {BrowserHostStartupError} When the runtime cannot receive fullscreen changes.
     */
    observeFullscreen() {
        const notifyFullscreenChange = this.getRuntime()?.Module?._MGP_Web_NotifyFullscreenChange;
        const notifyFullscreenFailure = this.getRuntime()?.Module?._MGP_Web_NotifyFullscreenFailure;
        if (typeof notifyFullscreenChange !== "function"
            || typeof notifyFullscreenFailure !== "function") {
            throw new BrowserHostStartupError(
                HostStage.WasmLoad,
                "native_fullscreen_change_missing",
                "The managed runtime does not expose the native fullscreen callbacks.");
        }

        const reportFullscreenChange = () => {
            const fullscreen = document.fullscreenElement === this.canvas;
            notifyFullscreenChange(fullscreen ? 1 : 0);
        };

        document.addEventListener("fullscreenchange", reportFullscreenChange);
        document.addEventListener("fullscreenerror", () => {
            notifyFullscreenFailure();
        });

        reportFullscreenChange();
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
