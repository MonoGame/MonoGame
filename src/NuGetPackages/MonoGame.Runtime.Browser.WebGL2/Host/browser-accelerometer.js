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

        if (this.accelerometerListener != null) {
            return SensorState.Ready;
        }

        if (!this.isPermissionRequired()) {
            this.start();
            return SensorState.Ready;
        }

        return SensorState.Initializing;
    }

    /**
     * Requests accelerometer permission from a direct user gesture.
     *
     * @returns {Promise<boolean>} Whether accelerometer monitoring is active.
     */
    async requestPermissionFromUserGestureAsync() {
        if (this.getRuntime() == null || !this.isSupported()) {
            return false;
        }

        if (this.accelerometerListener != null) {
            return true;
        }

        if (!this.isPermissionRequired()) {
            this.start();
            return true;
        }

        return this.requestPermissionAsync();
    }

    /** Stops motion event delivery without changing browser permission. */
    stop() {
        if (this.accelerometerListener != null) {
            globalThis.removeEventListener("devicemotion", this.accelerometerListener);
            this.accelerometerListener = null;
        }
    }

    /**
     * Requests browser motion permission and reports a denial to the native runtime.
     *
     * @returns {Promise<boolean>} Whether permission was granted.
     */
    async requestPermissionAsync() {
        try {
            const permission = await DeviceMotionEvent.requestPermission();
            if (permission !== "granted") {
                this.getRuntime().Module._MGP_Web_NotifyAccelerometerState(SensorState.NoPermissions);
                return false;
            }

            this.start();
            return true;
        }
        catch {
            this.getRuntime().Module._MGP_Web_NotifyAccelerometerState(SensorState.NoPermissions);
            return false;
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
                this.getRuntime().Module._MGP_Web_NotifyAccelerometerReading(
                    acceleration.x / 9.80665,
                    acceleration.y / 9.80665,
                    acceleration.z / 9.80665);
            };
            globalThis.addEventListener("devicemotion", this.accelerometerListener);
        }

        this.getRuntime().Module._MGP_Web_NotifyAccelerometerState(SensorState.Ready);
    }
}
