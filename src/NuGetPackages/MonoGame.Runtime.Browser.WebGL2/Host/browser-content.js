// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

import { BrowserHostStartupError, HostStage } from "./browser-host-common.js";

/**
 * Stages published content into the Emscripten filesystem before synchronous managed APIs use it.
 */
export class BrowserContent {
    /**
     * Creates the content staging service.
     *
     * @param {{ contentBaseUri: string, startupContentManifestUri: string }} config Host content configuration.
     * @param {() => object | null} getRuntime Gets the managed runtime.
     * @param {(stage: string, message: string) => void} logStage Logs staging progress.
     */
    constructor(config, getRuntime, logStage) {
        this.config = config;
        this.getRuntime = getRuntime;
        this.logStage = logStage;
        this.assetPackStagingPromises = new Map();
        this.contentBase64ByPath = new Map();
    }

    /**
     * Stages the manifest required before the managed application starts.
     *
     * @returns {Promise<void>} Completes when startup content is available in the virtual filesystem.
     */
    async stageStartupContentAsync() {
        this.logStage(HostStage.ContentStaging, "Staging startup content.");
        await this.stageContentManifestAsync(this.config.startupContentManifestUri);
        this.logStage(HostStage.ContentStaging, "Startup content staged.");
    }

    /**
     * Downloads a manifest and stages its listed assets in the virtual filesystem.
     *
     * @param {string} manifestUri Application relative manifest URI.
     * @returns {Promise<void>} Completes when every listed asset is staged.
     * @throws {BrowserHostStartupError} When the manifest or an asset cannot be downloaded.
     */
    async stageContentManifestAsync(manifestUri) {
        const fileSystem = this.getFileSystem();
        const requestUri = this.resolveContentUri(manifestUri);
        const response = await fetch(requestUri);
        if (!response.ok) {
            throw new BrowserHostStartupError(
                HostStage.ContentStaging,
                "content_manifest_fetch_failed",
                `The content manifest '${manifestUri}' could not be downloaded. HTTP ${response.status}.`);
        }

        const manifestText = await response.text();

        // Normalize each entry before it becomes a virtual-filesystem path.
        const contentPaths = manifestText
            .split(/\r?\n/)
            .map((path) => path.trim())
            .filter((path) => path.length > 0)
            .map((path) => this.normalizeVfsPath(path));

        // TODO: Just going to request all startup assets
        //       we might need add a bounded request pool
        //       if projects exceed browser connection limits
        await Promise.all(contentPaths.map(async (contentPath) => {
            const contentResponse = await fetch(this.resolveContentUri(contentPath));
            if (!contentResponse.ok) {
                throw new BrowserHostStartupError(
                    HostStage.ContentStaging,
                    "content_asset_fetch_failed",
                    `The content asset '${contentPath}' could not be downloaded. HTTP ${contentResponse.status}.`);
            }

            // Emscripten writeFile does not create parents, while managed content opens the published path unchanged.
            const directoryPath = contentPath.substring(0, contentPath.lastIndexOf("/"));
            if (directoryPath.length > 0) {
                fileSystem.mkdirTree(`/${directoryPath}`);
            }

            fileSystem.writeFile(`/${contentPath}`, new Uint8Array(await contentResponse.arrayBuffer()));
        }));
    }

    /**
     * Stages a named asset pack once, sharing the work with concurrent callers.
     *
     * @param {string} assetPackName Asset pack to stage.
     * @returns {Promise<void>} Completes when the asset pack is staged.
     */
    async stageAssetPackAsync(assetPackName) {
        const normalizedAssetPackName = this.normalizeAssetPackName(assetPackName);
        let stagingPromise = this.assetPackStagingPromises.get(normalizedAssetPackName);
        if (stagingPromise == null) {
            const manifestUri = `Content/asset-packs/${encodeURIComponent(normalizedAssetPackName)}.txt`;
            this.logStage(HostStage.ContentStaging, `Staging asset pack '${normalizedAssetPackName}'.`);
            stagingPromise = this.stageContentManifestAsync(manifestUri);
            this.assetPackStagingPromises.set(normalizedAssetPackName, stagingPromise);

            try {
                await stagingPromise;
                this.logStage(HostStage.ContentStaging, `Asset pack '${normalizedAssetPackName}' staged.`);
            }
            catch (error) {
                this.assetPackStagingPromises.delete(normalizedAssetPackName);
                throw error;
            }

            return;
        }

        await stagingPromise;
    }

