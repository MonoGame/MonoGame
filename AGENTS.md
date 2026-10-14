# Purpose
- MonoGameDX12 is a MonoGame fork used as the engine for several games. Keep it game-agnostic.
- Major internal changes are welcome as long as the XNA-style public API stays.

# Upstream
- Never submit anything to MonoGame/MonoGame: no PRs, issues or comments. Its rules forbid AI-assisted contributions.
- The `upstream` branch only fast-forwards to MonoGame's `develop`. Never commit to it.
- Sync by fast-forwarding `upstream`, then merging it into `main`.

# Branches and Pull Requests
- `main` is the development branch. Work branches start from `main` and PRs target `main`.
- Humans open and merge PRs, squashed to one commit each.
- Agent writes title + short summary (max 100 words), and no AI session links.

# Code Style
- Follow CODESTYLE.md and the existing style of the file being changed.
- Keep each file's line endings; most framework and native files are CRLF.

# Native Platform
- A new `MGG_` export gets a DX12 implementation plus stubs in the Vulkan and OpenGL backends.
- Managed APIs a backend can't support throw NotSupportedException or report the feature as unavailable.
- Windows natives build for x64 only.
- Run DX12 changes with `MGG_DX12_DEBUG=1` and fix every debug layer error they report.

# Packages
- Games consume the fork as NuGet packages versioned `3.8.6-siki.N`.
- Bump N on every rebuild that games pick up; NuGet caches packages by version.
- Build: `dotnet run --project build/Build.csproj -- --target "Build Native" --build-version <version>`, then the same with `--target "Pack Native Runtime" --exclusive`.
