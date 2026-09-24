// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

import { SongEventType } from "./browser-host-common.js";

/**
 * Handles browser audio activation and Song playback through one media element.
 */
export class BrowserAudio {
    /**
     * Creates the browser audio service.
     *
     * @param {HTMLElement} root Host element that contains the hidden media element.
     * @param {string} contentBaseUri Base URI for published Song media.
     * @param {() => object | null} getRuntime Gets the managed runtime.
     */
    constructor(root, contentBaseUri, getRuntime) {
        this.root = root;
        this.contentBaseUri = contentBaseUri;
        this.getRuntime = getRuntime;
        this.audioActivationPromise = null;
        this.audioContext = null;
        this.songElement = null;
        this.songGainNode = null;
        this.activeSong = null;
    }

    /**
     * Listens for canvas gestures that can activate browser audio.
     *
     * @param {HTMLCanvasElement} canvas Game canvas.
     */
    observeActivation(canvas) {
        const activateAudio = () => {
            this.audioActivationPromise = this.activateAsync();
            void this.loadPendingSongAfterActivationAsync(this.audioActivationPromise);
        };

        canvas.addEventListener("pointerdown", activateAudio, { passive: true });
        canvas.addEventListener("touchend", activateAudio);
        canvas.addEventListener("keydown", activateAudio);
    }

    /**
     * Creates the shared audio graph when needed and resumes it.
     *
     * @returns {Promise<boolean>} Whether the audio context is running.
     */
    async activateAsync() {
        const AudioContextConstructor = globalThis.AudioContext ?? globalThis.webkitAudioContext;
        if (AudioContextConstructor == null) {
            return false;
        }

        this.ensureSongElement();

        try {
            if (this.audioContext == null) {
                this.audioContext = new AudioContextConstructor();
                const source = this.audioContext.createMediaElementSource(this.songElement);
                this.songGainNode = this.audioContext.createGain();
                source.connect(this.songGainNode);
                this.songGainNode.connect(this.audioContext.destination);
            }

            await this.audioContext.resume();
            return this.audioContext.state === "running";
        }
        catch {
            return false;
        }
    }

    /**
     * Replaces the active Song with the requested Song.
     *
     * Playback waits for browser audio activation when needed.
     *
     * @param {number} songId Song handle.
     * @param {string} mediaPath Published Song media path.
     * @param {number} positionMilliseconds Initial playback position in milliseconds.
     * @param {number} volume MediaPlayer volume.
     * @param {number} commandId Song command identifier.
     * @returns {boolean} `true`; media failures are reported through a Song event.
     */
    playSong(songId, mediaPath, positionMilliseconds, volume, commandId) {
        this.ensureSongElement();

        const mediaUri = this.resolveSongUri(mediaPath);
        if (mediaUri == null || !this.canPlaySongMedia()) {
            this.reportSongEvent(songId, commandId, SongEventType.Failed);
            return true;
        }

        if (this.audioContext == null || this.songGainNode == null) {
            this.reportSongEvent(songId, commandId, SongEventType.Failed);
            return true;
        }

        this.stopActiveSong();

        this.activeSong = {
            songId,
            commandId,
            mediaUri,
            positionSeconds: Math.max(0, positionMilliseconds / 1000),
            volume
        };

        if (this.audioContext.state === "running") {
            this.loadActiveSongMedia();
        }
        else {
            // WebKit can keep resume() pending until touchend, so loading now could lose the user gesture.
            this.activeSong.waitingForAudio = true;
            if (this.audioActivationPromise != null) {
                void this.loadPendingSongAfterActivationAsync(this.audioActivationPromise);
            }
        }

        return true;
    }

    /**
     * Pauses the active Song when its handle and command match.
     *
     * @param {number} songId Song handle.
     * @param {number} commandId Song command identifier.
     */
    pauseSong(songId, commandId) {
        if (this.isActiveSong(songId, commandId)) {
            this.songElement.pause();
        }
    }

    /**
     * Resumes the active Song when its handle and command match.
     *
     * @param {number} songId Song handle.
     * @param {number} commandId Song command identifier.
     */
    resumeSong(songId, commandId) {
        if (this.isActiveSong(songId, commandId)) {
            this.startActiveSongPlayback(false);
        }
    }

    /**
     * Stops the active Song when its handle matches.
     *
     * @param {number} songId Song handle.
     */
    stopSong(songId) {
        if (this.activeSong?.songId === songId) {
            this.stopActiveSong();
        }
    }

    /**
     * Updates the active Song volume when its handle matches.
     *
     * @param {number} songId Song handle.
     * @param {number} volume MediaPlayer volume.
     */
    setSongVolume(songId, volume) {
        if (this.activeSong?.songId !== songId) {
            return;
        }

        this.activeSong.volume = volume;
        this.applyActiveSongVolume();
    }

    /**
     * Returns the active Song position, or zero when the handle does not match.
     *
     * @param {number} songId Song handle.
     * @returns {number} Playback position in milliseconds.
     */
    getSongPosition(songId) {
        if (this.activeSong?.songId !== songId || !Number.isFinite(this.songElement.currentTime)) {
            return 0;
        }

        return Math.floor(this.songElement.currentTime * 1000);
    }

