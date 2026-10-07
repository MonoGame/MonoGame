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
        this.songAbortController = null;
        this.activationAbortController = new AbortController();
        this.isDisposed = false;
    }

    /**
     * Listens for canvas gestures that can activate browser audio.
     *
     * @param {HTMLCanvasElement} canvas Game canvas.
     */
    observeActivation(canvas) {
        const activateAudio = () => this.requestAudioActivation();

        const eventOptions = { signal: this.activationAbortController.signal };
        canvas.addEventListener("pointerdown", activateAudio, { passive: true, signal: this.activationAbortController.signal });
        canvas.addEventListener("touchend", activateAudio, eventOptions);
        canvas.addEventListener("keydown", activateAudio, eventOptions);
    }

    /** Requests browser audio activation and updates a startup Song when it succeeds. */
    requestAudioActivation() {
        if (this.isDisposed) {
            return;
        }

        this.audioActivationPromise = this.activateAsync();
        void this.loadPendingSongAfterActivationAsync(this.audioActivationPromise);
    }

    /**
     * Creates the shared audio graph when needed and resumes it.
     *
     * @returns {Promise<boolean>} Whether the audio context is running.
     */
    async activateAsync() {
        if (this.isDisposed) {
            return false;
        }

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

            const audioContext = this.audioContext;
            await audioContext.resume();
            return !this.isDisposed && audioContext.state === "running";
        }
        catch {
            return false;
        }
    }

    /** Stops media playback, releases audio resources, and removes gesture listeners. */
    dispose() {
        if (this.isDisposed) {
            return;
        }

        this.isDisposed = true;
        this.activationAbortController.abort();
        this.songAbortController?.abort();
        this.songAbortController = null;
        this.activeSong = null;

        if (this.songElement != null) {
            this.songElement.pause();
            this.songElement.removeAttribute("src");
            this.songElement.load();
            this.songElement.remove();
            this.songElement = null;
        }

        this.songGainNode?.disconnect();
        this.songGainNode = null;

        if (this.audioContext != null) {
            const audioContext = this.audioContext;
            this.audioContext = null;
            void audioContext.close().catch((error) => {
                console.warn("[MonoGame.Web Host]", "Audio context close failed.", error);
            });
        }

        this.audioActivationPromise = null;
    }

    /**
     * Replaces the active Song with the requested Song.
     *
     * Playback begins muted before browser audio activation when needed.
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

        this.stopActiveSong();

        this.activeSong = {
            songId,
            commandId,
            mediaUri,
            positionSeconds: Math.max(0, positionMilliseconds / 1000),
            volume,
            waitingForAudio: !this.isAudioRunning(),
            waitingForPlaybackActivation: false
        };

        // A game may start a song on the initial load. However the actual
        // audio cannot be heard until audio is unlocked from a user gesture
        // such as clicking the canvas.
        // So we start the song muted so its timeline can advance before user interaction
        // unlocks audio. Unmuting avoids starting or restarting playback after activation.
        this.songElement.muted = this.activeSong.waitingForAudio;
        this.loadActiveSongMedia();

        if (this.activeSong.waitingForAudio) {
            if (this.audioActivationPromise == null) {
                this.requestAudioActivation();
            }
            else {
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
        if (this.isDisposed || !activated || !this.isAudioRunning()) {
            return;
        }

        const activeSong = this.activeSong;
        if (activeSong == null || !activeSong.waitingForAudio) {
            return;
        }

        activeSong.waitingForAudio = false;
        this.songElement.muted = false;

        if (activeSong.waitingForPlaybackActivation) {
            activeSong.waitingForPlaybackActivation = false;
            this.startActiveSongPlayback(false);
        }
    }

    /** Loads the active Song after the shared audio graph is ready. */
    loadActiveSongMedia() {
        const activeSong = this.activeSong;
        if (activeSong == null) {
            return;
        }

        this.songElement.src = activeSong.mediaUri;
        this.applyActiveSongVolume();
        // Metadata establishes a seekable timeline before applying the requested Song position.
        const songAbortController = new AbortController();
        this.songAbortController = songAbortController;
        this.songElement.addEventListener("loadedmetadata", () => this.startActiveSongPlayback(true), { once: true, signal: songAbortController.signal });
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
                if (activeSong.waitingForAudio) {
                    activeSong.waitingForPlaybackActivation = true;
                }
                else {
                    this.reportActiveSongEvent(SongEventType.Failed);
                }
            }
        });
    }

    /** Stops and unloads the active Song so stale events cannot complete its replacement. */
    stopActiveSong() {
        this.songAbortController?.abort();
        this.songAbortController = null;

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

    /** @returns {boolean} Whether the shared audio graph can emit sound. */
    isAudioRunning() {
        return this.audioContext?.state === "running" && this.songGainNode != null;
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
