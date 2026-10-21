// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

import {
    BrowserHostError,
    CanvasResizePolicy,
    HostStage
} from "./browser-host-common.js";

const MaximumStagedDropBytes = 64 * 1024 * 1024;

/**
 * Handles browser canvas, lifecycle, fullscreen, and motion events for the native runtime.
 */
export class BrowserWindow {
    /**
     * Creates the browser window service.
     *
     * @param {object} host Browser host coordinator.
     */
    constructor(host) {
        this.host = host;
        this.canvas = null;
        this.canvasResizeObserver = null;
        this.initialCanvasWidth = 0;
        this.initialCanvasHeight = 0;
        this.eventAbortController = new AbortController();
        this.isDisposed = false;
        this.dropFileSystem = null;
        this.dropTask = Promise.resolve();
        this.nextDropBatchId = 0;
        this.stagedDropBatches = [];
        this.stagedDropBytes = 0;
    }

    /**
     * Resolves the configured game canvas and prepares it for the native runtime.
     */
    resolveCanvas() {
        this.host.logStage(HostStage.CanvasCreation, "Resolving host canvas.");

        const canvas = this.host.config.canvas;

        if (!(canvas instanceof HTMLCanvasElement)) {
            throw new BrowserHostError(
                HostStage.CanvasCreation,
                "host_canvas_invalid",
                "The browser host canvas must be an HTMLCanvasElement.");
        }

        this.canvas = canvas;

        // Canvas elements are not focusable by default, but SDL keyboard input and focus lifecycle require focus.
        if (!canvas.hasAttribute("tabindex")) {
            canvas.tabIndex = 0;
        }

        this.suppressBrowserMouseDefaults();
        globalThis.Module = globalThis.Module || {};
        globalThis.Module.canvas = canvas;
        this.host.logStage(HostStage.CanvasCreation, `Canvas '${canvas.id}' ready at ${canvas.width}x${canvas.height}.`);
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

    /** Configures the native runtime's optional pointer lock compatibility behavior. */
    configurePointerLock() {
        if (!this.host.config.pointerLockEnabled) {
            return;
        }

        this.host.native.setPointerLockEnabled(this.host.config.pointerLockEnabled ? 1 : 0);
    }

    /** Prevents SDL browser callbacks from overriding Adaptive canvas sizing. */
    configureCanvasSize() {
        if (this.host.config.canvasResizePolicy === CanvasResizePolicy.Adaptive) {
            this.host.native.setCanvasResizeManaged(1);
        }
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
            if (this.isDisposed) {
                return;
            }

            onContextLost();
        }, { signal: this.eventAbortController.signal });
    }

    /**
     * Starts forwarding device-pixel canvas size changes after the managed initial presentation when the `Adaptive` policy is selected.
     *
     * @throws {BrowserHostError} When the runtime or browser cannot report canvas size changes.
     */
    observeCanvasSize() {
        if (this.host.config.canvasResizePolicy !== CanvasResizePolicy.Adaptive) {
            return;
        }

        const initialCanvasBounds = this.canvas.getBoundingClientRect();
        this.initialCanvasWidth = Math.round(initialCanvasBounds.width * globalThis.devicePixelRatio);
        this.initialCanvasHeight = Math.round(initialCanvasBounds.height * globalThis.devicePixelRatio);

        const reportCanvasSize = (width, height) => {
            const normalizedWidth = Math.round(width);
            const normalizedHeight = Math.round(height);
            if (normalizedWidth <= 0 || normalizedHeight <= 0) {
                return;
            }

            if (normalizedWidth === this.initialCanvasWidth
                && normalizedHeight === this.initialCanvasHeight) {
                this.initialCanvasWidth = 0;
                this.initialCanvasHeight = 0;
                return;
            }

            this.initialCanvasWidth = 0;
            this.initialCanvasHeight = 0;
            this.host.native?.notifyCanvasResize(normalizedWidth, normalizedHeight);
        };

        const getDevicePixelContentBoxSize = (entry) => {
            const size = entry.devicePixelContentBoxSize;
            if (size == null) {
                return null;
            }

            return Array.isArray(size) ? size[0] : size;
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
                    const devicePixelSize = getDevicePixelContentBoxSize(entry);

                    if (devicePixelSize != null) {
                        reportCanvasSize(devicePixelSize.inlineSize, devicePixelSize.blockSize);
                    }
                    else {
                        reportCanvasSize(
                            entry.contentRect.width * globalThis.devicePixelRatio,
                            entry.contentRect.height * globalThis.devicePixelRatio);
                    }
                }
            }
        });

        try {
            this.canvasResizeObserver.observe(this.canvas, { box: "device-pixel-content-box" });
        }
        catch {
            this.canvasResizeObserver.observe(this.canvas);
        }
    }

    /**
     * Forwards page and window focus changes to the native runtime.
     *
     * @throws {BrowserHostError} When the runtime cannot receive focus changes.
     */
    observeBrowserLifecycle() {
        const notifyFocus = (focused) => this.host.native?.notifyFocusChange(focused ? 1 : 0);
        const notifyDocumentFocus = () => notifyFocus(!document.hidden && document.hasFocus());

        const eventOptions = { signal: this.eventAbortController.signal };
        document.addEventListener("visibilitychange", notifyDocumentFocus, eventOptions);
        globalThis.addEventListener("focus", () => notifyFocus(true), eventOptions);
        globalThis.addEventListener("blur", () => notifyFocus(false), eventOptions);

        notifyDocumentFocus();
    }

    /**
     * Forwards confirmed browser fullscreen changes and request failures.
     *
     * @throws {BrowserHostError} When the runtime cannot receive fullscreen changes.
     */
    observeFullscreen() {
        const reportFullscreenChange = () => {
            const fullscreen = document.fullscreenElement === this.host.root;
            this.host.native?.notifyFullscreenChange(fullscreen ? 1 : 0);
        };

        const eventOptions = { signal: this.eventAbortController.signal };
        document.addEventListener("fullscreenchange", reportFullscreenChange, eventOptions);
        document.addEventListener("fullscreenerror", () => {
            this.host.native?.notifyFullscreenFailure();
        }, eventOptions);

        reportFullscreenChange();
    }

    /**
     * Stages browser-dropped files in the Emscripten filesystem and forwards their paths through the native event queue.
     *
     * @param {{ mkdirTree: Function, rmdir: Function, unlink: Function, writeFile: Function }} fileSystem Emscripten filesystem.
     * @throws {BrowserHostError} When the runtime cannot receive browser file drops.
     */
    observeFileDrop(fileSystem) {
        this.dropFileSystem = fileSystem;
        const notifyFileDrop = (filePath) => this.host.native?.notifyFileDrop(filePath);
        const notifyFileDropComplete = () => this.host.native?.notifyFileDropComplete();

        const eventOptions = { signal: this.eventAbortController.signal };
        this.canvas.addEventListener("dragenter", (event) => event.preventDefault(), eventOptions);
        this.canvas.addEventListener("dragover", (event) => {
            event.preventDefault();
            if (event.dataTransfer != null) {
                event.dataTransfer.dropEffect = "copy";
            }
        }, eventOptions);
        this.canvas.addEventListener("drop", (event) => {
            event.preventDefault();
            const files = Array.from(event.dataTransfer?.files ?? []);
            if (files.length === 0) {
                return;
            }

            this.dropTask = this.dropTask
                .then(() => this.stageDroppedFilesAsync(files, fileSystem, notifyFileDrop, notifyFileDropComplete))
                .catch((error) => {
                    console.error("[MonoGame.Web Host]", "Browser file drop staging failed.", error);
                });
        }, eventOptions);
    }

    /**
     * Stages one browser drop before raising its native completion event.
     *
     * @param {File[]} files Browser files captured from the drop event.
     * @param {{ mkdirTree: Function, rmdir: Function, unlink: Function, writeFile: Function }} fileSystem Emscripten filesystem.
     * @param {Function} notifyFileDrop Native callback for one staged file.
     * @param {Function} notifyFileDropComplete Native callback for a completed drop.
     * @returns {Promise<void>} Completes after every supported file within the current staging budget is processed.
     */
    async stageDroppedFilesAsync(files, fileSystem, notifyFileDrop, notifyFileDropComplete) {
        const batchDirectoryPath = `/tmp/monogame-drop/${this.nextDropBatchId++}`;
        const batch = {
            directoryPath: batchDirectoryPath,
            fileDirectoryPaths: [],
            files: []
        };
        let stagedFileCount = 0;

        for (let index = 0; index < files.length; index++) {
            if (this.isDisposed) {
                return;
            }

            const file = files[index];
            const fileName = normalizeDroppedFileName(file?.name);
            if (fileName == null || typeof file.arrayBuffer !== "function") {
                console.warn("[MonoGame.Web Host]", "Skipped a browser-dropped item without a usable file name or byte stream.");
                continue;
            }

            const fileSize = file.size;
            if (!Number.isSafeInteger(fileSize)
                || fileSize < 0
                || fileSize > MaximumStagedDropBytes - this.stagedDropBytes) {
                console.warn("[MonoGame.Web Host]", `Skipped browser-dropped file '${fileName}' because it exceeds the ${MaximumStagedDropBytes}-byte staging limit.`);
                continue;
            }

            const fileDirectoryPath = `${batchDirectoryPath}/${index}`;
            const filePath = `${fileDirectoryPath}/${fileName}`;
            try {
                const fileBytes = new Uint8Array(await file.arrayBuffer());
                if (this.isDisposed) {
                    return;
                }

                if (fileBytes.byteLength > MaximumStagedDropBytes - this.stagedDropBytes) {
                    console.warn("[MonoGame.Web Host]", `Skipped browser-dropped file '${fileName}' because its byte stream exceeds the staging limit.`);
                    continue;
                }

                fileSystem.mkdirTree(fileDirectoryPath);
                fileSystem.writeFile(filePath, fileBytes);
                this.stagedDropBytes += fileBytes.byteLength;
                batch.fileDirectoryPaths.push(fileDirectoryPath);
                batch.files.push({ byteLength: fileBytes.byteLength, path: filePath });
            }
            catch (error) {
                console.warn("[MonoGame.Web Host]", `Skipped browser-dropped file '${fileName}' because it could not be staged.`, error);
                continue;
            }

            if (this.isDisposed) {
                return;
            }

            notifyFileDrop(filePath);
            stagedFileCount++;
        }

        if (!this.isDisposed && stagedFileCount > 0) {
            this.stagedDropBatches.push(batch);
            notifyFileDropComplete();
        }
    }

    /** Releases the oldest staged browser drop after native dispatches its completion event. */
    releaseNextDropBatch() {
        const batch = this.stagedDropBatches.shift();
        const fileSystem = this.dropFileSystem;
        if (batch == null || fileSystem == null) {
            return;
        }

        let releasedByteLength = 0;
        for (const file of batch.files) {
            try {
                fileSystem.unlink(file.path);
                releasedByteLength += file.byteLength;
            }
            catch (error) {
                console.warn("[MonoGame.Web Host]", `Could not release browser dropped file '${file.path}'.`, error);
            }
        }

        for (let index = batch.fileDirectoryPaths.length - 1; index >= 0; index--) {
            try {
                fileSystem.rmdir(batch.fileDirectoryPaths[index]);
            }
            catch (error) {
                console.warn("[MonoGame.Web Host]", `Could not release browser drop directory '${batch.fileDirectoryPaths[index]}'.`, error);
            }
        }

        try {
            fileSystem.rmdir(batch.directoryPath);
        }
        catch (error) {
            console.warn("[MonoGame.Web Host]", `Could not release browser drop directory '${batch.directoryPath}'.`, error);
        }

        this.stagedDropBytes -= releasedByteLength;
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

        while (this.stagedDropBatches.length > 0) {
            this.releaseNextDropBatch();
        }

        this.dropFileSystem = null;
    }

    /** Requests fullscreen for the browser host root. */
    requestFullscreen() {
        if (document.fullscreenElement === this.host.root) {
            return;
        }

        void this.requestHostFullscreenAsync();
    }

    /** Requests exit from browser host fullscreen. */
    exitFullscreen() {
        if (document.fullscreenElement !== this.host.root) {
            this.host.native?.notifyFullscreenFailure();
            return;
        }

        void document.exitFullscreen().catch(() => {
            this.host.native?.notifyFullscreenChange(document.fullscreenElement === this.host.root ? 1 : 0);
        });
    }

    /**
     * Requests fullscreen for the browser host root and reports the result to the native runtime.
     *
     * @returns {Promise<void>} Completes after the browser accepts or rejects the request.
     */
    async requestHostFullscreenAsync() {
        if (typeof this.host.root.requestFullscreen !== "function") {
            this.host.native?.notifyFullscreenFailure();
            return;
        }

        try {
            await this.host.root.requestFullscreen();
        }
        catch {
            this.host.native?.notifyFullscreenFailure();
        }
    }

}

/**
 * Returns a single browser file name safe to append to a virtual filesystem path.
 *
 * @param {unknown} fileName Browser-provided file name.
 * @returns {string | null} Normalized file name, or `null` when it is unusable.
 */
function normalizeDroppedFileName(fileName) {
    if (typeof fileName !== "string") {
        return null;
    }

    const normalizedFileName = fileName.trim().replace(/[\\/]/g, "_");
    return normalizedFileName.length > 0
        && normalizedFileName !== "."
        && normalizedFileName !== ".."
        ? normalizedFileName
        : null;
}
