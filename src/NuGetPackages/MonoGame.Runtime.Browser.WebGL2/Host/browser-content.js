// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

import { BrowserHostError, HostStage } from "./browser-host-common.js";

const MaxConcurrentContentFetches = 6;

/**
 * Stages published content into the Emscripten filesystem before synchronous managed APIs use it.
 */
export class BrowserContent {
    /**
     * Creates the content staging service.
     *
     * @param {object} host Browser host coordinator.
     */
    constructor(host) {
        this.host = host;
        this.assetPackStagingPromises = new Map();
    }

    /**
     * Stages the manifest required before the managed application starts.
     *
     * @returns {Promise<void>} Completes when startup content is available in the virtual filesystem.
     */
    async stageStartupContentAsync() {
        this.host.logStage(HostStage.ContentStaging, "Fetching assets...");
        await this.stageContentManifestAsync(this.host.config.startupContentManifestUri);
        this.host.logStage(HostStage.ContentStaging, "Startup content staged.");
    }

    /**
     * Downloads a manifest and stages its listed assets in the virtual filesystem.
     *
     * @param {string} manifestUri Application relative manifest URI.
     * @returns {Promise<void>} Completes when every listed asset is staged.
     * @throws {BrowserHostError} When the manifest or an asset cannot be downloaded.
     */
    async stageContentManifestAsync(manifestUri) {
        const fileSystem = this.host.native.fileSystem;
        const normalizedManifestUri = normalizeContentPath(manifestUri);
        if (normalizedManifestUri == null) {
            throw new BrowserHostError(
                HostStage.ContentStaging,
                "content_uri_invalid",
                "The browser host received an empty content path.");
        }

        const requestUri = this.resolveContentUri(normalizedManifestUri);
        const response = await fetch(requestUri);
        if (!response.ok) {
            throw new BrowserHostError(
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

        await this.stageContentPathsAsync(contentPaths, fileSystem);
    }

    /**
     * Stages content files with bounded parallel downloads.
     *
     * @param {string[]} contentPaths Validated application-relative content paths.
     * @param {object} fileSystem Emscripten filesystem.
     * @returns {Promise<void>} Completes when every content file is staged.
     */
    async stageContentPathsAsync(contentPaths, fileSystem) {
        const abortController = new AbortController();
        let nextContentPathIndex = 0;
        let completedFiles = 0;
        const workerCount = Math.min(MaxConcurrentContentFetches, contentPaths.length);
        const workers = [];
        this.host.notify(this.host.callbacks.onStatus, {
            stage: HostStage.ContentStaging,
            message: `Fetching assets: 0/${contentPaths.length} files`
        }, "onStatus");

        for (let workerIndex = 0; workerIndex < workerCount; workerIndex++) {
            workers.push((async () => {
                while (nextContentPathIndex < contentPaths.length) {
                    const contentPath = contentPaths[nextContentPathIndex++];
                    const contentResponse = await fetch(this.resolveContentUri(contentPath), {
                        signal: abortController.signal
                    });
                    if (!contentResponse.ok) {
                        throw new BrowserHostError(
                            HostStage.ContentStaging,
                            "content_asset_fetch_failed",
                            `The content asset '${contentPath}' could not be downloaded. HTTP ${contentResponse.status}.`);
                    }

                    const contentBytes = new Uint8Array(await contentResponse.arrayBuffer());

                    // Emscripten writeFile does not create parents, while managed content opens the published path unchanged.
                    const directoryPath = contentPath.substring(0, contentPath.lastIndexOf("/"));
                    if (directoryPath.length > 0) {
                        fileSystem.mkdirTree(`/${directoryPath}`);
                    }

                    fileSystem.writeFile(`/${contentPath}`, contentBytes);
                    completedFiles++;
                    this.host.notify(this.host.callbacks.onStatus, {
                        stage: HostStage.ContentStaging,
                        message: `Fetching assets: ${completedFiles}/${contentPaths.length} files`
                    }, "onStatus");
                }
            })());
        }

        try {
            await Promise.all(workers);
        }
        catch (error) {
            abortController.abort();
            await Promise.allSettled(workers);
            throw error;
        }
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
            const manifestUri = `_monogame/asset-packs/${encodeURIComponent(normalizedAssetPackName)}.txt`;
            this.host.logStage(HostStage.ContentStaging, `Staging asset pack '${normalizedAssetPackName}'.`);
            stagingPromise = this.stageContentManifestAsync(manifestUri);
            this.assetPackStagingPromises.set(normalizedAssetPackName, stagingPromise);

            try {
                await stagingPromise;
                this.host.logStage(HostStage.ContentStaging, `Asset pack '${normalizedAssetPackName}' staged.`);
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
     * Returns a safe relative virtual-filesystem path.
     *
     * @param {string} path Manifest path.
     * @returns {string} Validated virtual filesystem path.
     * @throws {BrowserHostError} When the path is empty, absolute, or contains parent traversal.
     */
    normalizeVfsPath(path) {
        const normalizedPath = normalizeContentPath(path);
        if (normalizedPath == null
            || normalizedPath.startsWith("/")
            || normalizedPath.startsWith("./")
            || normalizedPath === ".."
            || normalizedPath.startsWith("../")
            // Parent traversal could escape the virtual filesystem when the path is written.
            || normalizedPath.includes("/../")
            || normalizedPath.endsWith("/..")
            // A manifest path is relative to the application and cannot be a URL or a drive path.
            || normalizedPath.includes(":")) {
            throw new BrowserHostError(
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
     * @throws {BrowserHostError} When the name is empty or contains path syntax.
     */
    normalizeAssetPackName(assetPackName) {
        if (typeof assetPackName !== "string"
            || assetPackName.length === 0
            // Asset-pack names become manifest file names, so separators and traversal syntax are not allowed.
            || assetPackName.includes("/")
            || assetPackName.includes("\\")
            || assetPackName.includes("..")) {
            throw new BrowserHostError(
                HostStage.ContentStaging,
                "asset_pack_name_invalid",
                `The asset pack name '${assetPackName}' is invalid.`);
        }

        return assetPackName;
    }

    /**
     * Returns the content URL relative to the application content base.
     *
     * @param {string} normalizedPath Normalized application-relative content path.
     * @returns {string} Absolute content URL.
     */
    resolveContentUri(normalizedPath) {
        return new URL(normalizedPath, new URL(this.host.config.contentBaseUri, document.baseURI)).toString();
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