    /**
     * Returns the active Song duration, or zero when it is not available.
     *
     * @param {number} songId Song handle.
     * @returns {number} Duration in milliseconds.
     */
    getSongDuration(songId) {
        if (this.activeSong?.songId !== songId || !Number.isFinite(this.songElement.duration)) {
            return 0;
        }

        return Math.floor(this.songElement.duration * 1000);
    }

    /** Creates the hidden media element once so its Web Audio source remains stable across Songs. */
    ensureSongElement() {
        if (this.songElement != null) {
            return;
        }

        const songElement = document.createElement("audio");
        songElement.hidden = true;
        songElement.preload = "metadata";
        songElement.crossOrigin = "anonymous";
        songElement.setAttribute("aria-hidden", "true");
        songElement.addEventListener("ended", () => this.reportActiveSongEvent(SongEventType.Completed));
        songElement.addEventListener("error", () => this.reportActiveSongEvent(SongEventType.Failed));
        this.root.appendChild(songElement);
        this.songElement = songElement;
    }

    /** @returns {boolean} Whether the browser reports MP3 media support. */
    canPlaySongMedia() {
        return this.songElement != null && this.songElement.canPlayType("audio/mpeg") !== "";
    }

    /**
     * Returns the published media URL, or `null` when the path is invalid.
     *
     * @param {string} mediaPath Published Song media path.
     * @returns {string | null} Absolute HTTP(S) media URL.
     */
    resolveSongUri(mediaPath) {
        try {
            const relativeMediaPath = mediaPath.replace(/^[\\/]+/, "");
            const uri = new URL(relativeMediaPath, new URL(this.contentBaseUri, document.baseURI));
            // Songs are fetched by the page; non-HTTP schemes bypass the published-asset contract.
            return uri.protocol === "http:" || uri.protocol === "https:" ? uri.toString() : null;
        }
        catch {
            return null;
        }
    }

    /**
     * Loads a pending Song after browser audio is activated.
     *
     * @param {Promise<boolean>} activationPromise Audio activation result.
     */
    async loadPendingSongAfterActivationAsync(activationPromise) {
        const activated = await activationPromise;
        if (!activated || this.audioContext?.state !== "running" || this.songGainNode == null) {
            return;
        }

        if (this.activeSong == null || !this.activeSong.waitingForAudio) {
            return;
        }

        this.loadActiveSongMedia();
    }

    /** Loads the active Song after the shared audio graph is ready. */
    loadActiveSongMedia() {
        const activeSong = this.activeSong;
        if (activeSong == null) {
            return;
        }

        activeSong.waitingForAudio = false;
        this.songElement.src = activeSong.mediaUri;
        this.applyActiveSongVolume();
        // Metadata establishes a seekable timeline before applying the requested Song position.
        this.songElement.addEventListener("loadedmetadata", () => this.startActiveSongPlayback(true), { once: true });
        this.songElement.load();
    }

    /**
     * Starts playback for the active Song.
     *
     * @param {boolean} applyStartPosition Whether to seek before playback.
     */
    startActiveSongPlayback(applyStartPosition) {
        const activeSong = this.activeSong;
        if (activeSong == null) {
            return;
        }

        if (applyStartPosition) {
            try {
                this.songElement.currentTime = activeSong.positionSeconds;
            }
            catch {
                this.reportActiveSongEvent(SongEventType.Failed);
                return;
            }
        }

        void this.songElement.play().catch(() => {
            if (this.isActiveSong(activeSong.songId, activeSong.commandId)) {
                this.reportActiveSongEvent(SongEventType.Failed);
            }
        });
    }

    /** Stops and unloads the active Song so stale events cannot complete its replacement. */
    stopActiveSong() {
        if (this.activeSong == null || this.songElement == null) {
            return;
        }

        this.activeSong = null;
        this.songElement.pause();
        this.songElement.removeAttribute("src");

        // load() commits source removal and cancels the media request in every supported browser.
        this.songElement.load();
    }

    /** Applies the active Song volume, clamped to the browser range. */
    applyActiveSongVolume() {
        if (this.activeSong == null) {
            return;
        }

        const volume = Math.min(1, Math.max(0, this.activeSong.volume));
        if (this.songGainNode != null) {
            this.songGainNode.gain.value = volume;
        }
        else {
            this.songElement.volume = volume;
        }
    }

    /**
     * Returns whether a command belongs to the active Song.
     *
     * @param {number} songId Song handle.
     * @param {number} commandId Song command identifier.
     * @returns {boolean} Whether the command matches the active Song.
     */
    isActiveSong(songId, commandId) {
        return this.activeSong?.songId === songId && this.activeSong.commandId === commandId;
    }

    /**
     * Reports a terminal event for the active Song and clears it.
     *
     * @param {number} type Song event type.
     */
    reportActiveSongEvent(type) {
        const activeSong = this.activeSong;
        if (activeSong == null) {
            return;
        }

        this.activeSong = null;
        this.reportSongEvent(activeSong.songId, activeSong.commandId, type);
    }

    /**
     * Reports a Song event to the managed runtime.
     *
     * @param {number} songId Song handle.
     * @param {number} commandId Song command identifier.
     * @param {number} type Song event type.
     */
    reportSongEvent(songId, commandId, type) {
        const notifySongEvent = this.getRuntime()?.Module?._MGM_Web_NotifySongEvent;
        if (typeof notifySongEvent === "function") {
            notifySongEvent(songId, commandId, type);
        }
    }
}
