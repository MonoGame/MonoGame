// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

const invalidNameCharacters = "/\\:";
export const storageTotalSpace = 5 * 1024 * 1024;

function validName(name) {
    if (typeof name !== "string" || name.trim().length === 0) {
        return false;
    }

    // Reject directory navigation
    if (name === "." || name === "..") {
        return false;
    }

    for (const character of name) {
        const characterCode = character.charCodeAt(0);

        // Reject Ascii control characters (0-31 and 127)
        const isControlCharacter = characterCode < 32 || characterCode === 127;
        if (isControlCharacter) {
            return false;
        }

        // Reject path separators and colons
        // (e.g. saves/save.dat or C:\save.dat)
        if (invalidNameCharacters.includes(character)) {
            return false;
        }
    }

    return true;
}

function validPath(path) {
    if (typeof path !== "string" || path.length === 0) {
        return false;
    }

    const relativePath = path.endsWith("/") ? path.slice(0, -1) : path;
    const segments = relativePath.split("/");
    for (const segment of segments) {
        if (!validName(segment)) {
            return false;
        }
    }

    return true;
}

function keyFor(title, player, name) {
    if (!validName(title)) {
        throw new Error("Invalid storage title.");
    }

    if (!Number.isInteger(player) || player < -1 || player > 3) {
        throw new Error("Invalid storage player index.");
    }

    if (!validName(name) || name.startsWith("~")) {
        throw new Error("Invalid storage container name.");
    }

    return "monogame:storage:" + JSON.stringify([title, player, name]);
}

function readEntries(entries) {
    if (!Array.isArray(entries)) {
        throw new Error("Invalid storage snapshot.");
    }

    const result = new Map();
    for (const entry of entries) {
        // Require a unique valid path/data pair
        if (!Array.isArray(entry) || entry.length !== 2 || !validPath(entry[0]) || result.has(entry[0])) {
            throw new Error("Invalid storage entry.");
        }

        const path = entry[0];
        const data = entry[1];

        // Directories hold a null value, files are base64
        if (path.endsWith("/") ? data !== null : typeof data !== "string" || btoa(atob(data)) !== data) {
            throw new Error("Invalid storage file data.");
        }

        result.set(path, data);
    }

    for (const path of result.keys()) {
        const bare = path.endsWith("/") ? path.slice(0, -1) : path;

        // Path cannot be both a file and a directory
        if (result.has(bare) && result.has(bare + "/")) {
            throw new Error("Conflicting storage entries.");
        }

        let slash = bare.indexOf("/");
        while (slash !== -1) {

            if (result.get(bare.slice(0, slash + 1)) !== null) {
                throw new Error("Missing storage directory.");
            }

            slash = bare.indexOf("/", slash + 1);
        }
    }

    return result;
}

/** Maintains one native runtime's staged localStorage container snapshots. */
export class BrowserStorage {
    constructor() {
        this.containers = new Map();
        this.nextId = 1;
    }

    open(title, player, name, canCreate) {
        try {
            const key = keyFor(title, player, name);
            const stored = globalThis.localStorage.getItem(key);
            if (stored === null && !canCreate) {
                return 0;
            }

            let entries = new Map();
            if (stored !== null) {
                const snapshot = JSON.parse(stored);
                if (snapshot === null) {
                    throw new Error("Invalid storage snapshot.");
                }

                entries = readEntries(snapshot.entries);
            }

            const id = this.nextId++;
            this.containers.set(id, { key, entries, invalid: false });
            return id;
        }
        catch (error) {
            console.error("MonoGame storage operation failed.", error);
            return 0;
        }
    }

    close(id) {
        this.containers.delete(id);
    }

    remove(title, player, name) {
        try {
            globalThis.localStorage.removeItem(keyFor(title, player, name));
            return true;
        }
        catch (error) {
            console.error("MonoGame storage operation failed.", error);
            return false;
        }
    }

    enumerate(id) {
        try {
            return Array.from(this.containers.get(id).entries.keys()).join("\n");
        }
        catch (error) {
            console.error("MonoGame storage operation failed.", error);
            return null;
        }
    }

    load(id, path) {
        try {
            const data = this.containers.get(id).entries.get(path);
            if (!validPath(path) || path.endsWith("/") || typeof data !== "string") {
                throw new Error("Storage file not found or invalid.");
            }

            return Uint8Array.from(atob(data), character => character.charCodeAt(0));
        }
        catch (error) {
            console.error("MonoGame storage operation failed.", error);
            return null;
        }
    }

    mutate(id, path, operation, bytes) {
        const container = this.containers.get(id);
        try {
            if (!validPath(path) || path.endsWith("/") !== (operation === "mkdir" || operation === "rmdir")) {
                throw new Error("Invalid storage path.");
            }

            const entries = container.entries;
            switch (operation) {
                case "mkdir": {
                    let parent = "";
                    for (const part of path.slice(0, -1).split("/")) {
                        parent += part;
                        if (entries.has(parent)) {
                            throw new Error("A storage file occupies the directory path.");
                        }

                        parent += "/";
                        entries.set(parent, null);
                    }

                    break;
                }
                case "rmdir": {
                    for (const key of entries.keys()) {
                        if (key.startsWith(path)) {
                            entries.delete(key);
                        }
                    }

                    break;
                }
                case "delete": {
                    entries.delete(path);
                    break;
                }
                case "save": {
                    const slash = path.lastIndexOf("/");
                    if (entries.has(path + "/") || (slash !== -1 && entries.get(path.slice(0, slash + 1)) !== null)) {
                        throw new Error("Invalid storage parent directory.");
                    }

                    let binary = "";
                    for (const byte of bytes) {
                        binary += String.fromCharCode(byte);
                    }

                    entries.set(path, btoa(binary));
                    break;
                }
                default:
                    throw new Error("Invalid storage operation.");
            }

            return true;
        }
        catch (error) {
            console.error("MonoGame storage operation failed.", error);
            if (container) {
                // The shared adapter ignores mutation failures; never persist a partial staged batch.
                container.invalid = true;
            }

            return false;
        }
    }

    commit(id) {
        try {
            const container = this.containers.get(id);
            if (container.invalid) {
                throw new Error("Invalid staged storage batch; close the container and reopen it.");
            }

            const snapshot = JSON.stringify({ entries: Array.from(container.entries) });

            globalThis.localStorage.setItem(container.key, snapshot);
        }
        catch (error) {
            console.error("MonoGame storage operation failed.", error);
        }
    }
}

export function getStorageFreeSpace() {
    let usedBytes = 0;

    try {
        const storage = globalThis.localStorage;
        for (let i = 0; i < storage.length; i++) {
            const key = storage.key(i);
            if (key === null) {
                continue;
            }

            const value = storage.getItem(key);
            if (value !== null) {
                usedBytes += (key.length + value.length) * 2;
            }
        }
    }
    catch (error) {
        console.error("MonoGame storage capacity query failed.", error);
        return 0;
    }

    return Math.max(0, storageTotalSpace - usedBytes);
}