    /**
     * Returns content for the synchronous content bridge, or `null` when it cannot be loaded.
     *
     * @param {string} relativePath Application-relative content path.
     * @returns {string | null} Base64-encoded content.
     */
    tryGetContentBase64(relativePath) {
        const normalizedPath = normalizeContentPath(relativePath);
        if (normalizedPath == null) {
            return null;
        }

        if (this.contentBase64ByPath.has(normalizedPath)) {
            return this.contentBase64ByPath.get(normalizedPath);
        }

        const requestUri = this.resolveContentUri(normalizedPath);
        if (requestUri == null) {
            return null;
        }

        // TODO: Using synchronous XHR preserves TitleContainer semantics for now.
        //       We can replace this with an async browser asset pipeline once the
        //       framework loading can do async boundaries.
        const request = new XMLHttpRequest();
        request.open("GET", requestUri, false);
        request.overrideMimeType("text/plain; charset=x-user-defined");

        try {
            request.send();
        }
        catch {
            return null;
        }

        if (request.status < 200 || request.status >= 300) {
            return null;
        }

        // The content bridge returns a string, so cache the encoded bytes and avoid repeat synchronous requests.
        const encodedContent = encodeResponseTextAsBase64(request.responseText);
        this.contentBase64ByPath.set(normalizedPath, encodedContent);
        return encodedContent;
    }

    /**
     * Returns the Emscripten filesystem used to stage content.
     *
     * @returns {{ mkdirTree: Function, writeFile: Function }} Emscripten filesystem.
     * @throws {BrowserHostStartupError} When the runtime does not provide content staging APIs.
     */
    getFileSystem() {
        const fileSystem = this.getRuntime()?.Module?.FS;
        if (fileSystem == null
            || typeof fileSystem.mkdirTree !== "function"
            || typeof fileSystem.writeFile !== "function") {
            throw new BrowserHostStartupError(
                HostStage.ContentStaging,
                "emscripten_filesystem_missing",
                "The managed runtime did not expose an Emscripten filesystem capable of staging content.");
        }

        return fileSystem;
    }

    /**
     * Returns a safe Content-rooted virtual-filesystem path.
     *
     * @param {string} path Manifest path.
     * @returns {string} Validated virtual filesystem path.
     * @throws {BrowserHostStartupError} When the path is empty, outside `Content`, or contains parent traversal.
     */
    normalizeVfsPath(path) {
        const normalizedPath = normalizeContentPath(path);
        if (normalizedPath == null
            || !normalizedPath.startsWith("Content/")
            // Parent traversal could escape the content root when the path is written to the virtual filesystem.
            || normalizedPath.includes("/../")
            || normalizedPath.endsWith("/..")) {
            throw new BrowserHostStartupError(
                HostStage.ContentStaging,
                "content_manifest_path_invalid",
                `The content manifest contains an invalid path '${path}'.`);
        }

        return normalizedPath;
    }

    /**
     * Returns an asset pack name that can be used as a manifest filename.
     *
     * @param {string} assetPackName Asset pack name.
     * @returns {string} Validated asset pack name.
     * @throws {BrowserHostStartupError} When the name is empty or contains path syntax.
     */
    normalizeAssetPackName(assetPackName) {
        if (typeof assetPackName !== "string"
            || assetPackName.length === 0
            // Asset-pack names become manifest file names, so separators and traversal syntax are not allowed.
            || assetPackName.includes("/")
            || assetPackName.includes("\\")
            || assetPackName.includes("..")) {
            throw new BrowserHostStartupError(
                HostStage.ContentStaging,
                "asset_pack_name_invalid",
                `The asset pack name '${assetPackName}' is invalid.`);
        }

        return assetPackName;
    }

    /**
     * Returns the content URL relative to the application content base.
     *
     * @param {string} relativePath Application relative content path.
     * @returns {string} Absolute content URL.
     * @throws {BrowserHostStartupError} When the path is empty.
     */
    resolveContentUri(relativePath) {
        const normalizedPath = normalizeContentPath(relativePath);
        if (normalizedPath == null) {
            throw new BrowserHostStartupError(
                HostStage.ContentStaging,
                "content_uri_invalid",
                "The browser host received an empty content path.");
        }

        return new URL(normalizedPath, new URL(this.config.contentBaseUri, document.baseURI)).toString();
    }
}

/**
 * Returns a content path with URL separators, or `null` when it is empty.
 *
 * @param {string} relativePath Content path to normalize.
 * @returns {string | null} Path using URL separators.
 */
function normalizeContentPath(relativePath) {
    if (relativePath == null || relativePath.length === 0) {
        return null;
    }

    // Build items may use Windows separators; URLs and Emscripten paths require forward slashes.
    return relativePath.replace(/\\/g, "/");
}

/**
 * Returns the XHR response bytes as Base64.
 *
 * @param {string} responseText XHR response text with byte values preserved.
 * @returns {string} Base64-encoded response bytes.
 */
function encodeResponseTextAsBase64(responseText) {
    let binary = "";

    for (let index = 0; index < responseText.length; index += 1) {
        binary += String.fromCharCode(responseText.charCodeAt(index) & 0xff);
    }

    return btoa(binary);
}
