// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

import { SensorState } from "./browser-host-common.js";

/** Handles browser accelerometer permission and device-motion readings. */
export class BrowserAccelerometer {
    /**
     * Creates the browser accelerometer service.
     *
     * @param {() => object | null} getRuntime Gets the managed runtime.
     */
    constructor(getRuntime) {
        this.getRuntime = getRuntime;
        this.accelerometerListener = null;
        this.canvas = null;
        this.permissionGestureListener = null;
        this.permissionRequestPending = false;
        this.isRequested = false;
    }

    /**
     * Stores the game canvas used to request motion permission after the sensor is started.
     *
     * @param {HTMLCanvasElement} canvas Game canvas.
     */
    observePermissionGesture(canvas) {
        this.canvas = canvas;
    }

    /** @returns {boolean} Whether the browser supports device-motion events. */
    isSupported() {
        return typeof DeviceMotionEvent !== "undefined";
    }

    /** @returns {boolean} Whether motion permission requires a user gesture. */
    isPermissionRequired() {
        return this.isSupported()
            && typeof DeviceMotionEvent.requestPermission === "function";
    }

    /**
     * Returns the accelerometer state without prompting for permission.
     *
     * @returns {number} Accelerometer state.
     */
    request() {
        if (!this.isSupported()) {
            return SensorState.NotSupported;
        }

        this.isRequested = true;

        if (this.accelerometerListener != null) {
            return SensorState.Ready;
        }

        if (!this.isPermissionRequired()) {
            this.start();
            return SensorState.Ready;
        }

        this.addPermissionGestureListener();
        return SensorState.Initializing;
    }

    /** Stops motion event delivery without changing browser permission. */
    stop() {
        this.isRequested = false;
        this.removePermissionGestureListener();
        if (this.accelerometerListener != null) {
            globalThis.removeEventListener("devicemotion", this.accelerometerListener);
            this.accelerometerListener = null;
        }
    }

    /** Adds the one-shot canvas gesture handler required by browsers such as iOS Safari. */
    addPermissionGestureListener() {
        if (this.canvas == null || this.permissionGestureListener != null) {
            return;
        }

        this.permissionGestureListener = () => {
            void this.requestPermissionFromCanvasGestureAsync();
        };
        this.canvas.addEventListener("touchend", this.permissionGestureListener, { passive: true });
    }

    /** Removes the pending canvas gesture handler. */
    removePermissionGestureListener() {
        if (this.canvas != null && this.permissionGestureListener != null) {
            this.canvas.removeEventListener("touchend", this.permissionGestureListener);
            this.permissionGestureListener = null;
        }
    }

    /**
     * Requests browser motion permission from the direct canvas gesture and reports a denial to the native runtime.
     *
     * @returns {Promise<boolean>} Whether permission was granted.
     */
    async requestPermissionFromCanvasGestureAsync() {
        if (!this.isRequested || this.permissionRequestPending || this.getRuntime() == null || !this.isSupported()) {
            return false;
        }

        if (this.accelerometerListener != null) {
            return true;
        }

        if (!this.isPermissionRequired()) {
            this.start();
            return true;
        }

        this.permissionRequestPending = true;
        try {
            const permission = await DeviceMotionEvent.requestPermission();
            if (permission !== "granted") {
                this.notifyState(SensorState.NoPermissions);
                return false;
            }

            if (!this.isRequested) {
                return false;
            }

            this.start();
            return true;
        }
        catch {
            this.notifyState(SensorState.NoPermissions);
            return false;
        }
        finally {
            this.permissionRequestPending = false;
            this.removePermissionGestureListener();
        }
    }

    /** Reports a sensor state when the managed runtime remains available. */
    notifyState(state) {
        const notifyState = this.getRuntime()?.Module?._MGP_Web_NotifyAccelerometerState;
        if (typeof notifyState === "function") {
            notifyState(state);
        }
    }

    /** Starts forwarding device motion readings and reports that the accelerometer is ready. */
    start() {
        if (this.accelerometerListener == null) {
            this.accelerometerListener = (event) => {
                const acceleration = event.accelerationIncludingGravity;
                if (acceleration?.x == null || acceleration?.y == null || acceleration?.z == null) {
                    return;
                }

                // Browser motion uses meters per second squared;
                // MonoGame accelerometer readings are expressed in standard gravity units.
                // So we convert by dividing by 9.80665
                const notifyReading = this.getRuntime()?.Module?._MGP_Web_NotifyAccelerometerReading;
                if (typeof notifyReading === "function") {
                    notifyReading(
                        acceleration.x / 9.80665,
                        acceleration.y / 9.80665,
                        acceleration.z / 9.80665);
                }
            };
            globalThis.addEventListener("devicemotion", this.accelerometerListener);
        }

        this.notifyState(SensorState.Ready);
    }
}
