// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

import { BrowserHostError, HostStage } from "./browser-host-common.js";

/** Connects KeyboardInput requests to an application owned HTML dialog. */
export class BrowserKeyboard {
    /** @param {object} host Browser host coordinator. */
    constructor(host) {
        this.host = host;
        this.dialog = null;
        this.events = null;
        this.previousFocus = null;
        const dialog = host.config.keyboardDialog;
        if (dialog == null) {
            return;
        }
        if (!(dialog instanceof HTMLDialogElement) || !host.root.contains(dialog)) {
            throw new BrowserHostError(HostStage.HostBootstrap, "keyboard_dialog_invalid", "keyboardDialog must be an HTMLDialogElement inside the host root.");
        }
        this.title = dialog.querySelector("[data-monogame-keyboard-title]");
        this.description = dialog.querySelector("[data-monogame-keyboard-description]");
        this.input = dialog.querySelector("[data-monogame-keyboard-input]");
        this.cancelButton = dialog.querySelector("[data-monogame-keyboard-cancel]");
        this.form = dialog.querySelector("form");
        if (!(this.title instanceof HTMLElement)
            || !(this.description instanceof HTMLElement)
            || !(this.input instanceof HTMLInputElement)
            || !(this.cancelButton instanceof HTMLButtonElement)
            || this.cancelButton.type !== "button"
            || !(this.form instanceof HTMLFormElement)
            || this.input.form !== this.form) {
            throw new BrowserHostError(HostStage.HostBootstrap, "keyboard_dialog_invalid", "keyboardDialog requires marked title, description, input, and cancel button elements, with the input inside its form and the cancel button using type='button'.");
        }
    }

    /**
     * Opens the supplied dialog.
     * @param {string} title Requested title.
     * @param {string} description Requested description.
     * @param {string} defaultText Initial text.
     * @param {boolean} password Whether to mask the input.
     * @returns {boolean} False when the page did not supply a keyboard dialog.
     */
    show(title, description, defaultText, password) {
        const dialog = this.host.config.keyboardDialog;
        if (dialog == null) {
            return false;
        }
        this.cancel();
        this.previousFocus = document.activeElement;
        this.dialog = dialog;
        const events = new AbortController();
        this.events = events;
        const options = { signal: events.signal };
        this.title.textContent = title;
        this.description.textContent = description;
        this.input.type = password ? "password" : "text";
        this.input.value = defaultText;
        let composing = false;
        this.input.addEventListener("compositionstart", () => { composing = true; }, options);
        this.input.addEventListener("compositionend", () => { composing = false; }, options);
        const finish = (result) => {
            if (this.events !== events) {
                return;
            }
            this.cancel();
            this.host.native?.completeKeyboardInput(result ?? "", result == null ? 1 : 0);
        };
        this.cancelButton.addEventListener("click", () => finish(null), options);
        dialog.addEventListener("cancel", (event) => {
            event.preventDefault();
            finish(null);
        }, options);
        dialog.addEventListener("close", () => {
            // Ignore a queued close event from a previous request after reopening.
            if (!dialog.open) {
                finish(null);
            }
        }, options);
        this.form.addEventListener("submit", (event) => {
            event.preventDefault();
            if (!composing) {
                finish(this.input.value);
            }
        }, options);
        for (const eventName of ["keydown", "keyup", "keypress"]) {
            dialog.addEventListener(eventName, (event) => {
                // SDL listens on window; dialog typing must stay out of game controls.
                event.stopPropagation();
                if (event.type === "keydown" && event.key === "Enter" && event.isComposing) {
                    event.preventDefault();
                }
            }, options);
        }
        try {
            dialog.showModal();
            this.input.focus();
            this.input.setSelectionRange(defaultText.length, defaultText.length);
            return true;
        }
        catch (error) {
            this.cancel();
            throw error;
        }
    }

    /** Closes a request. */
    cancel() {
        const dialog = this.dialog;
        if (dialog == null) {
            return;
        }
        this.dialog = null;
        this.events.abort();
        this.events = null;
        this.input.value = "";
        dialog.close();
        const previousFocus = this.previousFocus;
        this.previousFocus = null;
        if (previousFocus?.isConnected && !this.host.isDisposed) {
            previousFocus.focus({ preventScroll: true });
        }
    }

    /** Cancels a pending request. */
    dispose() {
        if (this.dialog != null) {
            this.cancel();
            this.host.native?.completeKeyboardInput("", 1);
        }
    }
}
