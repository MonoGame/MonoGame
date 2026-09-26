// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

/** Lists the stages reported when browser host startup fails. */
export const HostStage = Object.freeze({
    HostBootstrap: "HostBootstrap",
    CanvasCreation: "CanvasCreation",
    WebGL2Creation: "WebGL2Creation",
    ContextLoss: "ContextLoss",
    WasmLoad: "WasmLoad",
    ContentStaging: "ContentStaging",
    ManagedExports: "ManagedExports",
    RuntimeBoundary: "RuntimeBoundary"
});

/** Lists the canvas resize policies accepted by the browser host. */
export const CanvasResizePolicy = Object.freeze({
    Adaptive: "Adaptive",
    Project: "Project",
    None: "None"
});

/** Lists the accelerometer states sent to the native runtime. */
export const SensorState = Object.freeze({
    NotSupported: 0,
    Ready: 1,
    Initializing: 2,
    NoData: 3,
    NoPermissions: 4,
    Disabled: 5
});

/** Lists terminal Song events sent to the native runtime. */
export const SongEventType = Object.freeze({
    Completed: 0,
    Failed: 1
});

/** Represents an error reported by the browser host. */
export class BrowserHostError extends Error {
    /**
     * Creates an error with a startup stage and code.
     *
     * @param {string} stage Startup stage where the error occurred.
     * @param {string} code Error code reported by the host.
     * @param {string} message Message shown to the user.
     */
    constructor(stage, code, message) {
        super(message);
        this.name = "BrowserHostError";
        this.stage = stage;
        this.code = code;
    }
}
